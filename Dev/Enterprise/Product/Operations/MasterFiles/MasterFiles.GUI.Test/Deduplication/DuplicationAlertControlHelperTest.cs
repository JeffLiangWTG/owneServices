using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DuplicationAlertControlHelperTest : TestCaseWithFactory
	{
		public void TestOnlyShowDuplicationPopupIfScoringResultIsSufficient()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var minimumConfidents = new Dictionary<string, ConfidenceRating>()
			{
				["NON"] = ConfidenceRating.None,
				["LOW"] = ConfidenceRating.Low,
				["MED"] = ConfidenceRating.Medium,
				["UND"] = ConfidenceRating.Undefined
			};
			var scores = new[] { 0, .2, .5, .8, .99, 1 };

			foreach (var baseConfident in minimumConfidents)
			{
				using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseConfident.Key))
				{
					foreach (var confidence in Enum.GetValues(typeof(ConfidenceRating)).Cast<ConfidenceRating>())
					{
						if (confidence == ConfidenceRating.Undefined)
						{
							continue;
						}
						using (var form = new ZOrganisationsForm(orgHeader))
						using (var testControl = new NameAndAddressControlForTesting())
						{
							form.Controls.Add(testControl);
							form.Show();

							var master = Factory.New<OrgHeader>();
							var args = new DuplicationEventArgs(master, new object(), new[] { new ScoringResult { Score = scores[(int)confidence] } }, Enumerable.Empty<PatternMatchingResultModel>());
							var helper = new DuplicateAlertControlHelper();
							helper.ShowDuplicateAlert(testControl, args);

							var control = helper.ExistingAlertControl;
							if (confidence > baseConfident.Value)
							{
								AssertNotNull($"We should have a duplicate control for {confidence}", control);
							}
							else
							{
								AssertNull($"We should not have a duplicate control for {confidence}", control);
							}
						}
					}
				}
			}
		}

		public void TestAlertControlIsAddedToFrom()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				var master = Factory.New<OrgHeader>();
				var args = new DuplicationEventArgs(master, new object(), new[] { new ScoringResult { Score = 1 } }, Enumerable.Empty<PatternMatchingResultModel>());
				form.Controls.Add(testControl);
				form.Show();

				var helper = new DuplicateAlertControlHelper();
				helper.ShowDuplicateAlert(testControl, args);

				var control = helper.ExistingAlertControl;
				AssertNotNull("We should have a duplicate control in the form", control);
			}
		}

		public void TestAlertControlDoesntLoadWhenControlIsDisposed()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				var args = new DuplicationEventArgs(new object(), new object(), new[] { new ScoringResult { Score = 1 } }, Enumerable.Empty<PatternMatchingResultModel>());
				form.Controls.Add(testControl);
				form.Show();
				testControl.Dispose();

				var helper = new DuplicateAlertControlHelper();
				helper.ShowDuplicateAlert(testControl, args);

				var control = helper.ExistingAlertControl;
				AssertEquals(true, testControl.IsDisposed);
				AssertNull("We should NOT have a duplicate control in the form", control);
			}
		}

		public void TestAlertControlIsNotAddedToTheForm()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MED"))
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertNull("We should not have a duplicate control in the form", testControl.Controls.Find("DuplicateAlertControl", false).FirstOrDefault());
			}
		}

		public void TestViewMatches_WhenTargetIsDeleted_CorrectMessageDisplayedAndNoCrash()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var targetToDelete = Factory.NewWithValidTestData<GlbPerson>();
			var targetPerson = new DeduplicationGlbPerson(targetToDelete);
			var targetPersons = new List<DeduplicationGlbPerson>()
			{
				targetPerson
			};

			var scoringResult = new List<ScoringResult>
			{
				new ScoringResult
				{
					MasterPK = masterPerson.PK.ToGuid(),
					MasterType = typeof(IGlbPerson),
					Score = 1,
					TargetPK = targetPerson.PK,
					TargetType = typeof(IGlbPerson)
				}
			};

			var args = new DuplicationEventArgs(masterPerson, targetPersons, scoringResult, new List<PatternMatchingResultModel>());
			var helper = new DuplicateAlertControlHelper();

			using (var form = new ZForm())
			using (var testControl = new ContactsUserControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				helper.ShowDuplicateAlert(testControl, args);
				var duplicateAlertControl = helper.ExistingAlertControl;

				targetToDelete.Delete();
				Factory.Save();

				AssertNoExceptionThrown("No exception is thrown", duplicateAlertControl.DisplayDetailsAndFixesButton.PerformClick);
				AssertEquals($@"Unable to view matches.

The reason is:
At least one of following matches no longer exists.

You can rectify this by doing the following:
Save and reload the form. If the problem persists, please contact {BrandingFactory.Instance.ProductName} Support.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
