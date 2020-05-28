using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextHyperLink : Text
{
    public List<string> links;

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    private static string _output;
    private List<int> linkRanges;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null) return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        var extents = rectTransform.rect.size;

        linkRanges = new List<int>();
        links = new List<string>();
        var linkInfo = GetLinkInfo(this.text);
        linkInfo = GetLinkRanges(linkInfo, 0);
        linkInfo += ".";

        /*text = text.Replace("[", "").Replace("]", "");
        text = text.Replace("</link>", "");
        text = text.Replace("<link=", "");
        text = text.Replace(">", "");*/
        // END EDIT

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(linkInfo, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;

        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3) toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3) toFill.AddUIVertexQuad(m_TempVerts);
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    private string GetLinkInfo(string input)
    {
        _output = input;
        var starCheckKey = "<link=";
        var endCheckKey = ">";
        if (_output.Contains(starCheckKey) && _output.Contains(endCheckKey))
        {
            try
            {
                _output = "";
                var start = input.Substring(0, input.IndexOf(starCheckKey, StringComparison.Ordinal) + starCheckKey.Length);
                var end = input.Substring(input.IndexOf(starCheckKey, StringComparison.Ordinal) + starCheckKey.Length);
                var content = end.Substring(0, end.IndexOf(endCheckKey, StringComparison.Ordinal));
                content = content.Replace("\"", "");
                links.Add(content);

                var final = end.Substring(end.IndexOf(endCheckKey, StringComparison.Ordinal));
                if (final.Contains(starCheckKey) && final.Contains(endCheckKey))
                {
                    final = GetLinkInfo(final);
                }

                _output = start + final;
            }
            catch (Exception e)
            {
                Debug.LogError($"<qnt> GetLinkInfo: {e}");
            }
        }

        return _output;
    }

    private string GetLinkRanges(string input, int begin)
    {
        _output = input;
        var starCheckKey = "<link=>";
        var endCheckKey = "</link>";
        if (_output.Contains(starCheckKey) && _output.Contains(endCheckKey))
        {
            try
            {
                _output = "";

                var startIndex = input.IndexOf(starCheckKey, StringComparison.Ordinal) + begin;
                linkRanges.Add(startIndex);
                var start = input.Substring(0, input.IndexOf(starCheckKey, StringComparison.Ordinal));
                var end = input.Substring(input.IndexOf(starCheckKey, StringComparison.Ordinal) + starCheckKey.Length);
                var content = end.Substring(0, end.IndexOf(endCheckKey, StringComparison.Ordinal));
                linkRanges.Add(startIndex + content.Length - 1);
                var final = end.Substring(end.IndexOf(endCheckKey, StringComparison.Ordinal) + endCheckKey.Length);

                var first = start + content;
                if (final.Contains(starCheckKey) && final.Contains(endCheckKey))
                {
                    final = GetLinkRanges(final, first.Length);
                }

                _output = first + final;
            }
            catch (Exception e)
            {
                Debug.LogError($"<qnt> GetLinkRanges: {e}");
            }
        }

        return _output;
    }

    public string FindIntersectingWord(Vector3 position, Camera uiCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, uiCamera, out var localPosition);
        int characterIndex = UITextUtilities.GetCharacterIndexFromPosition(cachedTextGenerator, pixelsPerUnit, localPosition);
        if (characterIndex > 0)
        {
            for (int i = 0; i < linkRanges.Count; i += 2)
            {
                var begin = linkRanges[i];
                var end = linkRanges[i + 1];
                if (characterIndex >= begin && characterIndex <= end)
                {
                    return links[i / 2];
                }
            }
        }

        return "";
    }
}