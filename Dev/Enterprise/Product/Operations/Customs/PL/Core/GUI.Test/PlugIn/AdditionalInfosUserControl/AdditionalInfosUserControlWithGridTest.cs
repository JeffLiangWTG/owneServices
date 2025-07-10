using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class AdditionalInfosUserControlWithGridTest : TestCaseWithFactory
{
	const string import = EU.Business.MessageTypeList.Codes.Import;
	const string export = EU.Business.MessageTypeList.Codes.Export;

	public void TestGridDefaultColumn_Import()
	{
		AssertGridColumn(new AdditionalInfosTestData(true));
	}

	public void TestGridDefaultColumn_Export()
	{
		AssertGridColumn(new AdditionalInfosTestData(false));
	}

	void AssertGridColumn(TestData testData)
	{
		using (var frm = new ZForm(testData.GetBindingParent(Factory)))
		using (var control = new AdditionalInfosUserControlWithGrid())
		{
			new ControlRebinder().Rebind(control, "FilteredInvoiceLines", testData.BindingString);
			frm.Controls.Add(control);
			frm.Show();
			var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

			CombineAssertions(() =>
			{
				AssertSequencesEqual("Order", testData.OrderedColumnDetails.Select(x => x.ColumnName), grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
				AssertSequencesEqual("Names", testData.OrderedColumnDetails.Select(x => x.ColumnCaption), grid.DefaultColumns.Select(x => grid.GetColumnCaption(x.ColumnName)));
				AssertSequencesEqual("Widths", testData.OrderedColumnDetails.Select(x => x.ColumnWidth), grid.DefaultColumns.Select(x => grid.GetColumnStyle(x.ColumnName).Width));
			});
		}
	}

	abstract class TestData
	{
		public abstract string BindingString { get; }

		public abstract BusinessObject GetBindingParent(BusinessObjectFactory factory);

		public abstract (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails { get; }
	}

	class AdditionalInfosTestData : TestData
	{
		public AdditionalInfosTestData(bool isImport)
		{
			IsImport = isImport;
		}

		public override string BindingString => "FilteredInvoiceLines";

		public override BusinessObject GetBindingParent(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = IsImport ? import : export;
			return declaration;
		}

		public override (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => IsImport
			? ImportAvailableColumnNames
			: ExportAvailableColumnNames;

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] ImportAvailableColumnNames => new[]
		{
			(CusSupportingInfo.Schema.CSI_Code, "Full Type", 80),
			(CusSupportingInfo.Schema.CSI_Description, "Description", 530)
		};
		(string ColumnName, string ColumnCaption, int ColumnWidth)[] ExportAvailableColumnNames => new[]
		{
			(CusSupportingInfo.Schema.CSI_SubType, "Kind", 80),
			(CusSupportingInfo.Schema.CSI_Code, "Full Type", 80),
			(CusSupportingInfo.Schema.CSI_ReferenceNumber, "Reference", 131),
			(CusSupportingInfo.Schema.CSI_Description, "Description", 530)
		};

		public bool IsImport;
	}
}
