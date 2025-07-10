using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs.Parser;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMGSTCodeParser
	{
		public CARMGSTCodeParser(string workingDirectory)
		{
			this.workingDirectory = workingDirectory;
			xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusCodelistConfiguration(Constants.CodeType.CAGST));
			xmlWriter.SetDataSource(Constants.DataSource.CARMGSTCodes);
			xmlWriter.SetUpdateType(UpdateType.Full);
			cARMParserHelper = new CARMParserHelper(workingDirectory);
		}
		readonly string workingDirectory;
		readonly IXmlWriter xmlWriter;
		readonly CARMParserHelper cARMParserHelper;

		public void ParseXMLfilesIntoXML(string exportFilePath, DateTime latestUpdateOnDate)
		{
			var gstCodePatchList = CsvLoader.Deserialize<CACTaxRate>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.GSTCodePatchFileName));
			var gstCodePatchDict = gstCodePatchList.ToDictionary(gstCode => gstCode.TaxRefNumber, gstCode => gstCode);

			var refCusCodes = new List<RefCusCodeList>();
			cARMParserHelper.ParseEntriesByFilesCore<CARMGSTCode, RefCusCodeList>(Constants.CARMAPIQueryTypes.GSTCodesQueryType, gstCode =>
			{
				CACTaxRate gstCodePatch = null;
				if (gstCodePatchDict.TryGetValue(gstCode.GSTCode, out gstCodePatch))
				{
					gstCode.PopulateByCACTaxRate(gstCodePatch);
					gstCodePatchDict.Remove(gstCode.GSTCode);
				}
				var refCusCode = gstCode.ParseToRefCusCodeList();
				return refCusCode;
			}, (gstCode, refCusCode) => refCusCodes.Add(refCusCode));

			foreach (var gstCodePatchLeft in gstCodePatchDict.Values)
			{
				var gstCode = new CARMGSTCode();
				gstCode.PopulateByCACTaxRate(gstCodePatchLeft);
				var refCusCode = gstCode.ParseToRefCusCodeList();
				if (refCusCode != null)
				{ refCusCodes.Add(refCusCode); }
			}

			var sortedRefCusCodes = refCusCodes.OrderBy(o => o.ZZD_Code);
			foreach (var refCusCode in sortedRefCusCodes)
			{
				xmlWriter.PopulateData(refCusCode);
			}
			xmlWriter.SetPublicationTime(latestUpdateOnDate);
			xmlWriter.SaveXml(exportFilePath);
		}
	}
}
