using UnityEngine;
using UnityEngine.UI;
using MF;

public class SetTexture : MonoBehaviour
{
    [SerializeField]
    private MediaFoundationWebCamera webcamera;

    public RawImage image;

    public async void Start()
    {
        RenderTexture texture = await webcamera.GetTextureAsync();
        image.texture = texture;
        image.SetNativeSize();
    }
}
