using Dache.Client;
using EFCachingProvider.Caching;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DacheCache.Provider {
    public sealed class DacheCache : ICache {

        private static readonly string _entitySetRelationsKey = "__EntitySetRelationship__";

        private static readonly CacheClient _client = new CacheClient();

        public DacheCache() {
            List<CacheEntry> relationships = null;
            if (!_client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out relationships)) {
                relationships = new List<CacheEntry>();
                _client.AddOrUpdate(_entitySetRelationsKey, relationships);
            }
        }

        public bool ShouldCache(string rawKey, Dictionary<string, string> parameters) {
            string hash = String.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(rawKey)).Select(x => x.ToString("X2")));
            string type = rawKey.Substring(0, rawKey.IndexOf('|')).Trim();
            string command = rawKey.Substring(rawKey.IndexOf('|') + 1).Trim();
            command = command.Replace("\n", string.Empty);
            command = command.Replace("\t", string.Empty);
            command = command.Replace("\r", string.Empty);

            string path = null;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DacheCache.ConfigFile"])) {
                path = ConfigurationManager.AppSettings["DacheCache.ConfigFile"];
            }

            if (path == null) {
                return true;
            } else if (path.StartsWith("~/") && HttpContext.Current != null) {
                path = HttpContext.Current.Server.MapPath(path);
            }

            bool isLearning = true;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DacheCache.Learning"])) {
                Boolean.TryParse(ConfigurationManager.AppSettings["DacheCache.Learning"], out isLearning);
            }

            XmlDocument document = new XmlDocument();
            if (File.Exists(path)) {
                document.Load(path);
            } else {
                AddElement(document, "DacheCache", null, document);
            }

            XmlNodeList nodes = document.SelectNodes("/DacheCache/Entry[@ID='" + hash + "']");
            foreach (XmlNode item in nodes) {
                if (item.InnerText.Equals(command, StringComparison.InvariantCultureIgnoreCase)) {
                    try {
                        return Convert.ToBoolean(item.Attributes["Cacheable"].Value);
                    } catch {
                        return false;
                    }
                }
            }

            if (isLearning) {
                XmlElement newEntryNode = AddElement(document, "Entry", command, document.LastChild);
                newEntryNode.Attributes.Append(CreateAttribute(document, "ID", hash));
                newEntryNode.Attributes.Append(CreateAttribute(document, "Type", type));
                newEntryNode.Attributes.Append(CreateAttribute(document, "Cacheable", "false"));
                document.Save(path);
            }
            return false;
        }

        private static XmlElement AddElement(XmlDocument document, string name, string innerText, XmlNode parentNode) {
            XmlElement element = document.CreateElement(name);
            element.InnerText = innerText;
            if (parentNode != null) {
                parentNode.AppendChild(element);
            }
            return element;
        }

        private XmlAttribute CreateAttribute(XmlDocument document, string name, string value) {
            XmlAttribute attribute = document.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
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

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTime absoluteExpiration) {
            List<CacheEntry> cacheEntries = null;
            _client.TryGet<List<CacheEntry>>(_entitySetRelationsKey, out cacheEntries);
            Guid cacheKey = Guid.NewGuid();
            if (cacheEntries != null) {
                CacheEntry cacheEntry = cacheEntries.SingleOrDefault(x => x.Key == key);
                if (cacheEntry == null) {
                    cacheEntries.Add(new CacheEntry(cacheKey, key, dependentEntitySets));
                } else {
                    cacheEntry.CacheKey = cacheKey;
                }
                _client.AddOrUpdate(_entitySetRelationsKey, cacheEntries);
            }
            _client.AddOrUpdate(cacheKey.ToString(), value, new TimeSpan(0, 30, 0));
        }
    }
}
