using UnityEngine;
using System.Collections.Generic;

public static class TransformExtension
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var ts = queue.Dequeue();
            if (ts.name == name)
                return ts;
            foreach (Transform t in ts)
                queue.Enqueue(t);
        }

        return null;
    }
}