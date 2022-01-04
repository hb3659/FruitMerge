using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public bool isDrag = false;
    public bool isMerge = false;
    public int level;

    public Rigidbody2D rigid;
    public new CircleCollider2D collider;
    public Animator animator;
    public new SpriteRenderer renderer;

    float deadTime;

    void OnEnable()
    {
        animator.SetInteger("Level", level);
    }

    void OnDisable()
    {
        // 초기화
        level = 0;
        isDrag = false;
        isMerge = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        collider.enabled = true;
    }

    void Update()
    {
        if (isDrag)
        {
            // 스크린 좌표계를 월드 좌표계로
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // x 좌표 제한
            float leftWall = -4.65f + transform.localScale.x * .5f;
            float rightWall = 4.65f - transform.localScale.x * .5f;

            if (mousePosition.x < leftWall)
                mousePosition.x = leftWall;
            else if (mousePosition.x > rightWall)
                mousePosition.x = rightWall;

            mousePosition.y = 8f;
            mousePosition.z = 0f;

            transform.position = Vector3.Lerp(transform.position, mousePosition, .2f);
        }
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    // 충돌 중
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Circle")
        {
            Circle other = collision.gameObject.GetComponent<Circle>();

            // 합성
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 내가 아래에 있을 경우
                // 동일한 높이에서 내가 오른쪽에 있을 경우
                if (myY < otherY || (myY == otherY && myX > otherX))
                {
                    // 상대방이 나에게 흡수된다.
                    other.Hide(transform.position);
                    // 성장
                    LevelUP();
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if (deadTime >= 2f)
                renderer.color = new Color(.9f, .2f, .2f);

            if(deadTime >= 5f)
            {
                manager.GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0f;
            renderer.color = Color.white;
        }
    }

    public void Hide(Vector3 targetPosition)
    {
        isMerge = true;

        rigid.simulated = false;
        collider.enabled = false;

        if (targetPosition == transform.position)
            effect.Play();

        StartCoroutine(HideRoutine(targetPosition));
    }

    IEnumerator HideRoutine(Vector3 targetPosition)
    {
        int frameCount = 0;

        while (frameCount < 20)
        {
            frameCount++;

            transform.position = Vector3.Lerp(transform.position, targetPosition, .2f);

            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUP()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        StartCoroutine(LevelUPRoutine());
    }

    IEnumerator LevelUPRoutine()
    {
        yield return new WaitForSeconds(.1f);

        animator.SetInteger("Level", level + 1);
        EffectPlay();

        yield return new WaitForSeconds(.15f);
        level++;

        manager.MaxLevel = Mathf.Max(level, manager.MaxLevel);

        isMerge = false;
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;

        effect.Play();
    }
}
