using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class WCONomenclatureCopier
	{
		public WCONomenclatureCopier(string pdfFilePath, string xlsFilePath, string configFilePath)
		{
			this.PDFFilePath = pdfFilePath;
			this.XLSFilePath = xlsFilePath;
			this.ConfigFilePath = configFilePath;
		}
		protected string PDFFilePath { get; }
		protected string XLSFilePath { get; }
		protected string ConfigFilePath { get; }

		public void ConvertToXMLFile(string outputFilePath, DateTime wcoPublicationDate)
		{
			var nomenclatures = GetWCONomenclaturesAndTransformBeforeWriting(wcoPublicationDate);
			nomenclatures.AddRange(GetWCOTariffsAndTransformBeforeWriting(wcoPublicationDate, nomenclatures));
			new NomenclaturePDFParser(nomenclatures).Update(PDFFilePath);
			new WCONomenclatureExcelParser(ConfigFilePath, XLSFilePath, nomenclatures).ConvertToXMLFile(outputFilePath, wcoPublicationDate.Date);
		}

		List<RefCusNomenclatureGroup> GetWCONomenclaturesAndTransformBeforeWriting(DateTime wcoPublicationDate)
		{
			var result = new List<RefCusNomenclatureGroup>();
			for (var i = 0; i < 10; i++)
			{
				result.AddRange(GetWCONomenclaturesAndTransformBeforeWritingCore(wcoPublicationDate, i.ToString(CultureInfo.CurrentCulture)));
			}
			foreach (var subChapterStartString in subChapterStartStrings)
			{
				result.AddRange(GetWCONomenclaturesAndTransformBeforeWritingCore(wcoPublicationDate, subChapterStartString));
			}
			return result;
		}

		List<RefCusNomenclatureGroup> GetWCONomenclaturesAndTransformBeforeWritingCore(DateTime wcoPublicationDate, string strStartWith)
		{
			var result = new List<RefCusNomenclatureGroup>();
			var wcoNomenclatures = new RefDataEntityLoader(SafeRepository).GetNomenclatureData(Constants.DataGrouping.WCO, strStartWith, wcoPublicationDate);
			foreach (var nomenclature in wcoNomenclatures)
			{
				if (!StringComparer.InvariantCultureIgnoreCase.Equals(nomenclature.ZZ5_Description, deletedDescription))
				{
					result.Add(new RefCusNomenclatureGroup
					{
						ZZ5_Value = nomenclature.ZZ5_Value,
						ZZ5_Description = nomenclature.ZZ5_Description,
						ZZ5_StartDate = nomenclature.ZZ5_StartDate.DateTime,
						ZZ5_EndDate = nomenclature.ZZ5_EndDate.DateTime,
						ZZ5_CompositeKey = nomenclature.ZZ5_CompositeKey
					});
				}
			}
			return result;
		}
		List<string> subChapterStartStrings = new List<string>() { Constants.RomanNumericCharacters.I, Constants.RomanNumericCharacters.V, Constants.RomanNumericCharacters.X };

		List<RefCusNomenclatureGroup> GetWCOTariffsAndTransformBeforeWriting(DateTime wcoPublicationDate, IEnumerable<RefCusNomenclatureGroup> wcoNomenclatures)
		{
			var result = new List<RefCusNomenclatureGroup>();
			for (var i = 0; i < 10; i++)
			{
				var wcoTariffs = new RefDataEntityLoader(SafeRepository).GetTariffData(Constants.DataGrouping.WCO, i.ToString(CultureInfo.CurrentCulture), wcoPublicationDate);
				foreach (var tariff in wcoTariffs)
				{
					if (!wcoNomenclatures.Any(x => x.ZZ5_Value == tariff.ZZ1_TariffCode) &&
						!StringComparer.InvariantCultureIgnoreCase.Equals(tariff.ZZ1_Description, deletedDescription))
					{
						result.Add(new RefCusNomenclatureGroup
						{
							ZZ5_Value = tariff.ZZ1_TariffCode,
							ZZ5_Description = tariff.ZZ1_Description,
							ZZ5_StartDate = tariff.ZZ1_StartDate.DateTime,
							ZZ5_EndDate = tariff.ZZ1_EndDate.DateTime,
							ZZ5_CompositeKey = tariff.ZZ1_CompositeKeyOnZZ5
						});
					}
				}
			}
			return result;
		}
		const string deletedDescription = "[DELETED]";
		protected virtual ISafeRepository SafeRepository => new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));
	}
}
