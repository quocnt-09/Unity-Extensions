using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace QNT.Extension
{
    public static class UnityUitl
    {
        public static readonly string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss tt";

        public static void DrawText(GUISkin guiSkin, string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0)
        {
#if UNITY_EDITOR
            var prevSkin = GUI.skin;
            if (guiSkin == null)
                Debug.LogWarning("editor warning: guiSkin parameter is null");
            else
                GUI.skin = guiSkin;

            GUIContent textContent = new GUIContent(text);

            GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
            if (color != null) style.normal.textColor = (Color)color;
            if (fontSize > 0) style.fontSize = fontSize;

            Vector2 textSize = style.CalcSize(textContent);
            Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

            if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
            {
                var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
                Handles.Label(worldPosition, textContent, style);
            }

            GUI.skin = prevSkin;
#endif
        }

        public static void DrawRect(GUISkin guiSkin, Vector3 position, float w, float h, Color? color = null)
        {
#if UNITY_EDITOR
            var prevSkin = GUI.skin;
            if (guiSkin == null)
                Debug.LogWarning("editor warning: guiSkin parameter is null");
            else
                GUI.skin = guiSkin;

            GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
            if (color != null) style.normal.textColor = (Color)color;

            Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);

            if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
            {
                var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, screenPoint.z));
                var p1 = worldPosition + new Vector3(-w / 2f, -h / 2f, 0);
                var p2 = worldPosition + new Vector3(-w / 2f, h / 2f, 0);
                var p3 = worldPosition + new Vector3(w / 2f, h / 2f, 0);
                var p4 = worldPosition + new Vector3(w / 2f, -h / 2f, 0);

                Handles.DrawLine(p1, p2);
                Handles.DrawLine(p2, p3);
                Handles.DrawLine(p3, p4);
                Handles.DrawLine(p4, p1);
            }

            GUI.skin = prevSkin;
