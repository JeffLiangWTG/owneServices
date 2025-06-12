using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Collections.Generic;
using System;

namespace CargoWise.eHub.Products.CACustoms.Transformations.Helper
{
    public class CACustomsGroupingHelper
    {
        private static Key GenerateKey(XPathNavigator packingLineCollection, string InvoiceLine)
        {
            var key = new Key();

            if(packingLineCollection != null)
            {
                foreach (XPathNavigator packingLine in packingLineCollection.Select("PackingLine"))
                {
                    var id = packingLine.SelectSingleNode("ID").Value;
                    var isLinked = packingLine.Select("PackedItemCollection/PackedItem/CommercialInvoiceLineLink").OfType<XPathNavigator>().Select(x => x.Value).Any(x => x == InvoiceLine);
                    if (isLinked)
                    {
                        key.Add(id);
                        key.AddRange(GenerateKey(packingLine.SelectSingleNode("PackingLineCollection"), InvoiceLine));
                    }
                }
            }
            return key;
        }

        public static XPathNavigator GroupByPackingLine(XPathNodeIterator commercialInvoiceLineCollection, XPathNodeIterator packingLineCollection)
        {
            if (commercialInvoiceLineCollection.CurrentPosition == 0) commercialInvoiceLineCollection.MoveNext();
            if (packingLineCollection.CurrentPosition == 0) packingLineCollection.MoveNext();
            var xmlGroups = new XElement("Groups");
            var groups = commercialInvoiceLineCollection.Current.Select("CommercialInvoiceLine").OfType<XPathNavigator>().GroupBy(
                node => GenerateKey(packingLineCollection.Current, node.SelectSingleNode("Link").Value),
                node => node,
                new KeyEqualityComparer());
            foreach (var group in groups)
            {
                var xmlGroup = new XElement("Group", group.ToList().Select(x => new XElement("CommercialInvoiceLineID", x.SelectSingleNode("ID").Value)));
                xmlGroups.Add(xmlGroup);
            }
            return xmlGroups.CreateNavigator();
        }
    }

    internal class Key : List<string> { }

    internal class KeyEqualityComparer : GenericUnOrderedListComparer<Key, string>
    {
        public KeyEqualityComparer() : base(StringComparer.InvariantCultureIgnoreCase) { }
    }

    internal class GenericUnOrderedListComparer<T, K> : IEqualityComparer<T> where T : List<K>
    {
        protected IEqualityComparer<K> comparer;

        public GenericUnOrderedListComparer(IEqualityComparer<K> comparer)
        {
            this.comparer = comparer;
        }

        public bool Equals(T list1, T list2)
        {
            var countScores = new Dictionary<K, int>(comparer);
            list1.ForEach(item =>
            {
                if (countScores.ContainsKey(item))
                {
                    countScores[item]++;
                }
                else
                {
                    countScores.Add(item, 1);
                }
            });
            var noExtraItem = list2.TrueForAll(item =>
            {
                if (countScores.ContainsKey(item))
                {
                    countScores[item]--;
                    return true;
                }
                else
                {
                    return false;
                }
            });
            return noExtraItem && countScores.Values.All(itemCount => itemCount == 0);
        }

        public int GetHashCode(T obj)
        {
            return 1;
        }
    }
}
