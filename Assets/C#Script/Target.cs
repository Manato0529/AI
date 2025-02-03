using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int priority;
    public bool isExplored = false;  // 探索済みフラグ
    SphereCollider sphereCollider;
    Color startColor;
    public MeshRenderer meshRenderer;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        startColor=meshRenderer.material.color;
    }

    private void Update()
    {
        if (!isExplored)
        {
            sphereCollider.isTrigger = false;
            meshRenderer.material.color = startColor;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isExplored = true;
        sphereCollider.isTrigger = true;
        meshRenderer.material.color = Color.gray;
    }
}
