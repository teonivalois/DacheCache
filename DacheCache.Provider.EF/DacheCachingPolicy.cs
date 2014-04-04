using EFCache;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;

namespace DacheCache.Provider.EF {
    public class DacheCachingPolicy : CachingPolicy {
        protected override bool CanBeCached(ReadOnlyCollection<EntitySetBase> affectedEntitySets) {
            return true;
        }

        protected override void GetCacheableRows(ReadOnlyCollection<EntitySetBase> affectedEntitySets, out int minCacheableRows, out int maxCacheableRows) {
            minCacheableRows = 0;
            maxCacheableRows = Int32.MaxValue;
        }

        protected override void GetExpirationTimeout(ReadOnlyCollection<EntitySetBase> affectedEntitySets, out TimeSpan slidingExpiration, out DateTimeOffset absoluteExpiration) {
            slidingExpiration = new TimeSpan(0, 3, 0);
            absoluteExpiration = DateTimeOffset.MinValue;
        }
    }
}
