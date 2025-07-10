using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SupportingDocumentsUserControl))]
sealed class SupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using var control = new SupportingDocumentsUserControl();
		AssertEquals(typeof(SupportingDocument), control.BindingSource.DataSourceType);
	}

	public void TestGridColumnCount()
	{
		using var control = new SupportingDocumentsUserControl();
		var grid = control.SupportingDocumentsGrid;
		grid.SetDataBinding(supportingDocumentCollection, "");
		control.Show();
		AssertEquals(3, grid.Columns.Count);
	}

	public void TestCodeGridColumnStyle()
	{
		using var control = new SupportingDocumentsUserControl();
		var grid = control.SupportingDocumentsGrid;
		grid.SetDataBinding(supportingDocumentCollection, "");
		control.Show();

		var columnStyleInfo = grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code);
		AssertNotNull("Column Style Info: CSI_Code", columnStyleInfo);
		CombineAssertions(() =>
		{
			AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, columnStyleInfo.CharacterCasing);
			AssertEquals("CSI_Code: IsVisible", true, columnStyleInfo.IsVisible);
		});
	}

	public void TestReferenceNumberGridColumnStyle()
	{
		using var control = new SupportingDocumentsUserControl();
		var grid = control.SupportingDocumentsGrid;
		grid.SetDataBinding(supportingDocumentCollection, "");
		control.Show();

		var columnStyleInfo = grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
		AssertNotNull("Column Style Info: CSI_ReferenceNumber", columnStyleInfo);
		CombineAssertions(() =>
		{
			AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, columnStyleInfo.CharacterCasing);
			AssertEquals("CSI_ReferenceNumber: IsVisible", true, columnStyleInfo.IsVisible);
			AssertEquals("CSI_ReferenceNumber: Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnStyleInfo.Width);
		});
	}

	public void TestDocumentDescriptionGridColumnStyle()
	{
		using var control = new SupportingDocumentsUserControl();
		var grid = control.SupportingDocumentsGrid;
		grid.SetDataBinding(supportingDocumentCollection, "");
		control.Show();

		var columnStyleInfo = grid.GetColumnStyle(SupportingDocument.Schema.DocumentDescription);
		AssertNotNull("Column Style Info: DocumentDescription", columnStyleInfo);
		CombineAssertions(() =>
		{
			AssertEquals("DocumentDescription: CharacterCasing", CharacterCasing.Normal, columnStyleInfo.CharacterCasing);
			AssertEquals("DocumentDescription: IsVisible", true, columnStyleInfo.IsVisible);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = Factory.New<SupportingDocument>();
		supportingDocumentCollection = new SupportingDocumentCollection(supportingDocument);
	}

	SupportingDocument supportingDocument;
	SupportingDocumentCollection supportingDocumentCollection;
}
