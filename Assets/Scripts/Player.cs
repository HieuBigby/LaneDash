using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private AudioClip _moveClip, _loseClip, _pointClip;

    [SerializeField] private GameplayManager _gm;
    [SerializeField] private GameObject _explosionPrefab, _scoreParticlePrefab;
    [SerializeField] private List<Transform> _transLineBGs;

    private bool canClick;
    private int currentPoleIndex = 0;
    
    private Vector2 touchStartPos;
    private bool isSwiping = false;
    [SerializeField] private float swipeThreshold = 50f;

    private void Awake()
    {
        canClick = true;
        currentPoleIndex = 1; // Start at middle pole
    }

    private void Update()
    {
        if(canClick)
        {
            // Keyboard input
            if(Input.GetKeyDown(KeyCode.A))
            {
                MoveToPole(-1); // Move left
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                MoveToPole(1); // Move right
            }
            
            // Detect swipe input
            if(Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                isSwiping = true;
            }
            
            if(Input.GetMouseButtonUp(0) && isSwiping)
            {
                Vector2 touchEndPos = Input.mousePosition;
                float swipeDelta = touchEndPos.x - touchStartPos.x;
                
                if(Mathf.Abs(swipeDelta) > swipeThreshold)
                {
                    if(swipeDelta > 0) // Swipe right
                    {
                        MoveToPole(1);
                    }
                    else // Swipe left
                    {
                        MoveToPole(-1);
                    }
                }
                
                isSwiping = false;
            }
        }
    }
    
    private void MoveToPole(int direction)
    {
        int targetIndex = currentPoleIndex + direction;
        
        // Clamp to valid pole indices (0, 1, 2)
        if(targetIndex >= 0 && targetIndex < _transLineBGs.Count)
        {
            StartCoroutine(MoveToTargetPole(targetIndex));
            SoundManager.Instance.PlaySound(_moveClip);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySound(_loseClip);
            _gm.GameEnded();
        }
    }

    [SerializeField] private float _moveTime;
    private IEnumerator MoveToTargetPole(int targetIndex)
    {
        canClick = false;
        
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new Vector3(_transLineBGs[targetIndex].localPosition.x, startPos.y, startPos.z);
        
        float speed = 1 / _moveTime;
        float timeElasped = 0f;
        
        while(timeElasped < 1f)
        {
            timeElasped += speed * Time.fixedDeltaTime;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, timeElasped);
            yield return new WaitForFixedUpdate();
        }
        
        transform.localPosition = targetPos;
        currentPoleIndex = targetIndex;
        canClick = true;
    }
}
