using System.IO;
using System.Reflection;
using UnityEngine;

namespace SunkenCompass;

public class Utilities
{
    private static byte[] ReadEmbeddedFileBytes(string name)
    {
        using MemoryStream stream = new();
        Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + "." + name)!.CopyTo(stream);
        return stream.ToArray();
    }

    internal static Texture2D LoadTexture(string name)
    {
        Texture2D texture = null!;
        switch (name)
        {
            case "mask.png":
            case "compass.png":
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
                break;
            default:
                texture =  new Texture2D(0, 0, TextureFormat.RGBA32, true, true);
                break;
        }

        texture.LoadImage(ReadEmbeddedFileBytes("images." + name));


        return texture!;
    }

    internal static Sprite LoadSprite(string name)
    {
        Texture2D texture = LoadTexture(name);
        if (texture != null)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        return null!;
    }
}