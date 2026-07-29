import Foundation
import SwiftUI

/// A mock connection state used for the status rows on the home screen.
///
/// These are placeholders: the host shell does not actually connect to a
/// simulator or to glasses hardware. The states exist so the UI can be laid out
/// and reviewed before real transports are added.
enum ConnectionState: Equatable {
    case disconnected
    case connecting
    case connected

    var label: String {
        switch self {
        case .disconnected: return "Disconnected"
        case .connecting:   return "Connecting…"
        case .connected:    return "Connected"
        }
    }

    var systemImage: String {
        switch self {
        case .disconnected: return "circle"
        case .connecting:   return "circle.dotted"
        case .connected:    return "checkmark.circle.fill"
        }
    }

    var tint: Color {
        switch self {
        case .disconnected: return .secondary
        case .connecting:   return .orange
        case .connected:    return .green
        }
    }
}

/// Drives the home screen. All data is local and mocked.
@MainActor
final class HomeViewModel: ObservableObject {

    /// Mock status of the paired glasses simulator.
    @Published var simulatorState: ConnectionState = .disconnected

    /// Mock status of physical glasses hardware.
    @Published var glassesState: ConnectionState = .disconnected

    /// Whether a mock session is currently running.
    @Published var isSessionActive: Bool = false

    /// Notifications loaded from the bundled local JSON sample.
    @Published private(set) var notifications: [OpenFrameNotification] = []

    /// Non-nil when loading the bundled sample failed.
    @Published private(set) var loadErrorMessage: String?

    /// Load the bundled mock notifications from local JSON.
    func loadNotifications() {
        do {
            notifications = try NotificationLoader.loadSample()
            loadErrorMessage = nil
        } catch {
            notifications = []
            loadErrorMessage = error.localizedDescription
        }
    }

    /// Toggle the mock session. Also flips the simulator status so the button
    /// visibly does something during review.
    func toggleSession() {
        isSessionActive.toggle()
        simulatorState = isSessionActive ? .connected : .disconnected
    }
}
