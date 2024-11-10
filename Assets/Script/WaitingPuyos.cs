// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

public class WaitingPuyos : MonoBehaviour
{
    PuyoController[] _puyoControllers = new PuyoController[2];
    [SerializeField] PuyoController _prefabPuyo;

    public PuyoController[] GetNextPuyos()
    {
        PuyoController[] returnPuyos = _puyoControllers;
        _puyoControllers = GeneratePuyos();
        return returnPuyos;
    }

    PuyoController[] GeneratePuyos()
    {
        PuyoController[] returnPuyos = new PuyoController[2];

        returnPuyos[0] = Instantiate(_prefabPuyo, transform);
        returnPuyos[1] = Instantiate(_prefabPuyo, transform);
        returnPuyos[1].transform.SetPositionAndRotation(transform.position + Vector3.up, Quaternion.identity);

        int len = System.Enum.GetValues(typeof(PuyoType)).Length;
        returnPuyos[0].PuyoType = (PuyoType)Random.Range(1, len);
        returnPuyos[1].PuyoType = (PuyoType)Random.Range(1, len);

        return returnPuyos;
    }

    private void Awake()
    {
        foreach (Transform n in this.transform)
        {
            GameObject.Destroy(n.gameObject);
        }

        _puyoControllers = GeneratePuyos();
    }

}
