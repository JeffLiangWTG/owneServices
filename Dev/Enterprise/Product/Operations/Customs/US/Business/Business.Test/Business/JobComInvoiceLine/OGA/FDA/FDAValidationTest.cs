using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FDAValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationWillCalculateMergeOnce()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "91919191";
			tariff1.UE_ShortDescription = "BOB";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "91919192";
			tariff2.UE_ShortDescription = "BOB";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JZ_InvoiceCurrExRate = 1.084834m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "91919191";
			invoiceLine1.JI_LinePrice = 1000m;
			var fda1 = invoiceLine1.FDAs.AddNew();
			fda1.US_InvCurrFDAValue = 1500m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "91919191";
			invoiceLine2.JI_LinePrice = 1000m;
			var fda2 = invoiceLine2.FDAs.AddNew();
			fda2.US_InvCurrFDAValue = 1500m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var decMock = newFactory.LoadMoq<JobDeclaration>(declaration.PK);
			var dec = decMock.Object;
			var mergeManager = new MergeManager(dec);
			decMock.Protected().Setup<Customs.Business.MergeManager>("GetMergeManager").Returns(mergeManager);
			dec.RunPreSaveValidation();
			AssertEquals("Should only be once", 1, mergeManager.RequiresMergeCoreCount);
			newFactory = new BusinessObjectFactory();
			decMock = newFactory.LoadMoq<JobDeclaration>(declaration.PK);
			dec = decMock.Object;
			mergeManager = new MergeManager(dec);
			decMock.Protected().Setup<Customs.Business.MergeManager>("GetMergeManager").Returns(mergeManager);
			dec.InvoiceLines.RunPreSaveValidation();
			AssertEquals("Should only be more than once", true, mergeManager.RequiresMergeCoreCount > 1);
		}

		public void TestCheckFDAValueUSDRunningTotal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JZ_InvoiceCurrExRate = 1.084834m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.JI_LinePrice = 1000m;
			declaration.ResumeApportionment();
			invoiceLine.Validation.ValidateFDAValueUSDRunningTotalString();
			AssertHasWarning(invoiceLine.FDAValueUSDRunningTotalStringInfo, string.Format("The total USD FDA Value entered does not balance with the Customs Value ({0} USD).", 1085m));

			var fda = invoiceLine.FDAs.AddNew();
			declaration.ResumeApportionment();
			invoiceLine.Validation.ValidateFDAValueUSDRunningTotalString();
			AssertNoWarning(invoiceLine.FDAValueUSDRunningTotalStringInfo, string.Format("The total USD FDA Value entered does not balance with the Customs Value ({0} USD).", 1085m));

			fda.US_InvCurrFDAValue = 1500m;
			declaration.ResumeApportionment();
			fda.Validation.ValidateFDAValueInvCurrRunningTotal();
			AssertHasMessageError(fda.FDAValueInvCurrRunningTotalInfo, FDAValidation.TotalInvCurrFDAValueGreaterThanLinePrice);

			fda.US_InvCurrFDAValue = 1000m;
			declaration.ResumeApportionment();
			fda.Validation.ValidateFDAValueInvCurrRunningTotal();
			AssertNoMessageError(fda.FDAValueInvCurrRunningTotalInfo, FDAValidation.TotalInvCurrFDAValueGreaterThanLinePrice);
		}

		#region Implementation

		class MergeManager : Business.MergeManager
		{
			public MergeManager(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public int RequiresMergeCoreCount;
			protected override bool RequiresMergeCore
			{
				get
				{
					RequiresMergeCoreCount++;
					return base.RequiresMergeCore;
				}
			}
		}

		#endregion
	}
}
