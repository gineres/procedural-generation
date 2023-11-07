using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender; // Vai colocar o plano aqui, eh a superficie de onde vai ficar a textura

    public void DrawNoiseMap(float[,] noiseMap){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height]; // Matriz representada em 1 dimensao
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]); // Operacao para acessar array de 1 dimensao
            }
        }

        texture.SetPixels(colourMap); // Aplicando cores geradas
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3 (width, 1, height); // Altera a escala do plano!! Kool!
    }
}
