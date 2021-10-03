using UnityEngine;

[CreateAssetMenu(fileName = "CursorManager", menuName = "Cursor Manager", order = 0)]
public class CursorManager : ScriptableObject
{
    [SerializeField] private Texture2D normal, hover, grab;
    
    public void Hover()
    {
        Cursor.SetCursor(hover, Vector2.zero, CursorMode.Auto);
    }

    public void Grab()
    {
        Cursor.SetCursor(grab, Vector2.zero, CursorMode.Auto);
    }

    public void Normal()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
    }
}