using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business.Test
{
	public static class WorkflowEdiMessageTestHelper
	{
		public static IEDIMessagePurpose CreateMesagePurposeToIncludeDocuments(string xmlType, string purposeCode, params string[] eDocCodes)
		{
			var factory = new BusinessObjectFactory();
			var filter = MakeFilter(factory, purposeCode, EDIMessageContentFilterTypes.Codes.Exclude);

			EDIMessageContentFilterSpec schema = filter.UniversalShipment;
			if (xmlType == EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction)
			{
				schema = filter.UniversalTransaction;
			}
			else if (xmlType == EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent)
			{
				schema = filter.UniversalEvent;
			}

			foreach (var code in eDocCodes)
			{
				AddDocument(schema, code);
			}

			var purpose = MakePurpose(factory, purposeCode, "Blah", filter);
			factory.Save();
			return purpose;
		}

		public static EDIMessagePurpose MakePurpose(BusinessObjectFactory factory, string code, string description, EDIMessageContentFilter filter = null)
		{
			var purpose = factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = code;
			purpose.EMP_Description = "Dont let them see";
			if (filter != null)
			{
				purpose.EMP_ECF_Filter = filter.PK;
			}
			return purpose;
		}

		public static EDIMessageContentFilter MakeFilter(BusinessObjectFactory factory, string name, string filterType)
		{
			var filter = factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = name;
			filter.UniversalEvent.FilterType = filterType;
			filter.UniversalShipment.FilterType = filterType;
			filter.UniversalTransaction.FilterType = filterType;
			return filter;
		}

		public static EDIMessageContentFilterLine AddLine(EDIMessageContentFilterSpec schema, string schemaElement)
		{
			var filterLine = schema.Lines.AddNew();
			filterLine.SchemaElement = schemaElement;
			return filterLine;
		}

		public static EDIMessageContentFilterLine AddLine(EDIMessageContentFilterSpec schema, string schemaElement, string dataContext)
		{
			var filterLine = AddLine(schema, schemaElement);
			filterLine.DataContext = dataContext;
			return filterLine;
		}

		public static EDIMessageContentFilterDocument AddDocument(EDIMessageContentFilterSpec schema, string docType)
		{
			var document = schema.Documents.AddNew();
			document.DocumentType = docType;
			return document;
		}
	}
}
