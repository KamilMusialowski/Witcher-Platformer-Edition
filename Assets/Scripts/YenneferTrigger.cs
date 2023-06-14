using UnityEngine;


public class YenneferTrigger : MonoBehaviour
{

    [SerializeField] private GameObject yennefer;
    private Material yenneferMaterial;
    [SerializeField] private Material yenneferMaterial2;
    private float fade = 0.0f;

    private void Start()
    {
        yenneferMaterial = yennefer.GetComponent<SpriteRenderer>().material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            while(fade <= 1.0f)
            {
                fade += Time.deltaTime * 10;
                if (fade <= 1)
                {
                    fade = 1;
                    
                }
                yenneferMaterial2.SetFloat("_Fade", fade);
            }
            yennefer.SetActive(true);
        }
    }
}
