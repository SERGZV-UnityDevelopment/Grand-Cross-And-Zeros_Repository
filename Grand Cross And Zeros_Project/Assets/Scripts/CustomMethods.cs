using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMethods
{

	
}

public static class ConvertToSpriteExtensiton                           // This class allows you to convert a 2d texture to a sprite.
{
    public static Sprite ConvertToSprite(this Texture2D texture)        // This method converts texture 2d to Sprite.
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero); // Convert 2DTexture to Sprite
    }
}
