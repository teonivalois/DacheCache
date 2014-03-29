using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace DacheCache.Provider {
    public class DacheCachePolicy : CachingPolicy {

        protected override bool CanBeCached(EFCachingProvider.EFCachingCommandDefinition commandDefinition) {
            if (commandDefinition.IsCacheable()) {
                using (DbCommand command = commandDefinition.CreateCommand()) {
                    return ShouldCache(command);
                }
            }
            return false;
        }

        protected override void GetCacheableRows(EFCachingProvider.EFCachingCommandDefinition cachingCommandDefinition, out int minCacheableRows, out int maxCacheableRows) {
            minCacheableRows = 0;
            maxCacheableRows = Int32.MaxValue;
        }

        public bool ShouldCache(DbCommand command) {
            string commandText = command.CommandText;
            commandText = commandText.Replace("\n", string.Empty);
            commandText = commandText.Replace("\t", string.Empty);
            commandText = commandText.Replace("\r", string.Empty);

            string hash = String.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(commandText)).Select(x => x.ToString("X2")));

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
                if (item.InnerText.Equals(commandText, StringComparison.InvariantCultureIgnoreCase)) {
                    try {
                        return Convert.ToBoolean(item.Attributes["Cacheable"].Value);
                    } catch {
                        return false;
                    }
                }
            }

            if (isLearning) {
                XmlElement newEntryNode = AddElement(document, "Entry", commandText, document.LastChild);
                newEntryNode.Attributes.Append(CreateAttribute(document, "ID", hash));
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
    }
}
