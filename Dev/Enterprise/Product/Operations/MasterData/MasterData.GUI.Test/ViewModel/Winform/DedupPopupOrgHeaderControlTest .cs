using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(DedupPopupOrgHeaderControl))]
	public class DedupPopupOrgHeaderControlTest : BasherTest
	{
		public void TestDedupPopupOrgHeaderControl()
		{
			var master = Factory.NewWithValidTestData<OrgHeader>();
			var target = Factory.NewWithValidTestData<OrgHeader>();
			var scoringResultList = new List<ScoringResult>
			{
				TargetScorerController.Score(new DeduplicationOrgHeader(master), new DeduplicationOrgHeader(target), isStandardizingMaster: true),
				TargetScorerController.Score(new DeduplicationOrgHeader(target), new DeduplicationOrgHeader(master), isStandardizingMaster: true)
			};
			var dataSource = new DedupPopupBizoDataSource(master, scoringResultList, null);

			var control = new DedupPopupOrgHeaderControl();

			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				control.SetDataBinding(dataSource, string.Empty);
				form.Show();

				AssertEquals(typeof(DedupPopupBizoDataSource), control.BindingSource.DataSource.GetType());
				var currentBindingDataSource = control.CurrentDataItem as DedupPopupBizoDataSource;
				AssertNotNull(currentBindingDataSource);
				AssertEquals(2, currentBindingDataSource.DeduplicationResults?.Count);
				AssertEquals(2, control.tableLayoutPanel.Controls.OfType<DedupPopupOrgHeaderItemControl>().Count());
			}
		}

		public override Form GetFormToBash() => CreateFormToTest();

		Form CreateFormToTest()
		{
			var form = new ZEmptyFormForBasherTest() { CaptionRenderingEnabled = true };
			var control = new DedupPopupOrgHeaderControl();
			control.Dock = DockStyle.Fill;
			form.Size = ControlDpiScalingHelper.NewScaledSize(300, 300);
			form.Controls.Add(control);
			form.ControllerID = ControllerIDs.Organisation;
			return form;
		}
	}
}
