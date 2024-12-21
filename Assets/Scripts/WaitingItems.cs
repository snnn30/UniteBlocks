// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Items;
using UnityEngine;

namespace Board
{
    public class WaitingItems : MonoBehaviour
    {
        Puyo[] _puyos = new Puyo[2];
        Bomb _bomb;
        bool _isBomb = false;
        [SerializeField] Puyo _prefabPuyo;
        [SerializeField] Bomb _prefabBomb;



        public (Item[] items, bool _isBomb) GetNextItems()
        {
            if (_isBomb)
            {
                Item[] items = { _bomb, null };
                _bomb = GenerateBomb();
                return (items, _isBomb);
            }
            else
            {
                Item[] items = _puyos;
                _puyos = GeneratePuyos();
                return (items, _isBomb);
            }
        }

        Puyo[] GeneratePuyos()
        {
            Puyo[] returnPuyos = new Puyo[2];

            returnPuyos[0] = Instantiate(_prefabPuyo, transform);
            returnPuyos[1] = Instantiate(_prefabPuyo, transform);
            returnPuyos[1].transform.SetPositionAndRotation(transform.position + Vector3.up, Quaternion.identity);

            int len = System.Enum.GetValues(typeof(PuyoType)).Length;
            returnPuyos[0].PuyoType = (PuyoType)Random.Range(1, len);
            returnPuyos[1].PuyoType = (PuyoType)Random.Range(1, len);

            return returnPuyos;
        }

        Bomb GenerateBomb()
        {
            Bomb returnBomb = Instantiate(_prefabBomb, transform);
            return returnBomb;
        }

        private void Awake()
        {
            foreach (Transform n in this.transform)
            {
                GameObject.Destroy(n.gameObject);
            }

            _puyos = GeneratePuyos();
            _bomb = GenerateBomb();
        }

    }

}
