using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentDocumentSupporter))]
	internal class WhsAdjustmentDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestGetContactOrganisation()
		{
			var adjustment = (WhsAdjustment)BusinessObject;
			var client = Factory.New<OrgHeader>();
			adjustment.WD_OH_Client = client.PK;
			var contact = adjustment.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", adjustment.Client.PK, contact.OrgHeader.PK);
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsAdjustment, DocSupporter.BusinessContext);
		}

		#region TestSupportedDataContexts

		public override void TestSupportedDataContexts()
		{
			AssertEquals(true, DocSupporter.ListOfSupportedDataContexts.ContainsCode(Constants.DataContext.WhsAdjustment));
			AssertEquals(true, DocSupporter.ListOfSupportedDataContexts.ContainsCode(Constants.DataContext.GenericFreightJob));
		}

		#endregion

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers()
		{
			DocumentWrapper[] adjustmentDocumentWrappers = DocSupporter.GetDocumentWrappers(Constants.DataContext.WhsAdjustment, null);
			AssertEquals("Document Wrapper for WhsAdjustment Data Context should be created.", 1, adjustmentDocumentWrappers.Length);
			AssertEquals("Incorrect type of Document Wrapper.", "DocWhsAdjustment", adjustmentDocumentWrappers[0].GetType().Name);
			AssertEquals("Wrapped object should be WhsAdjustment.", typeof(WhsAdjustment), adjustmentDocumentWrappers[0].WrappedObject.GetType());

			DocumentWrapper[] genericFreightJobDocumentWrappers = DocSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Document Wrapper for GenericFreightJob Data Context should be created.", 1, genericFreightJobDocumentWrappers.Length);
			AssertEquals("Incorrect type of Document Wrapper.", "FreightWrapperFromWhsBO", genericFreightJobDocumentWrappers[0].GetType().Name);
			AssertEquals("Wrapped object should be WhsAdjustment.", typeof(WhsAdjustment), genericFreightJobDocumentWrappers[0].WrappedObject.GetType());
		}

		#endregion

		#region TestShowReasonForNotPrinting

		protected override bool GetExpectedShowReasonForNotPrinting()
		{
			return false;
		}

		#endregion

		#region Implementation

		protected override Constants.DataContext DataContext
		{
			get { return Constants.DataContext.WhsAdjustment; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsAdjustment>();
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsAdjustmentCustomiseDocuments;

		#endregion
	}
}
