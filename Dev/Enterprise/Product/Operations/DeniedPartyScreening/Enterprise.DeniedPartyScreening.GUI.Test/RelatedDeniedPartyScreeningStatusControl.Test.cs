using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(RelatedDeniedPartyScreeningStatusControl))]
	class RelatedDeniedPartyScreeningStatusControlTest : BasherTest
	{
		public void TestGridColumns()
		{
			using (var form = new DummyForm())
			{
				form.Show();

				AssertEquals(14, form.ScreeningStatusControl.LogsGrid.ColumnStyles.Count);

				var index = 0;
				CombineAssertions(() =>
				{
					AssertEquals("PJ_ScreenDate", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).ColumnName);
					AssertEquals("PJ_SystemCreateTimeUtc", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).ColumnName);
					AssertEquals("ScreenedByFullName", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("PJ_Status", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("StatusDescription", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("PJ_MatchingData", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("PJ_HighConfidenceResults", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("PJ_MediumConfidenceResults", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("PJ_LowConfidenceResultsCount", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("RelatedOrganization", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).ColumnName);
					AssertEquals("PJ_IncludedLists", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("PJ_ExcludedLists", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("PJ_ClearedReason", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).ColumnName);
					AssertEquals("SourceInformation", (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).ColumnName);
				});
			}
		}

		public void TestGridColumns_AreAllReadOnly()
		{
			using (var form = new DummyForm())
			{
				form.Show();
				AssertEquals("All Columns should be read only", true, form.ScreeningStatusControl.LogsGrid.ColumnStyles.Cast<ZGridColumnInfo>().All(style => style.IsReadOnly));
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
					AssertEquals("PJ_ScreenDate visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).IsVisible);
					AssertEquals("PJ_SystemCreateTimeUtc visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZDateEditColumnStyleInfo).IsVisible);
					AssertEquals("ScreenedByFullName visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_Status visible by default", false, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("StatusDescription visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_MatchingData visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_HighConfidenceResults visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_MediumConfidenceResults visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_LowConfidenceResultsCount visible by default", false, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("RelatedOrganization visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).IsVisible);
					AssertEquals("PJ_IncludedLists visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_ExcludedLists visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("PJ_ClearedReason not visible by default", false, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index++] as ZMultiLineTextBoxColumnInfo).IsVisible);
					AssertEquals("SourceInformation visible by default", true, (form.ScreeningStatusControl.LogsGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).IsVisible);
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
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				CaptionRenderingEnabled = true;
				ScreeningStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				Controls.Add(ScreeningStatusControl);
			}

			public RelatedDeniedPartyScreeningStatusControl ScreeningStatusControl { get; set; }
		}

		#endregion
	}
}
