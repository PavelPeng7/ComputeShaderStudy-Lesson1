using UnityEngine;
using System.Collections;

public class Tex2DBaseRenderer : MonoBehaviour
{
    #region Compute Shader Properties

    public ComputeShader shader;
    public int texResolution = 1024;

    #endregion

    #region Runtime Properties

    Renderer rend;
    RenderTexture outputTexture;

    int kernelHandle;

    #endregion

    #region Core Methods

    void Start() {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.useMipMap = true;
        outputTexture.autoGenerateMips = false;
        outputTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        InitShader();
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.U)) {
            DispatchShader(texResolution / 8, texResolution / 8, 1);
        }
    }

    #endregion

    #region Compute Shader Methods

    private void InitShader() {
        kernelHandle = shader.FindKernel("DrawTex2D");
        shader.SetTexture(0, "Result", outputTexture);
        shader.SetInt("_TexResolution", texResolution);

        rend.material.SetTexture("_MainTex", outputTexture);
        DispatchShader(texResolution / 8, texResolution / 8, 1);
        outputTexture.GenerateMips();
    }

    private void DispatchShader(int x, int y, int z) {
        shader.Dispatch(kernelHandle, x, y, z);
    }

    #endregion
}