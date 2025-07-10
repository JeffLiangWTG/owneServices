using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class AviationFuelTypeUserControlTest : TestCaseWithFactory
	{
		public void AviationFuelTypeGridLayout()
		{
			using (var control = new AviationFuelTypeUserControl())
			{
				var aviationFuelTypeControl = (ZGrid)control.Controls.Find("AviationFuelTypeGrid", true).First();
				var aviationFuelTypeColumsStyles = aviationFuelTypeControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

				CombineAssertions(() =>
				{
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_ReferenceNumber2, 0);
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_DateOfIssue, 1);
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_ReferenceNumber, 2);
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_Value, 3);
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_RX_NKCurrency, 4);
					AssertColumnVisibilyAndIndex(aviationFuelTypeColumsStyles, AviationFuelType.Schema.CSI_Description, 5);
				});
			}
		}

		void AssertColumnVisibilyAndIndex(ZGridColumnInfo[] columnsStyleList, ZString columnName, ZInt index)
		{
			var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
			AssertNotNull($"{columnName} not null", columnStyle);
			Assert($"{columnName} is visible", columnStyle.IsVisible);
			AssertEquals($"{columnName} index", Array.IndexOf(columnsStyleList, columnStyle), index);
		}

		public void TestGridColumnSizes()
		{
			using (var control = new AviationFuelTypeUserControl())
			{
				var aviationFuelTypeGrid = control.FindSingle<ZGrid>("AviationFuelTypeGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber2", 100, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_ReferenceNumber2).Width);
					AssertEquals("CSI_DateOfIssue", 70, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_DateOfIssue).Width);
					AssertEquals("CSI_ReferenceNumber", 150, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_ReferenceNumber).Width);
					AssertEquals("CSI_Value", 150, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_Value).Width);
					AssertEquals("CSI_Description", 55, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_RX_NKCurrency).Width);
					AssertEquals("CSI_Description", 500, aviationFuelTypeGrid.GetColumnStyle(AviationFuelType.Schema.CSI_Description).Width);
				});
			}
		}
	}
}
