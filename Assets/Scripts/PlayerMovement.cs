using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.ParticleSystem;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Controls;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 10f;
    private bool isFacingRight = true;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 16f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private ParticleSystem embers;
    [SerializeField] private Animator animator;
    [SerializeField] private CompositeCollider2D groundCol;

    private string currentAnim;
    const string Player_Run = "Run";
    const string Player_Idle = "Idle";
    const string Player_Jump = "jump";
    const string Player_Falling = "Falling";
    const string Player_Land = "Land";

    private float cayoteTime = 0.2f;
    private float cayoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool hasLanded = false;
    private float jumpTimer = 0f;
    private bool isJumping = false;
    private int oneJump = 0;
 

    public float maxEmbersAmount;
    public float minEmbersAmount;


    // Update is called once per frame
    void Update()
    {
        //if (isDashing)
        //{
        //    return;
        //}

        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();
        EmitEmbers();
        PlayAnimation();

        //cayote time
        if (isGrounded())
        {
            cayoteTimeCounter = cayoteTime;
        }
        else
        {
            cayoteTimeCounter -= Time.deltaTime;
        }
        //input buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        //jump logics
        if (jumpBufferCounter >0f && cayoteTimeCounter >0f)//normal jump
        {
            new WaitForSeconds(2f);
            rb.linearVelocityY = jumpingPower;

            jumpBufferCounter = 0;

            isJumping = true;
            oneJump = 0;     
        }
        if (Input.GetButtonUp("Jump") && rb.linearVelocityY > 0f)//jump- but depending on how long space is pressed
        {
            rb.linearVelocityY *= 0.5f;
            cayoteTimeCounter = 0f;
        }

        //fall quicker
        if (rb.linearVelocityY < 0f && rb.linearVelocityY > -9f)
        {
            rb.linearVelocityY *= 1.025f;
        }

        //dash
        if(Input.GetKeyDown(KeyCode.LeftShift)&& canDash)
        {
            StartCoroutine(Dash());
        }

        //level select (debug/test)
        if (Input.GetButtonDown("1"))
        {
            transform.position = new Vector3(90, -25, 0);
        }

    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

    }

    private void Flip()//changes direction of sprite
    {
        
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;

            //rotation of the embers particle when flipping sprite
            if (isFacingRight){
                embers.transform.rotation = Quaternion.Euler(0,0,-10);
                
            }
            if (!isFacingRight)
            {
                embers.transform.rotation = Quaternion.Euler(0, 0, 190);
                
            }
        }
    }
    private void EmitEmbers()
    {
        var emission = embers.emission;
        //maxEmbersAmount = 40f;
        //minEmbersAmount = 10f;

        emission.rateOverTime = maxEmbersAmount;

        if(rb.linearVelocityX > 0.5f|| rb.linearVelocityX < -0.5f)
        {
            emission.enabled = true;

            if (isGrounded())
            {
                emission.rateOverTime = maxEmbersAmount;
            }
            else
            {
                emission.rateOverTime = 10f;
            }
        }
        else
        {
            emission.enabled = false;
        }
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.25f, groundLayer);
    }

    void PlayAnimation()
    {
        //how long since jumping (without hitting ground)
        jumpTimer += Time.deltaTime;
        //Debug.Log(jumpTimer); 

        //jump
        if (isGrounded() && isJumping && oneJump==0 && rb.linearVelocityY >0.5f)
        {
            ChangeAnimation(Player_Jump);
            oneJump = 1;
            hasLanded = false;
            jumpTimer = 0f;
            //Debug.Log("jumped");
        }

        //falling
        if (!isGrounded() && rb.linearVelocityY < 0f)
        {
            ChangeAnimation(Player_Falling);
        }

        //landing
        if (isGrounded() && hasLanded == false && jumpTimer > 0.3f)
        {
            //Debug.Log(animLen);
            if (jumpTimer > 0.5f)//has fallen enough to bend knees when landing
            {
                ChangeAnimation(Player_Land);
                //Debug.Log("crunch");
                
            }
          
            hasLanded = true;
            isJumping = false;
            //Debug.Log("landed");
        }

        //run
        if ((rb.linearVelocityX < -0.5f && isGrounded()) || (rb.linearVelocityX > 0.5f && isGrounded()))
        {
            ChangeAnimation(Player_Run);
        }

        //idle
        if (rb.linearVelocityX > -0.3f && rb.linearVelocityX < 0.3f && isGrounded())
        {
            ChangeAnimation(Player_Idle);
        }
    }
    
    void ChangeAnimation(string newAnim)
    {
        if (currentAnim == newAnim) return;
        animator.Play(newAnim);
        currentAnim = newAnim;
        //Debug.Log(currentAnim);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }




}
