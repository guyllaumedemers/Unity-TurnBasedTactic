using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Flow
{
    public abstract void PreInitialize();
    public abstract void Initialize();
    public abstract void Refresh();
}
    