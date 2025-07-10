using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class DeliveryNotificationsUserControlTest : TestCaseWithFactory
	{
		public void TestDeliveryNotificationPartyGroupBoxVisible()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Customs Delivery Authority", control.DeliveryNotificationPartyGroupBox.Text);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Delivery Notification Party", control.DeliveryNotificationPartyGroupBox.Text);
			}

			);
		}

		public void TestDeliveryNotificationPartyGroupBoxText()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Customs Delivery Authority", control.DeliveryNotificationPartyGroupBox.Text);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Delivery Notification Party", control.DeliveryNotificationPartyGroupBox.Text);
			}

			);
		}

		public void TestControlTextForTSW()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Customs Delivery Authority", control.DeliveryNotificationPartyGroupBox.Text);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Text", "Delivery Notification Party", control.DeliveryNotificationPartyGroupBox.Text);
			}

			);
		}

		public void TestControlVisibilityForImport()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
			}

			);
		}

		public void TestControlVisibilityForExport()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
			}

			);
		}

		public void TestControlVisibilityForECIWriteOff()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", false, control.DeliveryNotificationPartyGroupBox.Visible);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", false, control.DeliveryNotificationPartyGroupBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
			}

			);
		}

		public void TestControlVisibilityForTSWImport()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
			}

			);
		}

		public void TestControlVisibilitySwitchingBetweenECIAndFormalEntry()
		{
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				control.HandleDeclarationControlVisibilityChanged();
				AssertEquals("DeliveryNotificationPartyGroupBox.Visible", true, control.DeliveryNotificationPartyGroupBox.Visible);
			}

			);
		}

		public void TestDeliveryNotificationPartyShouldBeSentInMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "WINTERFELL";
			Factory.Save();
			DeliveryNotificationsUserControlTestRunner((control, declaration) =>
			{
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var deliveryNotificationPartyFindBox = control.Controls.Find("DeliveryNotificationPartyFindBox", true).Single() as ZOrganisationFindBox;
				deliveryNotificationPartyFindBox.CodeBox.Text = org.OH_Code;
				deliveryNotificationPartyFindBox.PerformControlValidation();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var wrapper = new CusEntryHeaderWrapper(entryHeader as Business.Declaration.FormalEntry.CusEntryHeader, null);
				AssertEquals("WINTERFELL", (wrapper as IGoodsShipment).NotifyParties.Single().Name);
				var icrConsignment = new DeclarationConsignmentWrapper(declaration);
				AssertEquals("WINTERFELL", (icrConsignment as IICRConsignment).DeliveryNotifyParties.Single().Name);
				AssertEquals("WINTERFELL", (icrConsignment as ICREConsignment).DeliveryNotifyParties.Single().Name);
			}

			);
		}

		void DeliveryNotificationsUserControlTestRunner(Action<DeliveryNotificationsUserControlForTest, JobDeclaration> action)
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			{
				using (var control = new DeliveryNotificationsUserControlForTest())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					action(control, declaration);
				}
			}
		}
	}

	class DeliveryNotificationsUserControlForTest : DeliveryNotificationsUserControl
	{
		public new ZGroupBox DeliveryNotificationPartyGroupBox => base.DeliveryNotificationPartyGroupBox;
	}
}
