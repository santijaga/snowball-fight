using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    // Components
    Animator animator;

    // String Values
    [SerializeField] string directionXParameterString = "Direction X";
    [SerializeField] string directionYParameterString = "Direction Y";
    [SerializeField] string movingParameterString = "Is Moving";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimationParameters(Vector2 direction, bool isMoving)
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        animator.SetFloat(directionYParameterString, direction.y);
        animator.SetFloat(directionXParameterString, direction.x);
        animator.SetBool(movingParameterString, isMoving);
    }
}
