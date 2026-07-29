import SwiftUI

/// The OpenFrame host home screen.
///
/// Sections, top to bottom:
///  - Header: OpenFrame logo placeholder + title.
///  - Connection status: simulator + glasses (mock states).
///  - Session control: Start / Stop Session button (mock).
///  - Mock notifications: decoded from bundled local JSON.
struct HomeView: View {
    @StateObject private var viewModel: HomeViewModel

    init(viewModel: HomeViewModel) {
        _viewModel = StateObject(wrappedValue: viewModel)
    }

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

    // MARK: - Header

    private var header: some View {
        HStack(spacing: 14) {
            // Logo placeholder — swap for the real asset when available.
            RoundedRectangle(cornerRadius: 16, style: .continuous)
                .fill(
                    LinearGradient(
                        colors: [.blue, .purple],
                        startPoint: .topLeading,
                        endPoint: .bottomTrailing
                    )
                )
                .frame(width: 56, height: 56)
                .overlay(
                    Image(systemName: "eyeglasses")
                        .font(.system(size: 26, weight: .semibold))
                        .foregroundStyle(.white)
                )
                .accessibilityHidden(true)

            VStack(alignment: .leading, spacing: 2) {
                Text("OpenFrame")
                    .font(.largeTitle.bold())
                Text("Host")
                    .font(.headline)
                    .foregroundStyle(.secondary)
            }
            Spacer()
        }
    }

    // MARK: - Connection status

    private var statusSection: some View {
        SectionCard(title: "Connection") {
            StatusRow(
                title: "Simulator",
                systemImage: "display",
                state: viewModel.simulatorState
            )
            Divider()
            StatusRow(
                title: "Glasses",
                systemImage: "eyeglasses",
                state: viewModel.glassesState
            )
        }
    }

    // MARK: - Session control

    private var sessionSection: some View {
        Button(action: viewModel.toggleSession) {
            Label(
                viewModel.isSessionActive ? "Stop Session" : "Start Session",
                systemImage: viewModel.isSessionActive ? "stop.fill" : "play.fill"
            )
            .font(.headline)
            .frame(maxWidth: .infinity)
            .padding(.vertical, 14)
        }
        .buttonStyle(.borderedProminent)
        .tint(viewModel.isSessionActive ? .red : .blue)
    }

    // MARK: - Notifications

    private var notificationsSection: some View {
        SectionCard(title: "Notifications (mock)") {
            if let message = viewModel.loadErrorMessage {
                Label(message, systemImage: "exclamationmark.triangle.fill")
                    .font(.subheadline)
                    .foregroundStyle(.orange)
            } else if viewModel.notifications.isEmpty {
                Text("No notifications.")
                    .font(.subheadline)
                    .foregroundStyle(.secondary)
            } else {
                ForEach(Array(viewModel.notifications.enumerated()), id: \.element.id) { index, note in
                    if index > 0 { Divider() }
                    NotificationRow(notification: note)
                }
            }
        }
    }
}

// MARK: - Reusable pieces

/// A titled card with a rounded background.
private struct SectionCard<Content: View>: View {
    let title: String
    @ViewBuilder let content: Content

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            Text(title.uppercased())
                .font(.caption.bold())
                .foregroundStyle(.secondary)
            VStack(alignment: .leading, spacing: 12) {
                content
            }
            .frame(maxWidth: .infinity, alignment: .leading)
            .padding(16)
            .background(Color(.secondarySystemGroupedBackground))
            .clipShape(RoundedRectangle(cornerRadius: 16, style: .continuous))
        }
    }
}

/// One connection status line.
private struct StatusRow: View {
    let title: String
    let systemImage: String
    let state: ConnectionState

    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: systemImage)
                .foregroundStyle(.secondary)
                .frame(width: 24)
            Text(title)
                .font(.body)
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

/// One mock notification row.
private struct NotificationRow: View {
    let notification: OpenFrameNotification

    var body: some View {
        HStack(alignment: .top, spacing: 12) {
            Image(systemName: "app.badge.fill")
                .foregroundStyle(.blue)
                .frame(width: 24)
                .padding(.top, 2)
            VStack(alignment: .leading, spacing: 3) {
                HStack {
                    Text(notification.title)
                        .font(.subheadline.bold())
                    Spacer()
                    Text(relativeTimestamp)
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
                Text(notification.body)
                    .font(.subheadline)
                    .foregroundStyle(.primary)
                    .fixedSize(horizontal: false, vertical: true)
                Text(notification.appName)
                    .font(.caption)
                    .foregroundStyle(.secondary)
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
