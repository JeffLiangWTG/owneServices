using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashAdvanceRequestDocumentSupporter))]
	sealed class CashAdvanceRequestDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			AccCashAdvanceRequestHeader cashAdvanceRequest = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			AssertEquals("Document Supporter should be of type", typeof(CashAdvanceRequestDocumentSupporter), cashAdvanceRequest.CashAdvanceRequestDocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext ARInvoice is supported", true, CashAdvanceRequestDocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.CashAdvanceRequest))));
			AssertEquals("DataContext ARInvoice is supported", true, CashAdvanceRequestDocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.GenericFreightJob))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CashAdvanceRequest, CashAdvanceRequestDocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		public void TestGetDocumentWrappersInternal()
		{
			DocumentWrapper[] wrappers = CashAdvanceRequestDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CashAdvanceRequest, null);
			AssertNotNull("result should not be null", wrappers);

			wrappers = CashAdvanceRequestDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotNull("result should not be null", wrappers);
		}

		public void TestGetContactOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Org 1";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Org 2";
			org2.MainAddress.OA_Address1 = "2 Street";
			org2.OH_RL_NKClosestPort = "AUSYD";

			var cashAdvanceRequest = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvanceRequest.CAH_OH_Organization = org2.PK;
			AssertEquals(org2, cashAdvanceRequest.CashAdvanceRequestDocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Receivables, DocumentDirection.ANY).OrgHeader);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AccCashAdvanceRequestHeader>();
		}

		CashAdvanceRequestDocumentSupporter CashAdvanceRequestDocumentSupporter
		{
			get { return new CashAdvanceRequestDocumentSupporter(Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>()); }
		}

		#endregion
	}
}
