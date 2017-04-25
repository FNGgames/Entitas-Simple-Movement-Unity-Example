using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

// Game Components
[Game]
public class PositionComponent : IComponent
{
    public Vector2 value;
}

[Game]
public class MoveComponent : IComponent
{
    public Vector2 target;
}

[Game]
public class MoverComponent : IComponent
{
}

[Game]
public class MoveCompleteComponent : IComponent
{
}

[Game]
public class DirectionComponent : IComponent
{
    public float value;
}

[Game]
public class SpriteComponent : IComponent
{
    public string name;
}

[Game]
public class ViewComponent : IComponent
{
    public GameObject gameObject;
}


// Input Components
[Input, Unique]
public class LeftMouseComponent : IComponent
{
}

[Input, Unique]
public class RightMouseComponent : IComponent
{
}

[Input]
public class MouseDownComponent : IComponent
{
    public Vector2 position;
}

[Input]
public class MousePositionComponent : IComponent
{
    public Vector2 position;
}

[Input]
public class MouseUpComponent : IComponent
{
    public Vector2 position;
}
