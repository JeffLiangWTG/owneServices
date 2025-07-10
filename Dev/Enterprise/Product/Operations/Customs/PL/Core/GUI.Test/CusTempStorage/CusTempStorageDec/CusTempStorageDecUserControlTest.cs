using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class CusTempStorageDecUserControlTest : TestCaseWithFactory
{
	public void TestLinesTabPageCaption()
	{
		RunForControl(control =>
		{
			var linesTab = control.FindSingle<ZTabPage>("LinesTabPage");
			AssertEquals("Line Details", linesTab.CaptionResourceString.Caption);
		});
	}

	public void TestMessageStatusTextBoxCaption()
	{
		RunForControl(control =>
		{
			var statusTextBox = control.FindSingle<ZTextBox>("StatusTextBox");
			AssertEquals("Message Status", statusTextBox.CaptionResourceString.Caption);
		});
	}

	public void TestControls()
	{
		var header = CusTempStorageJobHeader.New(Factory);

		using (var form = new CusTempStorageForm(header))
		{
			form.Show();

			var userControlForPLugin = form.FindSingle<TemporyStorageUserControlForPlugin>("userControlForPLugin");
			var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
			var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
			mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;

			var cusDecTabPageUserControl = userControlForPLugin.FindSingle<CusTempStorageDecUserControl>("CusDecTabPageUserControl");
			CombineAssertions(() =>
			{
				AssertEquals("GuaranteeDescriptionTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("GuaranteeDescriptionTextBox").Visible);
				AssertEquals("SJH_CPH_GuaranteeGuidFindBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZGuidFindBox>("SJH_CPH_GuaranteeGuidFindBox").Visible);
				AssertEquals("SJH_IsSameConditionExpectedCheckBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZCheckBox>("SJH_IsSameConditionExpectedCheckBox").Visible);
				AssertEquals("SJH_IsExaminationExpectedCheckBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZCheckBox>("SJH_IsExaminationExpectedCheckBox").Visible);
				AssertEquals("StatusTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("StatusTextBox").Visible);
				AssertEquals("SJH_TempStorageEndDateUtcDateEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZDateEdit>("SJH_TempStorageEndDateUtcDateEdit").Visible);
				AssertEquals("CreatedDateEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZDateEdit>("CreatedDateEdit").Visible);
				AssertEquals("TSL_CustomsStatusTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("TSL_CustomsStatusTextBox").Visible);
				AssertEquals("OwnerReferenceNumberTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("OwnerReferenceNumberTextBox").Visible);
				AssertEquals("OwnerReferenceTypeDropEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZDropEdit>("OwnerReferenceTypeDropEdit").Visible);
				AssertEquals("UnionStatusDropEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZDropEdit>("UnionStatusDropEdit").Visible);
				AssertEquals("DestinationPlaceTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("DestinationPlaceTextBox").Visible);
				AssertEquals("GoodsLocationTextBox", true, cusDecTabPageUserControl.FindSingleOrDefault<ZTextBox>("GoodsLocationTextBox").Visible);
				AssertEquals("GoodsTypeDropEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZDropEdit>("GoodsTypeDropEdit").Visible);
				AssertEquals("GrossWeightCalcDropEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZCalcDropEdit>("GrossWeightCalcDropEdit").Visible);
				AssertEquals("PackageQuantityCalcEdit", true, cusDecTabPageUserControl.FindSingleOrDefault<ZCalcEdit>("PackageQuantityCalcEdit").Visible);
			});
		}
	}

	public void TestLineItemsGridColumns()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
		var storageDec = CusTempStorageDec.New(header);
		storageDec.CusTempStorageLines.AddNew();

		using (var form = new CusTempStorageForm(header))
		{
			form.Show();

			var userControlForPLugin = form.FindSingle<TemporyStorageUserControlForPlugin>("userControlForPLugin");
			var mainTabControl = userControlForPLugin.FindSingle<ZTabControl>("MainTabControl");
			var entrySummaryDeclarationTabPage = userControlForPLugin.FindSingle<ZTabPage>("EntrySummaryDeclarationTabPage");
			mainTabControl.SelectedTab = entrySummaryDeclarationTabPage;
			var cusDecTabPageUserControl = userControlForPLugin.FindSingle<CusTempStorageDecUserControl>("CusDecTabPageUserControl");
			var lineItemsGrid = cusDecTabPageUserControl.FindSingleOrDefault<ZGrid>(c => c.Name == "LinesGrid");

			var visibleColumnCount = lineItemsGrid.Columns.Where(x => x.IsVisible).Count();
			CombineAssertions(() =>
			{
				AssertEquals("Expected 15 columns", 15, visibleColumnCount);

				for (int i = 0; i < ExpectedDefaultColumnsForLineItemsGrid.Count; i++)
				{
					var column = lineItemsGrid.Columns[i];
					AssertNotNull("lineItemsGrid.Column", column);
					var expectedColumnName = ExpectedDefaultColumnsForLineItemsGrid[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			});
		}
	}

	void RunForControl(Action<TemporyStorageUserControlForPlugin> methodToRun)
	{
		var header = CusTempStorageJobHeader.New(Factory);
		using (var form = new ZForm(header))
		{
			using (var control = new TemporyStorageUserControlForPlugin())
			{
				form.Controls.Add(control);
				form.Show();
				methodToRun.Invoke(control);
			}
		}
	}

	List<string> ExpectedDefaultColumnsForLineItemsGrid
	{
		get
		{
			var columns = new List<string>();
			columns.Add("TSL_LineNo");
			columns.Add("TSL_OwnerReferenceType");
			columns.Add("TSL_OwnerReferenceNumber");
			columns.Add("TSL_CustomsStatus");
			columns.Add("TSL_GoodsDescription");
			columns.Add("TSL_GrossWeight");
			columns.Add("TSL_PackageQty");
			columns.Add("TSL_PackageType");
			columns.Add("TSL_UnionStatus");
			columns.Add("TSL_GoodsType");
			columns.Add("TSL_LocationOfGoods");
			columns.Add("TSL_DestinationPlace");
			columns.Add("TSL_ReferenceNumber");
			columns.Add("TSL_ReferenceNumberLine");
			columns.Add("TSL_ReferenceNumberType");

			return columns;
		}
	}
}
