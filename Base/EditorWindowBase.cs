using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器窗口基类
/// </summary>
public class EditorWindowBase : EditorWindow
{
    /// <summary>
    /// 界面层级
    /// <para>根据层级优先级访问界面焦点</para>
    /// </summary>
    public int Priority { get; set; }

    //重写OnFocus方法，自动排序聚焦
    private void OnFocus()
    {
        EditorWindowMgr.FoucusWindow();
    }
}
