using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoostPickup : MonoBehaviour
{
    public BoostType boostType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerInventory>() != null)
        {
            BoostManager.Instance.AddCharge(boostType);
            Destroy(gameObject);
        }
    }
}

