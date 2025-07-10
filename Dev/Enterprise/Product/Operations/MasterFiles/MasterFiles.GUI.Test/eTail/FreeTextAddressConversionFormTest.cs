using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing;

[TestedType(typeof(FreeTextAddressConversionForm<>))]
public class FreeTextAddressConversionFormTest : ZFormBasherTest
{
	public void TestFreeTextAddressConversionForm_TestDataGridBindingMember()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using (var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion))
		{
			form.Show();

			var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
			var similarAddressGrid =
				form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;

			CombineAssertions("Ensure the correct binding members", () =>
			{
				AssertEquals(conversion, form.DataSource);
				AssertEquals("AddressesToConvertView", freeTextAddressGrid.GetBindingMember());
				AssertEquals("AddressesToConvertView.SimilarOrgMatches", similarAddressGrid.GetBindingMember());
			});
		}
	}

	public void TestFreeTextAddressConversionForm_Ignore()
	{
		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		consigneeOrgHeader.OH_FullName = consignment.ConsigneeName;
		var address = consigneeOrgHeader.Addresses.AddNew();
		address.OA_Address1 = consignment.ConsigneeAddress1;
		address.OA_City = consignment.ConsigneeCity;
		Factory.Save();

		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();
		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
		CombineAssertions("The freetext grid should display 2 rows for conversion.AddressesToConvertView", () =>
		{
			AssertEquals(2, conversion.AddressesToConvertView.Count);
			AssertContainsExactElementsInAnyOrder(conversion.AddressesToConvertView, freeTextAddressGrid!.List);
		});

		freeTextAddressGrid!.Select(0);
		var selectedFreeTextAddress = freeTextAddressGrid!.GetFirstSelectedRow() as FreeTextAddress<DummyConsignment>;
		var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;

		CombineAssertions("The simiar org grid should display rows for the selectedFreeTextAddress.SimilarOrgMatches", () =>
		{
			AssertEquals(consigneeOrgHeader.Addresses.Count, selectedFreeTextAddress!.SimilarOrgMatches.Count);
			AssertContainsExactElementsInAnyOrder(selectedFreeTextAddress!.SimilarOrgMatches, similarAddressGrid!.List);
		});

		var ignoreButton = form.Controls.Find("IgnoreButton", true).Single() as ZButton;
		ignoreButton!.PerformClick();
		freeTextAddressGrid.Select(0);

		CombineAssertions("freeTextAddressGrid has 1 free text address left and similar address grid has 0 left", () =>
		{
			AssertEquals(1, freeTextAddressGrid!.List.Count);
			Assert(similarAddressGrid!.List == null || similarAddressGrid!.List.Count == 0);
		});

		ignoreButton!.PerformClick();
		CombineAssertions("freeTextAddressGrid has 0 free text address left and similar address grid has 0 left", () =>
		{
			Assert(freeTextAddressGrid!.List == null || freeTextAddressGrid!.List.Count == 0);
			Assert(similarAddressGrid!.List == null || similarAddressGrid!.List.Count == 0);
		});
	}

	public void TestFreeTextAddressConversionForm_Select()
	{
		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		consigneeOrgHeader.OH_FullName = consignment.ConsigneeName;
		var address = consigneeOrgHeader.Addresses.AddNew();
		address.OA_Address1 = consignment.ConsigneeAddress1;
		address.OA_City = consignment.ConsigneeCity;
		Factory.Save();

		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();
		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
		var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
		freeTextAddressGrid!.Select(0);
		similarAddressGrid!.Select(0);

		var selectButton = form.Controls.Find("SelectButton", true).Single() as ZButton;
		selectButton!.PerformClick();

		CombineAssertions("freeTextAddressGrid has 1 free text address left and similar address grid has 0 left", () =>
		{
			AssertEquals(1, freeTextAddressGrid!.List.Count);
			Assert(similarAddressGrid!.List == null || similarAddressGrid!.List.Count == 0);
		});
	}

	public void TestFreeTextAddressConversionForm_NewOrg_WhenNoAvailableAddressForSelection()
	{
		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		consigneeOrgHeader.OH_FullName = consignment.ConsigneeName;
		var address = consigneeOrgHeader.Addresses.AddNew();
		address.OA_Address1 = consignment.ConsigneeAddress1;
		address.OA_City = consignment.ConsigneeCity;
		Factory.Save();

		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();

		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
		var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
		var createNewOrgButton = form.Controls.Find("CreateNewOrgButton", true).Single() as ZButton;

		freeTextAddressGrid!.Select(0);
		similarAddressGrid!.Select(0);

		var selectButton = form.Controls.Find("SelectButton", true).Single() as ZButton;
		selectButton!.PerformClick();

		var ignoreButton = form.Controls.Find("IgnoreButton", true).Single() as ZButton;
		ignoreButton!.PerformClick();

		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
		createNewOrgButton!.PerformClick();

		AssertEquals("There is no address that needs to be used to create the organization.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestFreeTextAddressConversionForm_NewOrg()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();
		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
		var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
		freeTextAddressGrid!.Select(1);

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
		{
			if (obj is ZOrganisationsForm organisationsForm)
			{
				var orgHeader = organisationsForm.BusinessEntity as OrgHeader;
				var address = orgHeader!.MainAddress;

				CombineAssertions("Form should be pre-filled with consignment details", () =>
				{
					AssertEquals(consignment.ConsigneeAddress1, address.OA_Address1);
					AssertEquals(consignment.ConsigneeCountryCode, address.OA_RN_NKCountryCode);
					AssertEquals(consignment.ConsigneeState, address.OA_State);
					AssertEquals(consignment.ConsigneePostcode, address.OA_PostCode);
				});

				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				organisationsForm.FireSaveButton();
				organisationsForm.Close();
			}
		});

		var createNewOrgButton = form.Controls.Find("CreateNewOrgButton", true).Single() as ZButton;
		createNewOrgButton!.PerformClick();

		Assert("The consignee address should be linked to an org", !consignment.ConsigneeAddressId.IsEmpty);
	}

	public void TestFreeTextAddressConversionForm_DataGridHasNecessaryColumns()
	{
		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		consigneeOrgHeader.OH_FullName = consignment.ConsigneeName;
		var address = consigneeOrgHeader.Addresses.AddNew();
		address.OA_Address1 = consignment.ConsigneeAddress1;
		address.OA_City = consignment.ConsigneeCity;
		Factory.Save();

		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();
		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;
		var similarAddressGrid = form.Controls.Find("SimilarOrgsDisplayGrid", true).Single() as SimilarOrgsDisplayGrid;
		freeTextAddressGrid!.Select(0);

		var columnStyles = freeTextAddressGrid.ColumnStyles.OfType<ZGridColumnInfo>();
		CombineAssertions(() =>
		{
			var zGridColumnInfos = columnStyles as ZGridColumnInfo[] ?? columnStyles.ToArray();
			AssertEquals(8, freeTextAddressGrid.ColumnStyles.Count);
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "PartyName"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "WaybillNumber"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address1"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address2"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "City"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Postcode"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "State"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Country"));
		});
	}

	public void TestFreeTextAddressConversionForm_IgnoreButtonNotVisible()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion, ignoreButtonVisible: false);
		form.Show();

		var ignoreButton = form.Controls.Find("IgnoreButton", true).Single() as ZButton;
		AssertEquals("Ignore button should not be visible", false, ignoreButton!.Visible);

		var selectButton = form.Controls.Find("SelectButton", true).Single() as ZButton;
		var createNewOrgButton = form.Controls.Find("CreateNewOrgButton", true).Single() as ZButton;

		AssertEquals("Select button should be visible", true, selectButton!.Visible);
		AssertEquals("Create new org button should be visible", true, createNewOrgButton!.Visible);
	}

	public void TestFreeTextAddressConversionForm_DisabledPostingButtons()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion, enablePostingButtons: false);
		form.Show();

		var postingButtons = form.Controls.Find("PostingButtons", true).Single() as ZPostingButtonsUserControl;

		var saveButton = postingButtons?.SaveButton;
		AssertEquals("Save button should not be enabled", false, saveButton?.Enabled);

		var saveAndCloseButton = postingButtons?.SaveAndCloseButton;
		AssertEquals("Save&Close button should not be enabled", false, saveAndCloseButton?.Enabled);

		var closeButton = postingButtons?.CloseButton;
		AssertEquals("Close button should be enabled", true, closeButton?.Enabled);

		var formClosedEventFired = false;
		form.FormClosed += (_, _) => formClosedEventFired = true;

		closeButton?.PerformClick();
		AssertEquals("Form should be closed when close button is clicked", true, formClosedEventFired);
	}

	public void TestFreeTextAddressConversionForm_InitializeGridColumns_WithNullProvider()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });

		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion);
		form.Show();

		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;

		var columnStyles = freeTextAddressGrid?.ColumnStyles.OfType<ZGridColumnInfo>();
		CombineAssertions(() =>
		{
			var zGridColumnInfos = columnStyles as ZGridColumnInfo[] ?? columnStyles?.ToArray();
			AssertEquals(8, freeTextAddressGrid?.ColumnStyles.Count);
			AssertNotNull(zGridColumnInfos);
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "PartyName"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "WaybillNumber"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address1"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address2"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "City"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Postcode"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "State"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Country"));
		});
	}

	public void TestFreeTextAddressConversionForm_InitializeGridColumns_WithCustomProvider()
	{
		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });

		var customLayoutProvider = new TestGridColumnLayoutProvider();

		using var form = new FreeTextAddressConversionForm<DummyConsignment>(conversion, freeTextAddressGridLayoutProvider: customLayoutProvider);
		form.Show();

		var freeTextAddressGrid = form.Controls.Find("FreeTextAddressGrid", true).Single() as ZDisplayGrid;

		var columnStyles = freeTextAddressGrid?.ColumnStyles.OfType<ZGridColumnInfo>();
		CombineAssertions(() =>
		{
			var zGridColumnInfos = columnStyles as ZGridColumnInfo[] ?? columnStyles?.ToArray();
			AssertEquals(6, freeTextAddressGrid?.ColumnStyles.Count);
			AssertNotNull(zGridColumnInfos);
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "WaybillNumber"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "PartyName"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address1"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "Address2"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "City"));
			Assert(zGridColumnInfos.Any(c => c.ColumnName == "State"));
		});
	}

	protected override Form GetFormToBashCore()
	{
		var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		consigneeOrgHeader.OH_FullName = consignment.ConsigneeName;
		var address = consigneeOrgHeader.Addresses.AddNew();
		address.OA_Address1 = consignment.ConsigneeAddress1;
		address.OA_City = consignment.ConsigneeCity;
		Factory.Save();

		var conversion = new FreeTextAddressConversion<DummyConsignment>(Factory);
		conversion.AddToList(new List<DummyConsignment> { consignment });
		return new FreeTextAddressConversionForm<DummyConsignment>(conversion);
	}

	public override Type FormToBashType => typeof(FreeTextAddressConversionForm<DummyConsignment>);

	protected override void SetUp()
	{
		base.SetUp();
		consignment = Factory.NewWithValidTestData<DummyConsignment>();

		consignment.ConsigneeName = "ConsigneeName";
		consignment.ConsigneeAddress1 = "ConsigneeAddress1";
		consignment.ConsigneeAddress2 = "ConsigneeAddress2";
		consignment.ConsigneeCity = "Sydney";
		consignment.ConsigneeState = "NSW";
		consignment.ConsigneePostcode = "2134";
		consignment.ConsigneeCountryCode = "AU";
		consignment.ConsigneePhone = "+61 2 2907 5513";
		consignment.ConsigneeMobile = "+61 2 2907 5512";
		consignment.ConsigneeFax = "+61 2 2907 5511";
		consignment.ConsigneeEmail = "ConsigneeEmail@gmail.com";

		consignment.ShipperName = "ShipperName";
		consignment.ShipperAddress1 = "ShipperAddress1";
		consignment.ShipperAddress2 = "ShipperAddress2";
		consignment.ShipperCity = "Portland";
		consignment.ShipperState = "OR";
		consignment.ShipperPostcode = "97250";
		consignment.ShipperCountryCode = "US";
		consignment.ShipperPhone = "3321246689";
		consignment.ShipperMobile = "3321246688";
		consignment.ShipperFax = "ShipperFax";
		consignment.ShipperEmail = "ConsigneeEmail@gmail.com";
	}

	// Helper test class to verify grid layout provider behavior
	class TestGridColumnLayoutProvider : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.WaybillNumber, 200, info => { info.Caption = "House Bill"; });
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.PartyName, 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.Address1, 200);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.Address2, 160);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.City, 140);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<DummyConsignment>.Schema.State, 140);
			return builder.Build();
		}
	}

	DummyConsignment consignment;
}
