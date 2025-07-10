using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs.Parser;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMRateParser
	{
		public CARMRateParser(string workingDirectory)
		{
			this._workingDirectory = workingDirectory;
			_XmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusRateTypeConfiguration());
			_XmlWriter.SetDataSource(Constants.DataSource.TariffDataRateType);
			_XmlWriter.SetUpdateType(UpdateType.Full);
			cARMParserHelper = new CARMParserHelper(workingDirectory);
		}
		readonly string _workingDirectory;
		readonly IXmlWriter _XmlWriter;
		readonly CARMParserHelper cARMParserHelper;

		public void ParseXMLfilesIntoXML(string exportFilePath, DateTime latestUpdateOnDate)
		{
			var refCusRateType = new RefCusRateType
			{
				ZZR_RateType = Constants.RateTypeExs,
				ZZR_Description = Constants.RateTypeExsDescription
			};
			var rateCodeList = new List<RefCusRateCode>();
			cARMParserHelper.ParseEntriesByFilesCore<CARMExciseTaxCode, RefCusRateCode>(Constants.CARMAPIQueryTypes.ExciseTaxCodesQueryType, exciseTaxCode => exciseTaxCode.ParseToRefCusRateCode(), (exciseTaxCode, refRateCode) => rateCodeList.Add(refRateCode));
			refCusRateType.RefCusRateCodes = rateCodeList.ToArray();
			_XmlWriter.PopulateData(refCusRateType);
			_XmlWriter.SetPublicationTime(latestUpdateOnDate);
			_XmlWriter.SaveXml(exportFilePath);
		}
	}
}
