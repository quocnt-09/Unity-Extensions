using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public static class UITextUtilities
{
    /// <summary>
    /// Given an input string returns if it is a URL
    /// </summary>
    /// <param name="str">Input string.</param>
    /// <returns>Whether or not input string is a URL.</returns>
    public static bool hasLinkText(string str)
    {
        string strWOHTMLTags = RemoveHtmlLikeTags(str);
        string URL_WO_HTTP_PATTERN = @"(^|[\n ])(?<url>(www|ftp)\.[^ ,""\s<]*)";
        string URL_WITH_HTTP_PATTERN = @"(^|[\n ])(?<url>(http://www\.|http://|https://)[^ ,""\s<]*)";

        Regex urlregex = new Regex(URL_WO_HTTP_PATTERN, RegexOptions.IgnoreCase);
        Regex httpurlregex = new Regex(URL_WITH_HTTP_PATTERN, RegexOptions.IgnoreCase);

        return urlregex.IsMatch(strWOHTMLTags) || httpurlregex.IsMatch(strWOHTMLTags);
    }

    /// <summary>
    /// Given an input string returns a string without any HTML like tags
    /// </summary>
    /// <param name="str">Input string.</param>
    /// <returns>String without any HTML like tags.</returns>
    public static string RemoveHtmlLikeTags(string str)
    {
        string HTML_LIKE_TAGS_PATTERN = @"<[^>]+>| ";
        return Regex.Replace(str, HTML_LIKE_TAGS_PATTERN, "").Trim();
    }

    /// <summary>
    /// Given an input Text component, clicked position in local space and UI camera returns the word in the value of the Text component that was got clicked
    /// </summary>
    /// <param name="textComp">Text component.</param>
    /// <param name="position">Click position.</param>
    /// <param name="camera">UI camera.</param>
    /// <returns>Word in the value of the Text component that was clicked.</returns>
    public static string FindIntersectingWord(Text textComp, Vector3 position, Camera camera)
    {
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(textComp.GetComponent<RectTransform>(), position, camera, out localPosition);
        int characterIndex = GetCharacterIndexFromPosition(textComp, localPosition);
        if (!string.IsNullOrWhiteSpace(textComp.text.ToCharArray()[characterIndex] + ""))
        {
            return GetWordFromCharIndex(textComp.text, characterIndex);
        }

        return "";
    }

    public static int GetCharacterIndexFromPosition(Text textComp, Vector2 pos)
    {
        TextGenerator gen = textComp.cachedTextGenerator;

        if (gen.lineCount == 0) return 0;

        int line = GetUnclampedCharacterLineFromPosition(textComp.pixelsPerUnit, pos, gen);
        if (line < 0) return 0;
        if (line >= gen.lineCount) return gen.characterCountVisible;

        int startCharIndex = gen.lines[line].startCharIdx;
        int endCharIndex = GetLineEndPosition(gen, line);

        for (int i = startCharIndex; i < endCharIndex; i++)
        {
            if (i >= gen.characterCountVisible) break;

            UICharInfo charInfo = gen.characters[i];
            Vector2 charPos = charInfo.cursorPos / textComp.pixelsPerUnit;

            float distToCharStart = pos.x - charPos.x;
            float distToCharEnd = charPos.x + (charInfo.charWidth / textComp.pixelsPerUnit) - pos.x;
            if (distToCharStart < distToCharEnd) return i;
        }

        return endCharIndex;
    }

    public static int GetCharacterIndexFromPosition(TextGenerator gen, float pixelsPerUnit, Vector2 pos)
    {
        if (gen.lineCount == 0) return -1;
        int line = GetUnclampedCharacterLineFromPosition(pixelsPerUnit, pos, gen);
        if (line < 0) return 0;
        if (line >= gen.lineCount) return gen.characterCountVisible;

        int startCharIndex = gen.lines[line].startCharIdx;
        int endCharIndex = GetLineEndPosition(gen, line);

        for (int i = startCharIndex; i < endCharIndex; i++)
        {
            if (i >= gen.characterCountVisible) break;

            UICharInfo charInfo = gen.characters[i];
            Vector2 charPos = charInfo.cursorPos / pixelsPerUnit;

            float distToCharStart = pos.x - charPos.x;
            float distToCharEnd = charPos.x + (charInfo.charWidth / pixelsPerUnit) - pos.x;
            if (distToCharStart < distToCharEnd) return i;
        }

        return endCharIndex;
    }

    private static int GetUnclampedCharacterLineFromPosition(float pixelsPerUnit, Vector2 pos, TextGenerator generator)
    {
        // transform y to local scale
        float y = pos.y * pixelsPerUnit;
        float lastBottomY = 0.0f;

        for (int i = 0; i < generator.lineCount; ++i)
        {
            float topY = generator.lines[i].topY;
            float bottomY = topY - generator.lines[i].height;

            // pos is somewhere in the leading above this line
            if (y > topY)
            {
                // determine which line we're closer to
                float leading = topY - lastBottomY;
                if (y > topY - 0.5f * leading)
                    return i - 1;
                else
                    return i;
            }

            if (y > bottomY) return i;

            lastBottomY = bottomY;
        }

        // Position is after last line.
        return generator.lineCount;
    }

    private static int GetLineStartPosition(TextGenerator gen, int line)
    {
        line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
        return gen.lines[line].startCharIdx;
    }

    private static int GetLineEndPosition(TextGenerator gen, int line)
    {
        line = Mathf.Max(line, 0);
        if (line + 1 < gen.lines.Count) return gen.lines[line + 1].startCharIdx - 1;
        return gen.characterCountVisible;
    }

    private static string GetWordFromCharIndex(string str, int characterIndex)
    {
        string firstPartOfStr = str.Substring(0, characterIndex);
        string secondPartOfStr = str.Substring(characterIndex);

        string firstPart = firstPartOfStr;
        //Check for last index of space in first part of str and get text till that
        int lastIndexOfSpace = firstPartOfStr.LastIndexOf(' ');
        if (lastIndexOfSpace != -1)
        {
            firstPart = firstPartOfStr.Substring(lastIndexOfSpace);
        }

        string secondPart = secondPartOfStr;
        //Check for first index of space in second part of str and get text till that
        int firstIndexOfSpace = secondPartOfStr.IndexOf(' ');
        if (firstIndexOfSpace != -1)
        {
            secondPart = secondPartOfStr.Substring(0, firstIndexOfSpace);
        }

        //Check for new lines in first and second parts of the word and trim it
        int IndexOfNewLineInFirstPart = firstPart.IndexOf('\n');
        if (IndexOfNewLineInFirstPart != -1)
        {
            firstPart = firstPart.Substring(0, firstPart.IndexOf('\n'));
        }

        int IndexOfNewLineInSecondPart = secondPart.IndexOf('\n');
        if (IndexOfNewLineInSecondPart != -1)
        {
            secondPart = secondPart.Substring(0, secondPart.IndexOf('\n'));
        }

        return firstPart + secondPart;
    }
}