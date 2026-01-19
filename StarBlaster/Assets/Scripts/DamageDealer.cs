using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] int damage = 10; // Default collision damage (e.g. crashing into enemy)

    public int GetDamage()
    {
        return damage;
    }

    // NEW: Allow the Shooter/Config to override this dynamically (for projectiles)
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void Hit()
    {
        Destroy(gameObject);
    }
}
