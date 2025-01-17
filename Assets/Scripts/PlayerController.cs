// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniteBlocks
{
    public enum Direction
    {
        Up, Right, Down, Left,
    }

    public class PlayerController : MonoBehaviour
    {
        static readonly Dictionary<Direction, Vector2Int> s_DirectionVec = new Dictionary<Direction, Vector2Int>()
        {
            [Direction.Down] = Vector2Int.down,
            [Direction.Up] = Vector2Int.up,
            [Direction.Left] = Vector2Int.left,
            [Direction.Right] = Vector2Int.right,
        };

        [SerializeField]
        private BoardController m_BoardController;

        [SerializeField]
        private WaitingItems m_WaitingItems;

        [SerializeField]
        private PlayerSetting m_PlayerSetting;

        [SerializeField]
        private WaitingBomb m_WaitingBomb;

        private PlayerInput m_Input;
        private CancellationTokenSource m_MoveCTS;
        private CancellationTokenSource m_RotateCTS;
        private CancellationTokenSource m_DropCTS;

        private Item[] m_Items = new Item[2];
        private bool m_IsBomb;

        private Vector2Int m_Position;
        private Direction m_Rotation;

        private bool b_IsAcceptingInput;

        private bool IsAcceptingInput
        {
            get
            {
                if (Time.timeScale == 0) { return false; }
                return b_IsAcceptingInput;
            }
            set
            {
                b_IsAcceptingInput = value;
            }
        }

        private List<MotionHandle> m_Handles = new List<MotionHandle>();

        private float m_DropDelay;

        private void Awake()
        {
            m_Input = GetComponent<PlayerInput>();
            m_DropDelay = m_PlayerSetting.AutoDropDelay;
            IsAcceptingInput = true;
        }

        private void Start()
        {
            ChangeOperationPuyos();
            StartDrop().Forget();
        }

        private void OnEnable()
        {
            m_Input.actions["Move"].started += OnMoveStarted;
            m_Input.actions["Move"].performed += OnMovePerformed;
            m_Input.actions["Move"].canceled += OnMoveCanceled;

            m_Input.actions["Rotate"].performed += OnRotatePerformed;
            m_Input.actions["Rotate"].canceled += OnRotateCanceled;

            m_Input.actions["Drop"].performed += OnDropPerformed;
            m_Input.actions["Drop"].canceled += OnDropCanceled;
        }

        private void OnDisable()
        {
            m_Input.actions["Move"].started -= OnMoveStarted;
            m_Input.actions["Move"].performed -= OnMovePerformed;
            m_Input.actions["Move"].canceled -= OnMoveCanceled;

            m_Input.actions["Rotate"].performed -= OnRotatePerformed;
            m_Input.actions["Rotate"].canceled -= OnRotateCanceled;

            m_Input.actions["Drop"].performed -= OnDropPerformed;
            m_Input.actions["Drop"].canceled -= OnDropCanceled;
        }

        private void OnDestroy()
        {
            DisposeCTS(ref m_MoveCTS);
            DisposeCTS(ref m_RotateCTS);
            DisposeCTS(ref m_DropCTS);
        }

        public async UniTask GroundingProcess()
        {
            IsAcceptingInput = false;
            foreach (var handle in m_Handles)
            {
                await handle;
            }

            if (m_IsBomb)
            {
                Destroy(((Bomb)m_Items[0]).gameObject);
                await m_BoardController.Explode(m_Position);
                m_Items[0] = null;
                ChangeOperationPuyos();
                IsAcceptingInput = true;
                return;
            }

            Destroy(((Block)m_Items[0]).transform.GetChild(0).gameObject);

            m_BoardController.Settle(m_Position, (Block)m_Items[0]);
            m_BoardController.Settle(CalcChildPuyoPos(m_Position, m_Rotation), (Block)m_Items[1]);

            bool gameOver = await m_BoardController.DropToBottom();
            if (gameOver) { return; }

            ChangeOperationPuyos();
            IsAcceptingInput = true;

            return;
        }

        public bool CanSet(Vector2Int pos, Direction rot)
        {
            if (!m_BoardController.CanSettle(pos)) { return false; }
            if (m_IsBomb) { return true; }
            if (!m_BoardController.CanSettle(CalcChildPuyoPos(pos, rot))) { return false; }
            return true;
        }

        static Vector2Int CalcChildPuyoPos(Vector2Int pos, Direction rot)
        {
            return pos + s_DirectionVec[rot];
        }

        void ChangeOperationPuyos()
        {
            m_Rotation = Direction.Up;
            var initialPos = BoardController.START_POS;
            m_Position = initialPos;
            this.transform.localPosition = new Vector3(m_Position.x, m_Position.y, 0);

            (m_Items, m_IsBomb) = m_WaitingItems.GetNextItems();
            if (m_IsBomb)
            {
                Bomb bomb = ((Bomb)m_Items[0]);
                bomb.transform.position = this.transform.position;
                bomb.transform.parent = this.transform;
            }
            else
            {
                Block parent = (Block)m_Items[0];
                Block child = (Block)m_Items[1];
                parent.transform.position = this.transform.position;
                child.transform.position = this.transform.position + Vector3.up;
                parent.transform.parent = this.transform;
                child.transform.parent = this.transform;
            }
        }

        static void DisposeCTS(ref CancellationTokenSource cts)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        void Move(float value)
        {
            var direction = (value < 0) ? Vector2Int.left : Vector2Int.right;
            var targetPos = m_Position + direction;

            if (!IsAcceptingInput) { return; }
            if (!CanSet(targetPos, m_Rotation)) { return; }

            Vector3 vec3 = new Vector3(direction.x, direction.y, 0);
            Vector3 preX = Vector3.zero;

            var handle = LMotion.Create(Vector3.zero, vec3, m_PlayerSetting.MoveDelay)
                .WithEase(Ease.OutQuart)
                .Bind(x =>
                {
                    var moveAmout = x - preX;
                    this.transform.position += moveAmout;
                    preX = x;
                })
                .AddTo(this);

            m_Handles.Add(handle);

            m_Position = targetPos;
            return;
        }

        void OnMoveStarted(InputAction.CallbackContext context)
        {
            Move(context.ReadValue<float>());
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            DisposeCTS(ref m_MoveCTS);
            m_MoveCTS = new CancellationTokenSource();
            MoveContinuous(m_MoveCTS.Token).Forget();
            async UniTask MoveContinuous(CancellationToken token)
            {
                while (true)
                {
                    Move(context.ReadValue<float>());
                    await UniTask.Delay(TimeSpan.FromSeconds(m_PlayerSetting.MoveDelay), cancellationToken: token);
                }
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            DisposeCTS(ref m_MoveCTS);
        }

        bool Rotate(float value)
        {
            if (m_IsBomb) { return true; }

            var isRight = (value < 0) ? false : true;
            Direction targetRot;
            float targetAmount;

            if (isRight)
            {
                targetRot = (Direction)((int)(m_Rotation + 1) % 4);
                targetAmount = -90;
            }
            else
            {
                targetRot = (Direction)((int)(m_Rotation + 3) % 4);
                targetAmount = 90;
            }

            if (!IsAcceptingInput) { return false; }
            if (!CanSet(m_Position, targetRot)) { return false; }

            float currentAngle = 0;
            var parentPuyo = (Block)m_Items[0];
            var childPuyo = (Block)m_Items[1];

            var handle = LMotion.Create(0f, targetAmount, m_PlayerSetting.RotateDelay)
                .WithEase(Ease.OutQuad)
                .Bind(x =>
                {
                    var rotateAmount = x - currentAngle;
                    var quaternion = Quaternion.AngleAxis(rotateAmount, Vector3.forward);
                    childPuyo.transform.localPosition -= parentPuyo.transform.localPosition;
                    childPuyo.transform.localPosition = quaternion * childPuyo.transform.localPosition;
                    childPuyo.transform.localPosition += parentPuyo.transform.localPosition;
                    currentAngle = x;
                })
                .AddTo(this);
            m_Handles.Add(handle);

            m_Rotation = targetRot;
            return true;
        }

        void OnRotatePerformed(InputAction.CallbackContext context)
        {
            DisposeCTS(ref m_RotateCTS);
            m_RotateCTS = new CancellationTokenSource();
            RotateContinuous(m_RotateCTS.Token).Forget();
            async UniTask RotateContinuous(CancellationToken token)
            {
                while (true)
                {
                    if (Rotate(context.ReadValue<float>())) { break; }
                    await UniTask.Yield(cancellationToken: token);
                }
            }
        }

        void OnRotateCanceled(InputAction.CallbackContext context)
        {
            DisposeCTS(ref m_RotateCTS);
        }

        async UniTask Drop()
        {
            Vector2Int targetPos = m_Position + Vector2Int.down;

            if (!IsAcceptingInput) { return; }

            if (!CanSet(targetPos, m_Rotation))
            {
                GameManager.Instance.IsGaugeIncreasing = false;
                await GroundingProcess();
                await StartDrop();
                return;
            }

            Vector3 vec3 = Vector3.down;
            Vector3 preX = Vector3.zero;

            var handle = LMotion.Create(Vector3.zero, vec3, m_DropDelay / 5)
                .WithEase(Ease.InOutQuad)
                .Bind(x =>
                {
                    var dropAmout = x - preX;
                    this.transform.position += dropAmout;
                    preX = x;
                })
                .AddTo(this);
            m_Handles.Add(handle);

            m_Position = targetPos;
        }

        void OnDropPerformed(InputAction.CallbackContext context)
        {
            DisposeCTS(ref m_DropCTS);
            m_DropCTS = new CancellationTokenSource();

            if (IsAcceptingInput) { GameManager.Instance.IsGaugeIncreasing = true; }
            m_WaitingBomb.IsBoosting = true;
            m_DropDelay = m_PlayerSetting.ManualDropDelay;

            DropContinuous(m_DropCTS.Token).Forget();
        }

        void OnDropCanceled(InputAction.CallbackContext context)
        {
            m_WaitingBomb.IsBoosting = false;
            m_DropDelay = m_PlayerSetting.AutoDropDelay;
        }

        async UniTask StartDrop()
        {
            DisposeCTS(ref m_DropCTS);
            m_DropCTS = new CancellationTokenSource();
            GameManager.Instance.IsGaugeIncreasing = false;

            await UniTask.WaitForSeconds(m_PlayerSetting.StagnationTime, cancellationToken: m_DropCTS.Token);

            if (IsAcceptingInput) { GameManager.Instance.IsGaugeIncreasing = true; }
            DropContinuous(m_DropCTS.Token).Forget();
        }

        async UniTask DropContinuous(CancellationToken token)
        {
            while (true)
            {
                await Drop();
                await UniTask.WaitForSeconds(m_DropDelay, cancellationToken: token);
            }
        }
    }
}
