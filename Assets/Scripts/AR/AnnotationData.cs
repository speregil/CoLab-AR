using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class AnnotationData : NetworkBehaviour
{
    private NetworkVariable<FixedString128Bytes> annotation = new NetworkVariable<FixedString128Bytes>();

    private GameObject roomCamera;
    private TMP_Text annotationText;

    public override void OnNetworkSpawn()
    {
        roomCamera = Camera.main.gameObject;
        annotationText = transform.GetChild(0).GetComponentInChildren<TMP_Text>();
        annotation.OnValueChanged += OnAnnotationChanged;
    }

    void Update()
    {
        if (roomCamera != null)
            transform.LookAt(roomCamera.transform);
    }

    public void UpdateAnnotation(string annotation)
    {
        if (IsServer)
        {
            this.annotation.Value = new FixedString128Bytes(annotation);
        }
    }

    public string GetModelID()
    {
        return annotation.Value.ToString();
    }

    public void OnAnnotationChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        annotationText.text = newValue.ToString();
    }
}