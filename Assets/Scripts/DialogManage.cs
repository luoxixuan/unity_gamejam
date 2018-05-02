using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class DialogManage : MonoBehaviour {

    public class DialogItem
    {
        public string Name { get; set; }
        public string DialogText { get; set; }
        public string IconPath { get; set; }
    }

    public class DialogNode
    {
        public string DialogName { get; set; }
        public DialogItem[] DialogArr { get; set; }
    }

    public static DialogManage m_me;
    public string m_sFilePath = "Assets/Resources/Data/dialogText.txt";

    private bool m_bIsStartDialog;  // 是否开始对话
    private DialogNode[] m_arrDialogArr;    // 对话List
    private DialogNode m_curDialogNode; // 当前对话Node
    private int m_iCurDialogItemIndex;  // 当前对话Item索引

    private Text m_tTalkText;
    private Image m_imgIconImage;

    // Use this for initialization
    void Start () {
        m_me = this;

        m_tTalkText = transform.Find("TalkText").GetComponent<Text>();
        m_imgIconImage = transform.Find("IconImage").GetComponent<Image>();

        LoadDialogs();

        gameObject.SetActive(false);
        m_bIsStartDialog = false;
    }

    public void clickToNextDialog() {
        nextDialog();
    }

    private void LoadDialogs()
    {
        if(false == File.Exists(m_sFilePath))
        {
            return;
        }
        StreamReader sr = new StreamReader(m_sFilePath);
        JsonSerializer serializer = new JsonSerializer();
        DialogNode[] nodes = (DialogNode[])serializer.Deserialize(new JsonTextReader(sr), typeof(DialogNode[]));
        m_arrDialogArr = nodes;
        sr.Dispose();
    }

    //下一段话
    private void nextDialog() {
        // 开始对话
        if (true == m_bIsStartDialog)
        {
            // 有下一对话
            if (m_iCurDialogItemIndex < m_curDialogNode.DialogArr.Length)
            {
                ShowDialogText(m_curDialogNode.DialogArr[m_iCurDialogItemIndex]);
            }
            else
            {
                EndDialog();
            }
        }
    }

	// Update is called once per frame
    // TODO 对话触发时应该剥夺人物行动能力
	void Update () {
        // 暂时做的键盘
        if (Input.GetKeyUp(KeyCode.Space))
        {
            nextDialog();
        }

    }

    // 根据名称 获取对话Node
    public DialogNode GetDialogNodeByName(string name)
    {
        Debug.Log("GetDialogNodeByName: " + name);
        if (null != name && null != m_arrDialogArr)
        {
            for(int i = 0; i < m_arrDialogArr.Length; i++)
            {
                if(m_arrDialogArr[i].DialogName == name)
                {
                    Debug.Log("GetDialogNodeByName: 取到了对话内容，第一句话是" + m_arrDialogArr[i].DialogArr[0].DialogText);
                    return m_arrDialogArr[i];
                }
            }
        }
        return null;
    }

    // 根据名称 开始对话
    public void StartDialogByName(string name)
    {
        DialogNode tempNode = GetDialogNodeByName(name);
        if(null == tempNode)
        {
            return;
        }

        if(null == tempNode.DialogArr || tempNode.DialogArr.Length <= 0)
        {
            return;
        }

        gameObject.SetActive(true);
        m_curDialogNode = tempNode;
        m_iCurDialogItemIndex = 0;
        ShowDialogText(tempNode.DialogArr[m_iCurDialogItemIndex]);
        m_bIsStartDialog = true;

        //开始对话，角色不能移动
        Completed.GameManager.instance.playerInDialog = true;
        //Completed.GameManager.instance.playersTurn = false;
        Debug.Log("StartDialogByName");
    }

    public void EndDialog()
    {
        m_bIsStartDialog = false;
        m_iCurDialogItemIndex = 0;
        m_curDialogNode = null;
        gameObject.SetActive(false);

        //结束对话，角色可以移动
        Completed.GameManager.instance.playerInDialog = false;
        //Completed.GameManager.instance.playersTurn = true;
        Debug.Log("EndDialog");
    }

    // 显示单个对话Item
    public void ShowDialogText(DialogItem item)
    {
        m_tTalkText.text = item.Name + " : " + item.DialogText;
        Sprite sp = Resources.Load<Sprite>(item.IconPath);
        m_imgIconImage.sprite = sp;

        m_iCurDialogItemIndex++;
    }
}
