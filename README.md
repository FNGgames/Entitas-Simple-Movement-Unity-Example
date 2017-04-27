## Introduction

The finished tutorial project is hosted on github [here](https://github.com/FNGgames/Entitas-Simple-Movement-Unity-Example). This tutorial will take you through the creation of it step-by-step. You can either use this page as a reference to explain how the finished project works, or follow along and create the project yourself from scratch.

As part of this tutorial you will see how to represent game state in Entitas (as components) and how to render that game state using Unity functionality (via systems). You'll also see how to pass Unity user-input into components that other systems can react to and carry out related game logic. Finally you'll implement a very simple AI system that allows entities to carry out movement commands issued by mouse clicks.

If you are brand new to Entitas, you should make sure to go over the [Hello World](https://github.com/sschmid/Entitas-CSharp/wiki/Unity-Tutorial-Hello-World) tutorial before you attempt this one.

## Step 1 - Installation

Start with an empty Unity project (set up for 2D), and add the Entitas library to your assets folder (for organisation, place this in a sub-folder called "Libraries"). Now create a folder called "Generated" for the code Entitas will generate for you and a folder called "Game Code" where you will keep your components and systems. In your Game Code folder, create a folder called "Components" and one called "Systems". 

## Step 2 - Entitas preferences and core file generation

Open the Entitas preferences menu from the Unity editor. Set code generators, data providers and post-processors to "Everything". Check that your target directory for generated code is the same as the Generated folder you created. In this example we will use both the `Input` and `Game` context. In the contexts field write these two names as a comma-separated list. Hit Generate.

Entitas will now generate the core files needed for your project, these include the base `Contexts` and `Feature` classes, which you'll need for System code, and the context attributes you will use when declaring components.

## Step 3 - Components

To represent entity position in space we'll need a `PositionComponent` (we're in 2D so we'll use a Vector2 to store the position). We're also going to represent the entity's direction as a degree value, so we'll need a float `DirectionComponent`. 

*Components.cs*
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

*Components.cs (contd)*
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

*Components.cs (contd)*
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

Finally we have the components from the Input context. We are expecting user input from the mouse, so we'll create components to store the mouse position that we will read from Unity's `Input`class. We want to distinguish between mouse down, mouse up, and mouse pressed (i.e. neither up nor down). We'll also want to distinguish the left from the right mouse buttons. There is only one left mouse button, so we can make use of the `Unique` attribute. 

*Components.cs (contd)*
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

You can save all of these Component definitions in a single file to keep the project simple and organised. In the finished project it is called `Components.cs`. Return to Unity and allow the code to compile. When compiled, hit **Generate** to generate the supporting files for your components. Now we can begin to use those components in our systems.

## Step 4 - View Systems

We need to communicate the game state to the player. We will do this through a series of ReactiveSystems that serve to bridge the gap between the underlying state and the visual representation in Unity. SpriteComponents provide us a link to a particular asset to render to the screen. We will render it using Unity's `SpriteRenderer` class. This requires that we also generate `GameObject`s to hold the `SpriteRenderer`s. This brings us to our first two systems:

### AddViewSystem

The purpose of this system is to identify entities that have a `SpriteComponent` but have not yet been given an associated `GameObject`. We therefore react on the addition of a `SpriteComponent` and filter for `!ViewComponent`. When the system is constructed, we will also create a parent `GameObject` to hold all of the child views. When we create a GameObject we set its parent then we use Entitas' `EntityLink` functionality to create a link between the gameobject and the entity that it belongs to. You'll see the effect of this linking if you open up your Unity heirarchy while your game runs - the view `GameObject`'s inspector pane will show the entity it represents and all of its components right there in the inspector.

*AddViewSystem.cs*
```csharp
using System.Collections.Generic;
using Entitas;
using Entitas.Unity;
using UnityEngine;

public class AddViewSystem : ReactiveSystem<GameEntity>
{
    readonly Transform _viewContainer = new GameObject("Game Views").transform;
    readonly GameContext _context;

    public AddViewSystem(Contexts contexts) : base(contexts.game)
    {
        _context = contexts.game;
    }

    protected override Collector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Sprite);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasSprite && !entity.hasView;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (GameEntity e in entities)
        {
            GameObject go = new GameObject("Game View");
            go.transform.SetParent(_viewContainer, false);
            e.AddView(go);
            go.Link(e, _context);
        }
    }
}
```

### RenderSpriteSystem

With the `GameObject`s in place, we can handle the sprites. This system reacts to the `SpriteComponent` being added, just as the above one does, only this time we filter for only those entities that *have* already had a `ViewComponent` added. If the entity has a `ViewComponent` we know it also has a `GameObject` which we can access and add or replace its `SpriteRenderer`.

*RenderSpriteSystem.cs*
```csharp
using System.Collections.Generic;
using Entitas;
using Entitas.Unity;
using UnityEngine;

public class RenderSpriteSystem : ReactiveSystem<GameEntity>
{
    public RenderSpriteSystem(Contexts contexts) : base(contexts.game)
    {
    }

    protected override Collector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Sprite);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasSprite && entity.hasView;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (GameEntity e in entities)
        {
            GameObject go = e.view.gameObject;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>(e.sprite.name);
        }
    }
}
```

### RenderPositionSystem

Next we want to make sure the position of the `GameObject` is the same as the value of `PositionComponent`. To do this we create a system that reacts to `PositionComponent`. We check in the filter that the entity also has a `ViewComponent`, since we will need to access its `GameObject` to move it.

*RenderPositionSystem.cs*
```csharp
using System.Collections.Generic;
using Entitas;

public class RenderPositionSystem : ReactiveSystem<GameEntity>
{
    public RenderPositionSystem(Contexts contexts) : base(contexts.game)
    {
    }

    protected override Collector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Position);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasPosition && entity.hasView;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (GameEntity e in entities)
        {
            e.view.gameObject.transform.position = e.position.value;
        }
    }
}
```

### RenderDirectionSystem

Finally we want to rotate the GameObject to reflect the value of the `DirectionComponent` of an entity. In this case we react to `DirectionComponent` and filter for `entity.hasView`. The code within the execute block is a simple method of converting degree angles to `Quaternion` rotations which can be applied to Unity `GameObject` `Transform`s.

*RenderDirectionSystem.cs*
```csharp
using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class RenderDirectionSystem : ReactiveSystem<GameEntity>
{
    readonly GameContext _context;

    public RenderDirectionSystem(Contexts contexts) : base(contexts.game)
    {
        _context = contexts.game;
    }

    protected override Collector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Direction);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasDirection && entity.hasView;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (GameEntity e in entities)
        {
            float ang = e.direction.value;
            e.view.gameObject.transform.rotation = Quaternion.AngleAxis(ang - 90, Vector3.forward);
        }
    }
}
```

### ViewSystems (Feature)

We will now put all of these systems inside a `Feature` for organisation. This will give use better visual debugging of the systems in the inspector, and simplify our GameController.

*ViewSystems.cs*
```csharp
using Entitas;

public class ViewSystems : Feature
{
    public ViewSystems(Contexts contexts) : base("View Systems")
    {
        Add(new AddViewSystem(contexts));
        Add(new RenderSpriteSystem(contexts));
        Add(new RenderPositionSystem(contexts));
        Add(new RenderDirectionSystem(contexts));
    }
}
```

## Step 5 - Movement Systems

We will now write a simple system to get AI entities to move to supplied target locations automatically. The desired behaviour is that the entity will turn to face the supplied movement target and then move towards it at a constant speed. When it reaches the target it should stop, and it's movement order should be removed.

We will acheive this with an Execute system that runs every frame. We can keep an up to date list of all the GameEntities that have a `MoveComponent` using the Group functionality. We'll set this up in the constructor then keep a readonly reference to the group in the system for later use. We can get the list of entities by calling `group.GetEntities()`.

We use the `MoveCompleteComponent` as a flag to show that the movement was completed by actually reaching a target. If we didn't use this flag, we could use `GroupEvent.Removed` instead, and remove the need for the extra flag component, but this leaves us no ability to distinguish between a successfully completed movement and a prematurely cancelled movement. In either case, we will not react to completed moves in this example, but if we set it up like this, you can choose to add that functionality later on without needing to come back and alter this code.

The `Execute()` method will take every entity with a `PositionComponent` and a `MoveComponent` and adjust it's position by a fixed amount in the direction of its move target. If the entity is within range of the target, the move is considered complete. We will also cleaup all the `MoveCompleteComponent`s during the cleanup phase of the system (which runs after every system has finished its execute phase). The cleanup part ensures that `MoveCompleteComponent` only flags entities that have completed their movement within *one frame* and not ones who finished a long time ago and who are waiting for new movement commands.

*MoveSystem.cs*
```csharp
using Entitas;
using UnityEngine;

public class MoveSystem : IExecuteSystem, ICleanupSystem
{
    readonly IGroup<GameEntity> _moves;
    readonly IGroup<GameEntity> _moveCompletes;
    const float _speed = 4f;

    public MoveSystem(Contexts contexts)
    {
        _moves = contexts.game.GetGroup(GameMatcher.Move);
        _moveCompletes = contexts.game.GetGroup(GameMatcher.MoveComplete);
    }

    public void Execute()
    {
        foreach (GameEntity e in _moves.GetEntities())
        {
            Vector2 dir = e.move.target - e.position.value;
            Vector2 newPosition = e.position.value + dir.normalized * _speed * Time.deltaTime;
            e.ReplacePosition(newPosition);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            e.ReplaceDirection(angle);

            float dist = dir.magnitude;
            if (dist <= 0.5f)
            {
                e.RemoveMove();
                e.isMoveComplete = true;
            }
        }
    }

    public void Cleanup()
    {
        foreach (GameEntity e in _moveCompletes.GetEntities())
        {
            e.isMoveComplete = false;
        }
    }
}
```

*MovementSystems,cs (feature)*
```csharp
using Entitas;

public class MovementSystems : Feature
{
    public MovementSystems(Contexts contexts) : base("Movement Systems")
    {
        Add(new MoveSystem(contexts));
    }
}
```






