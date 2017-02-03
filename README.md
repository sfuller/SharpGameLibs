# SharpGameLibs

## Summary
**SharpGameLibs** is a C#/.net framework geared towards game development. The design of this library
is heavily influenced by the hits and misses I have encountered throughout my game development carrer.

Please note that this library is very young and will likely change quite a bit in the near future.

## Components
**SharpGameLibs** is made up of different components which can be used independently.

* **SFuller.SharpGameLibs.**
  * **Core.** - Engine independent components
    * **IOC** - Provides inversion of control + dependency resolution.
    * **GameState** - Provides a state machine for transitions between diferent game states
    * **ViewManagement** - Provides a firewall between game logic and engine specific view logic.
  * **Unity.** - Components for use in Unity engine.
    * **ViewManagement** - A ViewManager implementation for Unity. View interfaces are bound to 
      prefabs with a view implementation attached.

## Contributing
Contributions and feedback are greatly appreciated. 
