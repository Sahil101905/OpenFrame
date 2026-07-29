# OpenFrame iOS Host — Complete Beginner Guide

A start-to-finish walkthrough of the **OpenFrame iPhone host shell** in `ios-host/`,
written for someone who has **never opened Xcode or written a line of Swift**.

By the end you will understand:

- what this app is, what it does today, and why it was built this way;
- what an Xcode project, target, scheme, simulator, and signing are;
- every Swift keyword and pattern that appears in this codebase, line by line;
- how data flows from a JSON file to pixels on screen;
- how to open, build, test, and run the app — with clicks **and** terminal equivalents;
- how to safely change the text, statuses, and sample data;
- how to read the error when something breaks.

> **Reading order.** If you just want to run it, skip to
> [§12 First-time Xcode walkthrough](#12-first-time-xcode-walkthrough-clicks--terminal).
> If you want to understand it, read top to bottom.

---

## Table of contents

1. [What this is, in one screen](#1-what-this-is-in-one-screen)
2. [What was done and why](#2-what-was-done-and-why)
3. [This machine's toolchain — what was actually checked](#3-this-machines-toolchain--what-was-actually-checked)
4. [Verified vs. not verified — read this before trusting anything](#4-verified-vs-not-verified--read-this-before-trusting-anything)
5. [Xcode concepts for absolute beginners](#5-xcode-concepts-for-absolute-beginners)
6. [Swift fundamentals, using this code](#6-swift-fundamentals-using-this-code)
7. [Architecture and full data/control flow](#7-architecture-and-full-datacontrol-flow)
8. [File-by-file tour](#8-file-by-file-tour)
9. [HomeView, section by section](#9-homeview-section-by-section)
10. [The exact notification decode path](#10-the-exact-notification-decode-path)
11. [The Start Session mock flow](#11-the-start-session-mock-flow)
12. [First-time Xcode walkthrough (clicks + terminal)](#12-first-time-xcode-walkthrough-clicks--terminal)
13. [The tests and what each one proves](#13-the-tests-and-what-each-one-proves)
14. [XcodeGen and the generated project](#14-xcodegen-and-the-generated-project)
15. [How to safely change text, status, and sample data](#15-how-to-safely-change-text-status-and-sample-data)
16. [Debugging common errors](#16-debugging-common-errors)
17. [Glossary](#17-glossary)
18. [Boundaries and sensible next steps](#18-boundaries-and-sensible-next-steps)

---

## 1. What this is, in one screen

**OpenFrame** is an idea for a "glasses OS": lightweight smart-glasses whose
heavy computing is done by a paired iPhone. The phone side of that system is
called the **host**. This repository currently contains only the very first
piece of the host: a **UI shell**.

A "shell" means the screens and layout exist, but the real machinery behind them
does not yet. The app you can build here:

- shows an OpenFrame logo placeholder and title,
- shows two **connection status** rows (Simulator, Glasses) that are **fake/mock**,
- has a **Start / Stop Session** button that flips a fake status so you can see
  it react,
- shows a list of **mock notifications** that are read from a small JSON file
  bundled inside the app.

It deliberately does **not** read your real iPhone notifications, does not use
Bluetooth, does not talk to any server, and does not touch Unity. Those are
future work. See [§18](#18-boundaries-and-sensible-next-steps).

The app is written in **Swift** using **SwiftUI** (Apple's modern UI framework).
It targets **iOS 16.0** and higher.

---

## 2. What was done and why

Here is the reasoning behind each decision, so the code reads as a set of
choices rather than magic.

**A UI shell first, transports later.** The riskiest parts of a glasses host —
Bluetooth pairing, live notification capture, networking — are also the ones
that need the visible screen to already exist so they have somewhere to send
data. Building the screen first, with mock data, lets the layout be reviewed
and the data *shape* be locked down before any hardware or OS permissions are
involved.

**Mock data from local JSON, not live notifications.** Standard iOS apps cannot
generally read the notifications posted by other apps. Notification extensions
only participate in notifications belonging to your own app; they do not grant a
system-wide notification feed. A future supported design therefore needs separate
feasibility work and may require app-specific integrations rather than a general
"read every notification" API. For this shell, the app instead ships a tiny JSON
file of realistic-looking notifications and decodes it.
This exercises the whole "receive → decode → display" path with zero moving
parts.

**One canonical JSON schema, mirrored into the app.** The *shape* of a
notification (its fields and types) is defined once in
`protocol/examples/notifications.sample.json` and documented in
`protocol/examples/README.md`. The app carries a **byte-for-byte identical
copy** at `ios-host/Sources/Resources/notifications.sample.json`. Keeping one
canonical definition means the phone and the (future) glasses can't drift apart
on what a notification looks like.

**Lenient decoding.** If a future notification arrives with a `priority` value
the app has never seen, the app treats it as `normal` rather than crashing. New
data should never break an old host.

**The Xcode project is generated, not hand-edited.** Xcode projects are stored
in a large, messy file (`project.pbxproj`) that is painful to review and easy to
corrupt in a merge. Instead this repo describes the project in a short,
human-readable file, `ios-host/project.yml`, and a tool called **XcodeGen**
produces the `.xcodeproj` from it. The generated project is committed so it can
be opened directly, but `project.yml` is the source of truth. See
[§14](#14-xcodegen-and-the-generated-project).

**No signing identity committed.** "Signing" is how Apple ties an app to a
developer account so it can run on a real iPhone. The project intentionally
leaves the developer team blank so it builds for the **Simulator** with no
account, and no personal credentials end up in the repo.

---

## 3. This machine's toolchain — what was actually checked

These are the **actual results** of inspecting the machine this guide was
written on (macOS `26.5.1`, build `25F80`). Your machine may differ — run the
same commands to see your own state.

| Command | Result on this machine | Meaning |
|---|---|---|
| `sw_vers` | macOS `26.5.1` (`25F80`) | The OS is recent enough for current tooling. |
| `xcode-select -p` | `/Library/Developer/CommandLineTools` | **Only the Command Line Tools are active — not full Xcode.** |
| `xcodebuild -version` | error: *"tool 'xcodebuild' requires Xcode"* | `xcodebuild` cannot run; there is no full Xcode to point it at. |
| `ls /Applications/Xcode*.app` | no matches | **Xcode.app is not installed.** |
| `swift --version` | Apple Swift `6.3.3` | A Swift compiler **is** present (it ships with the CLT). |
| `xcodegen --version` | `2.46.0` (`/opt/homebrew/bin/xcodegen`) | XcodeGen is installed via Homebrew. |
| `xcrun simctl list devices` | error: *"unable to find utility simctl"* | No iOS Simulator — it lives inside Xcode.app. |
| `python3 --version` | `3.9.6` | Handy for validating JSON. |

### Why full Xcode differs from the Command Line Tools (CLT)

Apple ships two different things:

- **Command Line Tools (CLT)** — a small package (`/Library/Developer/CommandLineTools`)
  with the Swift/Clang compilers, `git`, `make`, and a **macOS** SDK. Enough to
  compile plain command-line Swift that only uses **Foundation** (Apple's core,
  non-UI library). This is what's active here.
- **Xcode.app** — the multi-gigabyte IDE from the Mac App Store. **Only Xcode
  contains the iOS SDK, the iOS Simulator, the SwiftUI/UIKit frameworks for
  iOS, the `xcodebuild` build system, and code-signing machinery.**

So on a CLT-only machine you can compile and run Swift that imports only
`Foundation`, but you **cannot** build an iOS app — anything that
`import SwiftUI` or `import XCTest` needs the iOS SDK, which only Xcode provides.
That is exactly the situation on this machine, and it's why parts of this repo
could be verified here and parts could not.

`xcode-select` is the switch that chooses which developer directory is "active".
The setup guide's step 2 shows how to point it at Xcode once Xcode is installed:

```bash
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
xcode-select -p   # should then print the Xcode path, not the CommandLineTools path
```

---

## 4. Verified vs. not verified — read this before trusting anything

Honesty matters more than optimism. Here is precisely what was and was not
confirmed on this CLT-only machine. **No iOS build was performed, and none is
claimed.**

### ✅ Genuinely verified here (commands were run and passed)

1. **XcodeGen regenerates the project cleanly.**
   `xcodegen generate --spec ios-host/project.yml` prints
   *"Created project at .../OpenFrameHost.xcodeproj"*, and re-running it leaves
   `git status` clean — i.e. generation is deterministic and the committed
   `.xcodeproj` matches `project.yml`.
2. **Both JSON files are valid.** `python3 -m json.tool` parses the canonical
   `protocol/examples/notifications.sample.json` and the bundled
   `ios-host/Sources/Resources/notifications.sample.json` without error.
3. **The two JSON files are byte-for-byte identical.** `cmp` of the two files
   reports no differences.
4. **All six Swift files parse.** `swiftc -parse` succeeds on every `.swift`
   file (this checks syntax; it does not link against the iOS SDK).
5. **The model + loader decode logic actually works.** The Foundation-only files
   `OpenFrameNotification.swift` and `NotificationLoader.swift` were compiled with
   the CLT `swiftc` and run against the real sample. Confirmed: all 3
   notifications decode; each `timestamp` parses to a `Date`; an unknown
   `priority` ("urgent") falls back to `normal`; and a JSON object missing
   required fields throws. This mirrors what the unit tests assert.

### ❌ Could **not** be verified here (Xcode.app / iOS SDK / Simulator / signing / device absent)

- **The full iOS app build** (`xcodebuild ... build`) — needs Xcode + the iOS SDK.
- **Running the `XCTest` unit-test target** via `xcodebuild ... test` — the tests
  `import XCTest` and run in a simulator, both of which require Xcode. (The
  *decoding logic they cover* was verified independently, as noted above, but the
  XCTest harness itself was not executed.)
- **`NotificationLoader.loadSample()` reading from the app bundle** — this needs a
  built `.app`/`.xctest` bundle to read the resource from; only the lower-level
  `decode(_:)` was exercised directly.
- **SwiftUI rendering** — nothing in `HomeView.swift` was displayed; it can only
  run in the Simulator or on a device.
- **Signing and installing on a physical iPhone** — no connected device and no
  valid Apple development signing identity were detected on this Mac.

When this guide says "the app shows…", it is describing what the code *will*
produce once built in Xcode, based on reading the code — not something that was
seen running on this machine.

---

## 5. Xcode concepts for absolute beginners

Before the code, the vocabulary. These are the words Xcode's UI and the
terminal commands throw at you.

- **Xcode** — Apple's IDE (code editor + build system + debugger + Simulator
  launcher), installed as one big app from the Mac App Store.

- **Project** (`.xcodeproj`) — a folder/bundle that describes what to build:
  which source files, which settings, which targets. Here it's
  `ios-host/OpenFrameHost.xcodeproj`, and it is **generated** by XcodeGen from
  `project.yml` (you don't edit it by hand).

- **Workspace** (`.xcworkspace`) — an optional container that can group several
  projects. This repo has a trivial one inside the project that just points at
  itself (`contents.xcworkspacedata` → `location = "self:"`); nothing to manage.

- **Target** — one buildable output inside a project. This project has **two**:
  - `OpenFrameHost` — the iOS **application** (the thing that runs on the phone).
  - `OpenFrameHostTests` — a **unit-test bundle** that tests the app's code.

- **Scheme** — a saved recipe answering "when I press Run/Test, which targets
  build, in which configuration (Debug/Release), and what runs?" This repo ships
  one scheme, `OpenFrameHost`: **Run** launches the app in Debug; **Test** runs
  the `OpenFrameHostTests` target.

- **Simulator** — a Mac app (part of Xcode) that pretends to be an iPhone/iPad so
  you can run iOS apps without hardware. You pick a **destination** like
  "iPhone 15" when building.

- **Signing** — cryptographically stamping the app with a developer identity so
  Apple/iOS trusts it. The **Simulator needs none**. A **real device requires a
  Team** (even a free Apple ID works for development). This repo leaves the team
  unset on purpose.

- **Bundle** — a folder that macOS/iOS treats as a single item. A built app is a
  bundle (`OpenFrameHost.app`) containing the compiled code **and** its
  resources.

- **Resource** — a non-code file packaged inside the bundle, e.g.
  `notifications.sample.json`. At runtime the app finds it via `Bundle.main`.

- **Deployment target** — the *oldest* iOS version the app supports. Here:
  **iOS 16.0**.

- **Build / Test / Run**:
  - **Build** — compile the code into an app.
  - **Run** (⌘R) — build, then launch in the Simulator (or on a device).
  - **Test** (⌘U) — build, then execute the test target and report pass/fail.

- **Deriveddata** — Xcode's throwaway build-output cache. It's git-ignored;
  deleting it forces a clean rebuild and is a common fix for weird build states.

---

## 6. Swift fundamentals, using this code

Every concept below is illustrated with a snippet **quoted from this repo**, so
you learn Swift and the codebase at the same time. File paths point at where each
snippet lives.

### 6.1 Importing frameworks

```swift
import Foundation   // core types: String, Data, Date, JSONDecoder…
import SwiftUI       // UI types: View, Text, Color, VStack…
```

`import` makes another library's types available. **Foundation** is non-visual
(strings, dates, JSON). **SwiftUI** is the UI toolkit. The model/loader files
import only `Foundation` (that's why they compile without Xcode); the view files
import `SwiftUI` (that's why they need the iOS SDK).

### 6.2 `let` vs `var` — constants and variables

```swift
let id: String        // OpenFrameNotification.swift — never changes after set
@Published var isSessionActive: Bool = false   // HomeViewModel.swift — can change
```

`let` declares a **constant** (can't be reassigned). `var` declares a
**variable** (can). Prefer `let` unless you truly need to mutate. Notice each
notification field is a `let`: a decoded notification is immutable.

### 6.3 Types, and type inference

```swift
static let sampleResourceName = "notifications.sample"   // inferred as String
let title: String                                        // explicit type
```

Swift is **statically typed** — every value has a type. You can write the type
explicitly (`: String`) or let Swift **infer** it from the value on the right.
The literal `"notifications.sample"` is obviously a `String`, so no annotation is
needed.

### 6.4 `struct`, `class`, `enum`

Swift has three main ways to define your own types; all three appear here.

**`struct` — a value type** (copied when assigned/passed). Used for data and for
SwiftUI views:

```swift
// OpenFrameNotification.swift
struct OpenFrameNotification: Codable, Identifiable, Equatable {
    let id: String
    // …
}
```

**`class` — a reference type** (shared, not copied). Used for the view model,
which must be a single shared object the UI observes:

```swift
// HomeViewModel.swift
@MainActor
final class HomeViewModel: ObservableObject { … }
```

**`enum` — a fixed set of named cases.** Used for connection state, priority, and
errors:

```swift
// HomeViewModel.swift
enum ConnectionState: Equatable {
    case disconnected
    case connecting
    case connected
}
```

The words after the colon (`Codable`, `Identifiable`, `Equatable`,
`ObservableObject`) are **protocols** the type conforms to — see [§6.12](#612-protocols).

### 6.5 Properties

Values that belong to a type are its **properties**:

```swift
let title: String                          // stored property (a stored value)
@Published var glassesState: ConnectionState = .disconnected   // stored + observable
```

A property can have a default value (`= .disconnected`). `@Published` is a
property wrapper — see [§6.15](#615-property-wrappers-and-other--attributes).

### 6.6 Functions, parameters, and default values

```swift
// NotificationLoader.swift
static func loadSample(
    named name: String = sampleResourceName,
    from bundle: Bundle = .main
) throws -> [OpenFrameNotification] { … }
```

Read this as: a function `loadSample` that takes two parameters and **returns**
(`->`) an array of notifications, and may `throws` an error.

- `named name: String` — `named` is the **argument label** callers write;
  `name` is the name used inside the function.
- `= sampleResourceName` and `= .main` — **default values**, so a caller can
  write `loadSample()` with no arguments and get the bundled sample from the
  main app bundle. The hosted bundle test uses these same defaults.

Calling with defaults vs. overriding:

```swift
try NotificationLoader.loadSample()                     // uses .main
try NotificationLoader.loadSample(from: anotherBundle)  // optional override
```

### 6.7 Optionals — "maybe there's a value, maybe not"

A type with `?` may hold a value **or** `nil` (nothing):

```swift
// OpenFrameNotification.swift
var date: Date? {                                   // Date? = Date or nil
    OpenFrameNotification.isoFormatter.date(from: timestamp)
}
```

`ISO8601DateFormatter.date(from:)` returns `nil` if the string isn't a valid
date, so `date` is a `Date?`. Optionals force you to handle the "nothing" case
instead of crashing on it.

**Unwrapping with `guard`:**

```swift
// HomeView.swift — NotificationRow
private var relativeTimestamp: String {
    guard let date = notification.date else { return "" }
    // …use `date` here, guaranteed non-nil…
}
```

`guard let date = …` says: if `notification.date` is `nil`, run the `else` block
(here, return an empty string and stop); otherwise bind the unwrapped value to
`date` and continue. `guard` is the "bail out early" tool.

**Nil-coalescing `??`:**

```swift
// OpenFrameNotification.swift — Priority
self = Priority(rawValue: raw) ?? .normal
```

`a ?? b` means "use `a` if it's non-nil, otherwise `b`." If `raw` isn't a known
priority, `Priority(rawValue:)` returns `nil`, so `self` becomes `.normal`.

**Optional binding with `if let`:**

```swift
// HomeView.swift — notificationsSection
if let message = viewModel.loadErrorMessage {
    Label(message, systemImage: "exclamationmark.triangle.fill") …
}
```

Run the block only if `loadErrorMessage` is non-nil, with the unwrapped value
named `message`.

### 6.8 `switch`

Match a value against each case; for enums, Swift checks you covered them all:

```swift
// HomeViewModel.swift — ConnectionState
var label: String {
    switch self {
    case .disconnected: return "Disconnected"
    case .connecting:   return "Connecting…"
    case .connected:    return "Connected"
    }
}
```

Because `ConnectionState` has exactly three cases and all three are listed, no
`default:` is needed.

### 6.9 Arrays

`[Element]` is an ordered list:

```swift
@Published private(set) var notifications: [OpenFrameNotification] = []   // starts empty
```

`[OpenFrameNotification]` = "array of notifications". Useful members seen in this
code: `.count`, `.first`, `.isEmpty`, `.enumerated()`.

### 6.10 Closures

A **closure** is an inline block of code you can pass around. The button's action
is a closure (here, a reference to a method):

```swift
// HomeView.swift
Button(action: viewModel.toggleSession) { … }
```

A closure that builds a value on first use appears in the model:

```swift
// OpenFrameNotification.swift
private static let isoFormatter: ISO8601DateFormatter = {
    let formatter = ISO8601DateFormatter()
    formatter.formatOptions = [.withInternetDateTime]
    return formatter
}()          // ← the trailing () calls the closure immediately
```

The `{ … }()` pattern runs the closure once to produce the configured formatter
and stores the result. Because it's `static let`, it's created a single time and
reused.

### 6.11 Computed vs. stored vs. static properties

- **Stored** — holds a value: `let title: String`.
- **Computed** — runs code each time it's read (note: no `=`, just `{ … }`):

  ```swift
  var date: Date? { OpenFrameNotification.isoFormatter.date(from: timestamp) }
  ```

- **Static** — belongs to the type itself, not an instance:

  ```swift
  static let sampleResourceName = "notifications.sample"
  ```

  You call it as `NotificationLoader.sampleResourceName`, without making an
  instance.

### 6.12 Protocols

A **protocol** is a contract: "a type that conforms to me provides these
capabilities." This code uses several from the standard library:

- **`Codable`** — can be encoded to / decoded from formats like JSON. This is
  what lets `OpenFrameNotification` be read straight out of the JSON file.
- **`Identifiable`** — has an `id` so SwiftUI lists can track items. Satisfied by
  the existing `let id: String`.
- **`Equatable`** — two values can be compared with `==`.
- **`ObservableObject`** — a class whose changes SwiftUI watches (see the view
  model).
- **`Error` / `LocalizedError`** — a type usable in `throw`, with a
  human-readable message.
- **`View`** — the SwiftUI protocol every screen/component conforms to.

You declare conformance in the type header:

```swift
struct OpenFrameNotification: Codable, Identifiable, Equatable { … }
```

### 6.13 `Codable` and `CodingKeys`

`Codable` conformance is usually automatic — Swift matches JSON keys to property
names. This model spells the mapping out explicitly for stability:

```swift
// OpenFrameNotification.swift
enum CodingKeys: String, CodingKey {
    case id
    case appIdentifier
    case appName
    case title
    case body
    case timestamp
    case priority
    case category
}
```

A `CodingKeys` enum lists which JSON keys map to which properties. Here each case
name equals its property name and its raw JSON key, so the mapping is 1:1 and
obvious. Writing it out means a future rename can't silently change the wire
format.

The `Priority` enum customizes its own decoding to be lenient:

```swift
enum Priority: String, Codable, CaseIterable, Equatable {
    case low
    case normal
    case high

    init(from decoder: Decoder) throws {
        let raw = try decoder.singleValueContainer().decode(String.self)
        self = Priority(rawValue: raw) ?? .normal
    }
}
```

- `String`-backed enum: each case has a raw string (`"low"`, `"normal"`, `"high"`).
- `init(from decoder:)` is the custom decoder. It reads the raw string, then
  tries to build a `Priority` from it; **anything unrecognized becomes `.normal`**
  thanks to `?? .normal`. That's why `"urgent"` in the JSON decodes as `normal`.
- `CaseIterable` gives an automatic `.allCases` list (not heavily used here, but
  handy).

### 6.14 Errors, `throws`, `do`/`catch`

Swift models failure with typed errors you `throw` and `catch`.

**Defining errors** (an enum conforming to `Error`):

```swift
// NotificationLoader.swift
enum LoadError: Error, LocalizedError, Equatable {
    case resourceNotFound(name: String)
    case decodingFailed(String)

    var errorDescription: String? {
        switch self {
        case .resourceNotFound(let name):
            return "Bundled resource \"\(name).json\" was not found."
        case .decodingFailed(let detail):
            return "Failed to decode notifications: \(detail)"
        }
    }
}
```

Each case can carry data (`name`, `detail`). `LocalizedError.errorDescription`
provides the message shown to the user. `"\(name)"` is **string
interpolation** — it splices a value into a string.

**Throwing** and converting a low-level error into a friendly one:

```swift
static func decode(_ data: Data) throws -> [OpenFrameNotification] {
    do {
        return try JSONDecoder().decode([OpenFrameNotification].self, from: data)
    } catch {
        throw LoadError.decodingFailed(String(describing: error))
    }
}
```

- `try` marks a call that can fail.
- `do { … } catch { … }` runs the risky code and handles any thrown error; here
  it wraps the raw decoding error in a `LoadError.decodingFailed`.

**Catching at the call site** to update the UI instead of crashing:

```swift
// HomeViewModel.swift
func loadNotifications() {
    do {
        notifications = try NotificationLoader.loadSample()
        loadErrorMessage = nil
    } catch {
        notifications = []
        loadErrorMessage = error.localizedDescription
    }
}
```

If loading fails, the list is emptied and `loadErrorMessage` is set so the view
can show a warning row — the app stays alive.

### 6.15 Property wrappers and other `@` attributes

Swift uses `@` syntax for several related language features. In this project,
`@StateObject` and `@Published` are **property wrappers**; `@main` and
`@MainActor` are declaration attributes; and `@ViewBuilder` applies SwiftUI's
result-builder transformation. They all add compiler-defined behavior, but they
are not all technically property wrappers.

- **`@main` attribute** — marks the app's entry point (where the program starts):

  ```swift
  // OpenFrameHostApp.swift
  @main
  struct OpenFrameHostApp: App { … }
  ```

- **`@Published` property wrapper** — inside an `ObservableObject`, announces "when I change,
  refresh any view watching me":

  ```swift
  @Published var isSessionActive: Bool = false
  ```

- **`@StateObject` property wrapper** — in a view, creates and **owns** an observable object for
  the view's lifetime:

  ```swift
  // HomeView.swift
  @StateObject private var viewModel: HomeViewModel
  ```

  When any `@Published` property on `viewModel` changes, SwiftUI re-renders
  `HomeView`.

- **`@MainActor` attribute** — isolates the code to the main actor, which
  serializes UI-facing state on the main execution context:

  ```swift
  @MainActor
  final class HomeViewModel: ObservableObject { … }
  ```

- **`@ViewBuilder` result-builder attribute** — lets a property/parameter be written as a list of views
  (see [§6.18](#618-viewbuilder)).

### 6.16 Access control

Keywords that limit who can see a declaration:

```swift
@Published private(set) var notifications: [OpenFrameNotification] = []
private var header: some View { … }
private struct StatusRow: View { … }
final class HomeViewModel { … }
```

- **`private`** — visible only within the enclosing type/file. The helper views
  `SectionCard`, `StatusRow`, `NotificationRow` are `private` to `HomeView.swift`.
- **`private(set)`** — others can *read* `notifications`, but only the view model
  can *write* it. This protects the list from being mutated from the UI.
- **`final`** — the class can't be subclassed (a small performance/clarity win).

### 6.17 SwiftUI: `View`, `body`, declarative composition, modifiers

SwiftUI is **declarative**: you describe *what* the UI should look like for the
current data, and SwiftUI figures out the drawing and updates.

Every UI piece is a `struct` conforming to `View`, and must provide a computed
`body`:

```swift
// HomeView.swift
struct HomeView: View {
    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(alignment: .leading, spacing: 24) {
                    header
                    statusSection
                    sessionSection
                    notificationsSection
                }
                .padding(20)
            }
        }
    }
}
```

- `some View` means "some concrete `View` type — you don't need to know which."
- Views **nest** to compose layout: a `NavigationStack` contains a `ScrollView`
  containing a **`VStack`** (vertical stack) of four sub-sections.
- `VStack(alignment: .leading, spacing: 24)` stacks children left-aligned with 24
  points of gap.

**Modifiers** are methods that return a tweaked view. They chain top-to-bottom:

```swift
Text("OpenFrame")
    .font(.largeTitle.bold())          // set font
Text("Host")
    .font(.headline)
    .foregroundStyle(.secondary)       // dim color
```

`.padding(20)`, `.background(…)`, `.frame(…)`, `.tint(…)` are all modifiers.
Order matters — each wraps the result of the previous.

### 6.18 `@ViewBuilder`

`@ViewBuilder` lets you pass **several views in a row** as one argument. The
reusable `SectionCard` uses it so callers can drop multiple rows inside:

```swift
// HomeView.swift
private struct SectionCard<Content: View>: View {
    let title: String
    @ViewBuilder let content: Content

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            Text(title.uppercased()) …
            VStack(alignment: .leading, spacing: 12) { content } …
        }
    }
}
```

`<Content: View>` is a **generic**: `SectionCard` works with any view type you
give it. Usage — the two `StatusRow`s plus the `Divider` are the `content`:

```swift
SectionCard(title: "Connection") {
    StatusRow(title: "Simulator", systemImage: "display", state: viewModel.simulatorState)
    Divider()
    StatusRow(title: "Glasses", systemImage: "eyeglasses", state: viewModel.glassesState)
}
```

### 6.19 `ForEach` and `Identifiable`

`ForEach` builds one view per element of a collection. SwiftUI needs a stable
**identity** per row to animate/update correctly:

```swift
// HomeView.swift — notificationsSection
ForEach(Array(viewModel.notifications.enumerated()), id: \.element.id) { index, note in
    if index > 0 { Divider() }
    NotificationRow(notification: note)
}
```

- `.enumerated()` pairs each notification with its position, giving
  `(index, note)`. `Array(…)` makes that pairing usable by `ForEach`.
- `id: \.element.id` tells SwiftUI to identify each row by the notification's
  `id` (a **key path**, written `\.` ). Because `OpenFrameNotification` is
  `Identifiable`, its `id` is a natural, stable identity.
- Inside the closure, `if index > 0 { Divider() }` draws a separator line
  between rows but not above the first one.

That's the whole language surface this codebase uses. Everything below is these
pieces assembled.

---

## 7. Architecture and full data/control flow

The app follows a simple **Model → ViewModel → View** (MVVM) shape.

```
┌─────────────────────────── App bundle ───────────────────────────┐
│                                                                   │
│  notifications.sample.json  (Resource; copy of the canonical file)│
│              │                                                     │
│              ▼  Bundle.main.url(forResource:…)                     │
│  NotificationLoader.loadSample()  ── decode ──►  JSONDecoder       │
│              │                                                     │
│              ▼  [OpenFrameNotification]  (Model)                   │
│  HomeViewModel  (@MainActor ObservableObject)                     │
│      @Published notifications / states / error / isSessionActive  │
│              │  @Published change                                 │
│              ▼  SwiftUI observes                                  │
│  HomeView  ──►  header · statusSection · sessionSection ·         │
│                 notificationsSection  (View)                      │
│                                                                   │
│  OpenFrameHostApp (@main)  creates HomeView(viewModel:)           │
└───────────────────────────────────────────────────────────────────┘
```

**Layers:**

- **Model** (`Sources/Models/`) — plain data + logic, Foundation-only.
  `OpenFrameNotification` is the data; `NotificationLoader` reads and decodes it.
- **ViewModel** (`Sources/App/HomeViewModel.swift`) — holds the screen's state as
  `@Published` properties and the actions that change it (`loadNotifications`,
  `toggleSession`). Knows nothing about layout.
- **View** (`Sources/App/HomeView.swift`) — reads the view model's state and
  renders it; forwards button taps back to the view model.
- **Entry point** (`Sources/App/OpenFrameHostApp.swift`) — wires it all together
  at launch.

**Control flow, start to finish:**

1. iOS launches the app → runs `@main OpenFrameHostApp`.
2. `OpenFrameHostApp.body` creates a `WindowGroup` containing
   `HomeView(viewModel: HomeViewModel())` — a fresh view model is made and handed
   to the view.
3. `HomeView` appears. Its `.onAppear { viewModel.loadNotifications() }` fires.
4. `loadNotifications()` calls `NotificationLoader.loadSample()`, which finds the
   bundled JSON and decodes it into `[OpenFrameNotification]`, assigning it to the
   `@Published notifications` property (or setting `loadErrorMessage` on failure).
5. The `@Published` change triggers SwiftUI to re-render `HomeView`.
6. `notificationsSection` sees a non-empty list and renders a `NotificationRow`
   for each item.
7. Tapping **Start Session** calls `viewModel.toggleSession()`, which flips
   `isSessionActive` and `simulatorState`; those `@Published` changes re-render
   the button label/color and the Simulator status row.

---

## 8. File-by-file tour

Every file in the repo, what it is, and why it exists.

### Swift source

| File | Role |
|---|---|
| `ios-host/Sources/App/OpenFrameHostApp.swift` | `@main` entry point; creates the window and the first view. |
| `ios-host/Sources/App/HomeView.swift` | The whole home screen + its private sub-views. |
| `ios-host/Sources/App/HomeViewModel.swift` | Screen state (`@Published`) + the `ConnectionState` enum + actions. |
| `ios-host/Sources/Models/OpenFrameNotification.swift` | The `Codable` notification model + `Priority` enum + `date` parsing. |
| `ios-host/Sources/Models/NotificationLoader.swift` | Loads/decodes the bundled JSON; defines `LoadError`. |
| `ios-host/Tests/OpenFrameNotificationDecodingTests.swift` | Unit tests for decoding. |

**`OpenFrameHostApp.swift`** — 15 lines. Declares the app and its scene:

```swift
@main
struct OpenFrameHostApp: App {
    var body: some Scene {
        WindowGroup {
            HomeView(viewModel: HomeViewModel())
        }
    }
}
```

`App` is SwiftUI's program type; `Scene`/`WindowGroup` is the window that holds
your UI. This file is the only place the view model is created.

**`HomeViewModel.swift`** — the state and logic behind the screen. Contains the
`ConnectionState` enum (with `label`, `systemImage`, `tint` computed from each
case) and the `HomeViewModel` class with five `@Published` properties and two
methods. Fully covered in [§9](#9-homeview-section-by-section),
[§10](#10-the-exact-notification-decode-path), and
[§11](#11-the-start-session-mock-flow).

**`HomeView.swift`** — the layout. `HomeView` plus three private helper views
(`SectionCard`, `StatusRow`, `NotificationRow`). Covered in
[§9](#9-homeview-section-by-section).

**`OpenFrameNotification.swift`** — the data model. Covered in
[§6.13](#613-codable-and-codingkeys) and [§10](#10-the-exact-notification-decode-path).

**`NotificationLoader.swift`** — reads the resource and decodes it. Covered in
[§10](#10-the-exact-notification-decode-path).

### JSON

| File | Role |
|---|---|
| `protocol/examples/notifications.sample.json` | **Canonical** schema example — the single source of truth. |
| `ios-host/Sources/Resources/notifications.sample.json` | **Byte-for-byte copy** bundled into the app so it can be loaded at runtime. |

Both hold the same array of three notifications (Messages, Calendar, News). The
copy exists because an app can only load resources that are inside its own
bundle; the canonical file lives with the protocol docs. They **must stay
identical** — verified here with `cmp` (see [§4](#4-verified-vs-not-verified--read-this-before-trusting-anything)).

### Project / config

| File | Role |
|---|---|
| `ios-host/project.yml` | **XcodeGen spec** — the human-readable source of truth for the project. |
| `ios-host/OpenFrameHost.xcodeproj/` | The **generated** Xcode project (committed for convenience). |
| `…/xcshareddata/xcschemes/OpenFrameHost.xcscheme` | The shared **scheme** (build/run/test recipe). |
| `…/project.xcworkspace/contents.xcworkspacedata` | Trivial workspace pointing at the project itself. |
| `.gitignore` | Ignores Unity output and per-user Xcode state (`xcuserdata`, `DerivedData`). Keeps the generated `.xcodeproj` **tracked**. |
| `.gitattributes` | `* text=auto` — normalizes line endings. |

`project.yml` is small enough to read in full; its meaning is unpacked in
[§14](#14-xcodegen-and-the-generated-project).

### Docs

| File | Role |
|---|---|
| `README.md` (root) | Repository landing page; links to this guide and the others. |
| `documentation/setup-macos.md` | Prerequisites + how to build/test/run + out-of-scope list. |
| `documentation/ios-host-beginner-guide.md` | **This document.** |
| `ios-host/README.md` | Short developer entry point for the host module. |
| `protocol/examples/README.md` | Documents the notification schema and the sync rule. |

---

## 9. HomeView, section by section

`HomeView` is one `struct HomeView: View`. Its `body` is a `NavigationStack →
ScrollView → VStack` holding four sections, each a private computed property.
Here is each, top to bottom.

### 9.0 The container

```swift
var body: some View {
    NavigationStack {
        ScrollView {
            VStack(alignment: .leading, spacing: 24) {
                header
                statusSection
                sessionSection
                notificationsSection
            }
            .padding(20)
        }
        .background(Color(.systemGroupedBackground))
        .navigationTitle("")
        .navigationBarTitleDisplayMode(.inline)
    }
    .onAppear { viewModel.loadNotifications() }
}
```

- `NavigationStack` gives a standard iOS navigation bar (kept empty here).
- `ScrollView` makes the content scroll if it's taller than the screen.
- `VStack(… spacing: 24)` stacks the four sections with 24pt gaps, left-aligned.
- `.padding(20)` insets everything from the screen edges.
- `.background(Color(.systemGroupedBackground))` uses the iOS grey grouped
  background that adapts to light/dark mode.
- `.onAppear { viewModel.loadNotifications() }` — **this is the trigger** that
  loads the JSON the first time the screen shows.

### 9.1 Header

```swift
private var header: some View {
    HStack(spacing: 14) {
        RoundedRectangle(cornerRadius: 16, style: .continuous)
            .fill(LinearGradient(colors: [.blue, .purple],
                                 startPoint: .topLeading, endPoint: .bottomTrailing))
            .frame(width: 56, height: 56)
            .overlay(
                Image(systemName: "eyeglasses")
                    .font(.system(size: 26, weight: .semibold))
                    .foregroundStyle(.white)
            )
            .accessibilityHidden(true)

        VStack(alignment: .leading, spacing: 2) {
            Text("OpenFrame").font(.largeTitle.bold())
            Text("Host").font(.headline).foregroundStyle(.secondary)
        }
        Spacer()
    }
}
```

- `HStack` lays out horizontally: logo on the left, title text next to it.
- The logo is a **placeholder** — a blue→purple gradient rounded square with a
  white "eyeglasses" SF Symbol overlaid. Swap it for a real asset later.
- `.accessibilityHidden(true)` hides the decorative logo from VoiceOver.
- The `VStack` shows "OpenFrame" big and "Host" smaller/dimmed.
- `Spacer()` pushes everything to the left by filling the remaining width.

### 9.2 Connection status

```swift
private var statusSection: some View {
    SectionCard(title: "Connection") {
        StatusRow(title: "Simulator", systemImage: "display",
                  state: viewModel.simulatorState)
        Divider()
        StatusRow(title: "Glasses", systemImage: "eyeglasses",
                  state: viewModel.glassesState)
    }
}
```

A `SectionCard` titled "CONNECTION" holds two `StatusRow`s split by a `Divider`.
Each row reads its state from the view model (`simulatorState`, `glassesState`).
Both start `.disconnected` — these are **mock** placeholders.

A single `StatusRow`:

```swift
private struct StatusRow: View {
    let title: String
    let systemImage: String
    let state: ConnectionState

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: systemImage).foregroundStyle(.secondary).frame(width: 24)
            Text(title).font(.body)
            Spacer()
            HStack(spacing: 6) {
                Image(systemName: state.systemImage)
                Text(state.label)
            }
            .font(.subheadline)
            .foregroundStyle(state.tint)
        }
    }
}
```

The left side shows an icon + the row's name; `Spacer()` pushes a status
icon + label to the right. Crucially, the status icon (`state.systemImage`),
text (`state.label`), and color (`state.tint`) **all come from the
`ConnectionState` enum** — so changing the state changes all three at once. Those
are the computed properties defined on the enum:

```swift
// HomeViewModel.swift — ConnectionState
var label: String        { … "Disconnected"/"Connecting…"/"Connected" … }
var systemImage: String  { … "circle"/"circle.dotted"/"checkmark.circle.fill" … }
var tint: Color          { … .secondary/.orange/.green … }
```

### 9.3 Session control

```swift
private var sessionSection: some View {
    Button(action: viewModel.toggleSession) {
        Label(viewModel.isSessionActive ? "Stop Session" : "Start Session",
              systemImage: viewModel.isSessionActive ? "stop.fill" : "play.fill")
            .font(.headline)
            .frame(maxWidth: .infinity)
            .padding(.vertical, 14)
    }
    .buttonStyle(.borderedProminent)
    .tint(viewModel.isSessionActive ? .red : .blue)
}
```

- A full-width prominent `Button`. Its `action` is `viewModel.toggleSession`.
- The `? :` **ternary** operators make the button reflect state: when a session
  is active it reads "Stop Session" with a stop icon and turns **red**; otherwise
  "Start Session" with a play icon, **blue**.
- `.frame(maxWidth: .infinity)` stretches it across the width.

### 9.4 Notifications

```swift
private var notificationsSection: some View {
    SectionCard(title: "Notifications (mock)") {
        if let message = viewModel.loadErrorMessage {
            Label(message, systemImage: "exclamationmark.triangle.fill")
                .font(.subheadline).foregroundStyle(.orange)
        } else if viewModel.notifications.isEmpty {
            Text("No notifications.").font(.subheadline).foregroundStyle(.secondary)
        } else {
            ForEach(Array(viewModel.notifications.enumerated()), id: \.element.id) { index, note in
                if index > 0 { Divider() }
                NotificationRow(notification: note)
            }
        }
    }
}
```

Three-way state, handled in order:

1. **Error** — if `loadErrorMessage` is set, show an orange warning label with
   that message.
2. **Empty** — otherwise, if the list is empty, show "No notifications."
3. **Populated** — otherwise, render one `NotificationRow` per notification,
   separated by dividers.

A single `NotificationRow`:

```swift
private struct NotificationRow: View {
    let notification: OpenFrameNotification

    var body: some View {
        HStack(alignment: .top, spacing: 12) {
            Image(systemName: "app.badge.fill").foregroundStyle(.blue)
                .frame(width: 24).padding(.top, 2)
            VStack(alignment: .leading, spacing: 3) {
                HStack {
                    Text(notification.title).font(.subheadline.bold())
                    Spacer()
                    Text(relativeTimestamp).font(.caption).foregroundStyle(.secondary)
                }
                Text(notification.body).font(.subheadline).foregroundStyle(.primary)
                    .fixedSize(horizontal: false, vertical: true)
                Text(notification.appName).font(.caption).foregroundStyle(.secondary)
            }
        }
    }

    private var relativeTimestamp: String {
        guard let date = notification.date else { return "" }
        let formatter = DateFormatter()
        formatter.dateFormat = "HH:mm"
        return formatter.string(from: date)
    }
}
```

- A blue app icon on the left; the text block on the right.
- Top line: bold `title` on the left, a `HH:mm` time on the right.
- Middle: the `body`. `.fixedSize(horizontal: false, vertical: true)` lets long
  bodies wrap onto multiple lines instead of being truncated.
- Bottom: the dimmed `appName`.
- `relativeTimestamp` turns the parsed `Date` into `"14:32"`. If the timestamp
  didn't parse (`date == nil`), it returns `""` and no time is shown — a safe
  fallback.

### 9.5 `SectionCard` (the reusable wrapper)

Already shown in [§6.18](#618-viewbuilder). It draws the small-caps title, then a
rounded card (`.secondarySystemGroupedBackground`, 16pt corners) containing
whatever views you pass in. Both the Connection and Notifications sections reuse
it — one definition, consistent styling.

---

## 10. The exact notification decode path

This is the precise sequence from "app appears" to "notifications in memory,"
naming every function. **This logic was verified with the CLT compiler**
(see [§4](#4-verified-vs-not-verified--read-this-before-trusting-anything)); the
`Bundle.main` step specifically requires a built app and was not run here.

1. **View appears.** `HomeView.body`'s `.onAppear` calls
   `viewModel.loadNotifications()`.

2. **View model asks the loader** (`HomeViewModel.loadNotifications`):

   ```swift
   do {
       notifications = try NotificationLoader.loadSample()
       loadErrorMessage = nil
   } catch {
       notifications = []
       loadErrorMessage = error.localizedDescription
   }
   ```

3. **Locate the resource** (`NotificationLoader.loadSample`, using defaults
   `name = "notifications.sample"`, `bundle = .main`):

   ```swift
   guard let url = bundle.url(forResource: name, withExtension: "json") else {
       throw LoadError.resourceNotFound(name: name)
   }
   let data = try Data(contentsOf: url)
   return try decode(data)
   ```

   It looks up `notifications.sample.json` inside the app bundle. If it's missing,
   it throws `LoadError.resourceNotFound`; otherwise it reads the bytes.

4. **Decode the bytes** (`NotificationLoader.decode`):

   ```swift
   do {
       return try JSONDecoder().decode([OpenFrameNotification].self, from: data)
   } catch {
       throw LoadError.decodingFailed(String(describing: error))
   }
   ```

   `JSONDecoder` reads the JSON array into `[OpenFrameNotification]`. Any decoding
   problem is wrapped as `LoadError.decodingFailed(...)`.

5. **Per-notification decoding** uses the `CodingKeys` mapping in
   `OpenFrameNotification` and the custom `Priority.init(from:)`, which maps an
   unknown priority to `.normal`. `date` is *not* stored — it's computed on demand
   from `timestamp` via the shared `ISO8601DateFormatter`.

6. **Back in the view model**, on success `notifications` is set (and
   `loadErrorMessage` cleared); on failure `notifications` is emptied and
   `loadErrorMessage` gets the human-readable `error.localizedDescription`.

7. **UI updates.** Because both are `@Published`, SwiftUI re-renders
   `notificationsSection`, which then shows the rows, the empty text, or the
   error label per [§9.4](#94-notifications).

---

## 11. The Start Session mock flow

The button demonstrates state → UI reactivity with no real work behind it.

1. User taps **Start Session** → `sessionSection`'s button runs
   `viewModel.toggleSession`.

2. `toggleSession` (in `HomeViewModel`):

   ```swift
   func toggleSession() {
       isSessionActive.toggle()
       simulatorState = isSessionActive ? .connected : .disconnected
   }
   ```

   - `isSessionActive.toggle()` flips the `Bool` (false→true or true→false).
   - The Simulator status is set to `.connected` when a session starts, back to
     `.disconnected` when it stops.

3. Both properties are `@Published`, so SwiftUI re-renders:
   - the **button** now reads "Stop Session", shows a stop icon, and turns red
     (from the ternaries in `sessionSection`);
   - the **Simulator status row** flips to a green "Connected" with a filled
     checkmark (from `ConnectionState.connected`'s `label`/`systemImage`/`tint`).

4. Tapping again reverses everything.

Note the **Glasses** row never changes — there is deliberately no mock behavior
wired to it; it's a pure placeholder for the (future) real transport.

---

## 12. First-time Xcode walkthrough (clicks + terminal)

> **Reality check for this machine.** As recorded in
> [§3](#3-this-machines-toolchain--what-was-actually-checked), this Mac has
> **only the Command Line Tools — no Xcode.app** — so the steps that need Xcode
> (build/run/test in a Simulator) **cannot be performed here**. They are written
> for a machine that has Xcode installed. What *was* verifiable here is called
> out in [§4](#4-verified-vs-not-verified--read-this-before-trusting-anything) and
> in the "terminal alternative — no Xcode" boxes below.

### Step 0 — Install the tools

- **Xcode** — from the Mac App Store (search "Xcode"), or from
  [developer.apple.com/download](https://developer.apple.com/download/). It's
  several gigabytes; let it finish, then open it once to accept its license.
- **XcodeGen** — via Homebrew:

  ```bash
  brew install xcodegen
  ```

  Verify: `xcodegen --version` (this machine has `2.46.0`).

### Step 1 — Point the toolchain at Xcode

If both CLT and Xcode are installed, make Xcode active:

```bash
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
xcode-select -p                    # expect the Xcode path, NOT /Library/Developer/CommandLineTools
sudo xcodebuild -license accept    # first run only
```

On this machine `xcode-select -p` currently prints
`/Library/Developer/CommandLineTools`, which is exactly the "Xcode not active"
state the setup guide warns about.

### Step 2 — Generate the project

From the **repository root**:

```bash
xcodegen generate --spec ios-host/project.yml
```

Expected last line: `Created project at .../ios-host/OpenFrameHost.xcodeproj`.
*(This step was run successfully on this machine — XcodeGen doesn't need Xcode.)*

### Step 3 — Open it

```bash
open ios-host/OpenFrameHost.xcodeproj
```

Xcode opens. On the left is the **Navigator** (file tree). Along the top is the
**toolbar** with the scheme and destination selectors and the Run ▶ button.

### Step 4 — Pick scheme + destination

1. In the toolbar, make sure the scheme reads **OpenFrameHost** (it's the only
   one).
2. Click the destination dropdown next to it and choose an **iOS Simulator**,
   e.g. *iPhone 15*.

**Terminal alternative — list simulators:**

```bash
xcrun simctl list devices available
```

*(On this machine this errors with "unable to find utility simctl" because there
is no Xcode/Simulator.)*

### Step 5 — Run the app (⌘R)

Press **⌘R** (or the ▶ button). Xcode builds the app, boots the Simulator, and
launches OpenFrame. You should see the header, the two status rows, the Start
Session button, and three mock notifications.

**Terminal alternative — build for the Simulator (from `ios-host/`):**

```bash
xcodebuild \
  -project OpenFrameHost.xcodeproj \
  -scheme OpenFrameHost \
  -destination 'platform=iOS Simulator,name=iPhone 15' \
  build
```

*(Not runnable on this machine — `xcodebuild` requires Xcode.)*

### Step 6 — Run the tests (⌘U)

Press **⌘U**. Xcode builds and runs `OpenFrameHostTests`; a green checkmark means
all decoding tests passed.

**Terminal alternative — run tests (from `ios-host/`):**

```bash
xcodebuild \
  -project OpenFrameHost.xcodeproj \
  -scheme OpenFrameHost \
  -destination 'platform=iOS Simulator,name=iPhone 15' \
  test
```

*(Not runnable on this machine.)*

### Step 7 (optional) — Run on a real iPhone

Requires signing. In Xcode, select the `OpenFrameHost` target → **Signing &
Capabilities** → set your **Team** (a free Apple ID works) and let Xcode manage
signing. Then pick the connected device as the destination and press **⌘R**. Full
detail is in [`setup-macos.md`](./setup-macos.md) §5. Don't commit a personal
team id.

---

## 13. The tests and what each one proves

`ios-host/Tests/OpenFrameNotificationDecodingTests.swift` is an **XCTest** suite.
XCTest is Apple's testing framework; each `func test…` is a test, and `XCTAssert…`
calls check expectations. Running the suite needs Xcode (it `import XCTest` and
runs in a Simulator), so it was **not executed on this machine** — but the
underlying decode logic each test targets **was** verified with the CLT compiler
(see [§4](#4-verified-vs-not-verified--read-this-before-trusting-anything)).

`@testable import OpenFrameHost` lets the tests reach into the app's internal
types.

| Test | What it proves |
|---|---|
| `testDecodeSingleElementFromString` | A well-formed JSON object decodes into an `OpenFrameNotification` with every field mapped correctly (`id`, `appIdentifier`, `appName`, `title`, `body`, `timestamp`, `priority == .high`, `category`). Confirms the `CodingKeys` mapping. |
| `testTimestampParsesToDate` | The `timestamp` string parses to a non-nil `Date` via the computed `date` property — the ISO 8601 formatter works. |
| `testUnknownPriorityFallsBackToNormal` | A `priority` of `"urgent"` decodes to `.normal`, proving the lenient `Priority.init(from:)`. |
| `testMissingRequiredFieldThrows` | JSON missing required fields (`[ { "id": "x", "appName": "Example" } ]`) makes `decode` **throw** — required fields really are required. |
| `testBundledSampleDecodes` | `NotificationLoader.loadSample()` loads the committed `notifications.sample.json` from the bundle and every item has non-empty core fields and a valid date. This is the **model/sample contract**: the shipping model can always read the shipping sample. |

Together they cover the happy path, the lenient path, the failure path, and the
"the real bundled file still matches the model" guarantee.

---

## 14. XcodeGen and the generated project

### Why generate?

An `.xcodeproj` stores its settings in `project.pbxproj`, a long file full of
random-looking IDs. It's miserable to review and prone to merge conflicts.
**XcodeGen** lets you describe the project in a short YAML file (`project.yml`)
and generates the `.xcodeproj` from it, so the YAML is the reviewable source of
truth.

### `project.yml`, explained

```yaml
name: OpenFrameHost

options:
  bundleIdPrefix: com.openframe
  createIntermediateGroups: true
  deploymentTarget:
    iOS: "16.0"
  developmentLanguage: en

settings:
  base:
    SWIFT_VERSION: "5.0"
    CODE_SIGN_STYLE: Automatic

targets:
  OpenFrameHost:
    type: application
    platform: iOS
    deploymentTarget: "16.0"
    sources:
      - path: Sources
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.openframe.host
        TARGETED_DEVICE_FAMILY: "1"           # iPhone only
        GENERATE_INFOPLIST_FILE: YES
        INFOPLIST_KEY_CFBundleDisplayName: OpenFrame
        …

  OpenFrameHostTests:
    type: bundle.unit-test
    platform: iOS
    deploymentTarget: "16.0"
    sources:
      - path: Tests
    dependencies:
      - target: OpenFrameHost
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.openframe.host.tests
        GENERATE_INFOPLIST_FILE: YES

schemes:
  OpenFrameHost:
    build:  { targets: { OpenFrameHost: all } }
    run:    { config: Debug }
    test:   { targets: [ OpenFrameHostTests ] }
```

- **`name`** — the project name (`OpenFrameHost.xcodeproj`).
- **`options.deploymentTarget.iOS: "16.0"`** — minimum iOS version.
- **`settings.base`** — settings applied to everything: Swift language version
  `5.0`, automatic signing.
- **`targets.OpenFrameHost`** — the app: type `application`, iOS, its code lives
  in `Sources/`, bundle id `com.openframe.host`, iPhone-only
  (`TARGETED_DEVICE_FAMILY: "1"`), and Xcode auto-generates its Info.plist with a
  display name of "OpenFrame".
- **`targets.OpenFrameHostTests`** — the test bundle: type `bundle.unit-test`,
  code in `Tests/`, and it **depends on** the app target (so the app builds
  first and the tests can import it).
- **`schemes.OpenFrameHost`** — the recipe: build the app, run in Debug, test via
  the tests target. This is what fills the scheme selector in Xcode.
- **Note there is no `DEVELOPMENT_TEAM`** — intentional, so Simulator builds need
  no account.

### Regenerating

From the repository root:

```bash
cd ios-host
xcodegen generate
```

Do this whenever you add/rename/move files or edit `project.yml`. Generation is
**deterministic** — verified here: re-running it left `git status` clean, meaning
the committed `.xcodeproj` already matches `project.yml`. If you ever edit the
`.xcodeproj` by hand, the next `xcodegen generate` will overwrite your change —
edit `project.yml` instead.

---

## 15. How to safely change text, status, and sample data

Small, safe edits you can make to learn the codebase. After UI edits, rebuild in
Xcode; after JSON edits, keep the two copies in sync.

### Change on-screen text

- **App title / subtitle** — in `HomeView.swift`, the `header`:
  `Text("OpenFrame")` and `Text("Host")`.
- **Section titles** — the `SectionCard(title:)` arguments: `"Connection"`,
  `"Notifications (mock)"`.
- **Button words** — in `sessionSection`, the `"Stop Session"` / `"Start
  Session"` strings.
- **Empty-list text** — in `notificationsSection`, `Text("No notifications.")`.

These are plain string literals; change the text between the quotes.

### Change a mock status

The initial states live in `HomeViewModel.swift`:

```swift
@Published var simulatorState: ConnectionState = .disconnected
@Published var glassesState: ConnectionState = .disconnected
```

Change a default to `.connecting` or `.connected` to preview that look. To change
how a state renders (its words, icon, or color), edit the `label` / `systemImage`
/ `tint` switches on `ConnectionState`. Icons are **SF Symbols** names — browse
them in Apple's free **SF Symbols** app.

### Change the sample notifications — the sync rule

The app loads its **own copy**, so you must edit the canonical file **and** mirror
it. From the repository root:

1. Edit the canonical file `protocol/examples/notifications.sample.json`. Keep the
   fields exactly as documented in
   [`protocol/examples/README.md`](../protocol/examples/README.md): every object
   needs `id`, `appIdentifier`, `appName`, `title`, `body`, `timestamp`
   (ISO 8601, e.g. `2026-07-28T14:32:00Z`), `priority` (`low`/`normal`/`high`),
   and `category`.

2. Copy it over the bundled resource and confirm they match:

   ```bash
   cp protocol/examples/notifications.sample.json \
      ios-host/Sources/Resources/notifications.sample.json
   cmp protocol/examples/notifications.sample.json \
       ios-host/Sources/Resources/notifications.sample.json && echo "in sync"
   ```

3. Validate the JSON:

   ```bash
   python3 -m json.tool protocol/examples/notifications.sample.json > /dev/null && echo "valid JSON"
   ```

If you **add a field** to the schema, you must also add the property (and its
`CodingKeys` case) to `OpenFrameNotification.swift`, or the new field is simply
ignored on decode. If you make a new field required and don't add it to every
JSON object, `testMissingRequiredFieldThrows`-style decoding will fail.

> These edits touch Swift/JSON files. This guide **describes** them; it does not
> perform them.

---

## 16. Debugging common errors

| Symptom | Likely cause | Fix |
|---|---|---|
| `xcodebuild: error: tool 'xcodebuild' requires Xcode` | Only the CLT are active (this machine's exact state). | Install Xcode, then `sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer`. |
| `xcrun: error: unable to find utility "simctl"` | No Simulator because no Xcode. | Install Xcode; the Simulator ships inside it. |
| `xcodegen: command not found` | XcodeGen not installed / not on PATH. | `brew install xcodegen`; check `xcodegen --version`. |
| Build error: `no such module 'SwiftUI'` when parsing at the terminal | You tried to compile a SwiftUI file with the CLT (no iOS SDK). | Build in Xcode instead; the CLT can only handle Foundation-only files. |
| App runs but the notifications list is empty | The bundled resource wasn't found, or JSON failed to decode. | Confirm `notifications.sample.json` is under `Sources/Resources/`, re-run `xcodegen generate`, and validate the JSON with `python3 -m json.tool`. The UI will also show an orange error label with the reason. |
| "No notifications." even though JSON has items | The resource file isn't in the target's `Sources`, so it wasn't bundled. | Ensure it lives under `ios-host/Sources/` and regenerate the project. |
| Notifications show but no time on a row | That item's `timestamp` isn't valid ISO 8601, so `date` is `nil`. | Fix the timestamp to look like `2026-07-28T14:32:00Z`. |
| Changed the JSON but the app still shows old data | You edited only one of the two copies, or didn't rebuild. | Re-copy per [§15](#15-how-to-safely-change-text-status-and-sample-data) and rebuild. |
| Weird, stale build failures | Corrupt DerivedData cache. | In Xcode: Product → Clean Build Folder (⇧⌘K), or delete DerivedData. |
| Device build fails with a signing error | No Team set for a physical device. | Set a Team under Signing & Capabilities (Simulator needs none). |

**How to read the error label in-app.** If loading fails, `HomeViewModel` catches
it and sets `loadErrorMessage`, which `notificationsSection` renders as an orange
warning. The two messages come from `NotificationLoader.LoadError`:
*"Bundled resource "notifications.sample.json" was not found."* or
*"Failed to decode notifications: …"*.

---

## 17. Glossary

- **API** — the set of types/functions a piece of code exposes for others to use.
- **Bundle** — a folder treated as one item; a built app is a bundle holding code
  + resources. `Bundle.main` is the running app's own bundle.
- **CLT (Command Line Tools)** — Apple's compiler/tooling package without the full
  IDE or iOS SDK.
- **Codable** — Swift protocol for encoding/decoding to formats like JSON.
- **Closure** — an inline, passable block of code.
- **Computed property** — a property whose value is calculated each read.
- **Deployment target** — the oldest OS version an app supports (here iOS 16.0).
- **DerivedData** — Xcode's throwaway build cache.
- **Destination** — where a build runs (a Simulator model or a device).
- **Enum** — a type with a fixed set of named cases.
- **Foundation** — Apple's core, non-UI library (strings, dates, JSON…).
- **Generic** — code parameterized by type (e.g. `SectionCard<Content: View>`).
- **iOS SDK** — the libraries/headers for building iOS apps; ships inside Xcode.
- **Modifier** — a SwiftUI method returning a tweaked view (`.padding`, `.font`…).
- **MVVM** — Model / ViewModel / View architecture.
- **Optional** — a value that may be `nil`; written with `?`.
- **Property wrapper** — an `@`-attribute adding behavior to a property
  (`@Published`, `@StateObject`…).
- **Protocol** — a contract a type can conform to.
- **Resource** — a non-code file bundled with the app.
- **Scheme** — a saved build/run/test recipe in Xcode.
- **SF Symbols** — Apple's built-in icon set, referenced by name.
- **Signing** — stamping an app with a developer identity (needed for devices).
- **Simulator** — a Mac app emulating an iPhone/iPad; part of Xcode.
- **struct / class** — value type (copied) vs. reference type (shared).
- **SwiftUI** — Apple's declarative UI framework.
- **Target** — one buildable output in a project (the app; the tests).
- **View** — a SwiftUI screen or component; conforms to the `View` protocol.
- **XcodeGen** — tool that generates `.xcodeproj` from `project.yml`.
- **XCTest** — Apple's unit-testing framework.

---

## 18. Boundaries and sensible next steps

**Explicitly out of scope right now** (and intentionally so):

- **Live iOS notifications** — no `UserNotifications`, no notification service or
  content extensions, no reading of real device notifications. Everything shown is
  mock JSON.
- **Bluetooth** — no `CoreBluetooth`, no pairing, no glasses transport. The
  Glasses/Simulator rows are visual placeholders.
- **Networking** — no HTTP/WebSocket/sync; all data is local.
- **Unity** — this work does not add to or modify any Unity project.
- **Signing/distribution** — no team, profiles, or CI signing committed.

**This guide does not implement any of the above**, and (per
[§4](#4-verified-vs-not-verified--read-this-before-trusting-anything)) no iOS
build was performed here.

**Sensible next steps, roughly in order** (each is future work, not done):

1. **Verify the build on a machine with Xcode.** Run `⌘R` and `⌘U` (or the
   `xcodebuild` commands) to confirm the app runs and all tests pass on the iOS
   SDK — the one thing this CLT-only machine couldn't check.
2. **Real logo asset.** Replace the gradient placeholder in `header` with a proper
   asset catalog image.
3. **A notification store abstraction.** Introduce a small protocol for "where
   notifications come from," backed today by `NotificationLoader`, so a live
   source can be swapped in later without touching the views.
4. **Then, one transport at a time**, behind that abstraction: a Bluetooth link to
   glasses, or a networking layer. Treat cross-app notification capture as a separate iOS
   feasibility question: standard apps cannot read a system-wide notification
   feed, so only supported, app-specific sources should be designed and tested.

Keeping those separate is the whole point of shipping the shell first: the screen
and the data shape are settled, so each real capability can be added and reviewed
on its own.

---

### See also

- [`setup-macos.md`](./setup-macos.md) — concise prerequisites + build/test/run.
- [`../ios-host/README.md`](../ios-host/README.md) — the host module's developer entry point.
- [`../protocol/examples/README.md`](../protocol/examples/README.md) — the notification schema.
- [`../README.md`](../README.md) — repository landing page.
