using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using UnityEditor;

public class MapLoader : MonoBehaviour
{
    public Sprite sourceMap;
    public string mapPath;
    
    // Start is called before the first frame update
    void Start()
    {
        ReadPNGandWriteText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReadPNGandWriteText()
    {
        byte[] imageBytes = File.ReadAllBytes(mapPath);
        Texture2D image = new Texture2D(2, 2);
        image.LoadImage(imageBytes);

        using (StreamWriter writer = new StreamWriter(mapPath))
        {
            for (int x = 0; x < image.height; x++)
            {
                for (int y = 0; y < image.width; y++)
                {
                    UnityEngine.Color pixel = image.GetPixel(x, y);
                    if (pixel.r > 0.5 && pixel.g < 0.5 && pixel.b < 0.5)
                    {
                        writer.Write("#");
                    }
                    else if (pixel.r < 0.5 && pixel.g > 0.5 && pixel.b < 0.5)
                    {
                        writer.Write("$");
                    }
                    else if (pixel.r < 0.5 && pixel.g < 0.5 && pixel.b > 0.5)
                    {
                        writer.Write("%");
                    }
                }
                writer.WriteLine();
            }
            writer.Write(";");
        }
    }
}
