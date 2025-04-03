using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace MF
{
    public class MediaFoundationWebCamera : MonoBehaviour
    {
        [DllImport("CameraCaptureDLL")]
        private static extern int InitializeCamera([MarshalAs(UnmanagedType.LPWStr)] string instanceId);

        [DllImport("CameraCaptureDLL")]
        private static extern bool GetLatestFrame(int handle, IntPtr buffer, int bufferSize, out int width, out int height);

        [DllImport("CameraCaptureDLL")]
        private static extern void ShutdownCamera(int handle);

        [DllImport("CameraCaptureDLL")]
        private static extern IntPtr GetStatus(int handle);

        [Header("WMI形式 InstanceId(USB\\VID_...)")]
        [SerializeField]
        private string instanceId;

        [Header("WebCameraの縦の解像度")]
        [SerializeField]
        private int width = 1280;

        [Header("WebCameraの横の解像度")]
        [SerializeField]
        private int height = 720;

        [Header("ComputeShader(RGBA32描画)")]
        [SerializeField]
        private ComputeShader cameraShader;

        public RenderTexture Texture { get; private set; }

        private int handle = 0;

        private uint[] frameBuffer;

        private GCHandle frameHandle;

        private IntPtr bufferPtr;

        private ComputeBuffer computeBuffer;

        private bool frameReady = false;

        private readonly object frameLock = new();

        private void Start()
        {
            InitCamera();
            InitBuffers(width, height);
            StartFrameLoop();
        }

        private void Update()
        {
            Rendering();
        }

        /// <summary>
        /// instanceidをmediafoundation用の形式に変換してネイティブ側で使用するカメラを登録する
        /// ネイティブ側の戻り値としてそのカメラ制御に紐づくhandle indexをここで取得する
        /// </summary>
        private void InitCamera()
        {
            string normalized = instanceId.Replace("USB\\", "").Replace("\\", "#").ToLower();
            handle = InitializeCamera(normalized);

            if (handle == 0)
            {
                Debug.LogError("InitializeCamera failed: " + Marshal.PtrToStringUni(GetStatus(0)));
                return;
            }
        }

        /// <summary>
        /// ネイティブ側からのフレーム取得処理を別スレッドで実行
        /// </summary>
        /// <returns></returns>
        private async void StartFrameLoop()
        {
            while (true)
            {
                if (handle != 0)
                {
                    await Task.Run(() => FetchFrame());
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// ネイティブ側からフレームを取得する
        /// 取得したフレームの解像度が変化している場合は、メインスレッドに戻して解像度を更新する
        /// フレームの取得に成功したら、Update上で描画する
        /// </summary>
        private void FetchFrame()
        {
            if (GetLatestFrame(handle, bufferPtr, frameBuffer.Length * sizeof(uint), out int w, out int h))
            {
                if (w != width || h != height)
                {
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        InitBuffers(w, h);
                    });
                    return;
                }

                lock (frameLock)
                {
                    frameReady = true;
                }
            }
        }

        /// <summary>
        /// ネイティブ側から取得したフレームをComputeShaderで処理してRenderTextureに描画する
        /// </summary>
        private void Rendering()
        {
            if (!frameReady || computeBuffer == null)
            {
                return;
            }

            lock (frameLock)
            {
                computeBuffer.SetData(frameBuffer);
                frameReady = false;
            }

            int kernel = cameraShader.FindKernel("CSMain");
            cameraShader.SetInt("Width", width);
            cameraShader.SetInt("Height", height);
            cameraShader.SetBuffer(kernel, "Input", computeBuffer);
            cameraShader.SetTexture(kernel, "Result", Texture);

            int tgx = Mathf.CeilToInt(width / 8f);
            int tgy = Mathf.CeilToInt(height / 8f);
            cameraShader.Dispatch(kernel, tgx, tgy, 1);
        }

        /// <summary>
        /// テクスチャ、フレームバッファサイズ等を解像度に応じて初期化する
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void InitBuffers(int w, int h)
        {
            width = w;
            height = h;

            frameBuffer = new uint[width * height];
            if (frameHandle.IsAllocated)
            {
                frameHandle.Free();
            }

            frameHandle = GCHandle.Alloc(frameBuffer, GCHandleType.Pinned);
            bufferPtr = frameHandle.AddrOfPinnedObject();

            Texture?.Release();
            Texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true
            };
            _ = Texture.Create();

            computeBuffer?.Release();
            computeBuffer = new ComputeBuffer(width * height, sizeof(uint));
        }

        private void OnDestroy()
        {
            ShutdownCamera(handle);
            if (frameHandle.IsAllocated)
            {
                frameHandle.Free();
            }

            computeBuffer?.Release();
            Texture?.Release();
        }
    }
}