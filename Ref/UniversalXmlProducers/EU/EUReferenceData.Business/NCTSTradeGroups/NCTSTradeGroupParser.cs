using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsTradeGroupParser
	{
		public NctsTradeGroupParser(StringBuilder errorBuilder)
		{
			this.errorBuilder = errorBuilder;
		}
		readonly StringBuilder errorBuilder;

		public IEnumerable<RefCusTradeGroup> ParseXML(XDocument sourceXML)
		{
			return sourceXML != null ? PopulateRefCusTradeGroupFromNcts(sourceXML) : Enumerable.Empty<RefCusTradeGroup>();
		}

		IReadOnlyList<RefCusTradeGroup> PopulateRefCusTradeGroupFromNcts(XDocument inputDoc)
		{
			var refCusTradeGroup = new List<RefCusTradeGroup>();
			if (inputDoc != null)
			{
				var entries = inputDoc.Descendants().Where(e => e.Name.LocalName == Constants.NCTS.rdentry);
				var refCusTradeGroupCountries = new List<RefCusTradeGroupCountry>();
				foreach (var entry in entries)
				{
					var status = entry.Elements().Descendants().First(s => s.Name.LocalName == Constants.NCTS.state).Value;
					if (status == Constants.NCTS.valid)
					{
						var validFrom = entry.Elements().Descendants().First(v => v.Name.LocalName == Constants.NCTS.activefrom).Value;
						var codeName = entry.Elements().First(d => d.Name.LocalName == Constants.NCTS.dataitem).Value;
						var descList = entry.Elements().Where(l => l.Name.LocalName == Constants.NCTS.lsdlist);

						var englishDescription = GetEnglishDescription();

						if (!string.IsNullOrEmpty(englishDescription))
						{
							refCusTradeGroupCountries.Add(new RefCusTradeGroupCountry
							{
								ZZB_RN_NKTradeGroupCountryCode = codeName,
								ZZB_Description = englishDescription,
								ZZB_StartDate = DateTime.ParseExact(validFrom, Constants.NCTS.startDateFormat, CultureInfo.InvariantCulture),
							});
						}
						else
						{
							errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"English description missing => Skip record! NctsTradeGroupXMLProducer: dataItem '{codeName}'");
						}

						string GetEnglishDescription()
						{
							return descList.Elements()
								.FirstOrDefault(ds => ds.Name.LocalName == Constants.NCTS.descElemName
														&& (string)ds.Attribute(Constants.NCTS.lang) == Constants.NCTS.English)?.Value;
						}
					}
				}

				refCusTradeGroup.Add(new RefCusTradeGroup { RefCusTradeGroupCountries = refCusTradeGroupCountries.ToArray() });
			}
			return refCusTradeGroup;
		}
	}
}
