using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ExtendClassHelper
{
    [MenuItem("Assets/Extend Class", true)]
    private static bool ExtendClassValidate()
    {
        string path = GetPath();

        if (IsValidPath(path))
        {
            return true;
        }

        return false;
    }

    [MenuItem("Assets/Extend Class", priority = 0)]
    private static void ExtendClass()
    {
        string path = GetPath();

        string baseClass = "State";

        if (IsValidPath(path))
        {
            baseClass = Path.GetFileNameWithoutExtension(path);

            path = EditorUtility.SaveFilePanelInProject("Extend Class", "MyState.cs", "cs", "Please select file name to save class to:", Path.GetDirectoryName(path));
            if (!string.IsNullOrEmpty(path))
            {
                string className = Path.GetFileNameWithoutExtension(path);

                FileInfo fi = new FileInfo(path);

                if (fi.Exists)
                {
                    Debug.LogError("File already exists!");
                }
                else
                {
                    string o = string.Format(@"using UnityEngine;

public class {0} : {1}
{{
}}", className, baseClass);

                    byte[] Content = Encoding.UTF8.GetBytes(o);

                    using (Stream writer = fi.OpenWrite())
                    {
                        writer.Write(Content, 0, Content.Length);
                        writer.Flush();
                        writer.Close();
                    }

                    AssetDatabase.Refresh();
                }
            }
        }
    }

    private static bool IsValidPath(string path)
    {
        return path.Length > 0 && !Directory.Exists(path);
    }

    private static string GetPath()
    {
        var path = "";
        var obj = Selection.activeObject;

        if (obj == null) path = "Assets";
        else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

        return path;
    }
}
