using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public class RefCustariffFilterStripControlTest : ZFilterStripControlTest
	{
		[RequiresSTA]
		public void TestColumns()
		{
			using (var userControl = new RefCusTariffFilterStripControl())
			{
				CombineAssertions(() =>
				{
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[0], "ZZ1_ZZZ_NKDataGrouping", true, 61, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[1], "ZZ1_DataGroupingDescription", false, 138, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[2], "ZZ1_ZZI_NKTariffType", true, 76, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[3], "ZZ1_TariffTypeDescription", false, 244, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[4], "ZZ1_TariffCode", true, 75, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[5], "ZZ1_Description", true, 292, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[6], "ZZ1_AlternateLanguageDescription", true, 292, null);
					AssertColumnStyles((ZCheckBoxColumnStyleInfo)userControl.Grid.ColumnStyles[7], "ZZ1_IsSystem", true, 100, null);
					AssertColumnStyles((ZDateEditColumnStyleInfo)userControl.Grid.ColumnStyles[8], "ZZ1_StartDate", true, 95, null, ZDateTimePickerFormat.Long);
					AssertColumnStyles((ZDateEditColumnStyleInfo)userControl.Grid.ColumnStyles[9], "ZZ1_EndDate_ForDisplay", true, 109, null, ZDateTimePickerFormat.Long);
					AssertColumnStyles((ZDateEditColumnStyleInfo)userControl.Grid.ColumnStyles[10], "ZZ1_PublishedDate", false, 95, null, ZDateTimePickerFormat.Short);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[11], "ZZ1_ZZ8_UQ1", false, 44, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[12], "ZZ1_ZZ8_UQ2", false, 44, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[13], "ZZ1_ZZ8_UQ3", false, 44, null);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[14], "ZZ1_CRT_NKTariffVersion", false, 80, null);
				});
			}
		}

		void AssertColumnStyles(ZDateEditColumnStyleInfo columnInfo, string columnName, bool isVisible, int width, string groupName, ZDateTimePickerFormat dateTimeFormat)
		{
			AssertColumnStyles(columnInfo, columnName, isVisible, width, groupName);
			AssertEquals(columnName + "- DateTimeFormat", dateTimeFormat, columnInfo.DateTimeFormat);
		}

		void AssertColumnStyles(ZGridColumnInfo columnInfo, string columnName, bool isVisible, int width, string groupName)
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals(columnName + "- IsVisible", isVisible, columnInfo.IsVisible);
			AssertEquals(columnName + "- Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
			AssertEquals(columnName + "- GroupName", groupName, columnInfo.GroupName.Caption);
		}

		[RequiresSTA]
		public void TestGetNewFilteredGrid()
		{
			using (var control = new RefCusTariffFilterStripControl())
			{
				AssertType("FilteredGrid should be RefCusTariffGrid", typeof(RefCusTariffGrid), control.FilteredGrid);
			}
		}

		[RequiresSTA]
		public void TestInitializeMandatoryAttributeColumns()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var attrName1 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "ATT1", "Attribute Name 1", "TP1", Core.Constants.CountryCodes.China, "1P1");
			var attrName2 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "ATT2", "Attribute Name 2", "TP2", Core.Constants.CountryCodes.China, "2P1");

			var collection = new ChildTariffViewCollection(Factory, Core.Constants.CountryCodes.China, "1P1", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>() });
			using (var form = new ZForm())
			using (var control = new RefCusTariffFilterStripControl(collection, new RefCusTariffFilterStripBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.Grid;
				var customColumn1 = grid.GetColumnStyle("ATT1");
				AssertNotNull("Should have added a columnStyle for ATT1.", customColumn1);
				Assert("ColumnStyle for ATT1 should be visible by default.", customColumn1.IsVisible);
				AssertNullOrEmpty("Should not have a GroupName.", customColumn1.GroupName.Caption);
				AssertType("Should created a TextBoxColumnStyle for a STRING AttrName.", typeof(ZTextBoxColumnStyleInfo), customColumn1);
				var customColumn2 = grid.GetColumnStyle("ATT2");
				AssertNull("Should not have added a columnStyle for ATT2.", customColumn2);
			}
		}
	}
}
