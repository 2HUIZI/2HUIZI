using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MainWindow<T> : EditorWindowBase where T: EditorWindowBase
{
    public static T window;
    public static void Popup(string windowName,bool utility,Vector2 minResolution)
    {
        window = EditorWindow.GetWindow(typeof(T), utility, windowName) as T;
        window.minSize = minResolution;
        EditorWindowMgr.AddEditorWindow(window);
        window.Show();
    }

    public virtual void OnDestroy()
    {
        EditorWindowMgr.RemoveEditorWindow(window);
        EditorWindowMgr.DestoryAllWindow();
    }
}
