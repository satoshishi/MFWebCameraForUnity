using UnityEngine;
using UnityEngine.UI;
using MF;

public class SetTexture : MonoBehaviour
{
    [SerializeField]
    private MediaFoundationWebCamera webcamera;

    public RawImage image;

    public void Handle()
    {
        image.texture = webcamera.Texture;
    }
}
