# OpenFrame iPhone Host — macOS Setup

This guide gets the **OpenFrame host shell** (`ios-host/`) building and running on
macOS. The host is a SwiftUI app that currently displays connection-status
placeholders and **mock** notifications loaded from bundled local JSON.

---

## 1. Prerequisites

| Tool | Why | Check |
|------|-----|-------|
| **macOS** (Apple Silicon or Intel) | Build host | `sw_vers` |
| **Xcode** (full app, not just Command Line Tools) | Compiles/runs iOS apps, provides the iOS SDK & Simulator | `xcodebuild -version` |
| **XcodeGen** | Generates `OpenFrameHost.xcodeproj` from `project.yml` | `xcodegen --version` |

Install XcodeGen with Homebrew if needed:

```bash
brew install xcodegen
```

Install **Xcode** from the Mac App Store or [developer.apple.com/xcode](https://developer.apple.com/download/).
The iOS Simulator and iOS SDK ship inside Xcode; the standalone Command Line
Tools are **not** enough to build or run an iOS app.

The project targets **iOS 16.0**, a broadly available deployment target
supported by current Xcode and Simulators.

---

## 2. Select the full Xcode toolchain

If you have both the Command Line Tools and Xcode installed, point the
toolchain at Xcode (this is required for `xcodebuild` and Simulator builds):

```bash
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
xcode-select -p            # should print the path above, NOT /Library/Developer/CommandLineTools
sudo xcodebuild -license accept   # first run only
```

> If `xcode-select -p` prints `/Library/Developer/CommandLineTools`, only the
> CLT are active and `xcodebuild` cannot build the iOS app until Xcode is
> installed and selected.

---

## 3. Generate the Xcode project

`OpenFrameHost.xcodeproj` is generated from `project.yml` so it is fully
reproducible. Regenerate any time `project.yml` or the file layout changes:

```bash
cd ios-host
xcodegen generate
```

Expected output ends with `Created project at .../ios-host/OpenFrameHost.xcodeproj`.

> **Keep the JSON sample in sync.** The bundled sample
> `ios-host/Sources/Resources/notifications.sample.json` must stay a byte-for-byte
> copy of the canonical `protocol/examples/notifications.sample.json`. After
> editing the canonical file, re-copy and confirm they match:
>
> From the repository root:
>
> ```bash
> cp protocol/examples/notifications.sample.json \
>    ios-host/Sources/Resources/notifications.sample.json
> cmp protocol/examples/notifications.sample.json \
>     ios-host/Sources/Resources/notifications.sample.json && echo "in sync"
> ```

---

## 4. Open, build, and run

Open the generated project:

```bash
open ios-host/OpenFrameHost.xcodeproj
```

Then in Xcode:

1. Select the **OpenFrameHost** scheme (top toolbar).
2. Choose an **iOS Simulator** destination (e.g. *iPhone 15*).
3. Press **⌘R** to build & run, or **⌘U** to run the unit tests.

### Command line (equivalent)

Build for the Simulator:

```bash
cd ios-host
xcodebuild \
  -project OpenFrameHost.xcodeproj \
  -scheme OpenFrameHost \
  -destination 'platform=iOS Simulator,name=iPhone 15' \
  build
```

Run the unit tests (local JSON decoding):

```bash
cd ios-host
xcodebuild \
  -project OpenFrameHost.xcodeproj \
  -scheme OpenFrameHost \
  -destination 'platform=iOS Simulator,name=iPhone 15' \
  test
```

List available Simulator destinations with:

```bash
xcrun simctl list devices available
```

---

## 5. Optional: run on a connected iPhone (requires signing)

Running on real hardware requires a signing team; the Simulator does not.

1. Connect the iPhone and trust the Mac.
2. In Xcode, open **Signing & Capabilities** for the `OpenFrameHost` target.
3. Set your **Team** (a free Apple ID works for development) and let Xcode
   manage signing automatically. `PRODUCT_BUNDLE_IDENTIFIER` is
   `com.openframe.host` — change it if it collides with an existing profile.
4. Select the device as the run destination and press **⌘R**.
5. On the device, trust the developer profile under
   **Settings → General → VPN & Device Management** if prompted.

`project.yml` intentionally leaves the development team **unset** so the repo
builds for the Simulator with no credentials. Do not commit a personal team id.

---

## 6. Out of scope (explicitly not implemented)

This shell is deliberately minimal. The following are **not** part of it:

- **Live iOS notifications** — no `UserNotifications` access, no notification
  service/content extensions, no reading of real device notifications. All
  notifications shown are mock data from bundled local JSON.
- **Bluetooth** — no `CoreBluetooth`, no pairing, no glasses transport. The
  "Glasses" and "Simulator" rows are visual placeholders only.
- **Networking** — no HTTP/WebSocket/sync. Everything is loaded locally.
- **Unity** — this task does not add to or modify any Unity project.
- **Signing/distribution config** — no team, provisioning profiles, or CI
  signing are committed.

These are left as follow-up work once the shell is reviewed.

---

## 7. Validation without full Xcode

If only the Command Line Tools are installed (no Xcode), you cannot build or run
the iOS app, but you can still validate the pieces that don't need the iOS SDK:

```bash
# Run from the repository root.

# 1. Project regenerates cleanly
xcodegen generate --spec ios-host/project.yml

# 2. Canonical JSON is valid and matches the bundled copy
python3 -m json.tool protocol/examples/notifications.sample.json > /dev/null
cmp protocol/examples/notifications.sample.json \
    ios-host/Sources/Resources/notifications.sample.json

# 3. Swift sources parse
find ios-host -name '*.swift' -exec swiftc -parse {} \;
```

The `swift` compiler bundled with the Command Line Tools can also type-check and
run the Foundation-only model/loader against the sample JSON, which is how the
decoding path was verified on a CLT-only machine.
