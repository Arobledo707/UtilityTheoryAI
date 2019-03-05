using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
public class CanvasDebug : MonoBehaviour
{
    [SerializeField][Tooltip("The patron that is being monitored")]
    private Patron m_patron;

    private Color m_defaultColor;

    private Color kRed = Color.red;

    private Text m_movtivesText;
    private Text m_currentActionText;
    private Text m_actionsConsideredText;

    private void Awake()
    {
        m_movtivesText = transform.Find("Motives").gameObject.GetComponent<Text>();
        m_currentActionText = transform.Find("Action").gameObject.GetComponent<Text>();
        m_actionsConsideredText = transform.Find("ActionsConsidered").gameObject.GetComponent<Text>();
    }
	
	void Update()
    {
        if (m_patron != null)
        {
            UpdateMotivesText();
            UpdateCurrentAction();
            UpdateConsideredActions();
        }
    }

    private void UpdateCurrentAction()
    {
        m_currentActionText.text = "Current Action: " + m_patron.CurrentAction;
    }

    private void UpdateMotivesText()
    {
        m_movtivesText.text = "";

        foreach (KeyValuePair<Motive.Type, Motive> motive in m_patron.MotiveDictionary)
        {
            m_movtivesText.text += motive.Value.Name() + " " + motive.Value.MotiveValue() + '\n';
        }
    }

    private void UpdateConsideredActions()
    {
        m_actionsConsideredText.text = "Considered Actions:"+ '\n';

        foreach (string action in m_patron.ActionsConsidered)
        {
            m_actionsConsideredText.text += action + '\n';
        }
    }

    public void SetPatron(Patron patron)
    {
        ResetPatron();
        m_patron = patron;
        SpriteRenderer spriteRenderer = m_patron.gameObject.GetComponentInChildren<SpriteRenderer>();
        m_defaultColor = spriteRenderer.color;
        spriteRenderer.color = kRed;
    }

    private void ResetPatron()
    {
        if (m_patron != null)
        {
            m_patron.gameObject.GetComponentInChildren<SpriteRenderer>().color = m_defaultColor;
            m_patron = null;
        }
    }
}
#endif
