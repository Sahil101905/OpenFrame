# OpenFrame Host (iOS)

The OpenFrame iPhone **host shell** — a SwiftUI app that will provide compute for
the glasses. This initial version is a **UI shell only**.

> **New to Xcode or Swift?** Read the full, from-scratch
> **[iOS Host Beginner Guide](../documentation/ios-host-beginner-guide.md)** — it
> explains every Xcode/Swift concept used here, the architecture, each file, and
> how to build, test, run, and modify the app. This README is just the developer
> entry point and does not repeat that guide.

## What it does today

- Home screen with an OpenFrame logo placeholder and title.
- Simulator connection status (mock).
- Glasses connection status (mock).
- **Start / Stop Session** button (mock — toggles state so the UI reacts).
- A **mock notifications** section decoded from bundled local JSON.

## What it does *not* do

**No live iOS notifications, no Bluetooth, no networking, and no Unity changes.**
All notifications are mock data from local JSON; the status rows are placeholders.
See the out-of-scope section in
[`../documentation/setup-macos.md`](../documentation/setup-macos.md) and
[beginner guide §18](../documentation/ios-host-beginner-guide.md#18-boundaries-and-sensible-next-steps).

## Layout

```
ios-host/
├── project.yml                  # XcodeGen spec — source of truth for the project
├── OpenFrameHost.xcodeproj       # Generated: `xcodegen generate`
├── Sources/
│   ├── App/                      # App entry point + SwiftUI views + view model
│   ├── Models/                   # OpenFrameNotification (Codable) + NotificationLoader
│   └── Resources/
│       └── notifications.sample.json   # Exact copy of protocol/examples canonical sample
└── Tests/                        # Local JSON decoding tests
```

## Build

Requires **full Xcode** (not just the Command Line Tools) and **XcodeGen**
(`brew install xcodegen`). From this directory:

```bash
xcodegen generate
open OpenFrameHost.xcodeproj
```

Then pick an iOS Simulator and press **⌘R** to run or **⌘U** to test.

- Full prerequisites, Simulator/device, and terminal-only build/test commands:
  [`../documentation/setup-macos.md`](../documentation/setup-macos.md).
- Concept-by-concept walkthrough and a first-time Xcode tutorial:
  [`../documentation/ios-host-beginner-guide.md`](../documentation/ios-host-beginner-guide.md).

## Notification schema

The Swift model in `Sources/Models/OpenFrameNotification.swift` maps exactly to
the canonical JSON in [`../protocol/examples/`](../protocol/examples/README.md).
The bundled `Sources/Resources/notifications.sample.json` is a **byte-for-byte
copy** of that canonical file. Keep the three in sync — the sync command is in
[`../documentation/setup-macos.md`](../documentation/setup-macos.md), and the
decode path is explained in
[beginner guide §10](../documentation/ios-host-beginner-guide.md#10-the-exact-notification-decode-path).
