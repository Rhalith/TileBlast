using System;
using System.Collections.Generic;

namespace Scripts.Utilities
{
    /// <summary>
    /// Provides extension methods for generic list manipulation.
    /// </summary>
    public static class ListExtensions
    {
        #region Private Fields

        /// <summary>
        /// Random number generator used for shuffling.
        /// </summary>
        private static readonly Random rng = new Random();

        #endregion

        #region Public Methods

        /// <summary>
        /// Shuffles the elements of the list in place using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                // Swap the elements at positions k and n.
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        #endregion
    }
}