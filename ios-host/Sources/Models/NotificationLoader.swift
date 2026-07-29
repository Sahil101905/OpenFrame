import Foundation

/// Loads bundled mock notifications from local JSON.
///
/// This is intentionally the *only* source of notifications in the host shell:
/// nothing here touches the live iOS notification system, Bluetooth, or the
/// network. It reads `notifications.sample.json` from a bundle and decodes it
/// with ``OpenFrameNotification``.
enum NotificationLoader {

    /// Errors surfaced while loading the bundled sample.
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

    /// Default resource name (without extension) for the bundled sample.
    static let sampleResourceName = "notifications.sample"

    /// Decode notifications from an in-memory JSON payload.
    /// Exposed so tests can exercise decoding without a bundle.
    static func decode(_ data: Data) throws -> [OpenFrameNotification] {
        do {
            return try JSONDecoder().decode([OpenFrameNotification].self, from: data)
        } catch {
            throw LoadError.decodingFailed(String(describing: error))
        }
    }

    /// Load and decode the bundled sample notifications.
    ///
    /// - Parameters:
    ///   - name: Resource base name, defaults to ``sampleResourceName``.
    ///   - bundle: Bundle to search; defaults to `.main`. Tests pass their own
    ///     bundle so they can validate the exact committed sample.
    static func loadSample(
        named name: String = sampleResourceName,
        from bundle: Bundle = .main
    ) throws -> [OpenFrameNotification] {
        guard let url = bundle.url(forResource: name, withExtension: "json") else {
            throw LoadError.resourceNotFound(name: name)
        }
        let data = try Data(contentsOf: url)
        return try decode(data)
    }
}
