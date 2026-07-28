# OpenFrame Host (iOS)

The OpenFrame iPhone **host shell** — a SwiftUI app that will provide compute
for the glasses. This initial version is a UI shell only.

## What it does today

- Home screen with an OpenFrame logo placeholder and title.
- Simulator connection status (mock).
- Glasses connection status (mock).
- **Start / Stop Session** button (mock).
- A **mock notifications** section decoded from bundled local JSON.

## What it does *not* do

No live iOS notifications, no Bluetooth, no networking, no Unity. See the
out-of-scope section in [`../documentation/setup-macos.md`](../documentation/setup-macos.md).

## Layout

```
ios-host/
├── project.yml                 # XcodeGen spec — source of truth for the project
├── OpenFrameHost.xcodeproj      # Generated: `xcodegen generate`
├── Sources/
│   ├── App/                     # App entry point + SwiftUI views + view model
│   ├── Models/                  # OpenFrameNotification (Codable) + NotificationLoader
│   └── Resources/
│       └── notifications.sample.json   # Exact copy of protocol/examples canonical sample
└── Tests/                       # Local JSON decoding tests
```

## Build

```bash
cd ios-host
xcodegen generate
open OpenFrameHost.xcodeproj
```

Full instructions (prerequisites, Simulator, device, tests) are in
[`../documentation/setup-macos.md`](../documentation/setup-macos.md).

## Notification schema

The Swift model in `Sources/Models/OpenFrameNotification.swift` maps exactly to
the canonical JSON in [`../protocol/examples/`](../protocol/examples/). The
bundled `Sources/Resources/notifications.sample.json` is a byte-for-byte copy of
that canonical file. Keep the three in sync.
