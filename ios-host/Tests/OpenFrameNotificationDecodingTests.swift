import XCTest
@testable import OpenFrameHost

/// Tests for decoding the local notification JSON. These do not touch the
/// network, Bluetooth, or the live notification system.
final class OpenFrameNotificationDecodingTests: XCTestCase {

    /// A minimal, self-contained payload matching the canonical schema.
    private let singleElementJSON = """
    [
      {
        "id": "test-id-1",
        "appIdentifier": "com.example.app",
        "appName": "Example",
        "title": "Hello",
        "body": "World",
        "timestamp": "2026-07-28T14:32:00Z",
        "priority": "high",
        "category": "message"
      }
    ]
    """

    func testDecodeSingleElementFromString() throws {
        let data = Data(singleElementJSON.utf8)
        let notifications = try NotificationLoader.decode(data)

        XCTAssertEqual(notifications.count, 1)
        let note = try XCTUnwrap(notifications.first)
        XCTAssertEqual(note.id, "test-id-1")
        XCTAssertEqual(note.appIdentifier, "com.example.app")
        XCTAssertEqual(note.appName, "Example")
        XCTAssertEqual(note.title, "Hello")
        XCTAssertEqual(note.body, "World")
        XCTAssertEqual(note.timestamp, "2026-07-28T14:32:00Z")
        XCTAssertEqual(note.priority, .high)
        XCTAssertEqual(note.category, "message")
    }

    func testTimestampParsesToDate() throws {
        let data = Data(singleElementJSON.utf8)
        let note = try XCTUnwrap(try NotificationLoader.decode(data).first)
        XCTAssertNotNil(note.date, "ISO 8601 timestamp should parse to a Date")
    }

    func testUnknownPriorityFallsBackToNormal() throws {
        let json = """
        [
          {
            "id": "x",
            "appIdentifier": "com.example.app",
            "appName": "Example",
            "title": "t",
            "body": "b",
            "timestamp": "2026-07-28T14:32:00Z",
            "priority": "urgent",
            "category": "message"
          }
        ]
        """
        let note = try XCTUnwrap(try NotificationLoader.decode(Data(json.utf8)).first)
        XCTAssertEqual(note.priority, .normal)
    }

    func testMissingRequiredFieldThrows() {
        let json = """
        [ { "id": "x", "appName": "Example" } ]
        """
        XCTAssertThrowsError(try NotificationLoader.decode(Data(json.utf8)))
    }

    /// The committed sample in the app-under-test's main bundle must decode with
    /// the shipping model. This guards the model/sample contract.
    func testBundledSampleDecodes() throws {
        let notifications = try NotificationLoader.loadSample()

        XCTAssertFalse(notifications.isEmpty, "Bundled sample should not be empty")
        for note in notifications {
            XCTAssertFalse(note.id.isEmpty)
            XCTAssertFalse(note.appIdentifier.isEmpty)
            XCTAssertFalse(note.appName.isEmpty)
            XCTAssertFalse(note.title.isEmpty)
            XCTAssertNotNil(note.date, "Sample timestamps should be valid ISO 8601")
        }
    }
}
