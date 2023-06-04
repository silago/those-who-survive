using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class ActivityManager : MonoBehaviour
{
    public readonly Subject<Activity> NewActivityObservable = new Subject<Activity>();
    private readonly List<Activity> _activities = new List<Activity>();

    public void AddActivity(Activity activity)
    {
        _activities.Add(activity);
        NewActivityObservable.OnNext(activity);
    }

    public void CompleteActivity(Activity activity)
    {
        _activities.Remove(activity);
        activity?.callbackAfter?.Invoke();
    }

    public List<Activity> GetAll()
    {
        return _activities;
    }

    public Activity GetAwaiting()
    {
        return _activities.FirstOrDefault(x =>
            x.Status != ActivityStatus.Active && x.Status != ActivityStatus.Complete);
    }
}

public class Activity
{
    public Transform target;
    public Action callbackAfter;
    public Action callbackBefore;
    public ActivityType Type;
    public ActivityStatus Status;
    public float progres = 0;
    public GameObject item;
}

public enum ActivityStatus
{
    Default,
    Active,
    Paused,
    Complete
}

public enum ActivityType
{
    Build, Move, Create
}