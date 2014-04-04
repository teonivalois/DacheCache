using DacheCache.Provider.EF;
using EFCache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Linq;
using System.Web;

namespace DacheCache.Sample.Web.Models.Context {
public class AppDbConfiguration : DbConfiguration {
    public AppDbConfiguration() {
        var transactionHandler = new CacheTransactionHandler(new DacheCacheProvider());

        AddInterceptor(transactionHandler);

        Loaded +=
            (sender, args) => args.ReplaceService<DbProviderServices>(
            (s, _) => new CachingProviderServices(s, transactionHandler,
                new DacheCachingPolicy()));
    }
}
}