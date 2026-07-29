import Foundation

/// A single notification in the OpenFrame protocol.
///
/// The property names and `CodingKeys` map **exactly** to the canonical JSON in
/// `protocol/examples/notifications.sample.json` (mirrored into the app bundle as
/// `Sources/Resources/notifications.sample.json`). If you change one, change the
/// others — the decoding tests assert that the bundled sample round-trips.
struct OpenFrameNotification: Codable, Identifiable, Equatable {

    /// Stable unique identifier (UUID form in the samples).
    let id: String

    /// Reverse-DNS bundle identifier of the originating app.
    let appIdentifier: String

    /// Human-readable app name, used for display.
    let appName: String

    /// Primary line of the notification.
    let title: String

    /// Secondary line / detail of the notification.
    let body: String

    /// ISO 8601 / RFC 3339 UTC instant, e.g. `2026-07-28T14:32:00Z`.
    /// Stored as the raw string so the model matches the JSON exactly; use
    /// ``date`` for a parsed value.
    let timestamp: String

    /// Delivery priority. Unknown JSON values decode to ``Priority/normal``.
    let priority: Priority

    /// Free-form category hint, e.g. `message`, `reminder`, `news`.
    let category: String

    /// Explicit keys so the mapping to the JSON is unambiguous and stable.
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

    /// Notification priority. Decoding is lenient: any unrecognized string
    /// becomes ``normal`` so a schema addition never crashes the host.
    enum Priority: String, Codable, CaseIterable, Equatable {
        case low
        case normal
        case high

        init(from decoder: Decoder) throws {
            let raw = try decoder.singleValueContainer().decode(String.self)
            self = Priority(rawValue: raw) ?? .normal
        }
    }

    /// The `timestamp` parsed as a `Date`, or `nil` if it is not valid ISO 8601.
    var date: Date? {
        OpenFrameNotification.isoFormatter.date(from: timestamp)
    }

    private static let isoFormatter: ISO8601DateFormatter = {
        let formatter = ISO8601DateFormatter()
        formatter.formatOptions = [.withInternetDateTime]
        return formatter
    }()
}
