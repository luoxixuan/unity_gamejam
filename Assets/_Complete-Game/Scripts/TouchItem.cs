using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchItem : MonoBehaviour, IPointerClickHandler
{
    public SceneDecorateItem decorateItem;
    public int touchRadiusSquare = 2;
    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            Vector2 touchPoint = Camera.main.ScreenToWorldPoint(touch.position);
            float distance = (gameObject.transform.position.x - touchPoint.x) + (gameObject.transform.position.y - touchPoint.y);
            distance = Mathf.Abs(distance);
            if (touchRadiusSquare > distance) {
                Debug.Log("TouchItem::Update: touch item");
                onClick();
            }
        }
#endif
    }

    private void onClick() {
        if (decorateItem)
        {
            decorateItem.doTrigger();
        }
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("TouchItem::OnPointerClick: click item");
        onClick();

    }

    #endregion
}
