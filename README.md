# OpenFrame

**OpenFrame** is an early exploration of a "glasses OS": lightweight smart-glasses
that act as a basic home screen with widgets, with the heavy computing supplied by
a paired phone — and, later, extended with SDKs so third-party apps can run on the
glasses.

This repository currently contains the **first slice of the phone side**: the
OpenFrame iPhone **host shell**. It is a SwiftUI UI shell only — real transports
come later.

---

## Current state — the iOS host shell

A SwiftUI iPhone app (`ios-host/`) that today shows:

- an OpenFrame logo placeholder and title,
- two **mock** connection-status rows (Simulator, Glasses),
- a **Start / Stop Session** button that toggles mock state, and
- a list of **mock notifications** decoded from bundled local JSON.

It targets **iOS 16.0** and its Xcode project is generated from
[`ios-host/project.yml`](ios-host/project.yml) with **XcodeGen**.

### Not implemented (on purpose)

There are **no live iOS notifications, no Bluetooth, no networking, and no Unity
changes** in this repository. All notifications are mock data from local JSON;
the status rows are placeholders. These are deliberately left as follow-up work
so the shell can be reviewed first. See the out-of-scope section in
[`documentation/setup-macos.md`](documentation/setup-macos.md).

> **Note on verification.** The Xcode project generation, JSON validity/sync, and
> the Swift model+loader **decoding logic** were verified on a
> Command-Line-Tools-only machine. A full **iOS app build was not performed** here
> because Xcode.app / the iOS SDK / a Simulator were not present. Details:
> [beginner guide §4](documentation/ios-host-beginner-guide.md#4-verified-vs-not-verified--read-this-before-trusting-anything).

---

## Documentation

| Doc | What it's for |
|---|---|
| 📘 **[iOS Host Beginner Guide](documentation/ios-host-beginner-guide.md)** | **Start here if you're new to Xcode or Swift.** A full, from-scratch walkthrough: Xcode & Swift concepts, architecture, every file, how to build/test/run, how to change things safely, and debugging. |
| 🛠️ [macOS Setup](documentation/setup-macos.md) | Concise prerequisites and the exact build / test / run / device commands. |
| 📱 [iOS Host README](ios-host/README.md) | The host module's developer entry point and layout. |
| 🔌 [Notification Protocol Examples](protocol/examples/README.md) | The canonical notification JSON schema and the keep-in-sync rule. |
| 🥽 [Windows Unity Simulator Setup](documentation/setup-windows.md) | How to open and test the notification simulator in Unity. |
| 🔧 [Monocular Hardware Prototype BOM](documentation/prototype-bom.md) | Finalized parts, costs, safety notes, and build order for the first wearable prototype. |
| 🥽 [Hardware Prototype Assembly](documentation/prototype-assembly.md) | Step-by-step electronics, optics, combiner cutting, CAD, charging, and wear-test procedure. |
| 📐 [V0 CAD](cad/README.md) | STEP export of the V0 glasses assembly, the Onshape link, and the FeatureScript source. |

---

## Quick start

Requires a Mac. Building/running the app requires **full Xcode** (not just the
Command Line Tools) plus **XcodeGen** (`brew install xcodegen`). From the
repository root:

```bash
cd ios-host
xcodegen generate
open OpenFrameHost.xcodeproj
```

Then pick an iOS Simulator and press **⌘R** to run or **⌘U** to test. The full,
beginner-friendly walkthrough — including the terminal-only equivalents — is in
the [iOS Host Beginner Guide](documentation/ios-host-beginner-guide.md) and
[macOS Setup](documentation/setup-macos.md).

---

## Repository layout

```
OpenFrame/
├── README.md                       # You are here
├── cad/                            # V0 assembly STEP, FeatureScript source, Onshape link
├── documentation/
│   ├── ios-host-beginner-guide.md  # Full beginner walkthrough
│   └── setup-macos.md              # Prerequisites + build/test/run
├── ios-host/                       # The iOS host shell (SwiftUI)
│   ├── project.yml                 # XcodeGen spec — source of truth
│   ├── OpenFrameHost.xcodeproj/    # Generated project (committed)
│   ├── Sources/                    # App entry point, views, view model, models, resources
│   ├── Tests/                      # Local JSON decoding tests
│   └── README.md                   # Host module entry point
└── protocol/
    └── examples/                   # Canonical notification JSON schema + docs
```
