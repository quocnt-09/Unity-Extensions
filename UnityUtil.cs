namespace QNT.Extension
{
    public static class UnityUtil
    {
        public static bool IsNull<T>(this T source) where T : struct
        {
            return source.Equals(default(T));
        }

    }
}
