using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class CustomsMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLinesGridModifyLinesMenuItem()
		{
			var header = Factory.New<CusEntryHeader>();
			using (var form = new ZForm(header))
			using (var userControl = new CustomsMessageUserControl())
			{
				form.Controls.Add(userControl);
				((CargoWise.Windows.UI.ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(userControl, "");
				form.Show();
				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				entryLineGrid.SetDataBinding(header, "MergedLines");
				var menuItem = entryLineGrid.ContextMenu.MenuItems.FindByText("Modify Invoice Line Codes");
				AssertNotNull("Context Menu Item with a label Modify Invoice Line Codes", menuItem);
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Please select an Entry Line.", UnitTestUserNotification.Instance.LastMessage.Text);
				var entryLine = header.MergedLines.AddNew();
				entryLineGrid.ListManager.Position = 0;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Please select an Entry Line with Invoice Lines attached.", UnitTestUserNotification.Instance.LastMessage.Text);
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				invoiceLine.JI_CL = entryLine.PK;
				entryLine.RefreshInvoiceLines();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ZFormModaliser.LastFormShownForTest.GetType()", typeof(CusEntryLineSyncrhoniserForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestEntryHeadersCanBeActivatedIfTheyAreTheSameTypeAsTheSelectedEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.JobDeclaration = declaration;
				var entriesGrid = userControl.EntriesBoundGrid;
				entriesGrid.SetDataBinding(declaration, "CustomsEntryHeaders");
				var menuItem = entriesGrid.ContextMenu.MenuItems.FindByText("Set Entry 'Active'");
				AssertNotNull("Context Menu Item with a label Set Entry 'Active'", menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Cannot Activate - Please select an Entry first.", UnitTestUserNotification.Instance.LastMessage.Text);
				var entryHeader1 = declaration.CusEntryHeader;
				entryHeader1.Messages.AddNew(); // Make sure it can't be removed.
				entryHeader1.CH_IsActive = false;
				var entryHeader2 = declaration.CusEntryHeader;
				entryHeader2.Messages.AddNew(); // Make sure it can't be removed.
				AssertNotEquals("Precondition: entryHeader1 != entryHeader2", entryHeader1, entryHeader2);
				CusEntryHeader entryHeader3 = Factory.New<Business.Declaration.ECIWriteOff.CusEntryHeader>();
				entryHeader3.Messages.AddNew(); // Make sure it can't be removed.
				declaration.CustomsEntryHeaders.Add(entryHeader3);
				entryHeader3.CH_IsActive = false;
				AssertEquals("Precondition: entriesGrid.ListManager.Count", 3, entriesGrid.ListManager.Count);
				AssertEquals("Precondition: entriesGrid.ListManager.List[0]", entryHeader1, entriesGrid.ListManager.List[0]);
				AssertEquals("Precondition: entriesGrid.ListManager.List[1]", entryHeader2, entriesGrid.ListManager.List[1]);
				AssertEquals("Precondition: entriesGrid.ListManager.List[2]", entryHeader3, entriesGrid.ListManager.List[2]);
				AssertEquals("Precondition: entryHeader1.CH_IsActive", false, entryHeader1.CH_IsActive);
				AssertEquals("Precondition: entryHeader2.CH_IsActive", true, entryHeader2.CH_IsActive);
				AssertEquals("Precondition: entryHeader3.CH_IsActive", false, entryHeader3.CH_IsActive);
				entriesGrid.ListManager.Position = 0;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Entry Activated. You may need to Generate (Merge) this Entry to update the Entry Lines.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("entryHeader1.CH_IsActive", true, entryHeader1.CH_IsActive);
				AssertEquals("entryHeader2.CH_IsActive", false, entryHeader2.CH_IsActive);
				AssertEquals("entryHeader3.CH_IsActive", false, entryHeader3.CH_IsActive);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Cannot Activate - This Entry is already Active.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("entryHeader1.CH_IsActive", true, entryHeader1.CH_IsActive);
				AssertEquals("entryHeader2.CH_IsActive", false, entryHeader2.CH_IsActive);
				AssertEquals("entryHeader3.CH_IsActive", false, entryHeader3.CH_IsActive);
				entriesGrid.ListManager.Position = 2;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("LastMessage.Text", "Cannot Activate - You can only activate an Entry of the currently active Type. (NZF-Formal Entry)", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("entryHeader1.CH_IsActive", true, entryHeader1.CH_IsActive);
				AssertEquals("entryHeader2.CH_IsActive", false, entryHeader2.CH_IsActive);
				AssertEquals("entryHeader3.CH_IsActive", false, entryHeader3.CH_IsActive);
			}
		}

		public void TestSetupEntryHeaderColumns()
		{
			using (var userControl = new CustomsMessageUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("User control should have PackagesCount column unavailable", true, userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.PackagesCount).IsUnavailable);
					AssertEquals("User control should have CH_BGMReference column width set correctly", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference).Width);
					var recordAddedColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_RecordAdded);
					AssertNotNull("User control should have CH_RecordAdded column", recordAddedColumn);
					AssertEquals("CH_RecordAdded Column is not visible", false, recordAddedColumn.IsVisible);
					var ediTransmitDateColumn = (ZDateEditColumnStyleInfo)userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EDITransmitDate);
					AssertNotNull("User control should have CH_EDITransmitDate column", ediTransmitDateColumn);
					AssertEquals("CH_EDITransmitDate Column is set to Short Date Time Format", ZDateTimePickerFormat.Short, ediTransmitDateColumn.DateTimeFormat);
					AssertEquals("CH_EDITransmitDate Column is readonly", true, ediTransmitDateColumn.IsReadOnly);
					var isActiveColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_IsActive);
					AssertNotNull("User control should have CH_IsActive column", isActiveColumn);
					AssertEquals("CH_IsActive Column is readonly", true, isActiveColumn.IsReadOnly);
					AssertEquals("CH_IsActive Column is Mandatory", true, isActiveColumn.IsMandatory);
					var messageTypeColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType);
					AssertNotNull("User control should have CH_MessageType column", messageTypeColumn);
					AssertEquals("CH_MessageType Column is Visible", false, messageTypeColumn.IsVisible);
					AssertEquals("CH_MessageType Column is Grouped Correctly", "0F808B84-4C77-49E0-8D5A-8B6455A966AE", messageTypeColumn.GroupName.Key);
					AssertEquals("CH_MessageType Column cation", "Entry Type", messageTypeColumn.CaptionResourceString.Caption);
					var messageTypeDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageTypeDescription);
					AssertNotNull("User control should have MessageTypeDescription column", messageTypeDescriptionColumn);
					AssertEquals("MessageTypeDescription Column is Visible", false, messageTypeDescriptionColumn.IsVisible);
					AssertEquals("MessageTypeDescription Column is Grouped Correctly", "0F808B84-4C77-49E0-8D5A-8B6455A966AE", messageTypeDescriptionColumn.GroupName.Key);
					AssertEquals("MessageTypeDescription Column cation", "Entry Type Description", messageTypeDescriptionColumn.CaptionResourceString.Caption);
					var entryStatusColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus);
					AssertNotNull("User control should have CH_EntryStatus column", entryStatusColumn);
					AssertEquals("CH_EntryStatus Column is Visible", false, entryStatusColumn.IsVisible);
					AssertEquals("CH_EntryStatus Column is Grouped Correctly", "10B143BC-615A-4845-8E27-8DA0AC9C48E1", entryStatusColumn.GroupName.Key);
					var entryHeaderStatusDescriptionColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					AssertNotNull("User control should have EntryHeaderStatusDescription column", entryHeaderStatusDescriptionColumn);
					AssertEquals("EntryHeaderStatusDescription Column is Visible", false, entryHeaderStatusDescriptionColumn.IsVisible);
					AssertEquals("EntryHeaderStatusDescription Column is Grouped Correctly", "10B143BC-615A-4845-8E27-8DA0AC9C48E1", entryHeaderStatusDescriptionColumn.GroupName.Key);
					AssertNotNull("User control should have DutyTotalInMergedLines column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.DutyTotalInMergedLines));
					AssertNotNull("User control should have GSTAmount column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.GSTAmount));
					AssertNotNull("User control should have TotalMisc column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.TotalMisc));
					AssertNotNull("User control should have TotalEntryFeeAmount column", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.TotalEntryFeeAmount));
					var totalAmountPayableColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.TotalAmountPayableIncludingEntryFee);
					AssertNotNull("User control should have TotalAmountPayableIncludingEntryFee column", totalAmountPayableColumn);
					AssertEquals("TotalAmountPayableIncludingEntryFee Column is readonly", true, totalAmountPayableColumn.IsReadOnly);
					AssertEquals("TotalAmountPayableIncludingEntryFee Column is Mandatory", true, totalAmountPayableColumn.IsMandatory);
					var totalAmountReturnedColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TotalAmountReturned);
					AssertNotNull("User control should have CH_TotalAmountReturned column", totalAmountReturnedColumn);
					AssertEquals("CH_TotalAmountReturned Column is readonly", true, totalAmountReturnedColumn.IsReadOnly);
				}

				);
			}
		}
	}
}
