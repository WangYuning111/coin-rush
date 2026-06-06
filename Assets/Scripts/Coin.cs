using UnityEngine;

public class Coin : MonoBehaviour
{
    public int scoreValue = 1;
    [Header("Rotation and Floating")]
    public float rotateSpeed = 90f;
    public float floatAmplitude = 0.15f;
    public float floatSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.Instance.AddScore(scoreValue);
            AudioManager.Instance?.PlayCoinCollect();
            Destroy(gameObject);
        }
    }
}
