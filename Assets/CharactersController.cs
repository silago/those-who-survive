using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class CharactersController : MonoBehaviour
{
    [SerializeField] private List<Character> characters = new List<Character>();
    [SerializeField] private ActivityManager activityManager;

    private void Awake()
    {
        foreach (var character in characters)
        {
            character.ActivityObservable.Subscribe(a => OnCharActivityChange(character, a));
        }

        activityManager.NewActivityObservable.Subscribe(_ => { OnNewActivity(_); });
    }

    private void OnNewActivity(Activity activity)
    {
        var character = characters.FirstOrDefault(x => x.Activity == null);
        Debug.Log("On new activity");
        if (character != null)
        {
            character.SetActivity(activity);
        }
    }

    private void OnCharActivityChange(Character character, Activity activity)
    {
        if (activity == null)
        {
            var newActivity = activityManager.GetAwaiting();
            if (newActivity != null)
            {
                character.SetActivity(newActivity);
            }
        }
        else if (activity.Status == ActivityStatus.Paused)
        {
        }
        else if (activity.Status == ActivityStatus.Complete)
        {
            Debug.Log("find new activity for complete activity char");
            activityManager.CompleteActivity(activity);

            var newActivity = activityManager.GetAwaiting();
            if (newActivity != null)
            {
                character.SetActivity(newActivity);
            }
        }
    }
}