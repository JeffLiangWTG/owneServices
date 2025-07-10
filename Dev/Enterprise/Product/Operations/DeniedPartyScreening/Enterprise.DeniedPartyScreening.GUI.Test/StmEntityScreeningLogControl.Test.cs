using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(StmEntityScreeningLogControl))]
	class StmEntityScreeningLogControlTest : BasherTest
	{
		public void TestGridColumns()
		{
			using (var form = new DummyForm())
			{
				form.Show();
				AssertEquals(13, form.ScreeningLogControl.LogsGrid.ColumnStyles.Count);

				var index = 0;
				CombineAssertions(() =>
				{
					AssertEquals("PJ_ScreenDate", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo)?.ColumnName);
					AssertEquals("PJ_SystemCreateTimeUtc", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo)?.ColumnName);
					AssertEquals("ScreenedByFullName", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo)?.ColumnName);
					AssertEquals("PJ_Status", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo)?.ColumnName);
					AssertEquals("StatusDescription", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo)?.ColumnName);
					AssertEquals("PJ_MatchingData", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("PJ_HighConfidenceResults", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("PJ_MediumConfidenceResults", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("PJ_LowConfidenceResultsCount", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo)?.ColumnName);
					AssertEquals("PJ_IncludedLists", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("PJ_ExcludedLists", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("PJ_ClearedReason", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo)?.ColumnName);
					AssertEquals("SourceInformation", (form.ScreeningLogControl.LogsGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo)?.ColumnName);
				});
			}
		}

		public void TestGridColumns_VisibleDefaultValues()
		{
			using (var form = new DummyForm())
			{
				form.Show();

				var index = 0;
				CombineAssertions(() =>
				{
					AssertEquals("PJ_ScreenDate visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).IsVisible);
					AssertEquals("PJ_SystemCreateTimeUtc visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).IsVisible);
					AssertEquals("ScreenedByFullName visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_Status not visible by default", false, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("StatusDescription visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_MatchingData visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_HighConfidenceResults visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_MediumConfidenceResults visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_LowConfidenceResultsCount visible by default", false, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_IncludedLists visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_ExcludedLists visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_ClearedReason not visible by default", false, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("SourceInformation visible by default", true, (form.ScreeningLogControl.LogsGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).IsVisible);
				});
			}
		}

		#region Implementation

		public override Form GetFormToBash()
		{
			return new DummyForm();
		}

		class DummyForm : ZForm
		{
			public DummyForm() : base()
			{ }

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				ScreeningLogControl = new StmEntityScreeningLogControl();
				ScreeningLogControl.Dock = DockStyle.Fill;
				Controls.Add(ScreeningLogControl);

				Size = new Size(800, 600);
				CaptionRenderingEnabled = true;
			}

			public StmEntityScreeningLogControl ScreeningLogControl { get; set; }
		}

		#endregion
	}
}
