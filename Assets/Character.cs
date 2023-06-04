using System;
using System.Threading.Tasks;
using CraftingAnims;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CrafterController))]
public class Character : MonoBehaviour
{
    [SerializeField] private CrafterController crafterController;
    public bool isBusy = false;
    public Activity Activity { get; private set; }
    public readonly ISubject<Activity> ActivityObservable = new Subject<Activity>();
    private NavMeshAgent NavMeshAgent => crafterController.crafterNavigation.navMeshAgent;
    private bool isNavigating = false;
    public bool WannaSleep = false;

    public void SetActivity(Activity activity)
    {
        activity.Status = ActivityStatus.Active;
        isNavigating = true;
        Activity = activity;
        ActivityObservable.OnNext(activity);

        if (activity.Type == ActivityType.Build)
        {
            ProcessBuildActivity(activity);
        }
        else if (activity.Type == ActivityType.Create)
        {
            ProcessCreateActivity(activity);
        }
        else if (activity.Type == ActivityType.Move)
        {
            ProcessMoveActivity(activity);
        }
    }


    private async void ProcessCreateActivity(Activity activity)
    {
        crafterController.crafterNavigation.MeshNavToPoint(activity.target.position);
        isNavigating = true;
        await AwaitTarget();
        await Task.Delay(TimeSpan.FromSeconds(0.5f));


        crafterController.animator.SetBool("Use", true);
        crafterController.charState = CrafterState.Use;
        crafterController.LockMovement(1);
        await Task.Delay(TimeSpan.FromSeconds(1));
        crafterController.animator.SetBool("Use", false);
        crafterController.charState = CrafterState.Idle;
        crafterController.LockMovement(0.3f);
        await Task.Delay(TimeSpan.FromSeconds(0.5f / Time.timeScale));
        Activity.Status = ActivityStatus.Complete;
        await Task.Delay(TimeSpan.FromSeconds(1.5f / Time.timeScale));
        var prev = Activity;
        Activity = null;
        ActivityObservable.OnNext(prev);
    }

    private async void ProcessBuildActivity(Activity activity)
    {
        crafterController.crafterNavigation.MeshNavToPoint(activity.target.position);
        isNavigating = true;
        await AwaitTarget();

        await Task.Delay(TimeSpan.FromSeconds(0.5f));
        Debug.Log($"Processing {Activity} reached");
        crafterController.TriggerAnimation("ItemBeltTrigger");
        crafterController.showItem.ItemShow("hammer", 0.5f);
        crafterController.charState = CrafterState.Hammer;
        crafterController.LockMovement(1f);
        crafterController.RightHandBlend(true);
        await Task.Delay(TimeSpan.FromSeconds(0.5f / Time.timeScale));

        crafterController.TriggerAnimation("HammerTableTrigger");
        crafterController.LockMovement(1.9f);
        await Task.Delay(TimeSpan.FromSeconds(1.5f / Time.timeScale));
        Activity.Status = ActivityStatus.Complete;
        await Task.Delay(TimeSpan.FromSeconds(1.5f / Time.timeScale));
        var prev = Activity;
        Activity = null;
        ActivityObservable.OnNext(prev);
    }


    private async void ProcessMoveActivity(Activity activity)
    {
        crafterController.crafterNavigation.MeshNavToPoint(activity.item.transform.position);
        isNavigating = true;
        await AwaitTarget();

        await Task.Delay(TimeSpan.FromSeconds(0.5f));

        crafterController.animator.SetBool("Use", true);
        crafterController.charState = CrafterState.Use;
        crafterController.LockMovement(1);
        
        await Task.Delay(TimeSpan.FromSeconds(1));
        crafterController.animator.SetBool("Use", false);
        crafterController.charState = CrafterState.Idle;
        crafterController.LockMovement(0.3f);
        activity?.callbackBefore?.Invoke();

        crafterController.crafterNavigation.MeshNavToPoint(activity.target.transform.position);
        isNavigating = true;
        await AwaitTarget();


        crafterController.animator.SetBool("Use", true);
        crafterController.charState = CrafterState.Use;
        crafterController.LockMovement(1);
        await Task.Delay(TimeSpan.FromSeconds(1));
        crafterController.animator.SetBool("Use", false);
        crafterController.charState = CrafterState.Idle;
        crafterController.LockMovement(0.3f);

        activity?.callbackAfter?.Invoke();


        var prev = Activity;
        Activity = null;
        ActivityObservable.OnNext(prev);
    }

    private async Task AwaitTarget()
    {
        while (isNavigating)
        {
            if (crafterController.crafterNavigation.isNavigating)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            else
            {
                isNavigating = false;
            }
        }
    }
}