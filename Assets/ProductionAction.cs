using UnityEngine;

[CreateAssetMenu(menuName = "Create ProductionAction", fileName = "ProductionAction", order = 0)]
public class ProductionAction : ScriptableObject
{
    public object Requirement;
    public ProductionItem result;
}