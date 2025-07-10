using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.NL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

sealed class MessageSendingFormBottomSectionUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			AssertEquals(typeof(MessageSendingActionParent), control.BindingSource.DataSourceType);
		}
	}

	public void TestPresentationDateAndTimeOffsetEdit()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var presentationDateAndTimeOffsetEdit = control.PresentationDateAndTimeOffsetEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateTimeOffsetEdit>(presentationDateAndTimeOffsetEdit);
				AssertEquals("SendingObjectsCollection.PresentationDateTime", presentationDateAndTimeOffsetEdit.BindTo);
			});
		}
	}

	public void TestAgreeWithMinorDiscrepanciesCheckBox()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var agreeWithMinorDiscrepanciesCheckBox = control.AgreeWithMinorDiscrepanciesCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>(agreeWithMinorDiscrepanciesCheckBox);
				AssertEquals("SendingObjectsCollection.AgreeWithMinorDiscrepancies", agreeWithMinorDiscrepanciesCheckBox.BindTo);
			});
		}
	}

	public void TestJustificationTextBox()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var justificationTextBox = control.JustificationTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Control Type", justificationTextBox);
				AssertEquals("BindTo", "SendingObjectsCollection.Justification", justificationTextBox.BindTo);
			});
		}
	}

	public void TestTCI11DateEdit()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var tcI11DateEdit = control.TCI11DateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Control Type", tcI11DateEdit);
				AssertEquals("BindTo", "SendingObjectsCollection.TCI11", tcI11DateEdit.BindTo);
			});
		}
	}

	public void TestQueryInformationTextBox()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var queryInformationTextBox = control.QueryInformationTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Control Type", queryInformationTextBox);
				AssertEquals("BindTo", "SendingObjectsCollection.QueryInformation", queryInformationTextBox.BindTo);
			});
		}
	}

	public void TestActualConsigneeDocAddressControl()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var actualConsigneeDocAddressControl = control.ActualConsigneeDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>("Control Type", actualConsigneeDocAddressControl);
				AssertEquals("BindTo", "SendingObjectsCollection.ActualConsignee", actualConsigneeDocAddressControl.BindTo);
			});
		}
	}

	public void TestActualOfficeOfDestinationFindBox()
	{
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			var actualOfficeOfDestinationFindBox = control.ActualOfficeOfDestinationFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Control Type", actualOfficeOfDestinationFindBox);
				AssertEquals("BindTo", "SendingObjectsCollection.ActualOfficeOfDestination", actualOfficeOfDestinationFindBox.BindTo);
			});
		}
	}

	public void TestLayout_ByMessageType()
	{
		using (var form = new ZForm(messageSendingActionParent))
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var justificationTextBox = control.JustificationTextBox;
			var tcI11DateEdit = control.TCI11DateEdit;
			var queryInformationTextBox = control.QueryInformationTextBox;
			var actualConsigneeDocAddressControl = control.ActualConsigneeDocAddressControl;
			var actualOfficeOfDestinationFindBox = control.ActualOfficeOfDestinationFindBox;
			var presentationDateAndTimeOffsetEdit = control.PresentationDateAndTimeOffsetEdit;
			var agreeWithMinorDiscrepanciesCheckBox = control.AgreeWithMinorDiscrepanciesCheckBox;
			var messageSendingAction = messageSendingActionParent.SendingObjectsCollection[0];
			CombineAssertions($"Message Type: {messageSendingAction.MessageType}", () =>
			{
				AssertEquals("JustificationTextBox not visible", false, justificationTextBox.Visible);
				AssertEquals("TCI11DateEdit not visible", false, tcI11DateEdit.Visible);
				AssertEquals("QueryInformationTextBox not visible", false, queryInformationTextBox.Visible);
				AssertEquals("ActualConsigneeDocAddressControl not visible", false, actualConsigneeDocAddressControl.Visible);
				AssertEquals("ActualOfficeOfDestinationFindBox not visible", false, actualOfficeOfDestinationFindBox.Visible);
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox not visible", false, agreeWithMinorDiscrepanciesCheckBox.Visible);

				messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
				AssertEquals("JustificationTextBox visible", true, justificationTextBox.Visible);
				AssertEquals("TCI11DateEdit not visible", false, tcI11DateEdit.Visible);
				AssertEquals("QueryInformationTextBox not visible", false, queryInformationTextBox.Visible);
				AssertEquals("ActualConsigneeDocAddressControl not visible", false, actualConsigneeDocAddressControl.Visible);
				AssertEquals("ActualOfficeOfDestinationFindBox not visible", false, actualOfficeOfDestinationFindBox.Visible);
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox not visible", false, agreeWithMinorDiscrepanciesCheckBox.Visible);

				messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
				AssertEquals("JustificationTextBox not visible", false, justificationTextBox.Visible);
				AssertEquals("TCI11DateEdit visible", true, tcI11DateEdit.Visible);
				AssertEquals("QueryInformationTextBox visible", true, queryInformationTextBox.Visible);
				AssertEquals("ActualConsigneeDocAddressControl visible", true, actualConsigneeDocAddressControl.Visible);
				AssertEquals("ActualOfficeOfDestinationFindBox visible", true, actualOfficeOfDestinationFindBox.Visible);
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox not visible", false, agreeWithMinorDiscrepanciesCheckBox.Visible);

				messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.Amendment;
				AssertEquals("PresentationDateAndTimeOffsetEdit visible", true, presentationDateAndTimeOffsetEdit.Visible);
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox not visible", false, agreeWithMinorDiscrepanciesCheckBox.Visible);

				messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.RequestARelease;
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox visible", true, agreeWithMinorDiscrepanciesCheckBox.Visible);
			});
		}
	}

	public void TestLayout_ByStatus()
	{
		TestLayout_ByStatus("", true);
		TestLayout_ByStatus(NctsTransitStatusList.Codes.DeclarationRejected, true);
		TestLayout_ByStatus(NctsTransitStatusList.Codes.DeclarationAccepted, true);
		TestLayout_ByStatus(NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival, false);
	}

	void TestLayout_ByStatus(string status, bool expected)
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = status;
		using (var form = new ZForm(messageSendingActionParent))
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var presentationDateAndTimeOffsetEdit = control.PresentationDateAndTimeOffsetEdit;
			AssertEquals($"PresentationDateAndTimeOffsetEdit visible for status: {status}", expected, presentationDateAndTimeOffsetEdit.Visible);
		}
	}

	public void TestLayout_MessageSendingAction_Null()
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
		using (var form = new ZForm(messageSendingActionParent))
		using (var control = new MessageSendingFormBottomSectionUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var justificationTextBox = control.JustificationTextBox;
			var tcI11DateEdit = control.TCI11DateEdit;
			var queryInformationTextBox = control.QueryInformationTextBox;
			var presentationDateAndTimeOffsetEdit = control.PresentationDateAndTimeOffsetEdit;
			var agreeWithMinorDiscrepanciesCheckBox = control.AgreeWithMinorDiscrepanciesCheckBox;
			var messageSendingAction = messageSendingActionParent.SendingObjectsCollection[0];
			var actualConsigneeDocAddressControl = control.ActualConsigneeDocAddressControl;
			var actualOfficeOfDestinationFindBox = control.ActualOfficeOfDestinationFindBox;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
			messageSendingAction.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			control.MessageTypeChanged(null);
			CombineAssertions(() =>
			{
				AssertEquals("AgreeWithMinorDiscrepanciesCheckBox not visible", false, agreeWithMinorDiscrepanciesCheckBox.Visible);
				AssertEquals("PresentationDateAndTimeOffsetEdit not visible", false, presentationDateAndTimeOffsetEdit.Visible);
				AssertEquals("JustificationTextBox not visible", false, justificationTextBox.Visible);
				AssertEquals("TCI11DateEdit not visible", false, tcI11DateEdit.Visible);
				AssertEquals("QueryInformationTextBox not visible", false, queryInformationTextBox.Visible);
				AssertEquals("ActualConsigneeDocAddressControl not visible", false, actualConsigneeDocAddressControl.Visible);
				AssertEquals($"ActualOfficeOfDestinationFindBox not visible", false, actualOfficeOfDestinationFindBox.Visible);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		messageSendingActionParent = new MessageSendingActionParent(nctsHeader);
	}

	NctsHeader nctsHeader;
	MessageSendingActionParent messageSendingActionParent;
}
