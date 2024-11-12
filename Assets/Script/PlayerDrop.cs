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


    void Drop()
    {
        float moveAmout = Time.deltaTime / _dropDelay;
        _state.ShiftYDown(moveAmout);
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
