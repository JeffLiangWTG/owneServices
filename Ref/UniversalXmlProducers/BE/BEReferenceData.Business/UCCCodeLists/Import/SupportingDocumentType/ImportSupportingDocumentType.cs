using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class ImportSupportingDocumentType : IUCCImportCodeListDetails
	{
		public string Domain => Constants.UccConstants.ImportDomain;

		public string CodeType => Constants.ZZRefCusCodeList.UccImportSupportingDocumentType;

		public string CodeListType => Constants.UccCodeListTypes.SupportingDocumentType;

		public string DataSource => Constants.DataSources.BeSupportingDocuments;

		public List<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> {
			(Constants.AttributeNames.Level, Constants.AttributeValues.Header),
			(Constants.AttributeNames.Level, Constants.AttributeValues.Item),
			};

		XmlWriterConfiguration IUCCImportCodeListDetails.XmlWriterConfiguration => XMLGeneration.GetCodeListTypeWriterConfiguration(CodeType);
	}
}
