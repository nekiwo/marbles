using System.Collections;
using UnityEngine;
using System.Linq;

public class MPInteract_old : MonoBehaviour
{
    public bool IsDead = false;

    public GameObject MPDeathMenuObj;
    public GameObject particles;

    public class Death
    {
        public string code;
        public string id;
        public float[] pos;

        public Death(Vector3 position, string id)
        {
            this.code = MPClient_old.GameCode;
            this.id = id;
            this.pos = new float[] {position.x, position.y, position.z};
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("red") && !IsDead)
        {
            //DeathAnimation(false);
        }
    }

    void FixedUpdate()
    {
        GameObject LastPlatform;

        if (MPClient_old.IsHost)
        {
            LastPlatform = MPHost_old.RenderedPlatforms.Last();
        } else
        {
            LastPlatform = MPClient_old.RenderedPlatforms.Last();
        }

        if (!IsDead && transform.position.y < LastPlatform.transform.position.y - 10)
        {
            //DeathAnimation(false);
        }
    }

    public void DeathAnimation(bool IsLast)
    {
        IsDead = true;

        WebSockets.Emit("death", new Death(gameObject.transform.position, MPClient_old.ClientID));

        if (!IsLast)
        {
            GameObject ParticlesCopy = GameObject.Instantiate(particles);
            ParticlesCopy.transform.position = gameObject.transform.position;
            ParticleSystem system = ParticlesCopy.GetComponent<ParticleSystem>();
            system.Clear();
            system.Simulate(system.main.duration);
            system.Play();
        }

        MPDeathMenuObj.SetActive(true);
        gameObject.GetComponent<MPMovement_old>().enabled = false;
        if (MPClient_old.IsHost)
        {
            Camera.main.GetComponent<MPHost_old>().enabled = false;
            CancelInvoke();
        }

        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
}