#endif
        }

        public static bool IsNull<T>(this T source) where T : struct
        {
            return source.Equals(default(T));
        }

        const BindingFlags flagsCopy = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static void Copy(object originalObject, object copyObject)
        {
            FieldInfo[] originFields = originalObject.GetType().GetFields(flagsCopy);
            FieldInfo[] copyFields = copyObject.GetType().GetFields(flagsCopy);

            for (var i = 0; i < originFields.Length; i++)
            {
                FieldInfo fieldInfo = originFields[i];
                try
                {
                    FieldInfo resultField = copyFields.Where(f => f.Name.Equals(fieldInfo.Name)).Select(f => f).Single<FieldInfo>();
                    if (resultField != null)
                    {
                        resultField.SetValue(copyObject, fieldInfo.GetValue(originalObject));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"<>===### Copy Error: {ex}");
                }
            }
        }

        #region GameObject

        public static void SetLocalPosition(this Transform tf, Vector3 position)
        {
            tf.localPosition = position;
        }

        public static void ChangeMaterialColor(this Transform tf, Color color)
        {
            var materialPropertyBlock = new MaterialPropertyBlock();
            try
            {
                var skinnedMeshes = tf.GetComponentsInChildren<SkinnedMeshRenderer>();
                var length = skinnedMeshes.Length;

                for (int i = 0; i < length; i++)
                {
                    SkinnedMeshRenderer skinnedMesh = skinnedMeshes[i];
                    //skinnedMesh.sharedMaterial.SetColor("_Color", color);
                    skinnedMesh.GetPropertyBlock(materialPropertyBlock);
                    //skinnedMesh.sharedMaterial.SetColor("_Color", color);
                    materialPropertyBlock.SetColor("_Color", color);
                    skinnedMesh.SetPropertyBlock(materialPropertyBlock);
                }

                var meshes = tf.GetComponentsInChildren<MeshRenderer>();
                length = meshes.Length;
                for (int i = 0; i < length; i++)
                {
                    var mesh = meshes[i];
                    mesh.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetColor("_Color", color);
                    mesh.SetPropertyBlock(materialPropertyBlock);
                    //mesh.sharedMaterial.SetColor("_Color", color);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("<>===### change color ex: " + ex);
            }
        }

        public static bool IsPointerOverUIObject(Vector2 screenPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].gameObject.GetComponent<Image>())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool LayerInLayerMask(int layer, LayerMask layerMask)
        {
            return ((1 << layer) & layerMask) != 0;
        }

        public static LayerMask AddLayerMask(this LayerMask mask, string layer)
        {
            mask = mask | (1 << LayerMask.NameToLayer(layer));
            return mask;
        }

        public static LayerMask RemoveLayerMask(this LayerMask mask, string layer)
        {
            mask = mask & ~(1 << LayerMask.NameToLayer(layer));
            return mask;
        }

        public static void SetSafeEnable(this SpriteRenderer sp, bool alue)
        {
            if (sp.enabled != alue)
            {
                sp.enabled = alue;
            }
        }

        public static string GetStringTimeFull(double totalSecond, string space = " ", string d = "d", string h = "h", string m = "m", string s = "s")
        {
            var time = TimeSpan.FromSeconds(totalSecond);
            var stringTime = "";

            /*var cl = "9A9A9A";
            d = $"<color=#{cl}>{d}</color>";
            h = $"<color=#{cl}>{h}</color>";
            m = $"<color=#{cl}>{m}</color>";
            s = $"<color=#{cl}>{s}</color>";*/

            if ((int)time.TotalDays > 0)
            {
                stringTime = $"{(int)time.TotalDays:00}{d}{space}{time.Hours:00}{h}";
            }
            else if ((int)time.TotalHours > 0)
            {
                stringTime = $"{(int)time.TotalHours:00}{h}{space}{time.Minutes:00}{m}";
            }
            else if ((int)time.TotalMinutes > 0)
            {
                stringTime = $"{(int)time.TotalMinutes:00}{m}{space}{time.Seconds:00}{s}";
            }
            else
            {
                stringTime = $"{time.Seconds:00}";
            }

            return stringTime;
        }

        public static string GetStringTimeSort(double totalSecond, string space = " ", string d = "d", string h = "h", string m = "m", string s = "s")
        {
            var time = TimeSpan.FromSeconds(totalSecond);
            var stringTime = "";

            if (time.Days > 0)
            {
                if (time.Hours == 0)
                {
                    stringTime = $"{(int)time.TotalDays}{d}";
                }
                else
                {
                    stringTime = $"{(int)time.TotalDays}{d}{space}{time.Hours}{h}";
                }
            }
            else if ((int)time.TotalHours > 0)
            {
                if (time.Minutes == 0)
                {
                    stringTime = $"{(int)time.TotalHours}{h}";
                }
                else
                {
                    stringTime = $"{(int)time.TotalHours}{h}{space}{time.Minutes}{m}";
                }
            }
            else if (time.Minutes > 0)
            {
                if (time.Seconds == 0)
                {
                    stringTime = $"{time.Minutes}{m}";
                }
                else
                {
                    stringTime = $"{time.Minutes}{m}{space}{time.Seconds}{s}";
                }
            }
            else
            {
                stringTime = $"{time.Seconds}{s}";
            }

            return stringTime;
        }

        public static string TrimName(this string name, int numChar = 10, string sp = "...")
        {
            var strBuilder = new StringBuilder();
            if (name.Length > numChar)
            {
                for (int i = 0; i <= numChar - sp.Length; i++)
                {
                    strBuilder.Append(name[i]);
                }

                strBuilder.Append(sp);
                return strBuilder.ToString();
            }

            return name;
        }

        public static RaycastHit2D CheckHitRight(Vector2 origin, float distance, LayerMask layerMask)
        {
            return Physics2D.Raycast(origin, Vector2.right, distance, layerMask);
        }

        public static RaycastHit2D CheckHitLeft(Vector2 origin, float distance, LayerMask layerMask)
        {
            return Physics2D.Raycast(origin, Vector2.left, distance, layerMask);
        }

        #endregion

        #region RANDOM

        public static int GetRandomWeightedIndex(int[] weights)
        {
            // Get the total sum of all the weights.
            int weightSum = 0;
            for (int i = 0; i < weights.Length; ++i)
            {
                weightSum += weights[i];
            }

            // Step through all the possibilities, one by one, checking to see if each one is selected.
            int index = 0;
            int lastIndex = weights.Length - 1;
            while (index < lastIndex)
            {
                if (Random.Range(0, weightSum) < weights[index])
                {
                    return index;
                }

                weightSum -= weights[index++];
            }

            // No other item was selected, so return very last index.
            return index;
        }

        #endregion

        public static void SetColor(this Image img, Color color, float alpha)
        {
            color.a = alpha;
            img.color = color;
        }

        public static void SetColor(this Image img, Color color)
        {
            img.color = color;
        }
        public static void SetColorText(this Image text, string strColor)
        {
            if (ColorUtility.TryParseHtmlString($"#{strColor}", out var color))
            {
                SetColor(text, color);
            }
        }

        public static void SetColorAlpha(this Image igm, float a)
        {
            var color = igm.color;
            color.a = a;
            igm.color = color;
        }

        public static string GetEventString(this Enum enu)
        {
            return "E_" + enu;
        }

        public static string FormatString(this int money, bool Acronym = false)
        {
            if (Mathf.Abs(money) >= 1000)
            {
                if (Acronym && Mathf.Abs(money) >= 10000)
                {
                    return AbbreviateNumber(money);
                }

                return String.Format(CultureInfo.InvariantCulture, "{0:0,0}", money);
            }

            return money.ToString();
        }

        public static string GetDecimalFormat(string input)
        {
            var split = input.Split('.');
            if (split.Length < 2) return "N0";
            var countDecimal = input.Split('.')[1].Length;
            return "N" + countDecimal;
        }

        public static string FormatDecimalString(this decimal money)
        {
            return money.ToString(GetDecimalFormat(money.ToString(CultureInfo.InvariantCulture)));
        }

        public static string FormatString(this float value, bool Acronym = false, bool forceToInt = true)
        {
            if (forceToInt)
            {
                return FormatString((int)value, Acronym);
            }
            else
            {
                var text = $"{value:0.00}";
                return text;
            }
        }

        public static string FormatString(this long value, bool Acronym = false)
        {
            return FormatString((int)value, Acronym);
        }

        static string AbbreviateNumber(this long number)
        {
            for (int i = abbrevations.Count - 1; i >= 0; i--)
            {
                KeyValuePair<long, string> pair = abbrevations.ElementAt(i);
                if (Mathf.Abs(number) >= pair.Key)
                {
                    var roundedNumber = Mathf.FloorToInt(number / pair.Key);
                    return $"{roundedNumber}{pair.Value}";
                }
            }

            return number.ToString();
        }

        private static readonly SortedDictionary<long, string> abbrevations = new SortedDictionary<long, string> {
            { 1000, "K" },
            { 1000000, "M" },
            { 1000000000, "B" },
            { 1000000000000, "T" },
        };

        public static void SetAnchoredPositionX(this RectTransform rectTransform, float value)
        {
            var anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = value;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        public static void SetAnchoredPositionY(this RectTransform rectTransform, float value)
        {
            var anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.y = value;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        public static void SetSafeActive(this GameObject obj, bool value)
        {
            if (obj == null) return;

            if (obj.activeSelf != value)
            {
                obj.SetActive(value);
            }
        }

        public static bool IsPointerOverUiObject(Vector2 screenPosition)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].gameObject.GetComponent<Image>())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region Encrypt && Decrypt

        public static string Encrypt(string input, string password = "P@ssw0rd")
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(input);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));
                using (TripleDESCryptoServiceProvider trip = new TripleDESCryptoServiceProvider() { Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform tr = trip.CreateEncryptor();
                    byte[] result = tr.TransformFinalBlock(data, 0, data.Length);
                    return Convert.ToBase64String(result, 0, result.Length);
                }
            }
        }

        public static string Decrypt(string input, string password = "P@ssw0rd")
        {
            byte[] data = Convert.FromBase64String(input);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));
                using (TripleDESCryptoServiceProvider trip = new TripleDESCryptoServiceProvider() { Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform tr = trip.CreateDecryptor();
                    byte[] result = tr.TransformFinalBlock(data, 0, data.Length);
                    return UTF8Encoding.UTF8.GetString(result);
                }
            }
        }

        #endregion

        #region ENUM

        public static T ToEnum<T>(this int value)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        #endregion

        #region INT

        public static int ToInt(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static int ToIn(this bool value)
        {
            return value ? 1 : 0;
        }

        public static int ToInt(this string str)
        {
            str = str.Replace(",", "");

            int rs = 0;
            try
            {
                rs = int.Parse(str);
            }
            catch (Exception e)
            {
                Debug.LogError("Parse To Int Error: " + e);
            }

            return rs;
        }

        #endregion

        #region FLOAT

        public static float ToFloat(this string str)
        {
            str = str.Replace(",", "");

            float rs = 0;
            try
            {
                rs = float.Parse(str);
            }
            catch (Exception e)
            {
                Debug.LogError("Parse To Float Error: " + e);
            }

            return rs;
        }

        #endregion

        #region BOOL

        public static bool ToBool(this int value)
        {
            return value == 1 ? true : false;
        }

        public static bool ToBool(this string str)
        {
            bool rs = false;
            try
            {
                rs = bool.Parse(str);
            }
            catch (Exception e)
            {
                Debug.LogError("<qnt> Parse To Bool Error: " + e);
            }

            return rs;
        }

        #endregion

        #region OTHER
        public static string TrimName(string name, string extent = "...", int numCharacter = 8)
        {
            var strName = "";

            strName = name.Length > numCharacter ? $"{name.Substring(0, numCharacter)}{extent}" : name;

            return strName;
        }
        #endregion
    }
}