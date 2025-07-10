using System;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(AWBPrintForm))]
	public class AWBPrintFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var actions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All);

			return new AWBPrintForm(actions);
		}

		protected override bool AllowFormSizeFixed => true;

		public void TestFWBWithoutLicence()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = "AUSYD";

			consol.JK_MasterBillNum = "08112345678";

			consol.Shipments.AddNew();
			consol.Shipments[0].JS_OuterPacks = 7;
			var aWBActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.CCN))
			using (new AWBPrintForm(aWBActions))
			{
				Env.Licence.EzycargoInterface.AllowUsageForTest = false;
				try
				{
					Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					aWBActions.DoSendFWB();
					AssertContains("should show correct message", "Cargo 2000 Phase 1", UnitTestUserNotification.Instance.LastMessage.Text);

					Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					aWBActions.DoSendFHL();
					AssertContains("should show correct message", "Cargo 2000 Phase 1", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					Env.Licence.EzycargoInterface.AllowUsageForTest = null;
				}
			}
		}

		public void TestIsCreditControlledDeliveryOverriden()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "12345678";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			StmMenuItem laserMAWBMenuItem = MenuItemLoader("Laser MAWB", "AWB");
			StmMenuItem hawbBarcodeLabelsMenuItem = MenuItemLoader("HAWB Barcode Label 5 Inch", "AWB");
			StmMenuItem awbBarcodeLabelMenuItem = MenuItemLoader("AWB Barcode Label", "AWB");

			laserMAWBMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			hawbBarcodeLabelsMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			awbBarcodeLabelMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "08112345678";
			var securityStatus = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 7;

			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.LabelPrinter = awbActions.MAWBPrinter = printer.PK;
			awbActions.SendFWB = awbActions.SendFHL = false;
			awbActions.LaserAWB = true;
			awbActions.CarrierAWB = awbActions.NeutralAWB = false;
			awbActions.PrintMasterAirWaybill = awbActions.PrintBarcodeLabel = true;
			awbActions.PrintConsignmentSecurityDeclaration = false;
			awbActions.PrintHAWBBarcodeLabels = false;

			using (var awbPrintForm = new AWBPrintForm(awbActions))
			{
				awbPrintForm.Show();

				AssertEquals("Precondition", false, awbActions.HasErrors);
				AssertEquals("Precondition", false, awbActions.HasWarnings);
				AssertEquals("Precondition", 0, Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Length);

				ZButton okButton = awbPrintForm.Controls.Find("OKButton", false)[0] as ZButton;
				okButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert("Expeced print jobs created", Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Length > 0);

				shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				shipment.Consignor.OH_IsDebtor = true;
				shipment.Consignor.CompanyData.OB_AROnCreditHold = true;
				Factory.Save();

				shipment.Consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				awbActions.ValidatePrintMasterAirWaybill();
				awbActions.ValidatePrintBarcodeLabel();

				UnitTestUserNotification.Instance.AddOKAnswer();
				okButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Printing Document canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCanPrintWithNoWarnings()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "08112345678";
			var securityStatus = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;

			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.LabelPrinter = awbActions.MAWBPrinter = printer.PK;
			awbActions.SendFWB = awbActions.SendFHL = false;
			awbActions.LaserAWB = true;
			awbActions.CarrierAWB = awbActions.NeutralAWB = false;
			awbActions.PrintMasterAirWaybill = true;
			awbActions.PrintConsignmentSecurityDeclaration = false;
			awbActions.PrintBarcodeLabel = awbActions.PrintHAWBBarcodeLabels = false;

			using (var awbPrintForm = new AWBPrintForm(awbActions))
			{
				awbPrintForm.Show();

				AssertEquals("Precondition", false, awbActions.HasErrors);
				AssertEquals("Precondition", false, awbActions.HasWarnings);
				AssertEquals("Precondition", 0, Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Length);

				ZButton okButton = awbPrintForm.Controls.Find("OKButton", false)[0] as ZButton;
				okButton.PerformClick();

				Assert("Expeced print jobs created", Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Length > 0);
			}
		}

		#region Implementation

		class AWBActionsTestClass : ConsolAWBActions
		{
			public AWBActionsTestClass(ForwardingConsol consol, ActionsModeType actionsMode)
				: base(consol, actionsMode)
			{
			}

			public new void DoSendFWB()
			{
				base.DoSendFWB();
			}

			public new void DoSendFHL()
			{
				base.DoSendFHL();
			}
		}

		StmMenuItem MenuItemLoader(string documentName, string menuPath)
		{
			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, documentName);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Consol);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Documents);

			return Factory.LoadTop1<StmMenuItem>(filter);
		}

		#endregion
	}
}
