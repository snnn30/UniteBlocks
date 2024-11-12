// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDrop : MonoBehaviour
{
    [SerializeField] float _autoDropDelay = 0.48f;
    [SerializeField] float _manualDropDelay = 0.08f;

    float _dropDelay;
    PlayerInput _input;
    PlayerState _state;



    async void Drop()
    {
        float moveAmout = Time.deltaTime / _dropDelay;
        Vector2Int targetPos = new Vector2Int(_state.Position.x, Mathf.FloorToInt(transform.localPosition.y - moveAmout));

        if (!_state.IsAcceptingInput) { return; }

        if (!_state.CanSet(targetPos, _state.Rotation))
        {
            await _state.GroundingProcess();
            return;
        }

        transform.localPosition -= new Vector3(0, moveAmout, 0);
        if (transform.localPosition.y < _state.Position.y)
        {
            _state.Position = targetPos;
        }

        return;
    }

    void OnDropPerformed(InputAction.CallbackContext context)
    {
        _dropDelay = _manualDropDelay;
    }

    void OnDropCanceled(InputAction.CallbackContext context)
    {
        _dropDelay = _autoDropDelay;
    }


    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _state = GetComponent<PlayerState>();
        _dropDelay = _autoDropDelay;
    }

    private void Update()
    {
        Drop();
    }

    private void OnEnable()
    {
        _input.actions["Drop"].performed += OnDropPerformed;
        _input.actions["Drop"].canceled += OnDropCanceled;
    }

    private void OnDisable()
    {
        _input.actions["Drop"].performed -= OnDropPerformed;
        _input.actions["Drop"].canceled -= OnDropCanceled;
    }
}
