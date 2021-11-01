﻿using UnityEngine;
using System.Collections;
using Mirror;

public class DestroyObject : NetworkBehaviour
{

    public float timeOut = 10.0f;
    public bool detachChildren = false;
    public bool isParticle = false;

    public void Awake()
    {
        //if (!photonView.isMine) {
        //	return;
        //}

        if (isParticle)
        {
            ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
            timeOut = ps.duration;
        }

        Invoke("DestroyNow", timeOut);
    }

    public void DestroyNow()
    {
        if (detachChildren)
        {
            transform.DetachChildren();
        }

        //PhotonNetwork.Destroy(gameObject);
    }
}
