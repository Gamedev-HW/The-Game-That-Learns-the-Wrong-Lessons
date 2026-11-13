using UnityEngine;
using System.Collections;
using WrongLessons.Player;
using WrongLessons.Mimo;
using WrongLessons.World;
using WrongLessons.Game;
using WrongLessons.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace WrongLessons.Bootstrap
{
    // Creates a complete playable greybox level in an empty scene.
    // Builds and rebuilds the playable laboratory from Unity primitives at runtime.
    public sealed class LabBootstrap : MonoBehaviour
    {
        // Assign a material asset using Universal Render Pipeline/Lit in the Inspector.
        // Referencing an asset keeps its shader available in WebGL builds.
        [SerializeField] private Material runtimeMaterial;

        private Transform generatedRoot;
        private bool restarting;

        private GameObject NewGeneratedObject(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(generatedRoot, true);
            return go;
        }

        // Clone the referenced material so each primitive can have its own color.
        // Shader.Find alone is unreliable in player builds because shaders can be stripped.
        private Material MakeMaterial(Color color)
        {
            if (runtimeMaterial == null)
            {
                // Allow a Resources-based reference as an alternative to Inspector assignment.
                runtimeMaterial = Resources.Load<Material>("RuntimeMaterial");
            }

            if (runtimeMaterial == null)
            {
                Debug.LogError("LabBootstrap: Missing RuntimeMaterial. Create a Material " +
                    "asset with shader 'Universal Render Pipeline/Lit' and assign it " +
                    "to the Runtime Material field on LabBootstrap, or save it as " +
                    "Assets/Resources/RuntimeMaterial.mat. Level generation stopped.", this);
                throw new System.InvalidOperationException("LabBootstrap requires a RuntimeMaterial asset.");
            }

            var material = new Material(runtimeMaterial);
            // URP/Lit exposes _BaseColor; setting material.color also updates it.
            material.color = color;
            return material;
        }

        // Creates a colored primitive under the generated level root.
        private GameObject Shape(PrimitiveType primitive, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name; go.transform.SetParent(generatedRoot, true); go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = MakeMaterial(color);
            return go;
        }
        private GameObject Cube(string name, Vector3 position, Vector3 scale, Color color) =>
            Shape(PrimitiveType.Cube, name, position, scale, color);
        private GameObject Sphere(string name, Vector3 position, Vector3 scale, Color color) =>
            Shape(PrimitiveType.Sphere, name, position, scale, color);
        private GameObject Capsule(string name, Vector3 position, Vector3 scale, Color color) =>
            Shape(PrimitiveType.Capsule, name, position, scale, color);
        private GameObject Cylinder(string name, Vector3 position, Vector3 scale, Color color) =>
            Shape(PrimitiveType.Cylinder, name, position, scale, color);
        // Decorative primitives are parented to the visual capsule, so they rotate
        // with the player's last movement direction without affecting physics.
        // Creates a decorative primitive as a child of the specified transform.
        private void AddFacePart(Transform parent, string name, PrimitiveType primitive,
            Vector3 localPosition, Vector3 localScale, Color color)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material = MakeMaterial(color);
            var partCollider = part.GetComponent<Collider>();
            if (partCollider != null) Destroy(partCollider);
        }

        // Adds top-facing eyes that remain visible from the overhead camera.

        private void AddPlayerFace(Transform visual)
        {
            // Player Visual is scaled to (0.65, 0.85, 0.65).
            // Positions here are in the VISUAL's local space, not world units.
            // Put the face above the capsule so the top-down camera can see it.
            // Local +Z remains the player's forward direction.
            AddFacePart(visual, "Left Eye", PrimitiveType.Sphere,
                new Vector3(-0.30f, 1.13f, 0.24f),
                new Vector3(0.28f, 0.13f, 0.28f), Color.white);
            AddFacePart(visual, "Right Eye", PrimitiveType.Sphere,
                new Vector3(0.30f, 1.13f, 0.24f),
                new Vector3(0.28f, 0.13f, 0.28f), Color.white);
            AddFacePart(visual, "Left Pupil", PrimitiveType.Sphere,
                new Vector3(-0.30f, 1.23f, 0.31f),
                new Vector3(0.12f, 0.07f, 0.12f), Color.black);
            AddFacePart(visual, "Right Pupil", PrimitiveType.Sphere,
                new Vector3(0.30f, 1.23f, 0.31f),
                new Vector3(0.12f, 0.07f, 0.12f), Color.black);
        }

        // Adds collider-free decorative antenna pieces to Mimo.

        private void AddMimoAntenna(Transform mimo)
        {
            // Parent to Mimo so the antenna follows the character.
            // Mimo's capsule has a local Y scale of 0.85; its top is at local Y=1.
            AddFacePart(mimo, "Antenna Stem", PrimitiveType.Cylinder,
                new Vector3(0f, 1.27f, 0f),
                new Vector3(0.065f, 0.28f, 0.065f), Color.gray);
            AddFacePart(mimo, "Antenna Tip", PrimitiveType.Sphere,
                new Vector3(0f, 1.56f, 0f),
                new Vector3(0.23f, 0.23f, 0.23f), Color.red);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // Ensures a bootstrap exists when a scene starts without one.
        private static void AutoCreate()
        {
            if (FindFirstObjectByType<LabBootstrap>() != null) return;
            new GameObject("LabBootstrap").AddComponent<LabBootstrap>();
        }
        private void Awake()
        {
            // Check before creating objects, so a missing asset produces a clear error.
            if (runtimeMaterial == null)
                runtimeMaterial = Resources.Load<Material>("RuntimeMaterial");

            if (runtimeMaterial == null)
            {
                Debug.LogError("LabBootstrap: Assign a URP/Lit RuntimeMaterial in the Inspector " +
                    "or place RuntimeMaterial.mat inside Assets/Resources before building WebGL.", this);
                enabled = false;
                return;
            }

            BuildLab();
        }

        // Creates the level geometry, actors, interactive objects, and UI.

        private void BuildLab()
        {
            generatedRoot = new GameObject("Generated Lab").transform;
            Cube("Floor", new Vector3(0, -0.3f, 0), new Vector3(23, 0.5f, 14), Color.gray);
            Cube("North Wall", new Vector3(0, 1.5f, 7), new Vector3(16, 3, .4f), Color.white);
            Cube("South Wall", new Vector3(0, 1.5f, -7), new Vector3(16, 3, .4f), Color.white);
            Cube("West Wall", new Vector3(-8, 1.5f, 0), new Vector3(.4f, 3, 14), Color.white);
            Cube("East Wall Left", new Vector3(8, 1.5f, -4.5f), new Vector3(.4f, 3, 5), Color.white);
            Cube("East Wall Right", new Vector3(8, 1.5f, 4.5f), new Vector3(.4f, 3, 5), Color.white);
            var gate = Cube("Exit Gate", new Vector3(8, 1.5f, 0), new Vector3(.5f, 3, 4), Color.red);
            var state = NewGeneratedObject("Game State").AddComponent<LabGameState>();
            state.Configure(gate);
            var player = NewGeneratedObject("Player");
            player.transform.position = new Vector3(-5, 1.1f, -4);
            var character = player.AddComponent<CharacterController>();
            character.height = 2; character.radius = .4f;
            player.AddComponent<PlayerController>();
            var visual = Capsule("Player Visual", player.transform.position, new Vector3(.65f, .85f, .65f), Color.green);
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            Destroy(visual.GetComponent<Collider>());
            AddPlayerFace(visual.transform);
            var cameraGo = NewGeneratedObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 10;
            cameraGo.transform.position = new Vector3(0, 17, -13);
            cameraGo.transform.LookAt(new Vector3(0, 0, 0));
            cameraGo.AddComponent<AudioListener>();
            var lightGo = NewGeneratedObject("Directional Light");
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
            // Dark-blue training button: a sphere, matching the cyan remote button.
            Sphere("Training Button", new Vector3(-3, .65f, -2), new Vector3(.9f, .9f, .9f), Color.blue).AddComponent<TrainingButton>();
            var mimo = Capsule("Mimo", new Vector3(-2, .85f, 2), new Vector3(.8f, .85f, .8f), Color.yellow);
            AddMimoAntenna(mimo.transform);
            mimo.AddComponent<MimoBrain>();
            Sphere("Remote Blue Button", new Vector3(3, .65f, 3), new Vector3(.9f, .9f, .9f), Color.cyan);
            var box = Cube("Pushable Box", new Vector3(-4, .55f, 3), new Vector3(1, 1, 1), new Color(.7f, .4f, .1f));
            var rb = box.AddComponent<Rigidbody>(); rb.mass = 1.5f; rb.constraints = RigidbodyConstraints.FreezeRotation;
            var plate = Cylinder("Pressure Plate", new Vector3(0, .04f, 3), new Vector3(1.05f, .06f, 1.05f), Color.magenta);
            plate.AddComponent<PressurePlate>();
            var exit = Cylinder("Exit Trigger", new Vector3(9.2f, 1, 0), new Vector3(1.5f, 1f, 1.5f), Color.green);
            var exitCollider = exit.GetComponent<Collider>(); exitCollider.isTrigger = true;
            exit.AddComponent<ExitZone>();
            NewGeneratedObject("HUD").AddComponent<LabHUD>();
        }
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            bool restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            bool restart = Input.GetKeyDown(KeyCode.R);
#endif
            if (restart && !restarting)
                StartCoroutine(RebuildLab());
        }

        // Waits for the old generated objects to be destroyed before rebuilding.

        private IEnumerator RebuildLab()
        {
            restarting = true;
            // Destroy every generated object, including the old HUD and event listeners.
            Destroy(generatedRoot.gameObject);
            // Unity processes Destroy at the end of the frame. Wait so old listeners
            // unsubscribe before the new game raises any events.
            yield return null;
            BuildLab();
            restarting = false;
        }
    }
}
