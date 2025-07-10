using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(BrokeragePlugIn))]
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestShowPreSaveDialogsContainsCalculateDutyStrategy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFeeVAT = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, description: "營業稅", startDate: ZDateTime.MinSmallDateTimeValue, endDate: ZDateTime.MaxSmallDateTime);
			taxOrFeeVAT.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			using (var plugin = new BrokeragePlugIn(Shipment))
			{
				var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
				var entryLineFeesAfterReMerge = entryHeader.MergedLines[0].Fees.Cast<CusEntryLineFee>();
				var fee = entryLineFeesAfterReMerge.FirstOrDefault(x => x.CF_ChargeType == "VAT");
				Assert(!JobDeclaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(1, entryLineFeesAfterReMerge.Count());

				fee.TW_RateOverride = "ADD";
				Assert(JobDeclaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

				plugin.ShowPreSaveDialogs();
				Assert(!JobDeclaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(2, entryLineFeesAfterReMerge.Count());

				fee.Delete();
				Assert(JobDeclaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);

				plugin.ShowPreSaveDialogs();
				Assert(!JobDeclaration.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark);
				AssertEquals(1, entryLineFeesAfterReMerge.Count());
			}
		}

		public void TestGetCreateDeclarationHelperCore()
		{
			using (var plugin = new BrokeragePlugInForTest(Shipment))
			{
				AssertType<CreateDeclarationHelper>(plugin.ExposedDeclarationHelper);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		protected override void PrepareShipmentAndMergedDeclaration()
		{
			base.PrepareShipmentAndMergedDeclaration();
			JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = (JobComInvoiceHeader)JobDeclaration.Invoices.FirstOrDefault();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2948836m;

			var invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.FirstOrDefault();
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_EnteredUnitPrice = 2948836m;
			invoiceLine.JI_VatPymntMthd = "DEF";

			JobDeclaration.ResumeApportionment();
			JobDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		class BrokeragePlugInForTest : BrokeragePlugIn
		{
			public BrokeragePlugInForTest(ForwardingShipment shipment) : base(shipment)
			{
			}

			public Customs.Business.CreateDeclarationHelper ExposedDeclarationHelper => CreateDeclarationHelper;
		}
	}
}
