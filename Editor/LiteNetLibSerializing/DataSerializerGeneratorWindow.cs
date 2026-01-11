using LiteNetLib.Utils;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FishNet.Insthync.LiteNetLibSerializing
{
    public class DataSerializerGeneratorWindow : EditorWindow
    {
        private string ns = "FishNet.Insthync.LiteNetLibSerializing";
        private string className = "LiteNetLibSerializer";
        private string savePath = "Assets/Generated/LiteNetLibSerializer.generated.cs";

        [MenuItem("Tools/Fish-Networking/Insthync/Generate LiteNetLib Serializer")]
        public static void Open()
        {
            GetWindow<DataSerializerGeneratorWindow>(
                "LiteNetLib Serializer Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("LiteNetLib Serializer Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            ns = EditorGUILayout.TextField("Namespace", ns);
            className = EditorGUILayout.TextField("Class Name", className);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                BrowsePath();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            GUI.enabled = !string.IsNullOrEmpty(className) &&
                          !string.IsNullOrEmpty(savePath);

            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                Generate(ns, className, savePath);
            }

            GUI.enabled = true;
        }

        private void BrowsePath()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save Serializer",
                Application.dataPath,
                className,
                "cs");

            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    savePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogError("Path must be inside Assets/");
                }
            }
        }

        private static void Generate(string ns, string className, string savePath)
        {
            var types = TypeCache.GetTypesDerivedFrom<INetSerializable>();

            var sb = new StringBuilder();
            sb.AppendLine("// Generated from menu: `Tools/Fish-Networking/Insthync/Generate LiteNetLib Serializer`");
            sb.AppendLine("using FishNet.Serializing;");
            sb.AppendLine($@"
namespace {ns}
{{
    public static partial class {className}
    {{");

            foreach (var t in types)
            {
                bool isClass = t.IsClass;
                bool isStruct =
                    t.IsValueType &&
                    !t.IsPrimitive &&
                    !t.IsEnum;

                // Reader
                if (isStruct)
                {
                    sb.AppendLine($@"
        public static {t.Name} Read{t.Name}(this Reader reader)
        {{
            return reader.Get<{t.Name}>();
        }}");
                }
                else if (isClass)
                {
                    sb.AppendLine($@"
        public static {t.Name} Read{t.Name}(this Reader reader)
        {{
            return reader.Get<{t.Name}>(new {t.Name}());
        }}");
                }
                else
                {
                    continue;
                }

                // Writer
                sb.AppendLine($@"
        public static void Write{t.Name}(this Writer writer, {t.Name} data)
        {{
            writer.Put(data);
        }}");
            }
            sb.AppendLine($@"
    }}
}}");

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log($"LiteNetLib serializer generated at: {savePath}");
        }
    }
}
