using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class DangerousGoodsManifestMessageDialogTest : TestCaseWithFactory
	{
		#region SendButtonClick ShowErrorMessage

		public void TestSendButtonClick_MessageInfoHasErrors()
		{
			var voyage = Factory.New<JobVoyage>();

			var message = new DangerousGoodsManifestMessage(voyage);
			message.Direction = ZString.Empty;

			using (var dialog = new DangerousGoodsManifestMessageDialog(message))
			{
				dialog.Show();
				dialog.sendButton.PerformClick();

				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSendButtonClick_ShipmentHasErrors()
		{
			var voyage = CreateVoyage();
			CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Bill of Lading number must be unique per vessel-voyage.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_VesselHasErrors()
		{
			var voyage = CreateVoyage();
			CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading");

			voyage.Vessel.RV_LloydsNumber = ZString.Empty;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Vessel Lloyds/IMO Number of linked Sailing Schedule is missing.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_ConsigneeHasErrors()
		{
			var voyage = CreateVoyage();
			var billOfLading = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading");

			billOfLading.ConsigneeDocumentaryAddress.E2_CompanyName = ZString.Empty;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Company Name: Please enter a Company Name, or remove the override for this Address.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_ConsignorHasErrors()
		{
			var voyage = CreateVoyage();
			var billOfLading = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading");

			billOfLading.ConsignorDocumentaryAddress.E2_CompanyName = ZString.Empty;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Company Name: Please enter a Company Name, or remove the override for this Address.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_PackLineHasErrors()
		{
			var voyage = CreateVoyage();
			var billOfLading = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading");

			billOfLading.OuterPackLines.OfType<BillOfLadingPackLine>().First().JL_PackageCount = 0;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Number of packages are required.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_UNDGHasErrors()
		{
			var voyage = CreateVoyage();
			var billOfLading = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading");

			billOfLading.OuterPackLines.OfType<BillOfLadingPackLine>().First().UNDGs.OfType<UNDGDataItem>().First().DI_PackageCount = 0;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Dangerous Goods package count is required.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_NoRegistryConfigError()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2");
			var billOfLading3 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader2, "billofLading3");
			var billOfLading4 = CreateBillOfLading(voyage, "AUSYD", "AUBNE", OrgHeader1, "billofLading4");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader2.PK, "AUMEL_OH2");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader2.PK, "AUSYD_OH2");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Amendment;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals(string.Format("Error Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", message.Issues.Count), UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertContains("Principal is not configured for sending Dangerous Goods Manifest message to AUMEL.", message.Issues.OfType<PortMessageIssue>().First().Detail);
				}
			}
		}

		public void TestSendButtonClick_NoUndg()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1", false);
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2", false);
			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertContains(string.Format("There are no shipments on this voyage loading in {0}.", message.Port), UnitTestUserNotification.Instance.LastMessage.ToString());

					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Transit;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertContains(string.Format("There are no shipments on this voyage transiting in {0}.", message.Port), UnitTestUserNotification.Instance.LastMessage.ToString());

					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertContains(string.Format("There are no shipments on this voyage discharging in {0}.", message.Port), UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		#endregion

		#region SendButtonClick NoErrorMessage

		public void TestSendButtonClick_NoErrorMessage_SingleBillOfLadings()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2");
			var billOfLading3 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader2, "billofLading3");
			var billOfLading4 = CreateBillOfLading(voyage, "AUSYD", "AUBNE", OrgHeader1, "billofLading4");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader2.PK, "AUMEL_OH2");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader2.PK, "AUSYD_OH2");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Amendment;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifestReplacement, "AUMEL", "PIM");

					AssertNull(billOfLading2.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertNull(billOfLading3.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertNull(billOfLading4.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_MultipleBillOfLadings()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUSYD", "AUBNE", OrgHeader1, "billofLading2");
			var billOfLading3 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader2, "billofLading3");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifest, "AUSYD", "PEM");
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifest, "AUSYD", "PEM");

					AssertNull(billOfLading3.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_WithdrawalMessage()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Withdrawal;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading2, Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifestCancellation, "AUMEL", "PEM");

					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_Load()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifest, "AUSYD", "PEM");
					AssertNull(billOfLading2.Logs.MostRecentLogByEventTime(Events.DataExport));
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Withdrawal;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifestCancellation, "AUSYD", "PEM");
					AssertNull(billOfLading2.Logs.MostRecentLogByEventTime(Events.DataExport));
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUSYD";
					message.Direction = Constants.PortDirection.Load;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Amendment;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertMessageHasBeenSent(billOfLading1, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifestReplacement, "AUSYD", "PEM");
					AssertNull(billOfLading2.Logs.MostRecentLogByEventTime(Events.DataExport));
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_Transit()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUSYD", "AUBNE", OrgHeader1, "billofLading2");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Transit;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifest, "AUMEL", "PTM");
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Transit;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Withdrawal;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifestCancellation, "AUMEL", "PTM");
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUMEL";
					message.Direction = Constants.PortDirection.Transit;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Amendment;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifestReplacement, "AUMEL", "PTM");
				}
			}
		}

		public void TestSendButtonClick_NoErrorMessage_Discharge()
		{
			var voyage = CreateVoyage();
			var billOfLading1 = CreateBillOfLading(voyage, "AUSYD", "AUMEL", OrgHeader1, "billofLading1");
			var billOfLading2 = CreateBillOfLading(voyage, "AUMEL", "AUBNE", OrgHeader1, "billofLading2");

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", OrgHeader1.PK, "AUSYD_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUMEL", OrgHeader1.PK, "AUMEL_OH1");
			CreateDangerousGoodsManifestPort(dangerousGoodsManifestPorts, "AUBNE", OrgHeader1.PK, "AUBNE_OH1");

			var message = new DangerousGoodsManifestMessage(voyage);
			message.MessageType = PortMessageTypeList.Codes.Original;

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUBNE";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Original;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifest, "AUBNE", "PIM");
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUBNE";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Withdrawal;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifestCancellation, "AUBNE", "PIM");
				}

				Thread.Sleep(5);

				using (var dialog = new DangerousGoodsManifestMessageDialog(message))
				{
					message.Port = "AUBNE";
					message.Direction = Constants.PortDirection.Discharge;
					message.PrincipalPK = OrgHeader1.PK;
					message.MessageType = DangerousGoodsManifestMessageTypeList.Codes.Amendment;

					dialog.Show();
					dialog.sendButton.PerformClick();

					AssertEquals("Information Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull(billOfLading1.Logs.MostRecentLogByEventTime(Events.DataExport));
					AssertMessageHasBeenSent(billOfLading2, Events.MessageSent, Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifestReplacement, "AUBNE", "PIM");
				}
			}
		}

		void AssertMessageHasBeenSent(BillOfLading billOfLading, Event logEvent, string messageType, string port, string recipientRole)
		{
			var expectedReference = StmALog.GenerateEventReference("", new[] { Params.Location.AsKeyFor(port), Params.MessageType.AsKeyFor(messageType), Params.Department.AsKeyFor("Terminal") });

			var generatedLogEvent = billOfLading.Sailing.Voyage.Logs.MostRecentLogByEventTime(logEvent, expectedReference);
			var generatedDexEvent = billOfLading.Logs.MostRecentLogByEventTime(Events.DataExport);

			AssertNotNull(string.Format("{0} event", logEvent.Code), generatedLogEvent);
			AssertNotNull(string.Format("{0} event", Events.DataExport.Code), generatedDexEvent);

			var universalXml = new XPathDocument(new StringReader(generatedDexEvent.RelatedEDIMessage.Message.EM_MessageText)).CreateNavigator();
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("u", "http://www.cargowise.com/Schemas/Universal/2012/11");

			AssertEquals("EventType in the generated XML", logEvent.Code, universalXml.SelectSingleNode("/u:UniversalShipment/u:Shipment/u:DataContext/u:Workflow/u:EventType", namespaceManager).Value);
			AssertEquals("EventReference in the generated XML", expectedReference, universalXml.SelectSingleNode("/u:UniversalShipment/u:Shipment/u:DataContext/u:Workflow/u:TriggerReference", namespaceManager).Value);
			AssertEquals("RecipientRole in the generated XML", recipientRole, universalXml.SelectSingleNode("/u:UniversalShipment/u:Shipment/u:DataContext/u:Workflow/u:RecipientRoleCollection/u:RecipientRole", namespaceManager).Value);
		}

		#endregion

		#region TestIssueDetails

		public void TestIssueDetails()
		{
			var voyage = Factory.New<JobVoyage>();
			var message = new DangerousGoodsManifestMessage(voyage);

			var issue1 = message.Issues.AddNew(voyage.PK, "JV", "Issue1", "Detail 1");
			var issue2 = message.Issues.AddNew(voyage.PK, "JV", "Issue2", "Detail 2");
			var issue3 = message.Issues.AddNew(voyage.PK, "JV", "Issue3", "Detail 3");

			using (var dialog = new DangerousGoodsManifestMessageDialog(message))
			{
				dialog.Show();
				Application.DoEvents();

				SelectIssue(dialog, issue2);
				AssertEquals(issue2, dialog.CurrentIssue);

				SelectIssue(dialog, issue1);
				AssertEquals(issue1, dialog.CurrentIssue);

				SelectIssue(dialog, issue3);
				AssertEquals(issue3, dialog.CurrentIssue);
			}
		}

		void SelectIssue(DangerousGoodsManifestMessageDialog dialog, PortMessageIssue issue)
		{
			var grid = (ZGrid)typeof(DangerousGoodsManifestMessageDialog).InvokeMember("issuesGrid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, dialog, null);
			var manager = grid.ListManager;
			manager.Position = manager.List.IndexOf(issue);
		}

		#endregion

		#region Implementation

		JobVoyage CreateVoyage()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();

			voyage.JV_RV_NKVessel = vessel.RV_Code;
			voyage.JV_VoyageFlight = "ABCWTG";
			vessel.RV_LloydsNumber = "WTG";

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new DateTime(2022, 11, 29);

			var origin2 = voyage.Origins.AddNew();
			origin2.FillWithValidTestData();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			origin2.JA_E_DEP = new DateTime(2022, 12, 01);

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "AUMEL";
			destinations1.JB_E_ARV = new DateTime(2022, 11, 29);

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "AUBNE";
			destinations2.JB_E_ARV = new DateTime(2022, 12, 01);

			voyage.GenerateSailings();

			foreach (JobSailing sailing in voyage.Sailings)
			{
				sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			}

			Factory.Save();

			return voyage;
		}

		BillOfLading CreateBillOfLading(JobVoyage voyage, ZString loadingPort, ZString dischargePort, OrgHeader orgHeader, ZString houseBill, bool createUndg = true)
		{
			var sailingPK = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == loadingPort && x.JX_JB_RL_NKPortOfDischarge == dischargePort).PK;

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.Bulk;
			billOfLading.JS_OH_DeliveryAgent = orgHeader.PK;
			billOfLading.JS_JX = sailingPK;
			billOfLading.JS_GoodsDescription = "WTG";
			billOfLading.JS_HouseBill = houseBill;

			billOfLading.ConsigneePK = Consingee.PK;
			billOfLading.ConsignorPK = Consingor.PK;

			billOfLading.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			billOfLading.ConsigneeDocumentaryAddress.E2_CompanyName = "CONSIGNEE NAME";
			billOfLading.ConsigneeDocumentaryAddress.E2_Address1 = "CONSIGNEE ADDRESS 1";
			billOfLading.ConsigneeDocumentaryAddress.E2_Address2 = "CONSIGNEE ADDRESS 2";
			billOfLading.ConsigneeDocumentaryAddress.E2_Postcode = "65432";
			billOfLading.ConsigneeDocumentaryAddress.E2_City = "LOS ANGELES";
			billOfLading.ConsigneeDocumentaryAddress.E2_State = "CA";
			billOfLading.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "US";
			billOfLading.ConsigneeDocumentaryAddress.E2_Contact = "BROWN SMITH";
			billOfLading.ConsigneeDocumentaryAddress.E2_Phone = "+61 2 6958 6543";
			billOfLading.ConsigneeDocumentaryAddress.E2_Fax = "+61 2 6958 6544";
			billOfLading.ConsigneeDocumentaryAddress.E2_Email = "consingee@where.com";

			billOfLading.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			billOfLading.ConsignorDocumentaryAddress.E2_CompanyName = "CONSIGNOR NAME";
			billOfLading.ConsignorDocumentaryAddress.E2_Address1 = "CONSIGNOR ADDRESS 1";
			billOfLading.ConsignorDocumentaryAddress.E2_Address2 = "CONSIGNOR ADDRESS 2";
			billOfLading.ConsignorDocumentaryAddress.E2_Postcode = "2215";
			billOfLading.ConsignorDocumentaryAddress.E2_City = "SYDNEY";
			billOfLading.ConsignorDocumentaryAddress.E2_State = "NSW";
			billOfLading.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			billOfLading.ConsignorDocumentaryAddress.E2_Contact = "BOB SMITH";
			billOfLading.ConsignorDocumentaryAddress.E2_Phone = "+61 2 5684 6543";
			billOfLading.ConsignorDocumentaryAddress.E2_Fax = "+61 2 5684 6544";
			billOfLading.ConsignorDocumentaryAddress.E2_Email = "consingor@where.com";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();

			var container = billOfLading.FCLContainers.AddNew();
			container.JC_ContainerNum = "ABC";
			container.JC_RC = refContainer.PK;

			billOfLading.OuterPackLines.RemoveAndDeleteAll();
			var packLine = billOfLading.OuterPackLines.AddNew();

			if (createUndg)
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();

				var undg = packLine.UNDGs.AddNew();
				undg.DI_DG = substance.PK;
				undg.DI_DGFlashPoint = 2;
				undg.DI_PackageCount = 1;
				undg.DI_TechnicalName = "WTG";
				undg.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				undg.DI_DGWeight = 10;
				undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			}

			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 1;
			packLine.JL_DetailedDescription = "WTG";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_ActualWeight = 23630m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 23m;

			foreach (Transport transport in billOfLading.Transports)
			{
				transport.CarrierPK = Carrier.PK;
			}

			billOfLading.JS_JX = sailingPK;

			Factory.Save();

			return billOfLading;
		}

		protected override void SetUp()
		{
			base.SetUp();

			OrgHeader1 = CreateOrganisation("OrgHeader1", "USLAX", "OrgHeader1 ADDRESS 1", "LOS ANGELES", "+1 801 120 234");
			OrgHeader1.OH_Code = "OH1";
			OrgHeader1.OH_IsShippingProvider = true;
			OrgHeader1.OH_IsShippingLine = true;
			OrgHeader1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			OrgHeader2 = CreateOrganisation("OrgHeader2", "USLAX", "OrgHeader2 ADDRESS 1", "LOS ANGELES", "+1 801 120 234");
			OrgHeader2.OH_Code = "OH2";
			OrgHeader2.OH_IsShippingProvider = true;
			OrgHeader2.OH_IsShippingLine = true;
			OrgHeader2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Carrier = CreateOrganisation("CARRIER", "USLAX", "CARRIER ADDRESS 1", "LOS ANGELES", "+1 801 120 456");
			Carrier.OH_IsShippingLine = true;
			Carrier.OH_IsShippingProvider = true;
			Carrier.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Consingee = CreateOrganisation("CONSINGEE", "USLAX", "CONSINGEE ADDRESS 1", "LOS ANGELES", "+1 801 120 456");
			Consingor = CreateOrganisation("CONSIGNOR", "AUSYD", "CONSIGNOR ADDRESS 1", "SYDNEY", "+61 2 9156 4568");

			Factory.Save();

			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
		}
		OrgHeader OrgHeader1;
		OrgHeader OrgHeader2;

		OrgHeader Consingee;
		OrgHeader Consingor;
		OrgHeader Carrier;

		DangerousGoodsManifestPort CreateDangerousGoodsManifestPort(DangerousGoodsManifestPortCollection collection, ZString portCode, ZGuid principalPK, ZString senderID)
		{
			var port = collection.AddNew();

			port.Port = portCode;
			port.PrincipalPK = principalPK;
			port.SenderID = senderID;
			port.Enabled = true;

			return port;
		}

		public OrgHeader CreateOrganisation(ZString fullName, ZString closestPort, ZString address1, ZString city, ZString phone)
		{
			var orgHeader = base.Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = fullName;
			orgHeader.OH_RL_NKClosestPort = closestPort;
			orgHeader.MainAddress.OA_Address1 = address1;
			orgHeader.MainAddress.OA_City = city;
			orgHeader.MainAddress.OA_Phone = phone;
			return orgHeader;
		}

		#endregion
	}
}
