using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WarpGate : MonoBehaviour
{
    private bool playerIsNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }

    private void Update()
    {
        if (playerIsNear && Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            StartCoroutine(TryWarpWithDelay());
        }
    }

    private IEnumerator TryWarpWithDelay()
    {
        while (GameManager.Instance == null) 
        {
            yield return new WaitForSeconds(0.2f);
        }

        TryWarp();
    }

    private void TryWarp()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!GameManager.Instance.GameInProgress)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            GameManager.Instance.LoadNextStage();
        }
    }
}
