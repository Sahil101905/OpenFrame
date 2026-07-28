using System;
using System.IO;
using OpenFrame.Simulator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenFrame.Editor
{
    public static class OpenFrameSimulatorBuilder
    {
        private const string ScenePath = "Assets/OpenFrame/Scenes/OpenFrameSimulator.unity";
        private const string PrefabPath = "Assets/OpenFrame/Prefabs/NotificationCard.prefab";
        private const string JsonPath = "Assets/OpenFrame/Data/mock-notifications.json";

        [MenuItem("OpenFrame/Build Simulator Scene")]
        public static void Build()
        {
            EnsureFolders();
            GameObject cardPrefab = BuildNotificationCardPrefab();
            BuildScene(cardPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("OpenFrame simulator scene and notification prefab created.");
        }

        public static void BuildAndValidate()
        {
            Build();
            Validate();
        }

        [MenuItem("OpenFrame/Validate Simulator")]
        public static void Validate()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Require(scene.IsValid(), "OpenFrameSimulator scene could not be opened.");
            Require(GameObject.Find("FirstPersonCamera") != null, "First-person camera is missing.");
            Require(GameObject.Find("GlassesHUD") != null, "World-space Canvas is missing.");
            Require(GameObject.Find("NotificationSimulator") != null, "Notification controller is missing.");
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null, "Notification card prefab is missing.");

            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            Require(json != null, "Mock notification JSON is missing.");
            NotificationCollection collection = JsonUtility.FromJson<NotificationCollection>(json.text);
            Require(
                collection != null && collection.notifications != null && collection.notifications.Length > 0,
                "Mock notification JSON contains no notifications.");

            Canvas canvas = GameObject.Find("GlassesHUD").GetComponent<Canvas>();
            Require(canvas.renderMode == RenderMode.WorldSpace, "GlassesHUD Canvas is not world-space.");
            Require(
                canvas.transform.parent == GameObject.Find("FirstPersonCamera").transform,
                "GlassesHUD Canvas is not attached to the first-person camera.");
            Require(
                GameObject.Find("FirstPersonCamera").GetComponent<FirstPersonCameraController>() != null,
                "First-person camera controller is missing.");

            NotificationSimulatorController controller =
                GameObject.Find("NotificationSimulator").GetComponent<NotificationSimulatorController>();
            SerializedObject serializedController = new SerializedObject(controller);
            Require(
                serializedController.FindProperty("notificationsJson").objectReferenceValue != null,
                "Notification controller JSON reference is missing.");
            Require(
                serializedController.FindProperty("notificationCard").objectReferenceValue != null,
                "Notification controller card reference is missing.");

            NotificationCardView card = GameObject.Find("NotificationCard").GetComponent<NotificationCardView>();
            SerializedObject serializedCard = new SerializedObject(card);
            string[] textProperties =
            {
                "sourceApplicationText",
                "titleText",
                "messageBodyText",
                "timestampText"
            };
            foreach (string propertyName in textProperties)
            {
                Require(
                    serializedCard.FindProperty(propertyName).objectReferenceValue != null,
                    $"Notification card field {propertyName} is missing.");
            }

            Debug.Log("OPENFRAME_VALIDATION_PASSED");
        }

        private static GameObject BuildNotificationCardPrefab()
        {
            GameObject root = new GameObject(
                "NotificationCard",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(NotificationCardView));

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(620f, 250f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.085f, 0.94f);

            Text source = CreateText(root.transform, "SourceApplication", 24, FontStyle.Bold);
            SetRect(source.rectTransform, 28f, -22f, 390f, 34f);
            source.color = new Color(0.40f, 0.82f, 1f);

            Text timestamp = CreateText(root.transform, "Timestamp", 21, FontStyle.Normal);
            SetRect(timestamp.rectTransform, 430f, -24f, 160f, 32f);
            timestamp.alignment = TextAnchor.UpperRight;
            timestamp.color = new Color(0.72f, 0.78f, 0.86f);

            Text title = CreateText(root.transform, "Title", 32, FontStyle.Bold);
            SetRect(title.rectTransform, 28f, -66f, 560f, 46f);

            Text message = CreateText(root.transform, "MessageBody", 25, FontStyle.Normal);
            SetRect(message.rectTransform, 28f, -120f, 560f, 100f);
            message.verticalOverflow = VerticalWrapMode.Truncate;
            message.color = new Color(0.90f, 0.93f, 0.98f);

            root.GetComponent<NotificationCardView>().Configure(source, title, message, timestamp);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildScene(GameObject cardPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateCamera();
            CreateWorld();

            GameObject canvasObject = new GameObject(
                "GlassesHUD",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.transform.SetParent(camera.transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 0f, 2f);
            canvasObject.transform.localRotation = Quaternion.identity;
            canvasObject.transform.localScale = Vector3.one * 0.0025f;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 10;
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 450f);

            GameObject cardObject = PrefabUtility.InstantiatePrefab(cardPrefab, canvasObject.transform)
                as GameObject;
            NotificationCardView card = cardObject.GetComponent<NotificationCardView>();
            card.name = "NotificationCard";
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            GameObject controllerObject = new GameObject("NotificationSimulator");
            NotificationSimulatorController controller =
                controllerObject.AddComponent<NotificationSimulatorController>();
            controller.Configure(AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath), card);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject(
                "FirstPersonCamera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(FirstPersonCameraController));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1.65f, -4f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            return camera;
        }

        private static void CreateWorld()
        {
            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Simulator Floor";
            floor.transform.localScale = new Vector3(3f, 1f, 3f);

            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = "Orientation Landmark";
            landmark.transform.position = new Vector3(0f, 1f, 4f);
            landmark.transform.localScale = new Vector3(2f, 2f, 0.5f);
        }

        private static Text CreateText(Transform parent, string name, int fontSize, FontStyle style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }

        private static void SetRect(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/OpenFrame/Scenes");
            EnsureFolder("Assets/OpenFrame/Prefabs");
        }

        private static void EnsureFolder(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
