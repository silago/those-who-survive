using UnityEngine;

public class BaseItem : MonoBehaviour
{
    public ItemType ItemType { get; private set; }
    
    [SerializeField] public Transform slot;
    public ProductionItem ProductionItem { get; set; }

    public virtual void Initialize(ItemSettings settings)
    {
        ItemType = settings.ItemType;
    }
}