using EFCache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DacheCache.Provider {
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
