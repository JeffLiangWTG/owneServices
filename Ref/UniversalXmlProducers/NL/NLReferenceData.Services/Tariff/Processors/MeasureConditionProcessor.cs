using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Business;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class MeasureConditionProcessor
	{
		public MeasureCondition ConvertXElementToModel(XElement element)
		{
			var model = new MeasureCondition
			{
				ConditionCode = element.Attribute(at + "conditionCode")?.Value ?? string.Empty,
				National = element.Attribute(at + "national")?.Value ?? string.Empty,
				DateStart = GetDateTimeFromElement(element, at + "dateStart"),
				Type = element.Attribute(at + "type")?.Value ?? string.Empty,
				ChangeType = element.Attribute(at + "changeType")?.Value ?? string.Empty,
			};

			var descriptions = new List<MeasureConditionDescription>();
			foreach (var mcElement in element.Elements(xmlns + "measureConditionCodeDescription"))
			{
				var newItem = ConvertXElementToMeasureDescription(mcElement);
				descriptions.Add(newItem);
			}
			model.Descriptions = descriptions;

			return model;
		}

		public void LoadData(string filename)
		{
			MeasureConditionCodes = new List<MeasureCondition>();

			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				using (var xmlReader = XmlReader.Create(fileStream))
				{
					if (xmlReader.ReadToFollowing(ParentElement))
					{
						using (var detailReader = xmlReader.ReadSubtree())
						{
							while (detailReader.ReadToFollowing(ElementName))
							{
								if (XNode.ReadFrom(detailReader) is XElement element)
								{
									var model = ConvertXElementToModel(element);
									MeasureConditionCodes.Add(model);
								}
							}
						}
					}
				}
			}
		}

		public List<MeasureCondition> MeasureConditionCodes { get; set; }

		MeasureConditionDescription ConvertXElementToMeasureDescription(XElement element) =>
			new MeasureConditionDescription
			{
				Language = element.Attribute(at + "languageId")?.Value ?? string.Empty,
				Description = element.Attribute(at + "description")?.Value ?? string.Empty,
				National = element.Attribute(at + "national")?.Value ?? string.Empty,
			};

		static DateTime? GetDateTimeFromElement(XElement element, XName attributeName)
		{
			DateTime? dt = null;

			if (element?.Attribute(attributeName)?.Value != null)
			{
				dt = DateTime.Parse(element.Attribute(attributeName)?.Value, CultureInfo.CurrentCulture);
			}

			return dt;
		}

		const string ParentElement = "items";
		const string ElementName = "measureConditionCode";

		readonly XNamespace at = "http://www.arcticgroup.se/tariff/arctictariff/export";
		readonly XNamespace xmlns = "http://www.arcticgroup.se/tariff/arctictariff/export";
	}
}
