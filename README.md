Installation
--------------

```sh
PM> Install-Package DacheCache.Provider.EF -Pre
```

Install Dache (http://www.getdache.net) and follow the configuration instructions (https://github.com/ironyx/dache#installation-instructions). 

How to use it
========

To use it, you need first configure EF using code based configuration. Here is an example of such file:

```cs
public class MyApplicationDbConfiguration : DbConfiguration {
    public AppDbConfiguration() {
        var transactionHandler = new CacheTransactionHandler(new DacheCacheProvider());

        AddInterceptor(transactionHandler);

        Loaded +=
            (sender, args) => args.ReplaceService<DbProviderServices>(
            (s, _) => new CachingProviderServices(s, transactionHandler,
                new DacheCachingPolicy()));
    }
}
```

Done! You now have a L2 cache for your EF6 Data Context.

Credits
========

Many thanks to Aleyant Systems, LLC (http://www.aleyant.com) that pushed me towards this research and encouraged me to share our findings.
