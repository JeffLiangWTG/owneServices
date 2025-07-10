using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingShipmentDocumentSupporterGuiQueryProviderTest : TestCaseWithFactory
	{
		public void TestRegister()
		{
			IForwardingShipmentDocumentSupporterQueryProvider provider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
			AssertNull("prerequisite", provider);

			ForwardingShipmentDocumentSupporterGuiQueryProvider.Register(Factory);

			provider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();

			AssertNotNull(provider);
			Assert(provider is ForwardingShipmentDocumentSupporterGuiQueryProvider);
		}

		public void TestPrintAWBBarcodeLabel()
		{
			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();
			AssertEquals(true, queryProvider.PrintAWBBarcodeLabel());
		}

		public void TestGetDebtorsToPrint()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();

			DocumentShipment docShipment = new DocumentShipment(shipment, Core.Constants.DataContext.ChargeSheet);
			docShipment.SetDefaultsFromDataContext();

			AssertNotNull("Prerequisite", docShipment.DebtorsToPrint);
			AssertEquals("Prerequisite", 0, docShipment.DebtorsToPrint.Count);

			DebtorToSelectFromForPrinting[] debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertNull(debtorsToPrint);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_OA_LocalChargesAddr = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;

			AccChargeCode chargeCode = CreateChargeCode("tstcode");
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			JobCharge charge1 = CreateLineCharge(job, org1.PK, 20.000M, chargeCode.PK);
			charge1.JR_OH_SellAccount = org1.PK;

			docShipment = new DocumentShipment(shipment, Core.Constants.DataContext.ChargeSheet);
			docShipment.SetDefaultsFromDataContext();

			AssertContainsExactElementsInAnyOrder("Prerequisite", new[] { org1 }, docShipment.DebtorsToPrint.Cast<DebtorToSelectFromForPrinting>().Select((d) => d.Debtor));

			debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertContainsExactElementsInAnyOrder(new[] { docShipment.DebtorsToPrint[0] }, debtorsToPrint);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			JobCharge charge2 = CreateLineCharge(job, org2.PK, 20.000M, chargeCode.PK);
			charge2.JR_OH_SellAccount = org2.PK;

			docShipment = new DocumentShipment(shipment, Core.Constants.DataContext.ChargeSheet);
			docShipment.SetDefaultsFromDataContext();

			AssertContainsExactElementsInAnyOrder("Prerequisite", new[] { org1, org2 }, docShipment.DebtorsToPrint.Cast<DebtorToSelectFromForPrinting>().Select((d) => d.Debtor));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertNull(debtorsToPrint);
			AssertEquals(typeof(DocumentChargeSheet), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertContainsExactElementsInAnyOrder(new[] { docShipment.DebtorsToPrint[0], docShipment.DebtorsToPrint[1] }, debtorsToPrint);
			AssertEquals(typeof(DocumentChargeSheet), ZFormModaliser.LastFormShownDialogForTest.GetType());

			docShipment.DebtorsToPrint[0].OH_Calc_PrintDebtor = ZBool.False;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			debtorsToPrint = queryProvider.GetDebtorsToPrint(docShipment);

			AssertContainsExactElementsInAnyOrder(new[] { docShipment.DebtorsToPrint[1] }, debtorsToPrint);
			AssertEquals(typeof(DocumentChargeSheet), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestGetImportCargoLabelToPrint()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var importCargoLabel = new DocumentImportCargoLabel(shipment);

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(null, queryProvider.GetImportCargoLabelToPrint(importCargoLabel));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(importCargoLabel, queryProvider.GetImportCargoLabelToPrint(importCargoLabel));
		}

		public void TestGetLetterOfIndemnityOptions()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			DocumentShipment documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.LetterOfIndemnity);

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			LetterOfIndemnityOptions options = queryProvider.GetLetterOfIndemnityOptions(documentShipment);

			AssertNull(options);
			AssertEquals(typeof(DocumentNewDetailsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			options = queryProvider.GetLetterOfIndemnityOptions(documentShipment);

			AssertNotNull(options);
			AssertEquals(typeof(DocumentNewDetailsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			AssertEquals(false, options.ChangeMarksAndNumbers);
			AssertEquals(null, options.NewMarksAndNumbers);

			AssertEquals(false, options.ChangeGoodsDescription);
			AssertEquals(null, options.NewGoodsDescription);

			AssertEquals(false, options.ChangeWeight);
			AssertEquals(0m, options.NewWeight);
			AssertEquals(null, options.NewWeightUnit);

			AssertEquals(false, options.ChangeVolume);
			AssertEquals(0m, options.NewVolume);
			AssertEquals(null, options.NewVolumeUnit);
		}

		public void TestGetTransportToPrint()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			DocumentShipment documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.LetterOfIndemnity);

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();

			AssertNull(queryProvider.GetTransportToPrint(documentShipment));
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			Transport transport1 = shipment.Transports.AddNew();

			AssertEquals(transport1, queryProvider.GetTransportToPrint(documentShipment));
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			Transport transport2 = shipment.Transports.AddNew();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			AssertNull(queryProvider.GetTransportToPrint(documentShipment));
			AssertEquals(typeof(DocumentSelectTransportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			documentShipment.SelectedTransport = transport2;

			AssertEquals(transport2, queryProvider.GetTransportToPrint(documentShipment));
			AssertEquals(typeof(DocumentSelectTransportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestPrintShiLianDan()
		{
			const string messageForSLDAtShipmentAndPackLevel = "SLD number has been entered on the Shipment and also on the pack lines. The Shi Lian Dan will only use the SLD entered at the shipment level (refer to References > SLD type). If you’d like to print the document per Shi Lian Dan number at the pack level, remove the SLD from the Shipment and enter the number on each packline using the Shipping Order/Shi Lian Dan column.";
			const string messageForMissingSLDOnPacklines = "At least one packline has no Shipping Order/Shi Lian Dan number. The Shi Lian Dan document will only be issued for the packs where the Shipping Order/Shi Lian Dan number exists.";

			var shipment = Factory.New<ForwardingShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = new ForwardingShipmentDocumentSupporterGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			Assert("No SLD at shipment or pack level: cancel printing", !queryProvider.PrintShiLianDan(shipment));
			AssertEquals(messageForMissingSLDOnPacklines, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			pack1.JL_ExportRefNumber = "123";
			Assert("Missing SLD at pack level: cancel printing", !queryProvider.PrintShiLianDan(shipment));
			AssertEquals(messageForMissingSLDOnPacklines, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var sld = shipment.Numbers.AddNew();
			sld.CE_RN_NKCountryCode = "CN";
			sld.CE_EntryType = "SLD";
			sld.CE_EntryNum = "XYZ";
			Assert("SLD at both shipment and pack level: print anyway", queryProvider.PrintShiLianDan(shipment));
			AssertEquals(messageForSLDAtShipmentAndPackLevel, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			pack1.JL_ExportRefNumber = "";
			Assert("SLD at shipment level only: print without dialog", queryProvider.PrintShiLianDan(shipment));
			Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			pack1.JL_ExportRefNumber = "XYZ";
			pack2.JL_ExportRefNumber = "XYZ";
			Assert("SLDs at shipment and pack levels match: print without dialog", queryProvider.PrintShiLianDan(shipment));
			Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		#region Implementaion

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "test charege code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			return code;
		}

		JobCharge CreateLineCharge(JobHeader haderBizObj, ZGuid chargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = haderBizObj.PK;
			charge.JR_GE = haderBizObj.JH_GE;
			charge.JR_GB = haderBizObj.JH_GB;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalSellAmt = amount;
			charge.JR_OH_SellAccount = chargesPK;
			return charge;
		}

		#endregion
	}
}
