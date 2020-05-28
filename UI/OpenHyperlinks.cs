using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI pTextMeshPro;
    public TextHyperLink textComp;
    public Camera pCamera;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pTextMeshPro.enabled)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, pCamera);
            if (linkIndex != -1)
            {
                // was a link clicked?
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }

        if (textComp.enabled)
        {
            string clickedWord = textComp.FindIntersectingWord(eventData.position, pCamera);
            if (!string.IsNullOrEmpty(clickedWord))
            {
                Application.OpenURL(clickedWord);
            }
        }
    }
}