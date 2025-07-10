using System.Collections.Generic;
using System.Xml;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business;

public class CO2eXmlWriter : IXmlWriter
{
	public void WriteXML(IDataObject dataObject, SubStreamableStream stream, string nameSpace = null, IDataOverrideProvider overrideProvider = null)
	{
		ObjectFactory.Get<IXmlWriter>().WriteXML(dataObject, stream, nameSpace, overrideProvider);
		LoadAndSortXmlElements(stream);
	}

	void LoadAndSortXmlElements(SubStreamableStream stream)
	{
		var xmlDoc = new XmlDocument();
		xmlDoc.Load(stream);
		SortElementsRecursively(xmlDoc.DocumentElement);
		stream.SetLength(0);
		xmlDoc.Save(stream);
		stream.Position = 0;
	}

	void SortElementsRecursively(XmlElement element)
	{
		if (element == null)
		{
			return;
		}

		var toBeSortedElements = new List<XmlNode>();
		foreach (XmlNode child in element.ChildNodes)
		{
			if (child.NodeType == XmlNodeType.Element && ElementOrder.ContainsKey(child.Name))
			{
				toBeSortedElements.Add(child);
			}

			if (child.NodeType == XmlNodeType.Element)
			{
				SortElementsRecursively((XmlElement)child);
			}
		}

		toBeSortedElements.Sort((x, y) => GetElementOrder(x.Name).CompareTo(GetElementOrder(y.Name)));

		foreach (var child in toBeSortedElements)
		{
			element.RemoveChild(child);
			element.AppendChild(child);
		}
	}

	int GetElementOrder(string elementName)
	{
		return ElementOrder.TryGetValue(elementName, out var order) ? order : 0;
	}

	#region SuppressResourceStringsCheckRegion

	Dictionary<string, int> ElementOrder
	{
		get
		{
			return elementOrder ??= new Dictionary<string, int>
			{
				{ "DepartureFrom", 0 },
				{ "ArrivalAt", 1 },
				{ "PreCarriageShipmentCollection", 2 },
				{ "TransportLegCollection", 3 },
				{ "SubShipmentCollection", 4 },
				{ "PostCarriageShipmentCollection", 5 }
			};
		}
	}
	Dictionary<string, int> elementOrder;

	#endregion
}
