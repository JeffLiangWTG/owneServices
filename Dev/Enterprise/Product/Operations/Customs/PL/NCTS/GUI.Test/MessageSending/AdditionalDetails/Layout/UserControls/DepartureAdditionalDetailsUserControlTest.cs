using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class DepartureAdditionalDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			AssertEquals(typeof(MessageSendingObject), control.DataSourceType);
		}
	}

	public void TestJustificationTextBox()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var justificationTextBox = control.JustificationTextBox;
				AssertType<ZTextBox>("Type", justificationTextBox);
				AssertEquals("BindingMember", nameof(MessageSendingObject.Justification), justificationTextBox.GetBindingMember());
				AssertEquals("Multiline", true, justificationTextBox.Multiline);
			});
		}
	}

	public void TestAmendmentTypeDropEdit()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var amendmentTypeDropEdit = control.AmendmentTypeDropEdit;
				AssertType<ZDropEdit>("Type", amendmentTypeDropEdit);
				AssertEquals("BindingMember", nameof(MessageSendingObject.AmendmentType), amendmentTypeDropEdit.GetBindingMember());
			});
		}
	}

	public void TestPresentationDateAndTimeDateTimeOffsetEdit()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var presentationDateAndTimeDateTimeOffsetEdit = control.PresentationDateAndTimeDateTimeOffsetEdit;
				AssertType<ZDateTimeOffsetEdit>("Type", presentationDateAndTimeDateTimeOffsetEdit);
				AssertEquals("BindingMember", nameof(MessageSendingObject.PresentationDateTime), presentationDateAndTimeDateTimeOffsetEdit.GetBindingMember());
			});
		}
	}

	public void TestTC11DeliveryDateTimeOffsetEdit()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var tc11DeliveryDateTimeOffsetEdit = control.TC11DeliveryDateTimeOffsetEdit;
				AssertType<ZDateTimeOffsetEdit>("Type", tc11DeliveryDateTimeOffsetEdit);
				AssertEquals("BindingMember", nameof(MessageSendingObject.TC11DeliveryDate), tc11DeliveryDateTimeOffsetEdit.GetBindingMember());
			});
		}
	}

	public void TestAdditionalTextBox()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var additionalTextBox = control.AdditionalTextBox;
				AssertType<ZTextBox>("Type", additionalTextBox);
				AssertEquals("BindingMember", nameof(MessageSendingObject.AdditionalText), additionalTextBox.GetBindingMember());
			});
		}
	}

	public void TestActualConsigneeDocAddressControl()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var actualConsigneeDocAddressControl = control.ActualConsigneeDocAddressControl;
				AssertType<ZDocAddressControl>("Type", actualConsigneeDocAddressControl);
				AssertEquals("BindingMember", nameof(MessageSendingObject.ActualConsignee), actualConsigneeDocAddressControl.GetBindingMember());
			});
		}
	}

	public void TestDepartureOfficeOfEnquiryCodeFindBox()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var departureOfficeOfEnquiryCodeFindBox = control.DepartureOfficeOfEnquiryCodeFindBox;
				AssertType<ZCodeFindBox>("Type", departureOfficeOfEnquiryCodeFindBox);
				AssertEquals("BindingMember", nameof(MessageSendingObject.DepartureOfficeOfEnquiry), departureOfficeOfEnquiryCodeFindBox.GetBindingMember());
			});
		}
	}

	public void TestActualOfficeOfDestinationCodeFindBox()
	{
		using (var control = new DepartureAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var actualOfficeOfDestinationCodeFindBox = control.ActualOfficeOfDestinationCodeFindBox;
				AssertType<ZCodeFindBox>("Type", actualOfficeOfDestinationCodeFindBox);
				AssertEquals("BindingMember", nameof(MessageSendingObject.ActualOfficeOfDestination), actualOfficeOfDestinationCodeFindBox.GetBindingMember());
			});
		}
	}
}
