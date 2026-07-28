import SwiftUI

/// Entry point for the OpenFrame iPhone host shell.
///
/// This is a UI shell only. It shows connection status placeholders, a session
/// control, and a section of *mock* notifications loaded from bundled local
/// JSON. There is no live notification access, Bluetooth, or networking here.
@main
struct OpenFrameHostApp: App {
    var body: some Scene {
        WindowGroup {
            HomeView(viewModel: HomeViewModel())
        }
    }
}
