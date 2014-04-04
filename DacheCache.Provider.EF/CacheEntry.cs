using System;
using System.Collections.Generic;

namespace DacheCache.Provider.EF {

    [Serializable]
    internal class CacheEntry : IEquatable<CacheEntry> {

        internal Guid CacheKey { get; set; }

        internal string Key { get; set; }

        internal IEnumerable<string> DependentEntitySets { get; set; }

        public CacheEntry(Guid cacheKey, string key, IEnumerable<string> dependentEntitySets) {
            this.CacheKey = cacheKey;
            this.Key = key;
            this.DependentEntitySets = dependentEntitySets;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj) {
            CacheEntry other = obj as CacheEntry;
            if (other == null) {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="CacheEntry"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The other cache entry.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified cache entry is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CacheEntry other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }

            return this.Key.Equals(other.Key, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() {
            return this.Key.GetHashCode();
        }
    }
}
