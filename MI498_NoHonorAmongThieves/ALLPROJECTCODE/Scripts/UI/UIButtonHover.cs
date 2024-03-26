#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#endregion

/// <summary>
/// Expands and shrinks button text when button is selected
/// </summary>
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler
{
    #region Attributes
    /* Serialized Fields */
    [Header("Settings")]
    [Tooltip("Size of text when hovered over.")]
    public float selectedFontSize = 40f;
    [Tooltip("Size of text when not hovered over.")]
    public float unselectedFontSize = 25f;
    [Tooltip("A delay that changes the speed that text grows and shrinks.")]
    public float timeDelay = 0.01f;
    [Tooltip("Step size to increase text font.")]
    public float stepSize = 1f;
    [Tooltip("The color of text when hovered over.")]
    public Color32 selectedColor;
    [Tooltip("The color of text when not hovered over.")]
    public Color32 unselectedColor;
    [Tooltip("Is this button triggering a start game?")]
    public bool isStartGame = false;

    /* Private */
    // Reference to the text child of this button
    private TMP_Text buttonText = null;
    // Bool to signal when this button is hovered over
    private bool selected = false;

    /* Coroutines */
    private IEnumerator enlargeCoroutine;
    private IEnumerator minimizeCoroutine;
    #endregion

    #region Methods
    
    // Start is called before the first frame update
    void Start()
    {
        CheckReferences();
        buttonText.color = unselectedColor; // Set the button text color to the unselected color on start
        GetComponent<Button>().onClick.AddListener(() => ClickSFX());
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfButtonIsSelected();
    }

    /// <summary>
    /// Checks for references needed for this script.
    /// </summary>
    private void CheckReferences()
    {
        // Get reference to text component of this button
        buttonText = this.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
    }

    public void ClickSFX()
    {
        AkSoundEngine.PostEvent("buttonClick", this.gameObject);
    }

    /// <summary>
    /// check if the current button is selected or not.
    /// </summary>
    private void CheckIfButtonIsSelected()
    {
        //Check if this button is currently selected and shoule be animating
        if (EventSystem.current.currentSelectedGameObject == this.gameObject && !selected)
        {
            EnlargeButtonText();
        }
        else if (EventSystem.current.currentSelectedGameObject != this.gameObject && selected)
        {
            MinimizeButtonText();
        }
    }

    /// <summary>
    /// Prepares the button to be ennlarged and signals coroutine to start
    /// </summary>
    private void EnlargeButtonText()
    {
        // Stop the shrinking coroutine if needed
        if (minimizeCoroutine != null) StopCoroutine(minimizeCoroutine);
        // Get button ready to enlarge and start coroutine
        selected = true;
        buttonText.color = selectedColor;
        enlargeCoroutine = IncreaseFontSize();
        AkSoundEngine.PostEvent("buttonHover", this.gameObject);
        StartCoroutine(enlargeCoroutine);
    }

    /// <summary>
    /// Prepares the button to be shrunk and signals coroutine to start.
    /// </summary>
    private void MinimizeButtonText()
    {
        // Stop the growing coroutine if needed
        if (enlargeCoroutine != null) StopCoroutine(enlargeCoroutine);
        // Get button ready to shrink and start coroutine
        selected = false;
        buttonText.color = unselectedColor;
        minimizeCoroutine = DecreaseFontSize();
        StartCoroutine(minimizeCoroutine);
    }

    /// <summary>
    /// Gets the event data when the mouse is hovering over a button and sets
    /// that button to be the current selected button
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject);
    }


    /// <summary>
    /// Coroutine to increase the size of button text when it is selected
    /// </summary>
    /// <returns></returns>
    IEnumerator IncreaseFontSize()
    {
        // Over time, increase the font size of the button text
        for (float alpha = unselectedFontSize; alpha <= selectedFontSize; alpha += stepSize)
        {
            
            buttonText.fontSize = alpha;
            yield return new WaitForSecondsRealtime(timeDelay);
        }
    }

    /// <summary>
    /// Coroutine to decrease the size of button text when it is no longer selected
    /// </summary>
    /// <returns></returns>
    IEnumerator DecreaseFontSize()
    {
        // Over time, decrease the font size of the button text
        for (float alpha = selectedFontSize; alpha >= unselectedFontSize; alpha -= stepSize)
        {
            buttonText.fontSize = alpha;
            yield return new WaitForSecondsRealtime(timeDelay);
        }
    }
    #endregion
}
