using System.Windows.Forms;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	[TestedType(typeof(ISFFromShipmentCreatorDialog))]
	sealed class ISFFromShipmentCreatorDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestWhenMasterBillAndHouseBillAreTheSame_00830966()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "JAEUSL20233";
			var shipmentWithConsol = consol.Shipments.AddNew();
			shipmentWithConsol.JS_HouseBill = "JAEUSL20233";
			var header1 = Factory.New<CusISFHeader>();
			var bill1 = header1.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "JAEUSL20233";
			Factory.Save();
			var creator = new ISFFromShipmentCreator(Factory);
			using (var dialog = new ISFFromShipmentCreatorDialog(creator))
			{
				dialog.Show();
				creator.ShipmentPK = shipmentWithConsol.PK;
				AssertNoExceptionThrown(delegate
				{
					dialog.CreateButton.PerformClick();
				});
			}
		}

		public void TestCreateButton_ClickWarnsForDuplicate()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_HouseBill = "HB9658446";
			header1.BF_MasterBill = "MB9658446";
			var bill1 = header1.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "COMMON3322";
			for (var i = 0; i < 12; i++)
			{
				var headerExtra = Factory.New<CusISFHeader>();
				headerExtra.BF_OceanBill = "OB9468845";
				var billExtra = headerExtra.ReferenceDatas.AddNew();
				billExtra.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
				billExtra.BB_BillNum = "COMMON3322";
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "OB9468845";
			var shipmentWithConsol = consol.Shipments.AddNew();
			shipmentWithConsol.JS_HouseBill = "HB9658446";
			var shipmentWithDeclaration = Factory.New<ForwardingShipment>();
			shipmentWithDeclaration.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithDeclaration.JS_HouseBill = "HB9658446";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipmentWithDeclaration.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB9468845";
			declaration.JE_HouseBill = "COMMON3322";
			var shipmentOK = Factory.New<ForwardingShipment>();
			shipmentOK.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var creator = new ISFFromShipmentCreator(Factory);
			using (var dialog = new ISFFromShipmentCreatorDialog(creator))
			{
				var formCached = OpenedFormCache.GetInstance();
				try
				{
					dialog.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					creator.ShipmentPK = shipmentWithConsol.PK;
					dialog.CreateButton.PerformClick();
					var expectedMessage = @"The following bill numbers are already in use on following ISF Job(s):

Bill 'OB9468845':
ISF0000002, ISF0000003, ISF0000004, ISF0000005, ISF0000006, ISF0000007, ISF0000008, ISF0000009, ISF0000010, ISF0000011
ISF0000012, ISF0000013

Bill 'HB9658446':
ISF0000001";
					AssertMultilineASCIIEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					creator.ShipmentPK = shipmentWithDeclaration.PK;
					dialog.CreateButton.PerformClick();
					expectedMessage = @"The following bill number is already in use on following ISF Job(s):

Bill 'COMMON3322':
ISF0000001, ISF0000002, ISF0000003, ISF0000004, ISF0000005, ISF0000006, ISF0000007, ISF0000008, ISF0000009, ISF0000010
ISF0000011, ISF0000012, ISF0000013";
					AssertMultilineASCIIEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					creator.ShipmentPK = shipmentOK.PK;
					dialog.CreateButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					formCached.CloseAllCachedForms();
				}
			}
		}

		public void TestCreateButton_ClickSetsFalg()
		{
			var shipmentOK = Factory.New<ForwardingShipment>();
			shipmentOK.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var creator = new ISFFromShipmentCreator(Factory);
			using (var dialog = new ISFFromShipmentCreatorDialog(creator))
			{
				var formCached = OpenedFormCache.GetInstance();
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					dialog.Show();
					AssertEquals("CreateClicked", false, dialog.CreateClicked);
					dialog.CreateButton.PerformClick();
					AssertEquals("CreateClicked", false, dialog.CreateClicked);
					AssertEquals("There are errors that need to be corrected before this ISF Job can be created.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(ZErrorMessageBox), ZFormModaliser.LastFormShownDialogForTest.GetType());
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					creator.ShipmentPK = shipmentOK.PK;
					dialog.CreateButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("CreateClicked", true, dialog.CreateClicked);
				}
				finally
				{
					formCached.CloseAllCachedForms();
				}
			}
		}

		protected override Form GetFormToBashCore() => new ISFFromShipmentCreatorDialog(new ISFFromShipmentCreator(Factory));
	}
}
