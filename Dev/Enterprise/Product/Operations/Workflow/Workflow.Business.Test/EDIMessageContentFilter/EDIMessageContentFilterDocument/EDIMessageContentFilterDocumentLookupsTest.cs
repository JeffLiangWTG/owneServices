using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessageContentFilterDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypes()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			var helper = ObjectFactory.Get<IDocumentScanningHelper>();
			var expected = helper.GetDocTypesFromJobType(Core.Constants.DocManagerCodes.Shipment, Factory, true);

			var documentTypes = filter.UniversalShipment.Documents.AddNew().Lookups.DocumentTypes;

			AssertEquals(expected.Count, documentTypes.Count);
			foreach (var type in expected.Cast<IRefDocType>())
			{
				AssertEquals(documentTypes.GetDescriptionFromCode(type.RT_DocType), type.RT_DescMultilingual);
			}

			AssertEquals(0, filter.UniversalEvent.Documents.AddNew().Lookups.DocumentTypes.Count);
			AssertEquals(0, filter.UniversalTransaction.Documents.AddNew().Lookups.DocumentTypes.Count);
		}

		public void TestReferenceTypes()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			var helper = ObjectFactory.Get<IDocumentScanningHelper>();
			var expected = helper.GetDocTypesFromJobType(Core.Constants.DocManagerCodes.Shipment, Factory, true);

			var lookup = filter.UniversalShipment.Documents.AddNew().Lookups.ReferenceTypes;

			AssertEquals(expected.Count, lookup.Count);
			foreach (var type in expected.Cast<IRefDocType>())
			{
				AssertEquals(lookup.GetDescriptionFromCode(type.RT_DocType), type.RT_ReferenceType);
			}

			AssertEquals(0, filter.UniversalEvent.Documents.AddNew().Lookups.ReferenceTypes.Count);
			AssertEquals(0, filter.UniversalTransaction.Documents.AddNew().Lookups.ReferenceTypes.Count);
		}

		public void TestReferenceDescriptions()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			var expected = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes);

			var lookup = filter.UniversalShipment.Documents.AddNew().Lookups.ReferenceDescriptions;

			foreach (var reference in lookup)
			{
				AssertEquals(true, expected.ContainsCode(reference));
			}

			AssertEquals(expected.Count, filter.UniversalEvent.Documents.AddNew().Lookups.ReferenceDescriptions.Count);
			AssertEquals(expected.Count, filter.UniversalTransaction.Documents.AddNew().Lookups.ReferenceDescriptions.Count);
		}

		public void TestDocumentTypeSecurity()
		{
			var filter = Factory.New<EDIMessageContentFilter>();

			var documentTypes = filter.UniversalShipment.Documents.AddNew().Lookups.DocumentTypes;

			AssertEquals(true, documentTypes.ContainsCode(Core.Constants.RefDocTypes.CartageAdvice));
			AssertEquals(true, documentTypes.ContainsCode(Core.Constants.RefDocTypes.BillOfEntry));
			AssertEquals(true, documentTypes.ContainsCode(Core.Constants.RefDocTypes.CustomsAuthority));

			Env.Security.GetDocumentTypeUploadCheckPoint(Core.Constants.RefDocTypes.CartageAdvice).IsAllowed = false;
			Env.Security.GetDocumentTypeUploadCheckPoint(Core.Constants.RefDocTypes.CustomsAuthority).IsAllowed = false;

			documentTypes = filter.UniversalShipment.Documents.AddNew().Lookups.DocumentTypes;

			AssertEquals(false, documentTypes.ContainsCode(Core.Constants.RefDocTypes.CartageAdvice));
			AssertEquals(true, documentTypes.ContainsCode(Core.Constants.RefDocTypes.BillOfEntry));
			AssertEquals(false, documentTypes.ContainsCode(Core.Constants.RefDocTypes.CustomsAuthority));
		}
	}
}
