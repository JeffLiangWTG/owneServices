using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DuplicateAlertControlTest : TestCaseWithFactory
	{
		public void TestCloseButton_Click()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = dummy.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgHeader)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZForm())
			{
				using (var control = new DuplicateAlertControl(args))
				{
					control.DisplayDetailsAndFixes += (x, y) => control.Close();

					form.Controls.Add(control);
					form.Show();
					control.DisplayDetailsAndFixesButton.PerformClick();

					AssertEquals(0, form.Controls.Find(control.Name, true).Length);
				}
			}
		}

		public void TestSizeWhenMasterIsGlbPerson()
		{
			var dummy = Factory.New<GlbPerson>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = dummy.PK.ToGuid(),
				MasterType = typeof(GlbPerson),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(GlbPerson)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZForm())
			using (var control = new DuplicateAlertControl(args))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Control width for GlbPerson", 302, control.Width);

				AssertEquals($"Control height for GlbPerson", 214, control.Height);
				control.Close();
			}
		}

		public void TestSizeWhenMasterIsOrgHeader()
		{
			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = dummy.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgHeader)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZForm())
			using (var control = new DuplicateAlertControl(args))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Control width for OrgHeader", 278, control.Width);

				AssertEquals($"Control height for OrgHeader", 206, control.Height);
				control.Close();
			}
		}
	}
}
