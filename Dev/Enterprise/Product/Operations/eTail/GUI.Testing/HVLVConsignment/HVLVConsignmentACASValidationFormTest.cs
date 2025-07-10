using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVConsignmentACASValidationForm))]
	public class HVLVConsignmentACASValidationFormTest : ZFormBasherTest
	{
		public void TestSendMessageButton_CloseFormAfterSendSuccessfully()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			Factory.Save();

			var consignmentsToSend = consignmentHeader.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal);
			form.Show();
			form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();

			Assert(form.IsDisposed);
		}

		public void TestContextMenuItems()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();
				var grid = form.Controls.Find("GridHVLVConsignments", true).First() as ZGrid;
				var contextMenuItems = grid.ContextMenu.MenuItems;

				var openACASReportMenuItem = contextMenuItems.Find("OpenACASReport", true).FirstOrDefault();
				AssertNull(openACASReportMenuItem);
				// on pause until WI00413762 -eTail V2.ACAS Menu action to visualize consignment ACAS report is done
				//AssertNotNull("Open ACAS Report menu item is added to context menu", openACASReportMenuItem);

				//grid.ContextMenu.ShowPopupMenu();
				//AssertEquals("Open ACAS Report menu item should be visible by default", true, openACASReportMenuItem.Visible);
			}
		}
		public void TestGridHVLVConsignmentsZTextBoxColumnStyle()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			var columnNamesToTest = new List<string>
			{
				"ConsigneeName",
				"ConsigneeAddress1",
				"ConsigneeAddress2",
				"ConsigneeCity",
				"ConsigneeState",
				"ConsigneePostcode",
				"ConsigneeCountryCode",
				"ConsigneeMobile",
				"ConsigneeEmail",
				"ShipperName",
				"ShipperAddress1",
				"ShipperAddress2",
				"ShipperCity",
				"ShipperState",
				"ShipperPostcode",
				"ShipperCountryCode",
				"ShipperMobile",
				"ShipperEmail",
				"GoodsDescription",
				"ACASStatus",
				"ACASInterchangeStatus",
				"ACASMessageStatus",
			};

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();
				var grid = form.Controls.Find("GridHVLVConsignments", true).First() as ZGrid;

				foreach (var columnName in columnNamesToTest)
				{
					Assert(grid.Columns.Contains(columnName));
					var columnStyle = grid.GetColumnStyle(columnName);
					AssertType<ZTextBoxColumnStyleInfo>(columnStyle);
				}
			}
		}

		public void TestGridHVLVConsignmentsZCalcEditColumnStyle()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			var columnNamesToTest = new List<string>
			{
				"ItemCount",
				"ManifestedWeight",
				"ActualWeight",
			};

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();
				var grid = form.Controls.Find("GridHVLVConsignments", true).First() as ZGrid;

				foreach (var columnName in columnNamesToTest)
				{
					Assert(grid.Columns.Contains(columnName));
					var columnStyle = grid.GetColumnStyle(columnName);
					AssertType<ZCalcEditColumnStyleInfo>(columnStyle);
				}
			}
		}

		public void TestClickSendMessageButton_OnlyAvailableToRunAfterDataSaved()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();
				form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();

				AssertEquals("Please save the form before sending ACAS Report.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickSendMessageButton_ShouldHaveNoErrorMessageBeforeSending()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_ShipmentType = "HVL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();

			Factory.Save();

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);
			collection.RunPreSaveValidation();

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				var wrapper = collection.First() as HVLVConsignmentForACASWrapper;
				AssertHasMessageErrors("Precondition consignment has message error ", wrapper.GoodsDescriptionInfo);

				form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();

				AssertEquals("There are message errors that need to be corrected before sending ACAS messages.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickSendMessageButton_MessagesSendSuccessfully()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				Factory.Save();

				var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<IHVLVAirCargoAdvanceScreeningMessageSender>();

				var message = string.Empty;
				mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.TrySendACASReports(ACASReportAction.SendOriginal, out message))
					.Returns(() => true);

				using (ObjectFactory.Substitute(mockHVLVAirCargoAdvanceScreeningMessageSender.Object))
				{
					form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();
				}
			}

			AssertEquals("ACAS Report Sent", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestClickSendMessageButton_ShowsProgressForm()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var consignmentsToSend = consignmentHeader.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				Factory.Save();

				var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<IHVLVAirCargoAdvanceScreeningMessageSender>();

				var message = string.Empty;
				mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.TrySendACASReports(ACASReportAction.SendOriginal, out message));

				using (ObjectFactory.Substitute(mockHVLVAirCargoAdvanceScreeningMessageSender.Object))
				{
					form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();

					var lastForm = ZFormModaliser.LastFormShownForTest;
					AssertType<ProgressForm>("Should have shown progress form.", lastForm);
				}
			}
		}

		public void TestClickSendMessageButton_WhenMessagesSendFailed_ShowsErrorMessage()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var consignmentsToSend = consignmentHeader.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				Factory.Save();

				var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<IHVLVAirCargoAdvanceScreeningMessageSender>();

				var message = string.Empty;
				mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.TrySendACASReports(ACASReportAction.SendOriginal, out message))
					.Callback(new SendACASReportsCallBack((ACASReportAction a, out string msg) => msg = "Failed because of something"))
					.Returns(() => false);

				using (ObjectFactory.Substitute(mockHVLVAirCargoAdvanceScreeningMessageSender.Object))
				{
					form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();
				}
			}

			AssertEquals("ACAS Report Failed", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("Failed because of something", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestClickSendMessageButton_ShouldTryAcquireApplicationLockForShipment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			var consignmentsToSend = consignmentHeader.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				Factory.Save();

				var appLockKey = ("HVLV ACAS Report," + shipment.PK.ToString()).ToUpperInvariant();
				var connection = Db.NewExtraConnectionToMainDb();
				Assert("pre-condition", connection.TryGetLock(appLockKey, out var appLock));

				using (appLock)
				{
					form.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();
				}
			}

			AssertEquals("Send message should fail because shipment has been locked", "Failed to acquire lock for rows in table JobShipment, data being processed by other user.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestClickSaveButton_SavesConsignmentChangesUsingShipmentFactory()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_GoodsDescription = "AA";

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				Assert("Consignment has no changes", !consignment.HasChanges);
				AssertEquals("Consignment goods description", "AA", consignment.HVC_GoodsDescription);
			});

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			using (var form = new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal))
			{
				form.Show();

				var wrapper = collection.First() as HVLVConsignmentForACASWrapper;
				wrapper.GoodsDescription = "NEW GOODS DESCRIPTION";

				CombineAssertions(() =>
				{
					AssertEquals("Consignment goods description updated by wrapper", "NEW GOODS DESCRIPTION", consignment.HVC_GoodsDescription);
					Assert("Consignment has changes", consignment.HasChanges);
				});

				form.FireSaveButton();
				Assert("Consignment changes have been saved", !consignment.HasChanges);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			Factory.Save();

			var consignmentsToSend = header.Consignments.OfType<HVLVConsignment>();
			var collection = new HVLVConsignmentForACASWrapperCollection(consignmentsToSend);

			return new HVLVConsignmentACASValidationForm(collection, ACASReportAction.SendOriginal);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		delegate void SendACASReportsCallBack(ACASReportAction action, out string message);

		#endregion
	}
}
