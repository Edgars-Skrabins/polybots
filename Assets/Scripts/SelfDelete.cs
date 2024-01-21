using UnityEngine;

public class SelfDelete : MonoBehaviour
{
    [SerializeField] private float destroyTimer;

    private void Start()
    {
        Destroy(gameObject, destroyTimer);
    }
}