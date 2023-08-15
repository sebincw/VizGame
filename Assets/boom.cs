using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boom : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Destroy(gameObject, 1);
        //gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, -10000));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider collision)
    {
        {
            if (collision.gameObject.tag == "player")
            {
                GetComponent<ParticleSystem>().Play();
                //GetComponentInChildren<ParticleSystem>().Stop();
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
