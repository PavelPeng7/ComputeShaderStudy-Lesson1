using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class DiomondsController : MonoBehaviour
{
    #region ComputeShader Properties
        public ComputeShader shader;
        public int texResolution = 1024;
        public Color clearColor = new Color(); 
        public Color diomondsColor = new Color(); 
        public int count = 10;
        struct Diomonds
        {
            public Vector2 origin;
            public Vector2 velocity;
            public float radius;
        }
        Diomonds[] diomondsData; 
    #endregion

    #region Runtime Properties
        Renderer rend;
        RenderTexture outputTexture; 

        int diomondsHandle; 
        int clearHandle;
        
        ComputeBuffer buffer;
    #endregion

    #region Core Methods
        void Start()
        {
            outputTexture = new RenderTexture(texResolution, texResolution, 0);
            outputTexture.enableRandomWrite = true;
            outputTexture.wrapMode = TextureWrapMode.Repeat;
            outputTexture.Create();

            rend = GetComponent<Renderer>();
            rend.enabled = true;
            
            InitData();
            InitShader();
        }
        
        void Update()
        {
            DispatchKernel(10); 
        }
        
        private void OnDestroy()
        {
            buffer.Dispose();
        }
    #endregion

    #region Compute Shader Methods
        private void InitData()
        {
            diomondsHandle = shader.FindKernel("Diomonds");

            uint threadGroupSizeX;
            
            shader.GetKernelThreadGroupSizes(diomondsHandle, out threadGroupSizeX, out _, out _);

            int total = (int)threadGroupSizeX * count;
            diomondsData = new Diomonds[total];

            float speed = 100;
            float halfSpeed = speed * 0.5f;
            float minRadius = 5.0f;
            float maxRadius = 10.0f;
            float radiusRange = maxRadius - minRadius;

            for (int i = 0; i < total; i++)
            {
                Diomonds diomonds = diomondsData[i];
                diomonds.origin.x = Random.value * texResolution; 
                diomonds.origin.y = Random.value * texResolution; 
                diomonds.velocity.x = (Random.value * speed) - halfSpeed;
                diomonds.velocity.y = (Random.value * speed) - halfSpeed;
                diomonds.radius = Random.value * radiusRange + minRadius;
                diomondsData[i] = diomonds; 
            }
        }
        
        private void InitShader()
        {
            clearHandle = shader.FindKernel("Clear");
            
            shader.SetVector("clearColor", clearColor);
            shader.SetVector("diomondsColor", diomondsColor);
            shader.SetInt("texResolution", texResolution);
            
            int stride = (2 + 2 + 1) * 4;
            buffer = new ComputeBuffer(diomondsData.Length, stride);
            buffer.SetData(diomondsData); 
            shader.SetBuffer(diomondsHandle, "diomondsBuffer", buffer);

            shader.SetTexture(diomondsHandle, "Result", outputTexture);
            shader.SetTexture(clearHandle, "Result", outputTexture);

            rend.material.SetTexture("_MainTex", outputTexture);
        }

        private void DispatchKernel(int count)
        {
            shader.Dispatch(clearHandle, texResolution / 8, texResolution / 8, 1);
            shader.SetFloat("time", Time.time);
            shader.Dispatch(diomondsHandle, count, 1, 1);
        }
    

    #endregion
}