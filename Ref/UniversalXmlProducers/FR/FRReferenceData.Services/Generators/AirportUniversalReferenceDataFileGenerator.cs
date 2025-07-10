using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class AirportUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.ZoneFileName;
				yield return ApplicationConfig.Instance.AirportFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRAirportOutputFile;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			XmlDocument airportXmlDocument = new XmlDocument();
			airportXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.AirportFileName));

			var zoneList = GetZoneList();

			XmlNodeList airportList = airportXmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode airport in airportList)
			{
				var code = UniversalDataHelper.GetTagValue(airport, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(airport, "CHAMP2"));
				var startDate = UniversalDataHelper.GetStartDateFromTag(airport, "CHAMP3");
				var endDate = UniversalDataHelper.GetEndDateFromTag(airport, "CHAMP4");
				if (!UniversalDataHelper.CheckDatesAreValid(startDate, endDate))
				{
					continue;
				}

				var airportZoneId = UniversalDataHelper.GetTagValue(airport, "CHAMP5");




				if (zoneList.TryGetValue(airportZoneId, out var zone))
				{
					result.Add(new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = startDate,
						ZZD_EndDate = endDate,
						RefCusCodeListAttributes = GetAirportAttributes(zone)
					});
				}
			}

			return result.GroupBy(a => a.ZZD_Code).Select(a => a.OrderByDescending(x => x.ZZD_StartDate).First()).ToList();
		}

		static RefCusCodeListAttribute[] GetAirportAttributes(Zone zone)
		{
			var result = new List<RefCusCodeListAttribute>
			{
				UniversalDataHelper.CreateRefCusCodeListAttribute("Zone", zone.Code),
				UniversalDataHelper.CreateRefCusCodeListAttribute("PercentOutEU", zone.PercentOutEu),
				UniversalDataHelper.CreateRefCusCodeListAttribute("PercentInEu", zone.PercentInEu),
				UniversalDataHelper.CreateRefCusCodeListAttribute("PercentDomestic", zone.PercentDomestic)
			};
			return result.ToArray();
		}

		static Dictionary<string, Zone> GetZoneList()
		{
			var result = new Dictionary<string, Zone>();

			XmlDocument zoneXmlDocument = new XmlDocument();
			zoneXmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.ZoneFileName));
			XmlNodeList zoneList = zoneXmlDocument.GetElementsByTagName("ligne");

			foreach (XmlNode zone in zoneList)
			{
				var id = UniversalDataHelper.GetTagValue(zone, "CHAMP1");
				if (!result.ContainsKey(id))
				{
					result.Add(id, new Zone(HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(zone, "CHAMP2")), UniversalDataHelper.GetTagValue(zone, "CHAMP3"), UniversalDataHelper.GetTagValue(zone, "CHAMP4"), UniversalDataHelper.GetTagValue(zone, "CHAMP5")));
				}
			}

			return result;
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "EUIAT");
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			codeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var codeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codeListAttribute.IncludeColumn(x => x.ZZE_Value, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeListAttribute);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Airports";
	}
}
