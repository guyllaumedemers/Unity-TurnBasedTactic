namespace ScriptableObjects
{
    public static class IntExtensions
    {
        /// <summary>
        /// Check if value lower that zero
        /// </summary>
        /// <param name="source">source int</param>
        /// <returns>zero if lower than zero</returns>
        public static int ZeroCheck(this int source) => source < 0 ? 0 : source;
        
        /// <summary>
        /// Check if value is lower than zero
        /// </summary>
        /// <param name="source"></param>
        /// <returns>1 if lower than zero</returns>
        public static int OneCheck(this int source) => source < 0 ? 1 : source;        
    }
}