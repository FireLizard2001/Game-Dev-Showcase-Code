#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Mono.CSharp.Linq;
#endregion

/// <summary>
/// Expand and shrink the sub button text when parent button is selected
/// </summary>
public class UIExpandSubButtonText : MonoBehaviour
{
    #region Attributes
    /* Serialized Fields */
    [Header("Settings")]
    [Tooltip("Defualt font size of sub buttons.")]
    public float defaultFontSize = 30f;
    [Tooltip("Step size to increase button text.")]
    public float stepSize = 10f;
    [Tooltip("A delay that changes the speed that text grows and shrinks.")]
    public float timeDelay = 0.01f;

    /* Private */
    // Bool to signal when text is shown
    private bool shown = false;
    // Reference to the text child of this button
    private TMP_Text buttonText = null;

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
    /// Checks for references needed for this script.
    /// </summary>
    private void CheckReferences()
    {
        // Get reference to text component of this button
        buttonText = this.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Check if the sub buttons are supposed to be shown
    /// </summary>
    private void CheckToExpand()
    {
        if (transform.parent.gameObject.GetComponent<UIExpandSubButtons>().expanded && !shown)
        {
            ExpandText();
        }
        else if(!transform.parent.gameObject.GetComponent<UIExpandSubButtons>().expanded && shown)
        {
            ShrinkText();
        }
    }

    /// <summary>
    /// Prepares the button to be ennlarged and signals coroutine to start
    /// </summary>
    private void ExpandText()
    {
        // Stop the shrinking coroutine if needed
        if (minimizeCoroutine != null) StopCoroutine(minimizeCoroutine);
        //Get button ready to enlarge and start coroutine
        gameObject.GetComponent<Button>().enabled = true;
        gameObject.GetComponent<UIButtonHover>().enabled = true;
        shown = true;
        enlargeCoroutine = IncreaseFontSize();
        StartCoroutine(enlargeCoroutine);

    }

    /// <summary>
    /// Prepares the button to be shrunk and signals coroutine to start.
    /// </summary>
    private void ShrinkText()
    {
        // Stop the growing coroutine if needed
        if (enlargeCoroutine != null) StopCoroutine(enlargeCoroutine);
        //Get button ready to shrink and start coroutine
        gameObject.GetComponent<Button>().enabled = false;
        gameObject.GetComponent<UIButtonHover>().enabled = false;
        shown = false;
        minimizeCoroutine = DecreaseFontSize();
        StartCoroutine(minimizeCoroutine);
    }

    /// <summary>
    /// Coroutine to increase the size of button text when it is selected
    /// </summary>
    /// <returns></returns>
    IEnumerator IncreaseFontSize()
    {
        // Over time, increase the font size of the button text
        for (float alpha = 0; alpha <= defaultFontSize; alpha += stepSize)
        {
            buttonText.fontSize = alpha;
            yield return new WaitForSeconds(timeDelay);
        }
    }

    /// <summary>
    /// Coroutine to decrease the size of button text when it is no longer selected
    /// </summary>
    /// <returns></returns>
    IEnumerator DecreaseFontSize()
    {
        // Over time, decrease the font size of the button text
        for (float alpha = defaultFontSize; alpha >= 0; alpha -= stepSize)
        {
            buttonText.fontSize = alpha;
            yield return new WaitForSeconds(timeDelay);
        }
    }
    #endregion
}
