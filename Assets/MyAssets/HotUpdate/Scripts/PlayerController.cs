using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Animator anim;

    public float speed;
    public float gravity;

    Vector3 moveDirection;
    public float scrollSpeed = 0.5f;

    private Controls controls;
    private Vector2 moveInput;
    

    private void Awake()
    {
        controls = new Controls(); 
    }

    private void OnEnable()
    {
        controls.Enable(); // 激活输入监听
    }

    private void OnDisable()
    {
        controls.Disable(); // 注销输入监听
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

            Quaternion targetRotation = moveDirection != Vector3.zero ? Quaternion.LookRotation(moveDirection) : transform.rotation;
            transform.rotation = targetRotation;

            moveDirection = moveDirection * speed;
        }

        moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
        controller.Move(moveDirection * Time.deltaTime);

        if (moveInput != Vector2.zero)
        {
            anim.SetBool("Run", true);
        }
        else
        {
            anim.SetBool("Run", false);
        }
    }

    public void SidePos()
    {
        controller.Move(Vector3.right * 2.5f);
    }
}
