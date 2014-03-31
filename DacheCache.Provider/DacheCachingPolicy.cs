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
        protected override bool CanBeCached(string command, IReadOnlyDictionary<string, object> parameters, ReadOnlyCollection<EntitySetBase> affectedEntitySets) {
            return ShouldCache(command, parameters);
        }

        protected override void GetCacheableRows(string command, IReadOnlyDictionary<string, object> parameters, ReadOnlyCollection<EntitySetBase> affectedEntitySets, out int minCacheableRows, out int maxCacheableRows) {
            minCacheableRows = 0;
            maxCacheableRows = Int32.MaxValue;
        }

        protected override void GetExpirationTimeout(string command, IReadOnlyDictionary<string, object> parameters, ReadOnlyCollection<EntitySetBase> affectedEntitySets, out TimeSpan slidingExpiration, out DateTimeOffset absoluteExpiration) {
            slidingExpiration = new TimeSpan(0, 3, 0);
            absoluteExpiration = DateTimeOffset.MinValue;
        }

        public bool ShouldCache(string command, IReadOnlyDictionary<string, object> parameters) {
            if (parameters != null) {
                foreach (var parameter in parameters) {
                    command = command.Replace(parameter.Key, string.Format("{0}", parameter.Value));
                }
            }

            command = command.Replace("\n", string.Empty);
            command = command.Replace("\t", string.Empty);
            command = command.Replace("\r", string.Empty);

            string hash = String.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(command)).Select(x => x.ToString("X2")));

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
                        return true;
                    }
                }
            }

            if (isLearning) {
                XmlElement newEntryNode = AddElement(document, "Entry", command, document.LastChild);
                newEntryNode.Attributes.Append(CreateAttribute(document, "ID", hash));
                newEntryNode.Attributes.Append(CreateAttribute(document, "Cacheable", "true"));
                document.Save(path);
            }
            return true;
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
    }
}
