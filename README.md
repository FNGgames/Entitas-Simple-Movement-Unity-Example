# Introduction

The finished tutorial project is hosted on github [here](https://github.com/FNGgames/Entitas-Simple-Movement-Unity-Example). This tutorial will take you through the creation of it step-by-step. You can either use this page as a reference to explain how the finished project works, or follow along and create the project yourself from scratch.

As part of this tutorial you will see how to represent game state in Entitas (as components) and how to render that game state using Unity functionality (via systems). You'll also see how to pass Unity user-input into components that other systems can react to and carry out related game logic. Finally you'll implement a very simple AI system that allows entities to carry out movement commands issued by mouse clicks.

# Step 1 - Installation

Start with an empty Unity project (set up for 2D), and add the Entitas library to your assets folder (for organisation, place this in a sub-folder called "Libraries"). Now create a folder called "Generated" for the code Entitas will generate for you and a folder called "Game Code" where you will keep your components and systems. In your Game Code folder, create a folder called "Components" and one called "Systems". 

# Step 2 - Entitas preferences and core file generation

Open the Entitas preferences menu from the Unity editor. Set code generators, data providers and post-processors to "Everything". Check that your target directory for generated code is the same as the Generated folder you created. In this example we will use both the `Input` and `Game` context. In the contexts field write these two names as a comma-separated list. Hit Generate.

Entitas will now generate the core files needed for your project, these include the base `Contexts` and `Feature` classes, which you'll need for System code, and the context attributes you will use when declaring components.

# Step 3 - Components

To represent entity position in space we'll need a `PositionComponent` (we're in 2D so we'll use a Vector2 to store the position). We're also going to represent the entity's direction as a degree value, so we'll need a float `DirectionComponent`. 

```csharp
[Game]
public class PositionComponent : IComponent
{
    public Vector2 value;
}

[Game]
public class DirectionComponent : IComponent
{
    public float value;
}
```

We will also want to render our entities to screen. We'll do this with Unity's `SpriteRenderer`, but we will also need a Unity `GameObject` to hold the `SpriteRenderer`. We'll need two more components, a `ViewComponent` for the `GameObject` and a `SpriteComponent` which will store the name of the sprite we want to display.

```csharp
[Game]
public class ViewComponent : IComponent
{
    public GameObject gameObject;
}

[Game]
public class SpriteComponent : IComponent
{
    public string name;
}
```

We're going to move some of our entities, so we'll create a flag component to indicate entities that can move ("movers"). We'll also need a component to hold the movement target location and another flag to indicate that the movement has completed successfully. 

```csharp
[Game]
public class MoverComponent : IComponent
{
}

[Game]
public class MoveComponent : IComponent
{
    public Vector2 target;
}

[Game]
public class MoveCompleteComponent : IComponent
{
}
```

Finally we have the components from the Input context. We are expecting user input from the mouse, so we'll create components to store the mouse position that we will read from Unity's `Input`class. We want to distinguish between mouse down, mouse up, and mouse pressed (i.e. neither up nor down). We'll also want to distinguish the left mous from the right mouse buttons. There is only one left mouse button, so we can make use of the `Unique` attribute. 

```csharp
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
```
