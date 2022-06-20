using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class RepeateWindow<T> : EditorWindowBase where T: EditorWindowBase
{
    public static T window;
    public static void Popup(Vector3 position,Vector2 minResolution,bool utility,string windowTitle)
    {
        Rect leftUpRect = new Rect(new Vector2(0, 0), minResolution);
        window = GetWindowWithRectPrivate(typeof(T), leftUpRect, utility, windowTitle) as T;
        window.minSize = minResolution;
        EditorWindowMgr.AddRepeateWindow(window);

        int offset = (window.Priority - 10) * 30;
        window.position = new Rect(new Vector2(position.x + offset, position.y + offset), new Vector2(800, 400));
        window.Show();
        window.Focus();
    }

    private static EditorWindow GetWindowWithRectPrivate(Type t,Rect rect,bool utility,string title)
    {
        EditorWindow editorWindow = null;
        if (!editorWindow)
        {
            editorWindow = ScriptableObject.CreateInstance(t) as EditorWindow;
            editorWindow.minSize = new Vector2(rect.width, rect.height);
            //editorWindow.maxSize = new Vector2(rect.width, rect.height);
            editorWindow.position = rect;
            if (title!=null)
                editorWindow.titleContent = new GUIContent(title);

            if (utility)
                editorWindow.ShowUtility();
            else
                editorWindow.Show();
        }
        else
            editorWindow.Focus();
        return editorWindow;
    }

    public virtual void OnDestroy()
    {
        EditorWindowMgr.RemoveRepeateWindow(this);
        EditorWindowMgr.FoucusWindow();
    }
}
