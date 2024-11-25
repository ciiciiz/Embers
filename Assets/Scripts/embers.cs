using UnityEngine;

public class embers : MonoBehaviour
{
    [SerializeField] private Transform player;
    
    private Vector3 offset = new Vector3(0, (float)-0.5, 0);
    
    


    // Update is called once per frame
    void Update()
    {
        transform.position = player.position+offset;
        
    }
}
