using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ImportMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessageStatusErrorControlVerticalScrollable()
		{
			using (var form = new JobDeclarationForm(DeclarationTestHelper.GetDeclarationForeBond(Factory)))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var statusErrorsTabPage = (ZTabPage)messagesControl.Controls.Find("StatusErrorsTabPage", true).Single();
				statusErrorsTabPage.Select();
				var statusErrorsPanel = (ZPanel)messagesControl.Controls.Find("messagesStatusErrorsPanel", true).Single();
				Assert(statusErrorsPanel.AutoScroll);
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(420, 330, true), statusErrorsPanel.AutoScrollMinSize);
				AssertEquals(1, statusErrorsPanel.Controls.Count);
				AssertEquals(messagesControl.messagesStatusErrorsUserControl, statusErrorsPanel.Controls[0]);
				AssertEquals(statusErrorsTabPage, statusErrorsPanel.Parent);
			}
		}

		public void TestStatusesErrorsLabelVisibleDataBinding()
		{
			const string StatusLableVisibilityForBinding = "IsVisibleForBinding";
			var bizo = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var propertyPath = "ManufacturerDetails";
			using (var control = new ImportMessagesUserControl())
			{
				control.SetDataBinding(bizo, string.Empty);
				var binding = control.StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding];
				AssertNotNull(binding);
				AssertEquals("InBondRelatedRecords.MessagesToShow.StatusesAndErrorsVisible", binding.BindingMemberInfo.BindingMember);
				control.StatusesErrorsLabel.DataBindings.Clear();
				control.SetDataBinding(bizo, propertyPath);
				binding = control.StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding];
				AssertNotNull(binding);
				AssertEquals(propertyPath + ".InBondRelatedRecords.MessagesToShow.StatusesAndErrorsVisible", binding.BindingMemberInfo.BindingMember);
			}
		}

		public void TestShowMessageHeaderGrid()
		{
			using (var form = new JobDeclarationForm(DeclarationTestHelper.GetDeclarationForeBond(Factory)))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var messageDetailsTabPage = (ZTabPage)messagesControl.Controls.Find("MessageDetailsTabPage", true)[0];
				messageDetailsTabPage.Select();
				Assert("Message header Grid is visible by default", messagesControl.InBondHeadersGrid.Visible);
				Assert("Message header Grid is visible by default", messagesControl.InBondHeadersGrid.Parent.Visible);
				Assert("Message header Grid is visible by default", messagesControl.ShowMessageHeaderGrid);
				messagesControl.ShowMessageHeaderGrid = false;
				Assert("Message header Grid is hidden", !messagesControl.InBondHeadersGrid.Visible);
				Assert("Message header Grid is hidden", !messagesControl.InBondHeadersGrid.Parent.Visible);
			}
		}

		public void TestDisplayMessageProper()
		{
			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			declaration.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var normalMessage = Factory.New<MQEDIMessage>();
			normalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			normalMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			normalMessage.EM_LinkUniqueID = entryHeader.PK;
			var transmitEBondEdiMessage = Factory.NewWithValidTestData<EBondEDIMessage>();
			transmitEBondEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitEBondEdiMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			transmitEBondEdiMessage.EM_LinkUniqueID = entryHeader.PK;
			var receiveEBondEdiMessage = Factory.NewWithValidTestData<EBondEDIMessage>();
			receiveEBondEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveEBondEdiMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			receiveEBondEdiMessage.EM_LinkUniqueID = entryHeader.PK;
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var statusErrorsTabPage = messagesControl.FindSingle<ZTabPage>("StatusErrorsTabPage");
				var messageDetailsTabPage = messagesControl.FindSingle<ZTabPage>("MessageDetailsTabPage");
				messageDetailsTabPage.Select();
				var interpretationBox = messageDetailsTabPage.Controls.Find("MessageInterpretationBox", true)[0];
				var messageDetailsTextBox = messageDetailsTabPage.Controls.Find("MessageDetailsTextBox", true)[0];
				var grid = messagesControl.MessagesGrid;
				var listManager = grid.ListManager;
				AssertEquals("Precondition.", 3, listManager.Count);
				CombineAssertions(() =>
				{
					for (int i = 0; i < 3; i++)
					{
						grid.CurrentRowIndex = i;
						Application.DoEvents();
						var message = listManager?.GetCurrent() as BaseEDIMessage;
						var isVisible = message?.IsInterpretationInHtmlFormat ?? false;
						AssertEquals("Should only be visible when the current item is a receive eBond message.", isVisible, interpretationBox.Visible);
						AssertNotEquals("Should only be hidden when the current item is a receive eBond message.", isVisible, messageDetailsTextBox.Visible);
						var isStatusErrorsTabVisible = message?.HasStatusOrErrors ?? true;
						AssertEquals("Should only be visible when the current item is not eBond message.", isStatusErrorsTabVisible, statusErrorsTabPage.TabVisible);
					}
				});
			}
		}

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(message);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				Assert("should not be visible", !messagesControl.StatusesErrorsLabel.Visible);
				var msgStatusControl = messagesControl.messagesStatusErrorsUserControl;
				ZTextBoxColumnStyleInfo tariffColumn = (ZTextBoxColumnStyleInfo)msgStatusControl.StatusesAndErrorsGrid.ColumnStyles[1];
				AssertEquals("Tariff", tariffColumn.Caption);
				AssertEquals("non ACE declaration Status/Errors grid should not contains Tariff column", true, tariffColumn.IsUnavailable);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var msgStatusControl = messagesControl.messagesStatusErrorsUserControl;
				ZTextBoxColumnStyleInfo tariffColumn = (ZTextBoxColumnStyleInfo)msgStatusControl.StatusesAndErrorsGrid.ColumnStyles[1];
				AssertEquals("Tariff", tariffColumn.Caption);
				AssertEquals("ACE declaration Status/Errors grid should contains Tariff column", false, tariffColumn.IsUnavailable);
			}
		}

		public void TestControlsVisibilityForFTZAndDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(message);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				Assert("column should not be visible", !messagesControl.InBondHeadersGrid.Columns.Contains(MessageActionRelatedRecordWrapper.Schema.ReleaseDate));
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				var messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				Assert("column should not be visible", !messagesControl.InBondHeadersGrid.Columns.Contains(MessageActionRelatedRecordWrapper.Schema.ReleaseDate));
			}
		}

		public void TestMessagesGridDefaultColumns()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(message);
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				ImportMessagesUserControl messagesControl = (ImportMessagesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				ZGrid grid = messagesControl.MessagesGrid;
				for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
				{
					ZGridColumnInfo columnInfo = grid.ColumnStyles[i] as ZGridColumnInfo;
					AssertNotNull(columnInfo);
					ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
					AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
				}
			}
		}

		List<ZString> expectedColumnNamesInSortOrderList;
		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>();
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageNum);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.MessageTypeDescription);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_DateTimeInterchangeSent);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_User);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SystemCreateTimeUtc);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
				}

				return expectedColumnNamesInSortOrderList;
			}
		}
	}
}
