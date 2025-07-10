using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	abstract class BaseManifestsToOpenUserControlTestCase : TestCaseWithFactory
	{
		protected abstract Control CreateControl();

		protected abstract string ControlName { get; }

		protected abstract ExpectedGridColumnInfo[] ExpectedGridColumnInfos { get; }

		public void TestUserControlFieldsVisibility()
		{
			var expectedGridColumnInfos = ExpectedGridColumnInfos;
			using var form = new ZForm(header);
			using var control = CreateControl();
			form.Controls.Add(control);
			form.Show();

			var userControl = form.Controls.Find(ControlName, searchAllChildren: true).FirstOrDefault();
			AssertNotNull($"User control {ControlName} should be exists", userControl);

			var grid = userControl.FindSingle<ZArchitecture.ZGrid>(c => c.Name == "AdditionalInfoGrid");
			var gridColumnInfos = grid.ColumnStyles;

			AssertEquals("Visible ExpectedGridColumnInfos' length should be equal to grid's column count.", expectedGridColumnInfos.Count(g => g.IsVisible), gridColumnInfos.Count);

			foreach (ZGridColumnInfo gridColumnInfo in gridColumnInfos)
			{
				AssertNotNull(gridColumnInfo);

				var expectedColumnInfo =
					expectedGridColumnInfos.FirstOrDefault(c => c.FieldName.Equals(gridColumnInfo.ColumnName));

				AssertNotNull($"Column {gridColumnInfo.ColumnName} not found in expected list", expectedColumnInfo);

				var columnControl = userControl.FindSingleOrDefault<Control>(c => c.Name == expectedColumnInfo.ControlName);
				AssertNotNull($"Cannot find the control {expectedColumnInfo.ControlName} for column {gridColumnInfo.ColumnName}", columnControl);
				AssertEquals($"Visibility check for {expectedColumnInfo.ControlName}", expectedColumnInfo.IsVisible, columnControl.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader header;

		protected struct ExpectedGridColumnInfo()
		{
			public ExpectedGridColumnInfo(string fieldName, string controlName, bool isVisible) : this()
			{
				FieldName = fieldName;
				ControlName = controlName;
				IsVisible = isVisible;
			}

			public string FieldName { get; set; }

			public string ControlName { get; set; }

			public bool IsVisible { get; set; }
		}
	}
}
