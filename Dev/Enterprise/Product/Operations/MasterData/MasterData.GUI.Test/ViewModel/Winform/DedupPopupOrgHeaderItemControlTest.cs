using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DedupPopupOrgHeaderItemControlTest : TestCaseWithFactory
	{
		public void TestDedupPopupOrgHeaderItemControl_Binding()
		{
			var dataSource = GetDedupPopupBizoDataSource();

			var control = new DedupPopupOrgHeaderItemControl(dataSource, dataSource.DeduplicationResults[0]);
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(typeof(DeduplicationResultsListDataSource), control.BindingSource.DataSource.GetType());
				var currentBindingDataSource = control.CurrentDataItem as DeduplicationResultsListDataSource;
				AssertNotNull(currentBindingDataSource);

				CombineAssertions(() =>
				{
					AssertEquals("Name: ", control.fullNameCaptionLabel.Text);
					AssertEquals("TEST_ORG_TARGET", control.fullNameValueLabel.Text);
					AssertEquals("Code: ", control.codeCaptionLabel.Text);
					AssertEquals("TESORGT", control.codeValueLabel.Text);
				});
			}
		}

		public void TestDedupPopupOrgHeaderItemControl_Events()
		{
			var dataSource = GetDedupPopupBizoDataSource();

			var resultItem = dataSource.DeduplicationResults[0];

			using (var form = new ZForm())
			using (var parentControl = new DedupPopupOrgHeaderControl())
			using (var control = new DedupPopupOrgHeaderItemControlForTest(dataSource, resultItem, parentControl))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(Color.AliceBlue, control.detailTableLayoutPanel.BackColor);

				control.PerformClickGrid(null, null);
				AssertEquals(dataSource.SelectedResult, resultItem);
				AssertEquals(ColorHelper.SelectedItemColor, control.containerPanel.BackColor);

				control.PerformMouseEnterGrid(control.fullNameValueLabel, null);
				AssertEquals(ColorHelper.SelectedItemColor, control.containerPanel.BackColor);
				AssertEquals(ColorHelper.HighlightedItemColor, control.containerInnerPanel.BackColor);

				control.PerformMouseLeaveGrid(control.fullNameValueLabel, null);
				AssertEquals(ColorHelper.SelectedItemColor, control.containerPanel.BackColor);
				AssertEquals(ColorHelper.HighlightedItemColor, control.containerInnerPanel.BackColor);

				dataSource.SelectedResult = dataSource.DeduplicationResults[1];
				control.PerformMouseLeaveGrid(control.fullNameValueLabel, null);
				AssertEquals(Color.White, control.containerPanel.BackColor);
				AssertEquals(Color.White, control.containerInnerPanel.BackColor);

				control.OpenDetailsFormPerformed = false;
				control.PerformDoubleClickGrid(control.fullNameValueLabel, null);
				AssertEquals(dataSource.SelectedResult, resultItem);
				AssertEquals(parentControl.ActiveItemControl, control);
				Assert(control.OpenDetailsFormPerformed);
			}
		}

		public void TestDedupPopupOrgHeaderItemControl_WithParentControl()
		{
			var dataSource = GetDedupPopupBizoDataSource();

			using (var form = new ZForm())
			using (var parentControl = new DedupPopupOrgHeaderControl())
			{
				form.Controls.Add(parentControl);
				parentControl.SetDataBinding(dataSource, string.Empty);
				form.Show();

				var itemControls = parentControl.tableLayoutPanel.Controls.OfType<DedupPopupOrgHeaderItemControl>().Select(x => new DedupPopupOrgHeaderItemControlForTest(x)).ToArray();
				AssertEquals(2, itemControls.Length);

				using (var itemControl = itemControls[0])
				using (var otherItemControl = itemControls[1])
				{
					itemControl.PerformClickGrid(null, null);
					AssertEquals(itemControl, parentControl.ActiveItemControl);
					AssertEquals(itemControl.parentDataSource.SelectedResult, parentControl.ActiveItemControl.resultItem);
					AssertEquals(ColorHelper.SelectedItemColor, parentControl.ActiveItemControl.containerPanel.BackColor);

					otherItemControl.PerformClickGrid(null, null);
					AssertEquals(otherItemControl, parentControl.ActiveItemControl);
					AssertEquals(ColorHelper.SelectedItemColor, otherItemControl.containerPanel.BackColor);
					AssertEquals(Color.White, itemControl.containerInnerPanel.BackColor);

					//unselected the selected item
					otherItemControl.PerformClickGrid(null, null);
					AssertNull(otherItemControl.parentDataSource.SelectedResult);
					AssertNull(parentControl.ActiveItemControl);
					AssertEquals(ColorHelper.SelectedItemColor, otherItemControl.containerPanel.BackColor);
				}
			}
		}

		DedupPopupBizoDataSource GetDedupPopupBizoDataSource()
		{
			var master = Factory.NewWithValidTestData<OrgHeader>();
			master.OH_FullName = "TEST_ORG_MASTER";
			master.OH_Code = "TESORGM";

			var target = Factory.NewWithValidTestData<OrgHeader>();
			target.OH_FullName = "TEST_ORG_TARGET";
			target.OH_Code = "TESORGT";

			var scoringResultList = new List<ScoringResult>
		{
			TargetScorerController.Score(new DeduplicationOrgHeader(master), new DeduplicationOrgHeader(target), isStandardizingMaster: true),
			TargetScorerController.Score(new DeduplicationOrgHeader(target), new DeduplicationOrgHeader(master), isStandardizingMaster: true)
		};

			return new DedupPopupBizoDataSource(master, scoringResultList, null);
		}
	}

	class DedupPopupOrgHeaderItemControlForTest : DedupPopupOrgHeaderItemControl
	{
		internal bool OpenDetailsFormPerformed { get; set; }
		internal readonly DedupPopupOrgHeaderControl parentControl;

		internal DedupPopupOrgHeaderItemControlForTest(DedupPopupBizoDataSource parentDataSource, DeduplicationResultsListDataSource resultItem, DedupPopupOrgHeaderControl parentControl)
			: base(parentDataSource, resultItem)
		{
			this.parentControl = parentControl;
		}

		internal DedupPopupOrgHeaderItemControlForTest(DedupPopupOrgHeaderItemControl itemControl) : base(itemControl.parentDataSource, itemControl.resultItem)
		{
			parentControl = itemControl.Parent.Parent as DedupPopupOrgHeaderControl;
		}

		internal void PerformClickGrid(object sender, EventArgs ev) => ClickGrid(sender, ev);
		internal void PerformMouseEnterGrid(object sender, EventArgs ev) => MouseEnterGrid(sender, ev);
		internal void PerformMouseLeaveGrid(object sender, EventArgs ev) => MouseLeaveGrid(sender, ev);
		internal void PerformDoubleClickGrid(object sender, EventArgs ev) => DoubleClickGrid(sender, ev);
		protected override void OpenDetailsForm() => OpenDetailsFormPerformed = true;
		protected override DedupPopupOrgHeaderControl HeaderControl => parentControl;
	}
}
