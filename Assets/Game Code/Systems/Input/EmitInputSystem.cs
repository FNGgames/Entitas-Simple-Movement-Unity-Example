using Entitas;
using UnityEngine;

public class EmitInputSystem : IInitializeSystem, IExecuteSystem
{
    readonly InputContext _context;
    private InputEntity _leftMouseEntity;
    private InputEntity _rightMouseEntity;

    public EmitInputSystem(Contexts contexts)
    {
        _context = contexts.input;
    }

    public void Initialize()
    {
        // initialise the unique entities that will hold the mousee button data
        _context.isLeftMouse = true;
        _leftMouseEntity = _context.leftMouseEntity;

        _context.isRightMouse = true;
        _rightMouseEntity = _context.rightMouseEntity;
    }

    public void Execute()
    {
        // mouse position
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // left mouse button
        if (Input.GetMouseButtonDown(0))
            AddOrReplaceMouseDown(_leftMouseEntity, mousePosition);
        
        if (Input.GetMouseButton(0))
            AddOrReplaceMousePosition(_leftMouseEntity, mousePosition);
        
        if (Input.GetMouseButtonUp(0))
            AddOrReplaceMouseUp(_leftMouseEntity, mousePosition);
        

        // left mouse button
        if (Input.GetMouseButtonDown(1))
            AddOrReplaceMouseDown(_rightMouseEntity, mousePosition);
        
        if (Input.GetMouseButton(1))
            AddOrReplaceMousePosition(_rightMouseEntity, mousePosition);
        
        if (Input.GetMouseButtonUp(1))
            AddOrReplaceMouseUp(_rightMouseEntity, mousePosition);
        
    }

    // Add or replace the components
    private void AddOrReplaceMouseDown(InputEntity e, Vector2 mousePosition)
    {
        if (e.hasMouseDown)
            e.ReplaceMouseDown(mousePosition);
        else
            e.AddMouseDown(mousePosition);
    }

    private void AddOrReplaceMousePosition(InputEntity e, Vector2 mousePosition)
    {
        if (e.hasMousePosition)
            e.ReplaceMousePosition(mousePosition);
        else
            e.AddMousePosition(mousePosition);
    }

    private void AddOrReplaceMouseUp(InputEntity e, Vector2 mousePosition)
    {
        if (e.hasMouseUp)
            e.ReplaceMouseUp(mousePosition);
        else
            e.AddMouseUp(mousePosition);
    }
}
