using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PrintMAWBBarcodeLabelsMethodApplicator))]
	public class PrintMAWBBarcodeLabelsMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestOnCancel()
		{
			const string expectedLog = "ERROR: Operation was canceled.";

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ApplyApplicator(new BusinessObject[] { Factory.New<ForwardingConsol>() }, expectedLog);
		}

		public void TestNoConsols()
		{
			const string expectedLog = "ERROR: No consols selected.";

			ApplyApplicator(System.Array.Empty<BusinessObject>(), expectedLog);
		}

		public void TestSkip()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Skip;
			Applicator.FiveInchLabel = ZBool.True;
			Applicator.PackagesFromAWB = ZBool.True;
			Applicator.AWBLabelPrinter = CreatePrinterPK();
			Applicator.NumberOfCopies = 1;

			ForwardingConsol consolWithoutErrors = CreateConsolWithoutErrors();
			consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";
			ForwardingConsol consolWithErrors = Factory.New<ForwardingConsol>();
			consolWithErrors.JK_UniqueConsignRef = "BADCONSOL";
			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			string expectedLog =
				"WARNING: [HL BADCONSOL] is not an air consol and was skipped.\n" +
				"INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();
			expectedLog =
				"WARNING: [HL BADCONSOL] has no labels and was skipped.\n" +
				"INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			ForwardingShipment shipment = consolWithErrors.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;

			Factory.Save();
			expectedLog =
				"WARNING: [HL BADCONSOL] has errors and was skipped.\n" +
				"INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			expectedLog =
				"WARNING: [HL BADCONSOL] has errors and was skipped.\n" +
				"ERROR: All selected consols were skipped. See above for details.";
			ApplyApplicator(new BusinessObject[] { consolWithErrors }, expectedLog);
		}

		public void TestAbort()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Settings.OnErrorMessageError = AWBPrintSettings.Codes.Abort;
			Applicator.FiveInchLabel = ZBool.True;
			Applicator.PackagesFromAWB = ZBool.True;
			Applicator.AWBLabelPrinter = CreatePrinterPK();
			Applicator.NumberOfCopies = 1;

			ForwardingConsol consolWithoutErrors = CreateConsolWithoutErrors();
			consolWithoutErrors.JK_UniqueConsignRef = "GOODCONSOL";

			Factory.Save();
			string expectedLog = "INFO: [HL GOODCONSOL] processed successfully.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedLog);

			ForwardingConsol consolWithErrors = Factory.New<ForwardingConsol>();
			consolWithErrors.JK_UniqueConsignRef = "BADCONSOL";
			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();
			expectedLog = "ERROR: [HL BADCONSOL] is not an air consol.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			consolWithErrors.JK_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();
			expectedLog = "ERROR: [HL BADCONSOL] has no barcode labels.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);

			ForwardingShipment shipment = consolWithErrors.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;

			Factory.Save();
			expectedLog = "ERROR: [HL BADCONSOL] has errors.";
			ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithErrors }, expectedLog);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintMAWBBarcodeLabelsMethodApplicator(Settings, Factory);
		}

		AWBPrintSettings Settings
		{
			get { return settings ?? (settings = new AWBPrintSettings()); }
		}
		AWBPrintSettings settings;

		ForwardingConsol CreateConsolWithoutErrors()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 1;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_IsConsignee = ZBool.True;
			shipment.ConsigneePK = consignee.PK;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.OH_IsConsignor = ZBool.True;
			shipment.ConsignorPK = consignor.PK;
			return consol;
		}

		ZGuid CreatePrinterPK()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			return printer.PK;
		}

		#endregion

		new PrintMAWBBarcodeLabelsMethodApplicator Applicator => (PrintMAWBBarcodeLabelsMethodApplicator)base.Applicator;
	}
}
