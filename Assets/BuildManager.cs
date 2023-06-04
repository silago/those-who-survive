using System;
using System.Threading.Tasks;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [SerializeField] private ContentManager contentManager;
    [SerializeField] private ActivityManager activityManager;
    [SerializeField] private ItemsSettings items;
    [SerializeField] private Transform container;
    [SerializeField] private ItemBuildPreview itemBuildPreview;
    private ItemSettings selectedItemSettings;
    private BuildItem selectedItemPrefab;
    [SerializeField] private GameObject world;
    [SerializeField] private LayerMask floorLevel;
    [SerializeField] private LayerMask buildItemLevel;
    [SerializeField] private int tmpLayer = 0;
    [SerializeField] private BuildItem placeholderItem;
    private BuildItem[] grid = new BuildItem[100 * 100];
    private float cellSize = 0.5f;
    private int prevLayer;

    private int WorldToGrid(Vector3 input)
    {
        var x = Mathf.FloorToInt(input.x / cellSize) + 50;
        var y = Mathf.FloorToInt(input.z / cellSize) + 50;
        var index = y * 100 + x;
        return index;
    }


    private Vector3 ClampToGrid(Vector3 input)
    {
        var x = input.x - (input.x % cellSize);
        var z = input.z - (input.z % cellSize);
        return new Vector3(x, input.y, z);
    }

    private void Awake()
    {
        foreach (var item in items.items)
        {
            var newItem = Instantiate(itemBuildPreview, container);
            newItem.sprite.sprite = item.PreviewIcon;
            newItem.button.onClick.AddListener(() => OnBuildButtonClick(item));
        }

        Time.timeScale = 2f;
    }

    private Collider[] cols = null;

    private void OnDrawGizmos()
    {
        if (cols == null)
        {
            return;
        }
    }

    public bool HasIntersections(BuildItem item)
    {
        if (selectedItemPrefab == null)
        {
            return false;
        }

        var bounds = selectedItemPrefab.Collider.bounds;
        var result = Physics.OverlapBox(item.transform.position, bounds.extents*0.99f, item.transform.rotation,
            buildItemLevel);
        
        Debug.Log(result.Length);
        return result.Length > 0;
    }

    private Vector3 GetMeshOffset(BuildItem item)
    {
        var bounds = item.Collider.bounds;
        var localCenter = item.transform.position - bounds.center;
        var result = new Vector3(0, bounds.extents.y - localCenter.y, 0);
        //Debug.Log(bounds.extents.y + ": " + item.Collider.bounds.center.y + ":" + item.transform.position.y);
        return result;
    }


    private void Update()
    {
        if (Input.GetMouseButtonUp(1) && selectedItemPrefab != null)
        {
            Destroy(selectedItemPrefab.gameObject);
            selectedItemPrefab = null;
        }

        if (selectedItemPrefab)
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                var rotation = selectedItemPrefab.transform.rotation.eulerAngles;
                selectedItemPrefab.transform.rotation = Quaternion.Euler(rotation.x, rotation.y + 90, rotation.z);
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                var rotation = selectedItemPrefab.transform.rotation.eulerAngles;
                selectedItemPrefab.transform.rotation = Quaternion.Euler(rotation.x, rotation.y - 90, rotation.z);
            }


            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, floorLevel))
            {
                Debug.Log(hit.transform.gameObject.name);
                var clampedPos = ClampToGrid(hit.point);
                selectedItemPrefab.transform.position = clampedPos + GetMeshOffset(selectedItemPrefab);
                
                
                var hasIntersections = HasIntersections(selectedItemPrefab);
                if (hasIntersections)
                {
                    selectedItemPrefab.SetWrongPlaceColor();
                }
                else
                {
                    selectedItemPrefab.SetDefaultColor();
                }


                if (Input.GetMouseButton(0) && !hasIntersections)
                {
                    QueueBuilding();
                }
            }
        }
    }

    private void QueueBuilding()
    {
        selectedItemPrefab.Collider.gameObject.layer = prevLayer;
        var name = selectedItemPrefab.name;
        var buildItem = selectedItemPrefab;

        var placeholder = Instantiate(placeholderItem);
        //placeholder.transform.rotation = buildItem.transform.rotation;
        placeholder.transform.localScale = buildItem.Collider.bounds.size;


        var clampedPos = ClampToGrid(buildItem.transform.position);
        placeholder.transform.position =
            selectedItemPrefab.transform.position = clampedPos + GetMeshOffset(placeholderItem);

        selectedItemPrefab = Instantiate(selectedItemPrefab, world.transform);
        selectedItemPrefab.name = name;

        prevLayer = selectedItemPrefab.Collider.gameObject.layer;
        selectedItemPrefab.Collider.gameObject.layer = tmpLayer;

        buildItem.gameObject.SetActive(false);

        var settings = selectedItemSettings;

        activityManager.AddActivity(new Activity()
        {
            Status = ActivityStatus.Paused, Type = ActivityType.Build, target = buildItem.transform,
            callbackAfter = () =>
            {
                buildItem.gameObject.SetActive(true);
                buildItem.Initialize(settings);
                contentManager.AddItem(buildItem);
                Destroy(placeholder.gameObject);
            }
        });
    }

    private async void OnBuildButtonClick(ItemSettings settings)
    {
        selectedItemSettings = settings;
        await Task.Yield();
        selectedItemPrefab = Instantiate(settings.Prefab, world.transform);

        prevLayer = selectedItemPrefab.Collider.gameObject.layer;
        selectedItemPrefab.Collider.gameObject.layer = tmpLayer;
    }
}