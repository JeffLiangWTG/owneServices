using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(PreviousDocumentsUserControl))]
sealed class PreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestIAdditionalTabPage()
	{
		CombineAssertions(() =>
		{
			using var control = new PreviousDocumentsUserControl();
			var additionalTabPage = control as IAdditionalTabPage;
			AssertEquals("Previous Documents", additionalTabPage.AdditionalTabPageCaption.Caption);
			AssertEquals(1, additionalTabPage.TabPageSequence);
			AssertEquals(true, additionalTabPage.AdditionalControlVisibility.isVisible(null));
			AssertEquals(control, additionalTabPage.AdditionalTabPageUserControl);
		});
	}

	public void TestGridLayout()
	{
		var expectedColumns = new List<(string ColumnName, Type InfoType)>
		{
			("CSI_Code", typeof(ZCodeFindBoxColumnStyle)),
			("DocumentDescription", typeof(ZTextBoxColumnStyle)),
			("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyle))
		};

		AssertGridColumns("PreviousDocumentsGrid", expectedColumns);
	}

	void AssertGridColumns(string gridName, List<(string ColumnName, Type InfoType)> expectedColumns)
	{
		using var userControl = new PreviousDocumentsUserControl();
		var grid = userControl.AssertContainsControl<ZGrid>(gridName);
		CombineAssertions(() =>
		{
			foreach (var (columnName, columnStyleInfoType) in expectedColumns)
			{
				var column = grid.GetColumnStyle(columnName);
				AssertNotNull($"{columnName} Exists", column);
				AssertEquals($"{columnName} Type", columnStyleInfoType, column.ColumnStyleType);
				AssertEquals($"{columnName} is Visible", true, column.IsVisible);
			}
		});
	}
}
