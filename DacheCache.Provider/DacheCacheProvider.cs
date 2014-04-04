using Dache.Client;
using EFCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DacheCache.Provider {
    public sealed class DacheCacheProvider : ICache {

        private static readonly string _entitySetRelationsKey = "__EntitySetRelationship__";

        private static readonly CacheClient _client = new CacheClient();

        public DacheCacheProvider() {
            List<CacheEntry> relationships = null;
            if (!_client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out relationships)) {
                relationships = new List<CacheEntry>();
                _client.AddOrUpdate(_entitySetRelationsKey, relationships);
            }
        }

        public bool GetItem(string key, out object value) {
            List<CacheEntry> cacheEntries = null;
            _client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out cacheEntries);
            if (cacheEntries != null) {
                CacheEntry cacheEntry = cacheEntries.SingleOrDefault(x => x.Key == key);
                if (cacheEntry != null) {
                    return _client.TryGet<object>(cacheEntry.CacheKey.ToString(), out value);
                }
            }
            value = null;
            return false;
        }

        public void InvalidateItem(string key) {
            List<CacheEntry> cacheEntries = null;
            _client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out cacheEntries);
            if (cacheEntries != null) {
                CacheEntry cacheEntry = cacheEntries.SingleOrDefault(x => x.Key == key);
                if (cacheEntry != null) {
                    cacheEntries.Remove(cacheEntry);
                    _client.AddOrUpdate(_entitySetRelationsKey, cacheEntries);
                }
                _client.Remove(cacheEntry.CacheKey.ToString());
            }
        }

        public void InvalidateSets(IEnumerable<string> entitySets) {
            List<CacheEntry> cacheEntries = null;
            _client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out cacheEntries);
            if (cacheEntries != null) {
                Parallel.ForEach(entitySets, entitySet => {
                    IList<CacheEntry> dependentCacheEntries = cacheEntries.Where(entry => entry.DependentEntitySets.Contains(entitySet)).ToList();
                    Parallel.ForEach(dependentCacheEntries, cacheEntry => {
                        _client.Remove(cacheEntry.CacheKey.ToString());
                        cacheEntries.Remove(cacheEntry);
                        _client.AddOrUpdate(_entitySetRelationsKey, cacheEntries);
                    });
                });
            }
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTimeOffset absoluteExpiration) {
            List<CacheEntry> cacheEntries = null;
            _client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out cacheEntries);
            Guid cacheKey = Guid.NewGuid();
            if (cacheEntries != null) {
                CacheEntry cacheEntry = cacheEntries.SingleOrDefault(x => x.Key == key);
                if (cacheEntry == null) {
                    cacheEntries.Add(new CacheEntry(cacheKey, key, dependentEntitySets.ToArray()));
                } else {
                    cacheEntry.CacheKey = cacheKey;
                }
                _client.AddOrUpdate(_entitySetRelationsKey, cacheEntries);
            }
            if (slidingExpiration > TimeSpan.MinValue && slidingExpiration < TimeSpan.MaxValue) {
                _client.AddOrUpdate(cacheKey.ToString(), value, slidingExpiration);
            } else {
                _client.AddOrUpdate(cacheKey.ToString(), value, absoluteExpiration);
            }
        }
    }
}
