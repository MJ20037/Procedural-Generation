using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastDestinationSetup : MonoBehaviour
{
    [SerializeField] DirectedAgent directedAgent;

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2);
            if (Physics.Raycast(ray, out RaycastHit hit, 999f))
            {
                directedAgent.MoveToLocation(hit.point);
            }
        }
    }
}
