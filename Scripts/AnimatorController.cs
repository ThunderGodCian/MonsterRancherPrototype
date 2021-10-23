using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    public bool isAnimating;
    public bool isHittingEnemy;


    public void TriggerHitAnimation()
    {
        isHittingEnemy = true;
    }

    public void AnimationFirstFrame()
    {
        isAnimating = true;
    }

    public void AnimationLastFrame()
    {
        isAnimating = false;
        AnimationFinished();
    }


    public Action onAnimationFinished;
    public void AnimationFinished()
    {
        if(onAnimationFinished != null)
        {
            onAnimationFinished();
        }
    }
}
