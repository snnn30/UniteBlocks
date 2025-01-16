// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace UniteBlocks
{
    public class WaitingItems : MonoBehaviour
    {
        private Block[] m_Blocks = new Block[2];

        [SerializeField]
        private Block m_PrefabBlock;

        [SerializeField]
        private Bomb m_PrefabBomb;

        [SerializeField]
        private WaitingBomb m_WaitingBomb;

        [SerializeField]
        private SpriteMask m_PrefabBlockMask;

        [SerializeField]
        private BlockColorTable m_BlockColorTable;

        private void Awake()
        {
            // 位置調整用に作っているエディタ上のオブジェクトを破棄
            foreach (Transform n in this.transform)
            {
                GameObject.Destroy(n.gameObject);
            }

            m_Blocks = GenerateBlocks();
        }

        public (Item[] items, bool isBomb) GetNextItems()
        {
            if (m_WaitingBomb.IsActive)
            {
                Bomb bomb = GenerateBomb();
                Item[] items = { bomb, null };
                return (items, true);
            }
            else
            {
                Item[] items = m_Blocks;
                m_Blocks = GenerateBlocks();
                return (items, false);
            }
        }

        Block[] GenerateBlocks()
        {
            Block[] returnBlocks = new Block[2];

            returnBlocks[0] = Instantiate(m_PrefabBlock, transform);
            returnBlocks[1] = Instantiate(m_PrefabBlock, transform);
            returnBlocks[1].transform.SetPositionAndRotation(transform.position + Vector3.up, Quaternion.identity);

            int len = m_BlockColorTable.Colors.Count;
            returnBlocks[0].Color = m_BlockColorTable.Colors[Random.Range(0, len)];
            returnBlocks[1].Color = m_BlockColorTable.Colors[Random.Range(0, len)];

            Instantiate(m_PrefabBlockMask, returnBlocks[0].transform);

            return returnBlocks;
        }

        Bomb GenerateBomb()
        {
            m_WaitingBomb.UseGauge();
            Bomb returnBomb = Instantiate(m_PrefabBomb, transform);
            return returnBomb;
        }

    }

}
