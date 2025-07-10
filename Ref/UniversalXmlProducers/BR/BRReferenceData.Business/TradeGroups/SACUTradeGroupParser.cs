using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class SACUTradeGroupParser : BaseParser
	{
		public SACUTradeGroupParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(string[] inputData, string outputFileName, DateTime? publicationDateTime = null)
		{
			var publicationTime = publicationDateTime ?? DateTime.Now;

			if (outputFileName != null)
			{
				var refCusTradeGroupLists = GetRefCusTradeGroupLists(inputData);
				var writer = Helper.GetRefCusTradeGroupWriterConfiguration();
				Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusTradeGroupLists);
			}
		}

		protected static IEnumerable<RefCusTradeGroup> GetRefCusTradeGroupLists(string[] data)
		{
			var result = new List<RefCusTradeGroup>();
			var countries = new List<RefCusTradeGroupCountry>();
			foreach (var item in data)
			{
				if (Countries.TryGetValue(item, out string value))
				{
					countries.Add(
						new RefCusTradeGroupCountry()
						{
							ZZB_Description = item,
							ZZB_RN_NKTradeGroupCountryCode = value
						});
				}
				else
				{
					throw new InvalidOperationException($"Country not found in countries table, please notify the brazilian team!");
				}
			}
			result.Add(new RefCusTradeGroup()
			{
				RefCusTradeGroupCountries = countries.ToArray()
			});
			return result;
		}

		static IReadOnlyDictionary<string, string> Countries => countries;

		static IReadOnlyDictionary<string, string> countries
			= new Dictionary<string, string>
			{
				{ "Angola", "AO" },
				{ "Zimbabwe", "ZW" },
				{ "Mozambique", "MZ" },
				{ "Zambia", "ZM" },
				{ "Malawi", "MW" },
				{ "Botswana", "BW" },
				{ "Lesotho", "LS" },
				{ "Namibia", "NA" },
				{ "Eswatini", "SZ" },
				{ "South Africa", "ZA" }
			};
	}
}
