using System.Collections.Generic;

namespace RedBjorn.Utils
{
    public static class ListExtensions
    {
        /// <summary>
        /// Return random list value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Random<T>(this IList<T> list)
        {
            if (list != null && list.Count > 0)
            {
                return list[UnityEngine.Random.Range(0, list.Count)];
            }
            return default(T);
        }
    }
}
