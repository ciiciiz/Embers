using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Key : MonoBehaviour
{
    public GameObject door;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("key aquired");

            door.GetComponent<TilemapCollider2D>().enabled = false;

            this.gameObject.SetActive(false);
        }
    }

}
