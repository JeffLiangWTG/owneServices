using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DedupePopupPersonItemControlTest : TestCaseWithFactory
	{
		public void TestDedupePopupPersonItemControl_Binding()
		{
			var dataSource = GetDedupPopupBizoDataSource();

			var control = new DedupePopupPersonItemControl(dataSource, dataSource.DeduplicationResults[0]);
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var mainInfoLabel = control.Controls.Find("mainInfoLabel", true)[0] as ZLabel;
				var personTypeLabel = control.Controls.Find("personTypeLabel", true)[0] as ZLabel;
				var fullnameLabel = control.Controls.Find("fullnameLabel", true)[0] as ZLabel;

				CombineAssertions(() =>
				{
					AssertNotNull(mainInfoLabel);
					AssertEquals("Michael Kheirabi@qq.com", mainInfoLabel.Text);
					AssertNotNull(personTypeLabel);
					AssertEquals("Person", personTypeLabel.Text);
					AssertNotNull(fullnameLabel);
					AssertEquals("Test Person Name", fullnameLabel.Text);
				});
			}
		}

		[RequiresSTA]
		public void TestDedupePopupPersonItemControl_Events()
		{
			var dataSource = GetDedupPopupBizoDataSource();

			var resultItem = dataSource.DeduplicationResults[0];

			using (var form = new ZForm())
			using (var parentControl = new DedupePopupPersonControl())
			using (var control = new DedupePopupPersonItemControlForTest(dataSource, resultItem, parentControl))
			{
				form.Controls.Add(control);
				form.Show();

				var contentTableLayoutPanel = control.Controls.Find("contentTableLayoutPanel", true)[0] as KTableLayoutPanel;
				var containerPanel = control.Controls.Find("containerPanel", true)[0] as ZPanel;
				var containerInnerPanel = control.Controls.Find("containerInnerPanel", true)[0] as ZPanel;

				AssertNotNull(containerInnerPanel);
				AssertNotNull(containerPanel);
				AssertNotNull(contentTableLayoutPanel);

				AssertEquals(Color.AliceBlue, contentTableLayoutPanel.BackColor);

				control.PerformClickGrid(null, null);
				AssertEquals(dataSource.SelectedResult, resultItem);
				AssertEquals(ColorHelper.SelectedItemColor, containerPanel.BackColor);

				control.PerformMouseEnterGrid(null, null);
				AssertEquals(ColorHelper.SelectedItemColor, containerPanel.BackColor);
				AssertEquals(ColorHelper.HighlightedItemColor, containerInnerPanel.BackColor);

				control.PerformMouseLeaveGrid(null, null);
				AssertEquals(ColorHelper.SelectedItemColor, containerPanel.BackColor);
				AssertEquals(ColorHelper.HighlightedItemColor, containerInnerPanel.BackColor);

				dataSource.SelectedResult = dataSource.DeduplicationResults[1];
				control.PerformMouseLeaveGrid(null, null);
				AssertEquals(Color.White, containerPanel.BackColor);
				AssertEquals(Color.White, containerInnerPanel.BackColor);

				control.OpenDetailsFormPerformed = false;
				control.PerformDoubleClickGrid(null, null);
				AssertEquals(dataSource.SelectedResult, resultItem);
				AssertEquals(parentControl.ActiveItemControl, control);
				Assert(control.OpenDetailsFormPerformed);
			}
		}

		DedupPopupBizoDataSource GetDedupPopupBizoDataSource()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			masterPerson.PER_FullName = "Test Person Name";
			masterPerson.PER_HomePhone_Formatted = "+61 426 829 924";
			masterPerson.PER_EmailAddress2 = "Michael Kheirabi@qq.com";

			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "Test Person Name";
			targetPerson.PER_HomePhone_Formatted = "+61 426 829 924";
			targetPerson.PER_EmailAddress2 = "Michael Kheirabi@qq.com";

			var scoringResultList = new List<ScoringResult>
			{
				TargetScorerController.Score(new DeduplicationGlbPerson(masterPerson), new DeduplicationGlbPerson(targetPerson), isStandardizingMaster: true),
				TargetScorerController.Score(new DeduplicationGlbPerson(targetPerson), new DeduplicationGlbPerson(masterPerson), isStandardizingMaster: true)
			};

			return new DedupPopupBizoDataSource(masterPerson, scoringResultList, null);
		}
	}

	class DedupePopupPersonItemControlForTest : DedupePopupPersonItemControl
	{
		public bool OpenDetailsFormPerformed { get; set; }
		public readonly DedupePopupPersonControl parentControl;

		public DedupePopupPersonItemControlForTest(DedupPopupBizoDataSource parentDataSource, DeduplicationResultsListDataSource resultItem, DedupePopupPersonControl parentControl)
			: base(parentDataSource, resultItem)
		{
			this.parentControl = parentControl;
		}

		public DedupePopupPersonItemControlForTest(DedupePopupPersonItemControl itemControl) : base(itemControl.parentDataSource, itemControl.resultItem)
		{
			parentControl = itemControl.Parent.Parent as DedupePopupPersonControl;
		}

		public void PerformMouseEnterGrid(object sender, EventArgs ev)
		{
			MouseEnterGrid(null, null);
		}

		public void PerformClickGrid(object sender, EventArgs ev)
		{
			ClickGrid(null, null);
		}

		public void PerformMouseLeaveGrid(object sender, EventArgs ev)
		{
			MouseLeaveGrid(null, null);
		}

		public void PerformDoubleClickGrid(object sender, EventArgs ev)
		{
			DoubleClickGrid(null, null);
		}

		protected override void OpenDetailsForm() => OpenDetailsFormPerformed = true;

		protected override DedupePopupPersonControl PersonControl => parentControl;
	}
}
