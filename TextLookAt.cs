using System.Drawing;
using UnityEngine;

public class TextLookAt : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        transform.rotation = mainCamera.transform.rotation;
    }
}
