using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.ParticleSystem;
using Unity.VisualScripting;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    public float speed = 8f;
    public float jumpingPower = 10f;
    private bool isFacingRight = true;
    private bool canWalk = true;
    private bool zipLining = false;

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
    [SerializeField] private BoxCollider2D zipEnd1;
    [SerializeField] private BoxCollider2D zipEnd2;
    [SerializeField] private BoxCollider2D zipEnd3;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource soundEffect;
    [SerializeField] private AudioClip steps;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;

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

    //private bool hasLanded;
    private float jumpTimer = 0f;
    private bool isJumping = false;
    private int oneJump = 0;

    //private bool lastCheck = false;
    //private bool landed= false;

    public float maxEmbersAmount;
    public float minEmbersAmount;

    //zipline
    Vector3 pos1;
    Vector3 pos2;
    int oneZip = 0;
    int zipNum = 0;

    private float stepDelay = 0f;




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
        if (jumpBufferCounter > 0f && cayoteTimeCounter > 0f)//normal jump
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
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
        if (canWalk)
        {
             rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

        }
       
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
            if (isFacingRight)
            {
                embers.transform.rotation = Quaternion.Euler(0, 0, -10);

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

        if (rb.linearVelocityX > 0.5f || rb.linearVelocityX < -0.5f)
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

    //private bool LandedJustNow()
    //{
    //     //lastCheck
    //     // newCheck
    //     //if is on ground but lastcheck is false = has just landed
    //     if(isGrounded() && lastCheck == false)
    //    {
    //        landed = true;
    //    }
    //    else
    //    {
    //        landed = false;          
    //    }
    //    lastCheck = landed;

    //    return landed;
    //}

void PlayAnimation()
    {
        //how long since jumping (without hitting ground)
        jumpTimer += Time.deltaTime;
        //Debug.Log(jumpTimer); 
        

        //jump
        if (isGrounded() && isJumping && oneJump == 0 && rb.linearVelocityY > 0.5f)
        {
            ChangeAnimation(Player_Jump);
            oneJump = 1;
            //hasLanded = false;
            jumpTimer = 0f;
            //Debug.Log("jumped");
            PlaySound(jump);
        }

        //falling
        if (!isGrounded() && rb.linearVelocityY < 0f || canWalk == false && zipLining == true)
        {
            ChangeAnimation(Player_Falling);
        }

        ////landing
        //if (LandedJustNow() && hasLanded==false && jumpTimer>=1f)
        //{ 
        //    ChangeAnimation(Player_Land);
        //    Debug.Log("landed");

        //    hasLanded = true;
        //    //if (jumpTimer > 0.5f )//has fallen enough to bend knees when landing
        //    //{     
        //    //    //Debug.Log("crunch");
        //    //}
           
        //    //isJumping = false;          
        //}

        //run
        if ((rb.linearVelocityX < -0.5f && isGrounded()) || (rb.linearVelocityX > 0.5f && isGrounded()))
        {
            stepDelay += Time.deltaTime;
            ChangeAnimation(Player_Run);
            if(stepDelay > 0.4f)
            {
                 PlaySound(steps);
                stepDelay = 0f;
            }
           
        }

        //idle
        if (rb.linearVelocityX > -0.3f && rb.linearVelocityX < 0.3f && isGrounded())
        {
            ChangeAnimation(Player_Idle);
            //Debug.Log("idling");
        }
       
    }

    void ChangeAnimation(string newAnim)
    {
        if (currentAnim == newAnim) return;
        animator.Play(newAnim);
        currentAnim = newAnim;
        //Debug.Log(currentAnim);
    }

    void PlaySound(AudioClip soundClip)
    {
        soundEffect.clip = soundClip;
        if (soundClip == steps)
        {
            soundEffect.volume = 0.3f;
            soundEffect.pitch = 1f;
        }
        if(soundClip == jump)
        {
            soundEffect.volume = 0.02f;
            soundEffect.pitch = 0.6f;
        }
        soundEffect.Play();
        Debug.Log("sound played: " + soundClip.name);
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

 
    //collision triggers
    private void OnTriggerEnter2D(Collider2D other)
    {



        //zipLines
        if (other.gameObject.tag == "onZipline")//zip time !!
        {
            if(oneZip == 0)
            {
                StartCoroutine(Zipline(true));
                Debug.Log("zip");
                zipNum += 1;
                oneZip = 1;
            }   
        }
        if (other.gameObject.tag == "offZipline")//zip over :(
        {
            StartCoroutine(Zipline(false));
            Debug.Log("stop zip");
            oneZip = 0;
        }


        //next scene(end scene)

        if (other.gameObject.tag == ("newScene"))
        {
            StartCoroutine(EndScene());
        }

    }
    IEnumerator Zipline(bool state)
    {
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        if (state)
        {
            canWalk = false;
            zipLining = true;

            pos1 = rb.position;
            pos1.y += 1.2f;//offset

            if (zipNum == 0)//zipline 1
            {               
                pos2 = zipEnd1.transform.position;
            }
            if (zipNum == 1)//zipline 2
            {
                pos2 = zipEnd2.transform.position;
            }
            if (zipNum == 2)//zipline 3
            {
                pos2 = zipEnd3.transform.position;//wants to go to zipend1?????
            }

            pos2.y -= 0.8f;//offset
            float zipLength = Vector3.Distance(pos1, pos2);
            float startTime = Time.time;

            while (Vector3.Distance(rb.position, pos2) > 2f)
            {
                float zippedSoFar = (Time.time - startTime) * 10f; // Adjust speed factor
                float fractionOfZip = zippedSoFar / zipLength;

                rb.MovePosition(Vector3.Lerp(pos1, pos2, fractionOfZip));

                //Debug.Log("zipping " + zipNum);
                //Debug.Log("zipping to " + pos2);
                yield return null;
            }


        }
       
        canWalk = true;
        zipLining = false;
        rb.gravityScale = originalGravity;
        //Debug.Log(rb.gravityScale);
        
        
        
        //GetComponent<TilemapCollider2D>().enabled = true;

    }

    IEnumerator EndScene()
    {
        canWalk = false;
        while(music.volume > 0f)
        {
            music.volume -= 0.01f;
            yield return new WaitForSeconds(0.1f);

        }
        SceneManager.LoadScene("Endingcutscene");
        
    }

}  



