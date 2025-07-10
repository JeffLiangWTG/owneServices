using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using HtmlAgilityPack;
using static CargoWise.RefDbRepo.INReferenceData.Business.Constants.EDILocation;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class EDILocationParser
	{
		public EDILocationParser(StringBuilder errorBuilder)
		{
			this.errorBuilder = errorBuilder;
		}
		readonly StringBuilder errorBuilder;

		public IReadOnlyCollection<RefCusCodeList> ParseLocationData(HtmlNode locationTable)
		{
			var tableRows = locationTable.Descendants("tr").Skip(1);
			var ediLocations = tableRows.Select(row => ToEDILocation(row)).ToList();

			return PopulateRefCusCodeListFromEDILocation(ediLocations);
		}

		IReadOnlyCollection<RefCusCodeList> PopulateRefCusCodeListFromEDILocation(List<IEDILocation> locations)
		{
			var refCusCodeLists = new List<RefCusCodeList>();
			foreach (var location in locations)
			{
				var name = location.Name;
				var code = location.Code;
				var mailAddress = location.MailId;

				var transportMode = GetTransportModeFromLocationCode(code);
				if (transportMode != null)
				{
					var refCusCodeListAttribute = new RefCusCodeListAttribute { ZZE_Value = mailAddress };
					var refCusCodeOrAttributeTransportMode = new RefCusCodeOrAttributeTransportMode { ZZU_TransportMode = transportMode };

					refCusCodeLists.Add(new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = name,
						RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refCusCodeListAttribute },
						RefCusCodeOrAttributeTransportModes = new RefCusCodeOrAttributeTransportMode[] { refCusCodeOrAttributeTransportMode },
					});
				}
				else
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Invalid location code found! Skipped record for {code}");
				}
			}

			return refCusCodeLists;
		}

		static IEDILocation ToEDILocation(HtmlNode rowLocation)
		{
			var locationAttributes = rowLocation.Elements("td").Select(val => val.InnerText.Trim()).ToArray();
			return new EDILocationProvider(locationAttributes[1], locationAttributes[2], locationAttributes[3]);
		}

		static string GetTransportModeFromLocationCode(string code)
		{
			var function = code.Substring(code.Length - 1);
			switch (function)
			{
				case FunctionCodes._1:
					return TransportModes.Sea;
				case FunctionCodes._2:
					return TransportModes.Rail;
				case FunctionCodes._4:
					return TransportModes.Air;
				case FunctionCodes._5:
					return TransportModes.Mail;
				case FunctionCodes._7:
					return TransportModes.Fixed;
				case FunctionCodes._3:
				case FunctionCodes._6:
				case FunctionCodes.B:
					return TransportModes.Road;
				default:
					return null;
			}
		}
	}
}
