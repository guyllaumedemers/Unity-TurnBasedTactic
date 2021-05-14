using System;
using ScriptableObjects.Items;
using UnityEngine.Events;

[Serializable]
public class IntUnityEvent: UnityEvent<int> { }

public class IntIntItemUnityEvent: UnityEvent<int, int, Item> { }
