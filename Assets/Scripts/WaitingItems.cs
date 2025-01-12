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
        [SerializeField] Puyo _prefabPuyo;
        [SerializeField] Bomb _prefabBomb;
        [SerializeField] WaitingBomb _waitingBomb;
        [SerializeField] SpriteMask _prefabPuyoMask;



        public (Item[] items, bool _isBomb) GetNextItems()
        {
            if (_waitingBomb.IsActive)
            {
                Bomb bomb = GenerateBomb();
                Item[] items = { bomb, null };
                return (items, true);
            }
            else
            {
                Item[] items = _puyos;
                _puyos = GeneratePuyos();
                return (items, false);
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

            Instantiate(_prefabPuyoMask, returnPuyos[0].transform);

            return returnPuyos;
        }

        Bomb GenerateBomb()
        {
            _waitingBomb.UseGauge();
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
        }

    }

}
