using UnityEngine;
using System.Collections;

public class DiomondsController : MonoBehaviour
{
    #region ComputeShader相关属性
        public ComputeShader shader;
        public int texResolution = 1024;
        public Color clearColor = new Color(); 
        public Color circleColor = new Color(); 
        public int count = 10;
    #endregion
    
    Renderer rend;
    RenderTexture outputTexture; 

    int circlesHandle; 
    int clearHandle;
    
    ComputeBuffer buffer;
    
    struct Circle
    {
        public Vector2 origin;
        public Vector2 velocity;
        public float radius;
    }
    Circle[] circleData; 
    
    

    #region 周期
        void Start()
        {
            outputTexture = new RenderTexture(texResolution, texResolution, 0);
            outputTexture.enableRandomWrite = true;
            // outputTexture.useMipMap = true;
            // outputTexture.autoGenerateMips = true;
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

    #region MyRegion
        private void InitData()
        {
            circlesHandle = shader.FindKernel("Circles");

            uint threadGroupSizeX;
            
            shader.GetKernelThreadGroupSizes(circlesHandle, out threadGroupSizeX, out _, out _);

            int total = (int)threadGroupSizeX * count;
            circleData = new Circle[total];

            float speed = 100;
            float halfSpeed = speed * 0.5f;
            float minRadius = 5.0f;
            float maxRadius = 15.0f;
            float radiusRange = maxRadius - minRadius;

            for (int i = 0; i < total; i++)
            {
                Circle circle = circleData[i];
                circle.origin.x = Random.value * texResolution; 
                circle.origin.y = Random.value * texResolution; 
                circle.velocity.x = (Random.value * speed) - halfSpeed;
                circle.velocity.y = (Random.value * speed) - halfSpeed;
                circle.radius = Random.value * radiusRange + minRadius;
                circleData[i] = circle; 
            }
        }
        
        private void InitShader()
        {
            clearHandle = shader.FindKernel("Clear");
            
            shader.SetVector("clearColor", clearColor);
            shader.SetVector("circleColor", circleColor);
            shader.SetInt("texResolution", texResolution);


            int stride = (2 + 2 + 1) * 4;
            buffer = new ComputeBuffer(circleData.Length, stride);
            buffer.SetData(circleData); 
            shader.SetBuffer(circlesHandle, "circlesBuffer", buffer);

            shader.SetTexture(circlesHandle, "Result", outputTexture);
            shader.SetTexture(clearHandle, "Result", outputTexture);

            rend.material.SetTexture("_MainTex", outputTexture);
        }

        private void DispatchKernel(int count)
        {
            shader.Dispatch(clearHandle, texResolution / 8, texResolution / 8, 1);
            shader.SetFloat("time", Time.time);
            shader.Dispatch(circlesHandle, count, 1, 1);
        }
    

    #endregion



}