// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    PlayerInput _input;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    // _input.actions["Move"]だとおかしくなって、Rotateだと正常
    private void OnEnable()
    {
        _input.actions["Move"].started += OnTestStarted;
    }

    private void OnDisable()
    {
        _input.actions["Move"].started -= OnTestStarted;
    }

    void OnTestStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Test" + context.ReadValue<float>().ToString());
    }
}
