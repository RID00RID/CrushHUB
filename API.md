# API CrashHub

Обычный HTTP + JSON, без SDK и внешних зависимостей — подключается из любого движка: Unreal, Unity, Godot, GameMaker, собственный. Всё, что нужно, — уметь отправить POST с телом в JSON.

## Аутентификация

Каждый запрос требует заголовок `X-Api-Key` с ключом проекта. Ключ лежит в панели: проект → **Настройки** → «Ключ приложения». Там же указан адрес сервера.

Ключ должен принадлежать проекту, номер которого стоит в адресе, иначе `401`.

Базовый адрес: `{сервер}/ingest/{projectId}`.

## Проверка ключа

```http
GET /ingest/1/ping
X-Api-Key: ch_live_...
```

```json
{ "projectId": 1, "project": "Skyward Rift" }
```

Удобно дёрнуть при старте игры, чтобы убедиться, что ключ и адрес прописаны верно.

## Конфигурация машины игрока

Вызывается, когда игрок согласился делиться данными о системе. Повторный вызов с тем же `SystemID` обновляет конфигурацию и не создаёт дубликата — можно слать хоть при каждом запуске.

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

| Поле | Обязательно | Описание |
| --- | --- | --- |
| `SystemID` | да | Уникальный идентификатор машины. Чем стабильнее, тем лучше: аппаратный UUID, а не случайный GUID при каждом запуске |
| остальные | нет | Произвольные поля: JSON сохраняется целиком, незнакомое не теряется |

Обёртка `UserConfig` необязательна — можно прислать сам объект. `Memory` — в мегабайтах. Пример реального файла: [user-config.example.json](user-config.example.json).

Ответ `200`:

```json
{ "id": 2, "systemId": "9B41C7D2-1A55-4E90-8C31-77AA0E5512FF" }
```

## Краш

```http
POST /ingest/1/crashes
X-Api-Key: ch_live_...
Content-Type: application/json
```

```json
{
  "title": "Fatal error: Access violation - code 0xC0000005",
  "callstack": "AActor::TickActor()\nFEngineLoop::Tick()\nGuardedMain()",
  "version": "1.4.2",
  "platform": "Windows",
  "userId": "9B41C7D2-1A55-4E90-8C31-77AA0E5512FF",
  "occurredAt": "2026-08-14T11:30:00Z"
}
```

| Поле | Обязательно | Описание |
| --- | --- | --- |
| `title` | да | Заголовок ошибки, до 300 символов |
| `callstack` | нет | Стек вызовов текстом, переносы строк — `\n` |
| `version` | нет | Версия сборки игры |
| `platform` | нет | Платформа: Windows, PlayStation 5, Android и так далее |
| `userId` | нет | `SystemID` машины. Если конфигурацию ещё не присылали, машина заведётся автоматически |
| `occurredAt` | нет | Время краша в UTC. Без него берётся время получения |

Ответ `201`:

```json
{ "id": 12 }
```

## Обращение игрока

```http
POST /ingest/1/reports
X-Api-Key: ch_live_...
Content-Type: application/json
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

| Поле | Обязательно | Описание |
| --- | --- | --- |
| `category` | да | Категория обращения, до 100 символов. Список задаёт игра — панель показывает те, что реально приходили |
| `description` | да | Текст от игрока |
| `userId` | нет | `SystemID` машины |
| `createdAt` | нет | Время отправки в UTC |
| `screenshot` | нет | PNG или JPEG в base64, до 5 МБ. Префикс `data:image/png;base64,` допускается |

Формат картинки проверяется по сигнатуре файла, а не по тому, что написал клиент. Файл ложится в `wwwroot/uploads/{projectId}/`.

Ответ `201`:

```json
{ "id": 7, "screenshot": "/uploads/1/7f3c….png" }
```

## Ошибки

| Код | Когда |
| --- | --- |
| `400` | Не прошла валидация тела, битый base64 или неподдерживаемый формат скриншота |
| `401` | Ключ не передан, неизвестен или принадлежит другому проекту |

Тело ошибки — либо `{ "error": "..." }`, либо стандартный problem details от ASP.NET Core с полем `errors`.

## Порядок вызовов в игре

1. Игрок соглашается делиться данными о системе → `POST /users` с конфигурацией.
2. Дальше краши и обращения шлются с `userId`, равным `SystemID` — конфигурацию повторять не нужно.
3. Игрок отказался → не вызывайте `/users` и не передавайте `userId`. Данные сохранятся без привязки к машине.

Порядок не критичен: если событие придёт с незнакомым `SystemID`, машина заведётся сама, а конфигурация подтянется, когда игра её пришлёт.

## Пример на C#

```csharp
using System.Net.Http;
using System.Net.Http.Json;

var http = new HttpClient { BaseAddress = new Uri("https://crashhub.example.com/") };
http.DefaultRequestHeaders.Add("X-Api-Key", "ch_live_...");

await http.PostAsJsonAsync("ingest/1/crashes", new
{
    title = "Fatal error: Access violation - code 0xC0000005",
    callstack = stackTrace,
    version = "1.4.2",
    platform = "Windows",
    userId = systemId
});
```

## Пример на curl

```bash
curl -X POST https://crashhub.example.com/ingest/1/crashes \
  -H "X-Api-Key: ch_live_..." \
  -H "Content-Type: application/json" \
  -d '{"title":"Fatal error: Access violation","version":"1.4.2","platform":"Windows"}'
```
