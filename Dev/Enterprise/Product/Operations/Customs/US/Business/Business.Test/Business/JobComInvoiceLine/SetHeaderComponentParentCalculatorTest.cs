using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SetHeaderComponentParentCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateWhenJI_ParentChanges()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertEquals("US_SecondarySPI should be V as it is linked to X line", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);

			invoiceLine2.JI_ParentID = ZGuid.Empty;
			AssertEquals("US_SecondarySPI is empty as it is not linked to X line any more", ZString.Empty, invoiceLine2.US_SecondarySPI);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			AssertEquals("US_SecondarySPI is empty as it is not linked to X line", ZString.Empty, invoiceLine4.US_SecondarySPI);

			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine4.JI_ParentID = ZGuid.Empty;
			AssertEquals("US_SecondarySPI should stay as 'H'", SecondarySpecProgIndicatorList.Codes.H, invoiceLine4.US_SecondarySPI);

			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine4.JI_ParentID = invoiceLine2.PK;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("US_SecondarySPI should be V as it is linked to X line", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);
			AssertEquals("US_SecondarySPI should be V as it is linked to V line", SecondarySpecProgIndicatorList.Codes.V, invoiceLine4.US_SecondarySPI);
		}

		public void TestUpdateWhenUS_SecondarySPIChanges()
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine.PK;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("All children lines become V lines", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);
			AssertEquals("All children lines become V lines", SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SecondarySPI);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			AssertNotEquals("Child line is not a V line any more", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);
			AssertNotEquals("Child line is not a V line any more", SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SecondarySPI);

			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("JI_Parent should have been cleared", ZGuid.Empty, invoiceLine2.JI_ParentID);

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("invoiceLine4's JI_ParentID is calculated", invoiceLine2.PK, invoiceLine4.JI_ParentID);

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("invoiceLine6's JI_ParentID is calculated", invoiceLine5.PK, invoiceLine6.JI_ParentID);

			invoiceLine6.US_SecondarySPI = ZString.Empty;
			AssertEquals("invoiceLine6's JI_ParentID is cleared as it was pointing to X line", ZGuid.Empty, invoiceLine6.JI_ParentID);
		}

		public void TestUpdateWhenSetIndicatorChangesForACE()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = invoiceLine.PK;

			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("All children lines become V lines", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SetInd);
			AssertEquals("All children lines become V lines", SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SetInd);

			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("JI_Parent should have been cleared", ZGuid.Empty, invoiceLine2.JI_ParentID);

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("invoiceLine4's JI_ParentID is calculated", invoiceLine2.PK, invoiceLine4.JI_ParentID);

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("invoiceLine6's JI_ParentID is calculated", invoiceLine5.PK, invoiceLine6.JI_ParentID);

			invoiceLine6.US_SetInd = ZString.Empty;
			AssertEquals("invoiceLine6's JI_ParentID is cleared as it was pointing to X line", ZGuid.Empty, invoiceLine6.JI_ParentID);
		}

		public void TestSettingDefaultTariffNumberToTheFirstVLine()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var firstVLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Default tariff for 1st V line", "1234567890", firstVLine.JI_Tariff);

			var secondVLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Not default tariff for 2nd V line onwards", ZString.Empty, secondVLine.JI_Tariff);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoice = declaration.Invoices.AddNew();
		}
	}
}
