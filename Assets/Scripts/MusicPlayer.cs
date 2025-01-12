using UnityEngine;

public class MusicPlayer : MonoBehaviour
{



    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip bgMusic;


    private float introTimer;
    private float introTime = 61f;

 


    private void Start()
    {
        PlayMusic(intro);
    }
    void Update()
    {
        introTimer += Time.deltaTime;
        Debug.Log(introTimer);

        if(introTimer >=introTime)
        {

            if (source.clip!=bgMusic )
            {
                PlayMusic(bgMusic);
                source.loop = true;
            }

            
        }
    }

    private void PlayMusic(AudioClip sound)
    {
        source.clip = sound;
        source.Play();
    }

}
