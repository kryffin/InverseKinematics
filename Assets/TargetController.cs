using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{

    public float speed;

    void Update()
    {
        Vector3 motion = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            motion.y += 1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            motion.y -= 1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            motion.x += 1f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            motion.x -= 1f;
        }

        motion = motion.normalized;

        transform.Translate(motion * speed * Time.deltaTime);
    }
}
