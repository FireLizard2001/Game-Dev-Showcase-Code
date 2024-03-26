#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
#endregion

/// <summary>
/// Expands the background image for sub buttons
/// </summary>
public class UIExpandSubButtons : MonoBehaviour
{
    #region Attributes
    /* Serialized Fields */
    [Header("Settings")]
    [Tooltip("Width of background for sub buttons.")]
    public float backgroundWidth = 240f;
    [Tooltip("A delay that changes the speed that text grows and shrinks.")]
    public float timeDelay = 0.01f;
    [Tooltip("Step size to increase sub buttons.")]
    public float stepSize = 10f;

    [Header("References")]
    [Tooltip("Button that will have sub buttons expanded from.")]
    public Button parentButton = null;

    /* Hidden Public */
    [HideInInspector]
    public bool expanded = false;

    /* Coroutines */
    private IEnumerator enlargeCoroutine;
    private IEnumerator minimizeCoroutine;

    #endregion

    #region Methods
    // Start is called before the first frame update
    void Start()
    {
        CheckReferences();
    }

    // Update is called once per frame
    void Update()
    {
        CheckToExpand();
    }

    /// <summary>
    /// Checks reference needed for this script.
    /// </summary>
    private void CheckReferences()
    {
        if (parentButton == null)
        {
            Debug.LogError("Need reference to parent button that sub buttons will expand from.");
        }
    }

    /// <summary>
    /// Checks if the parent button is selected and expands or shrinks the sub button comparatively
    /// </summary>
    private void CheckToExpand()
    {
        // Check if the parent button is selected or not and expand or shrink sub buttons
        if (EventSystem.current.currentSelectedGameObject == parentButton.gameObject && !expanded)
        {
            ExpandSubButtons();
        }
        else if(EventSystem.current.currentSelectedGameObject != parentButton.gameObject && expanded)
        {
            // Loop through each of the sub buttons to check if they are selected or not
            bool childSelected = false;
            foreach (Transform child in transform)
            {
                if (EventSystem.current.currentSelectedGameObject == child.gameObject)
                {
                    childSelected = true;
                    break;
                }
            }
            // if none of the sub buttons are selected, shrink the expanded buttons
            if (!childSelected) ShrinkSubButtons();
        }
    }

    /// <summary>
    /// Prepares the sub buttons to be expanded and starts the coroutine
    /// </summary>
    private void ExpandSubButtons()
    {
        // Stop the shrinking coroutine if needed
        if (minimizeCoroutine != null) StopCoroutine(minimizeCoroutine);
        expanded = true;
        enlargeCoroutine = IncreaseFontSize();
        StartCoroutine(enlargeCoroutine);
    }

    /// <summary>
    /// Prepares the sub buttons to be shrunk and starts the coroutine
    /// </summary>
    private void ShrinkSubButtons()
    {
        // Stop the growing coroutine if needed
        if (enlargeCoroutine != null) StopCoroutine(enlargeCoroutine);
        expanded = false;
        minimizeCoroutine = DecreaseFontSize();
        StartCoroutine(minimizeCoroutine);
    }

    /// <summary>
    /// Coroutine to expand the sub buttons
    /// </summary>
    /// <returns></returns>
    IEnumerator IncreaseFontSize()
    {

        // Over time, increase the font size of the button text
        for (float alpha = 0; alpha <= backgroundWidth; alpha += stepSize)
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(alpha, 120);
            yield return new WaitForSeconds(timeDelay);
        }
    }

    /// <summary>
    /// Coroutine to shrink the sub buttons
    /// </summary>
    /// <returns></returns>
    IEnumerator DecreaseFontSize()
    {
        // Over time, decrease the font size of the button text
        for (float alpha = backgroundWidth; alpha >= 0; alpha -= stepSize)
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(alpha, 100);
            yield return new WaitForSeconds(timeDelay);
        }
    }

    #endregion
}
