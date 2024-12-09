using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.ParticleSystem;

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

    private string currentAnim;
    const string Player_Run = "Run";
    const string Player_Idle = "Idle";
    



    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();
        EmitEmbers();
        PlayAnimation();

        //jump
        if (Input.GetButtonDown("Jump") && isGrounded())//normal jump
        {
            rb.linearVelocityY = jumpingPower;   
        }
        if (Input.GetButtonUp("Jump") && rb.linearVelocityY > 0f)//jump- but depending on how long space is pressed
        {
            rb.linearVelocityY *= 0.5f;
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
            transform.position = new Vector3(-5, -3, 0);
        }
        if (Input.GetButtonDown("2"))
        {
            transform.position = new Vector3(-3, -34, 0);
        }
        if (Input.GetButtonDown("3"))
        {
            transform.position = new Vector3(44, -22, 0);
        }
        if (Input.GetButtonDown("4"))
        {
            transform.position = new Vector3(70, 13, 0);
        }
        if (Input.GetButtonDown("5"))
        {
            transform.position = new Vector3(-42, -14, 0);
        }
        if (Input.GetButtonDown("6"))
        {
            transform.position = new Vector3(152, -28, 0);
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
        float embersAmount = 50f;

        emission.rateOverTime = embersAmount;

        if(rb.linearVelocityX > 0.5f|| rb.linearVelocityX < -0.5f)
        {
            emission.enabled = true;

            if (isGrounded())
            {
                emission.rateOverTime = embersAmount;
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
        if (isGrounded()) { 
            if(rb.linearVelocityX < -0.5f || rb.linearVelocityX > 0.5f)
            {
             ChangeAnimation(Player_Run);
            }
            if (rb.linearVelocityX > -0.3f && rb.linearVelocityX < 0.3f)
            {
                ChangeAnimation(Player_Idle);
            }

        }
    }
    void ChangeAnimation(string newAnim)
    {
        if (currentAnim == newAnim) return;
        animator.Play(newAnim);
        currentAnim = newAnim;
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
