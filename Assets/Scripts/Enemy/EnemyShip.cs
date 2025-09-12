using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] private Animator anim;

    public void DestroyShip()
    {
        anim.SetTrigger("Blast");

        Destroy(gameObject, getClipLength("BlastAnimation"));
    }
    public float getClipLength(string key)
    {
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals(key))
            {
                return clip.length;
            }
        }
        return -1;
    }
}