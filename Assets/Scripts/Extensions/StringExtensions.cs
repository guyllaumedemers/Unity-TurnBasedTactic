using System;
using System.Linq;
using UnityEngine;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string source) =>
        source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            "" => throw new ArgumentException($"{nameof(source)} cannot be empty", nameof(source)),
            _ => source.First().ToString().ToUpper() + source.Substring(1)
        };
    
    public static bool Contains(this string source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;
    
    public static string AddColor(this string text, Color col) => $"<color={ColorHexFromUnityColor(col)}>{text}</color>";
    
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";
}