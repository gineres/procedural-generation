using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender; // Vai colocar o plano aqui, eh a superficie de onde vai ficar a textura
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture){
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height); // Altera a escala do plano!! Kool!
    }

    public void DrawMesh(MeshData meshData, Texture2D texture){
        meshFilter.sharedMesh = meshData.CreateMesh(); // Shared para o caso de gerar a mesh fora do modo gameplay
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
