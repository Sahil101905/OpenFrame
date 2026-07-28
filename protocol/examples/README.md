# OpenFrame Notification Protocol — Examples

This directory holds the **canonical** JSON examples for the OpenFrame
notification protocol. They are the single source of truth for the shape of a
notification as it moves between the phone host and the glasses.

> ⚠️ **Keep in sync.** `ios-host/Sources/Resources/notifications.sample.json`
> is an **exact byte-for-byte copy** of
> [`notifications.sample.json`](./notifications.sample.json) and the Swift model
> in `ios-host/Sources/Models/OpenFrameNotification.swift` decodes it directly.
> If you change the schema here, update the bundled copy and the Swift model
> together. See `documentation/setup-macos.md` for the sync command.

## `notifications.sample.json`

A **JSON array** of notification objects. Each object has the following fields.
All fields are **required** and non-null unless noted.

| Field           | Type     | Notes |
|-----------------|----------|-------|
| `id`            | string   | Stable unique identifier (UUID form in examples). |
| `appIdentifier` | string   | Reverse-DNS bundle id of the originating app. |
| `appName`       | string   | Human-readable app name for display. |
| `title`         | string   | Notification title / primary line. |
| `body`          | string   | Notification body / secondary line. |
| `timestamp`     | string   | ISO 8601 / RFC 3339 UTC instant, e.g. `2026-07-28T14:32:00Z`. |
| `priority`      | string   | One of `low`, `normal`, `high`. Unknown values decode as `normal`. |
| `category`      | string   | Free-form category hint, e.g. `message`, `reminder`, `news`. |

### Example (single element)

```json
{
  "id": "2c7f9a10-4b2e-4c8a-9d1f-0a1b2c3d4e5f",
  "appIdentifier": "com.apple.MobileSMS",
  "appName": "Messages",
  "title": "Alex Kim",
  "body": "Are we still on for lunch tomorrow?",
  "timestamp": "2026-07-28T14:32:00Z",
  "priority": "normal",
  "category": "message"
}
```

These are **mock** notifications for local development only. The host shell does
not request live iOS notifications — see the out-of-scope section of
`documentation/setup-macos.md`.
