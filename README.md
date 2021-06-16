# Unity-TurnBasedTactic

Welcome to Unity-TurnBasedTactic, a group project made in the academic field based on the classic : Final Fantasy Tactics.</br>
Useful scripts are located in Assets/Scripts/..

## Getting Started

This repository has for objective of showcasing what could be achieve with a group of four individuals under a short period of time.
Some of the features presented here are obviously not mine and I won't take credit for them. __A detail list of the features I worked on will follow.__

```
NOTE : This project is currently a broken version of itselft as we used un-authorized asset to ease the development process.
```

👉 A* Pathfinding</br>
👉 Odin Inspector

## Content

TurnBased Tactic is working with an __Event Based System__. Our objective was to prioritize scalability of the system relying on implementing the __Observer Pattern__
and avoiding referencing static values which would have involved major coupling over the project.

```
NOTE : Scripts created by myself can be found here.
```

* [Assets/Scripts/Dialogue](https://github.com/guyllaumedemers/Unity-TurnBasedTactic/tree/master/Assets/Scripts/Dialogue) : Dialogue System
* [Assets/Scripts/UI](https://github.com/guyllaumedemers/Unity-TurnBasedTactic/tree/master/Assets/Scripts/UI) : Action System 👉 *can be found under the name ActionMenu*
```
NOTE : UI interactions are broken down into several scipts. PlayerHUD is at the core of the UI System and is managed thru the UI Manager.
```
* [Assets/Scripts/Managers](https://github.com/guyllaumedemers/Unity-TurnBasedTactic/tree/master/Assets/Scripts/Managers) : All Managers  👉 *Event Based Script can be found under the name Battle Turn Manager and works in conjunction with the Input Manager who invoke his delegates during unit selection phase*
* [Assets/Scripts/Utilitis](https://github.com/guyllaumedemers/Unity-TurnBasedTactic/tree/master/Assets/Scripts/Utilities) : Custom Tweening lib

#### Game Mechanics and Features

* Grid System
* Tile Rules (*for map generation*)
* A*
* Input System
* Action System
* Battle Turn Management System 👉 *Event Based driven*
* AI
* Inventory System (*made with Odin inspector)
* Buffing System
* UI Management 👉 *Custom Tweening lib*
* Audio Management
* Animation Management
* Dialogue System 👉 *Using Dialogue behaviour*
* Level Management / Scene Loading

#### Design Pattern and Memory Optimization

* Observer Pattern
* Texture Loading

##### Contribution

* Action System
* Battle Turn Management (*Event Based driven*)
* UI Management
* Tweening lib
* Audio Management
* Dialogue System (*using Dialogue behaviour*)

## Resources

💬 References for patterns are given from : [Design Patterns: Elements of Reusable Object‑Oriented Software]()
