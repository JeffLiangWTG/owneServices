using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(DedupePopupPersonControl))]
	public class DedupePopupPersonControlTest : BasherTest
	{
		public void TestDedupePopupPersonControl()
		{
			var master = Factory.NewWithValidTestData<GlbPerson>();
			var target = Factory.NewWithValidTestData<GlbPerson>();

			var scoringResultList = new List<ScoringResult>
			{
				TargetScorerController.Score(new DeduplicationGlbPerson(master), new DeduplicationGlbPerson(target), isStandardizingMaster: true),
				TargetScorerController.Score(new DeduplicationGlbPerson(target), new DeduplicationGlbPerson(master), isStandardizingMaster: true)
			};
			var dataSource = new DedupPopupBizoDataSource(master, scoringResultList, null);

			var control = new DedupePopupPersonControl();

			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				control.SetDataBinding(dataSource, string.Empty);
				form.Show();

				AssertEquals(typeof(DedupPopupBizoDataSource), control.BindingSource.DataSource.GetType());
				var currentBindingDataSource = control.CurrentDataItem as DedupPopupBizoDataSource;
				AssertNotNull(currentBindingDataSource);
				AssertEquals(2, control.Controls.Find("DedupePopupPersonItemControl", true).Length);
			}
		}

		public override Form GetFormToBash() => CreateFormToTest();

		Form CreateFormToTest()
		{
			var form = new ZEmptyFormForBasherTest() { CaptionRenderingEnabled = true };
			var control = new DedupePopupPersonControl();
			control.Dock = DockStyle.Fill;
			form.Size = ControlDpiScalingHelper.NewScaledSize(300, 300);
			form.Controls.Add(control);
			return form;
		}
	}
}
