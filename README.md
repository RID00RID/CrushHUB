# CrashHub

Self-hosted приём крашей и обращений игроков из игры на Unreal Engine. Игра шлёт данные по HTTP, команда разбирает их в веб-панели.

## Возможности

- **Проекты** — по проекту на игру, у каждого свой ключ приложения для SDK.
- **Crash Report** — список крашей с фильтром по статусу и сортировкой по дате, карточка с callstack.
- **User Report** — обращения игроков с категориями, статусами, скриншотом и сортировкой по дате или статусу.
- **Пользователи** — машины игроков: конфигурация железа, её краши и обращения.
- **Dashboard** — сводка и графики крашей и обращений за неделю.
- **Роли** — администратор (создаёт проекты и пользователей, удаляет данные) и участник (только смотрит и меняет статусы).

## Стек

ASP.NET Core MVC (.NET 10), EF Core + SQL Server, ASP.NET Core Identity. Никаких внешних зависимостей на фронте — обычные Razor-страницы и немного ванильного JS.

## Запуск

Нужны .NET SDK 10 и SQL Server (подойдёт Express или LocalDB).

1. Пропишите строку подключения в `CrushHUB/appsettings.json`:

   ```json
   "Project": {
     "Database": {
       "ConnectionString": "Server=localhost;Database=CrushHUB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
     }
   }
   ```

   Её же можно задать переменной окружения `Project__Database__ConnectionString` — она перекрывает файл.

2. Примените миграции (база создастся сама):

   ```bash
   dotnet ef database update --project CrushHUB/CrushHUB.csproj
   ```

3. Запустите:

   ```bash
   dotnet run --project CrushHUB/CrushHUB.csproj
   ```

Панель откроется на `http://localhost:5045`. Первый вход — логин `admin`, пароль `admin`. **Смените пароль сразу после первого запуска**, эта учётная запись заводится миграцией и одинакова у всех.

## API приёма данных

Все запросы требуют заголовок `X-Api-Key` с ключом проекта — он лежит в панели в разделе «Настройки» проекта. Ключ должен принадлежать проекту, указанному в адресе, иначе `401`.

Базовый адрес: `{сервер}/ingest/{projectId}`.

### Проверка ключа

```http
GET /ingest/1/ping
X-Api-Key: ch_live_...
```

```json
{ "projectId": 1, "project": "Skyward Rift" }
```

### Конфигурация машины игрока

Вызывается, когда игрок согласился делиться данными о системе. Повторный вызов с тем же `SystemID` обновляет конфигурацию, дубликата не создаёт.

```http
POST /ingest/1/users
X-Api-Key: ch_live_...
Content-Type: application/json
```

```json
{
  "UserConfig": {
    "SystemID": "9B41C7D2-1A55-4E90-8C31-77AA0E5512FF",
    "os": { "name": "Windows 11 Pro", "version": "10.0.26200" },
    "CPU": "AMD Ryzen 9 7900X 12-Core Processor",
    "GPU": "NVIDIA GeForce RTX 3080 Ti",
    "Memory": "64629"
  }
}
```

Обёртка `UserConfig` необязательна — можно прислать сам объект. Обязателен только `SystemID`, остальные поля произвольны: JSON сохраняется целиком, незнакомые поля не теряются. `Memory` — в мегабайтах. Пример реального файла — [user-config.example.json](user-config.example.json).

Ответ: `{ "id": 2, "systemId": "9B41C7D2-..." }`.

### Краш

```http
POST /ingest/1/crashes
```

```json
{
  "title": "Fatal error: Access violation - code 0xC0000005",
  "callstack": "UnrealEditor-Engine!AActor::TickActor()\nUnrealEditor-Core!FEngineLoop::Tick()",
  "version": "1.4.2",
  "platform": "Windows",
  "userId": "9B41C7D2-1A55-4E90-8C31-77AA0E5512FF",
  "occurredAt": "2026-08-14T11:30:00Z"
}
```

Обязателен только `title` (до 300 символов). `userId` — это `SystemID` машины; если конфигурацию ещё не присылали, машина заведётся автоматически. Без `occurredAt` берётся время получения. Ответ: `201` и `{ "id": 12 }`.

### Обращение игрока

```http
POST /ingest/1/reports
```

```json
{
  "category": "Производительность",
  "description": "Просадки FPS на локации «Старый порт».",
  "userId": "9B41C7D2-1A55-4E90-8C31-77AA0E5512FF",
  "createdAt": "2026-08-14T12:00:00Z",
  "screenshot": "data:image/png;base64,iVBORw0KGgo..."
}
```

Обязательны `category` и `description`. `screenshot` — PNG или JPEG в base64 до 5 МБ, формат проверяется по сигнатуре файла; сохраняется в `wwwroot/uploads/{projectId}/`. Ответ: `201` и `{ "id": 7, "screenshot": "/uploads/1/....png" }`.

### Ошибки

| Код | Когда |
| --- | --- |
| `400` | Не прошла валидация тела, битый base64 или неподдерживаемый формат скриншота |
| `401` | Ключ не передан, неизвестен или принадлежит другому проекту |

Если игрок отказался делиться данными — не вызывайте `/users` и не передавайте `userId`. Краши и обращения сохранятся без привязки к машине.

## Структура

```
CrushHUB/
  Controllers/
    Api/            приём данных от игры
    Admin/          профиль и управление пользователями (partial-части AdminController)
    Home/           проекты и вкладки проекта (partial-части HomeController)
  Domain/           сущности, DbContext, репозитории
  Models/           модели представлений и тела API-запросов
  Services/         ключи, конфигурации машин, хранение скриншотов, роли
  Views/            Razor-страницы и общие каркасы
  wwwroot/          css, js, загруженные скриншоты
```

Загруженные скриншоты (`wwwroot/uploads`) — данные, а не код: в репозиторий они не попадают.

## Лицензия

[MIT](LICENSE) — можно использовать, изменять и распространять, в том числе в коммерческих проектах.
