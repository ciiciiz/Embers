using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();

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
            transform.position = new Vector3(-5, -34, 0);
        }
        if (Input.GetButtonDown("3"))
        {
            transform.position = new Vector3(44, -22, 0);
        }
        if (Input.GetButtonDown("4"))
        {
            transform.position = new Vector3(70, 13, 0);
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
        }
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.25f, groundLayer);
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
