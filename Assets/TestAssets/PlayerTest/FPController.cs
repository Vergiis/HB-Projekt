using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class FPController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float turnSpeed = 4.0f;
    public float runningSpeed = 11.5f;
    public float gravity = 1000.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookLimit = 79.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    PlayerInput playerInput;
    Vector2 currentMoveInput;
    Vector3 currentMove;
    Vector2 currentLooking;
    bool isMovePressed;
    public AnimationCurve lookCurve;

    Animator animator;

    public GameObject characterModel;

    [HideInInspector]
    public bool canMove = true;

    void Awake()
    {
        playerInput = new PlayerInput();

        playerInput.CharacterInput.Move.started += onMoveInput;
        playerInput.CharacterInput.Move.canceled += onMoveInput;
        playerInput.CharacterInput.Move.performed += onMoveInput;

        playerInput.CharacterInput.Look.started += onLookInput;
        playerInput.CharacterInput.Look.canceled += onLookInput;
        playerInput.CharacterInput.Look.performed += onLookInput;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void onLookInput(InputAction.CallbackContext context)
    {
        currentLooking = context.ReadValue<Vector2>();
    }

    public void onMoveInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        currentMove.x = currentMoveInput.x;
        currentMove.y = currentMoveInput.y;
        isMovePressed = currentMoveInput.x != 0 || currentMoveInput.y != 0;
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool("isWalking");

        if(isMovePressed && !isWalking)
        {
            animator.SetBool("isWalking", true);
        }
        else if(!isMovePressed && isWalking)
        {
            animator.SetBool("isWalking", false);
        }
    }

    void Update()
    {
        handleAnimation();

        if (canMove)
        {
            float yLookLarp = 0.0f;
            if (currentLooking.y > 0)
            {
                //yLookLarp = lookCurve.Evaluate(currentLooking.y * lookSpeed);
                yLookLarp = currentLooking.y * lookSpeed;
            }
            else
            {
                //yLookLarp = -lookCurve.Evaluate(-currentLooking.y * lookSpeed);
                yLookLarp = currentLooking.y * lookSpeed;
            }

            rotationX += -yLookLarp;
            rotationX = Mathf.Clamp(rotationX, -lookLimit, lookLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            float xLookLarp = 0.0f;
            if (currentLooking.x > 0)
            {
                //xLookLarp = lookCurve.Evaluate(currentLooking.x * lookSpeed);
                xLookLarp = currentLooking.x * lookSpeed;
            }
            else
            {
                //xLookLarp = -lookCurve.Evaluate(-currentLooking.x * lookSpeed);
                xLookLarp = currentLooking.x * lookSpeed;
            }

            transform.rotation *= Quaternion.Euler(0, xLookLarp, 0);
        }


        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);


        bool isRunning = false;
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * currentMove.y : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * currentMove.x : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        moveDirection.y = 0;

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (currentMove.x < 0 && currentMove.y > 0)
        {
            if (characterModel.transform.localRotation.z < -0.25)
                characterModel.transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * turnSpeed, Space.Self);
            else if (characterModel.transform.localRotation.z > -0.24)
                characterModel.transform.Rotate(new Vector3(0, 0, -45) * Time.deltaTime * turnSpeed, Space.Self);
        }
        else if (currentMove.x > 0 && currentMove.y > 0)
        {
            if (characterModel.transform.localRotation.z < 0.24)
                characterModel.transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * turnSpeed, Space.Self);
            else if (characterModel.transform.localRotation.z > 0.25)
                characterModel.transform.Rotate(new Vector3(0, 0, -45) * Time.deltaTime * turnSpeed, Space.Self);
        }
        else if (currentMove.x < 0)
        {
            if(characterModel.transform.localRotation.z > -0.5)
                characterModel.transform.Rotate(new Vector3(0, 0, -90) * Time.deltaTime * turnSpeed, Space.Self);
        }
        else if (currentMove.x > 0)
        {
            if(characterModel.transform.localRotation.z < 0.5)
                characterModel.transform.Rotate(new Vector3(0, 0, 90) * Time.deltaTime * turnSpeed, Space.Self);
        }
        
        else 
        {
            characterModel.transform.Rotate(new Vector3(0, 0, -(characterModel.transform.localRotation.z*180)) * Time.deltaTime * walkingSpeed * 2,Space.Self);
        }


        
    }

    private void OnEnable()
    {
        playerInput.CharacterInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.CharacterInput.Disable();
    }
}
