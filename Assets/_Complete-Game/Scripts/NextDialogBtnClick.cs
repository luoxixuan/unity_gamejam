using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDialog : MonoBehaviour
{

    public void Click()
    {
        Debug.Log("Button Clicked. ClickDialog.");
        if (null == DialogManage.m_me)
        {
            return;
        }
        DialogManage.m_me.clickToNextDialog();
    }
}
