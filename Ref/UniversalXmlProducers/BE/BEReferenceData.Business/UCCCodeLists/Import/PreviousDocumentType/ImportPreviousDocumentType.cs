using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportPreviousDocumentType : IUCCImportCodeListDetails
	{
		public string Domain => Constants.UccConstants.ImportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccImportPreviousDocumentType;

		public string CodeListType => Constants.UccCodeListTypes.PreviousDocumentType;

		public string DataSource => Constants.DataSources.BePreviousDocuments;

		public List<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			};

		XmlWriterConfiguration IUCCImportCodeListDetails.XmlWriterConfiguration => XMLGeneration.GetCodeListTypeWriterConfiguration(CodeType);
	}
}
