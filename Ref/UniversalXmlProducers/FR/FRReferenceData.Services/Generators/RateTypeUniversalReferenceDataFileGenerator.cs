using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RateTypeUniversalReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusRateType>
	{
		public override IEnumerable<string> InputFiles
		{
			get
			{
				yield return ApplicationConfig.Instance.RateTypeFileName;
			}
		}

		public override string OutputFile => ApplicationConfig.Instance.FRRateTypeOutputFile;

		protected override List<RefCusRateType> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusRateType>();

			var nonGroupedResult = new List<RefCusRateType>();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, ApplicationConfig.Instance.RateTypeFileName));

			XmlNodeList rateCodeList = xmlDocument.GetElementsByTagName("ligne");
			foreach (XmlNode rateCode in rateCodeList)
			{
				var code = UniversalDataHelper.GetTagValue(rateCode, "CHAMP1");
				var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(rateCode, "CHAMP2"));
				var isPort = UniversalDataHelper.GetTagValue(rateCode, "CHAMP5") == "1";
				var nature = UniversalDataHelper.GetTagValue(rateCode, "CHAMP10");
				var euCode = UniversalDataHelper.GetTagValue(rateCode, "CHAMP13");
				var enDate = UniversalDataHelper.GetTagValue(rateCode, "CHAMP16");

				var rateType = RateTypeHelper.GetRateType(code);

				var rateCodes = new RefCusRateCode[] { new RefCusRateCode() { ZY1_RateCode = code, ZY1_Description = description.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + description.Substring(1) } };
				if (code.Length == 4 && IsNumeric(code.Substring(1, 3)) && !string.IsNullOrEmpty(rateType) && string.IsNullOrEmpty(enDate))
				{
					nonGroupedResult.Add(new RefCusRateType()
					{
						ZZR_RateType = rateType,
						RefCusRateCodes = rateCodes,
					});
				}
			}

			var groupedResult = nonGroupedResult.GroupBy(a => a.ZZR_RateType).OrderBy(a => a.Key).ToList();

			foreach (var rateType in groupedResult)
			{
				var rateCodes = rateType.SelectMany(x => x.RefCusRateCodes).ToArray();
				var description = RateTypeHelper.GetRateTypeDescription(rateType.Key);
				if (!description.IsNullOrEmpty())
				{
					result.Add(new RefCusRateType()
					{
						ZZR_RateType = rateType.Key,
						ZZR_Description = description,
						RefCusRateCodes = rateCodes,
						ZZR_CustomsValueFormula = STATVALRateType.Contains(rateType.Key) ? "STATVAL" : string.Empty
					});
				}
			}

			return result;
		}

		static string[] STATVALRateType => new string[] { "RCP", "TDC", "OFI", "TPP", "TDB", "TGA", "TBO", "ROC", "CMP", "TDH", "TCG", "TIC", "TDF", "TIM", "TPC", "TDA" };

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var rateType = new EntityTypeConfiguration<RefCusRateType>(true);
			rateType.IncludeColumn(x => x.ZZR_RateType, true);
			rateType.IncludeColumn(x => x.ZZR_Description, false);
			rateType.IncludeColumnWithConstantValue(x => x.ZZR_IsPayable, false, 1);
			rateType.IncludeColumnWithConstantValue(x => x.ZZR_RX_NKFormulaCurrency, false, "");
			rateType.IncludeColumn(x => x.ZZR_CustomsValueFormula, false);
			rateType.IncludeColumnWithConstantValue(x => x.ZZR_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			rateType.IncludeColumn(x => x.RefCusRateCodes, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(rateType);

			var rateCode = new EntityTypeConfiguration<RefCusRateCode>(true);
			rateCode.IncludeColumn(x => x.ZY1_RateCode, true);
			rateCode.IncludeColumn(x => x.ZY1_Description, false);
			rateCode.IncludeColumnWithConstantValue(x => x.ZY1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(rateCode);

			return xmlWriterConfiguration;
		}

		static bool IsNumeric(string valueToCheck)
		{
			Regex regex = new Regex(@"^[-+]?\d*[.,]?\d*$");
			return regex.IsMatch(valueToCheck);
		}

		public override string DataSource => "FR - Rate types";
	}
}

