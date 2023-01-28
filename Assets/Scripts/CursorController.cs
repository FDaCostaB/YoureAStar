using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public static CursorController instance;

    public Texture2D loading, normal;
    
    private void Awake()
    {
        instance = this;
    }

    public void SetLoading()
    {
        Cursor.SetCursor(loading, new Vector2(loading.width / 2, loading.height / 2), CursorMode.Auto);
    }

    public void SetNormal()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
    }
}
