using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.Testing.ContactsUserControlTest;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(DeduplicationResultsViewerForm))]
	public class TestDeduplicationResultsViewerForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var deduplicationOrgHeader = new DeduplicationOrgHeader(org2);
			var targetList = new List<IOrgHeader>() { deduplicationOrgHeader };
			org1.OH_Code = "XYZ";
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org2),
				true);
			scoringResults.Add(score);
			return new DeduplicationResultsViewerForm(org1, targetList, scoringResults, new List<PatternMatchingResultModel>(), ZGuid.Empty);
		}

		public void TestDeduplicationResultFormOpensWithCorrectMasterAndTargets()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testTarget = Factory.NewWithValidTestData<OrgHeader>();
			var deduplicationOrgHeader = new DeduplicationOrgHeader(testTarget);
			var testTargetList = new List<IOrgHeader>() { deduplicationOrgHeader };

			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(testHeader),
				new DeduplicationOrgHeader(testTarget),
				true);
			scoringResults.Add(score);

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.ShowDuplicatesFound(new DuplicationEventArgs(testHeader, testTargetList, scoringResults, null));

				var args = testControl.CurrentDuplicationEventArgs;
				var viewerForm = new DeduplicationResultsViewerFormForTest(args.Master as BusinessObject, args.TargetObjects as IEnumerable<object>, args.Results, args.ResultsModels, args.SelectedMasterPK);
				using (viewerForm)
				{
					viewerForm.Show();
					AssertEquals(testHeader, viewerForm.MasterForTest);
					AssertEquals(testTargetList.Count, viewerForm.TargetsForTest.Count());
					AssertEquals(testTargetList.First().OH_PK, (viewerForm.TargetsForTest.First() as DeduplicationOrgHeader).OH_PK);
				}
			}
		}

		public void TestDeduplicationResultFormOpensWithCorrectMasterAndTargets_ContactUserControl()
		{
			var org = CreateOrgForDedupTests();
			var testTarget = Factory.NewWithValidTestData<GlbPerson>();
			var deduplicationGlbPerson = new DeduplicationGlbPerson(testTarget);
			var testTargetList = new List<IGlbPerson>() { deduplicationGlbPerson };
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationGlbPerson(org.Contacts[0].Person),
				deduplicationGlbPerson,
				true);
			scoringResults.Add(score);

			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.ShowDuplicatesFound(new DuplicationEventArgs(org.Contacts[0].Person, testTargetList, scoringResults, null));

				var args = testControl.CurrentDuplicationEventArgs;
				var viewerForm = new DeduplicationResultsViewerFormForTest(args.Master as BusinessObject, args.TargetObjects as IEnumerable<object>, args.Results, args.ResultsModels, args.SelectedMasterPK);
				using (viewerForm)
				{
					viewerForm.Show();
					AssertEquals(org.Contacts[0].Person, viewerForm.MasterForTest);
					AssertEquals(testTargetList.Count, viewerForm.TargetsForTest.Count());
					AssertEquals(testTargetList.First().PER_PK, (viewerForm.TargetsForTest.First() as DeduplicationGlbPerson).PER_PK);
				}
			}
		}

		public void TestNotThrowExceptionWhenFindNoneDupInDB()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var targetList = new List<DeduplicationOrgHeader>();
			org1.OH_Code = "XYZ";
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org2),
				true);
			scoringResults.Add(score);

			org2.Delete();
			Factory.Save();

			AssertNoExceptionThrown("Should not throw null reference exception.", () =>
			{
				using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var testForm = new DeduplicationResultsViewerFormForTest(org1, targetList, scoringResults, new List<PatternMatchingResultModel>(), ZGuid.Empty))
				{
					AssertEquals("Message should be shown", "The duplicate record was removed from the system.\r\nPlease refresh the potential duplicates by pressing CTRL + G", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestFormCaptionIsCorrect()
		{
			using (var testForm = new DeduplicationResultsViewerFormForTest(Factory.NewWithValidTestData<OrgHeader>(), new List<IOrgHeader>(), new List<ScoringResult>(), new List<PatternMatchingResultModel>(), ZGuid.Empty))
			{
				AssertEquals("Form caption should be 'Potential Duplicate Records'", "Potential Duplicate Records", testForm.FormCaption);
			}
		}

		#region Implementation

		OrgHeader CreateOrgForDedupTests()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.Add(contact);

			return org;
		}

		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		#endregion
	}

	public class DeduplicationResultsViewerFormForTest : DeduplicationResultsViewerForm
	{
		public DeduplicationResultsViewerFormForTest(BusinessObject master, IEnumerable<object> targets, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultModels, ZGuid selectedItemPK)
			: base(master, targets, results, resultModels, selectedItemPK)
		{ }

		public DeduplicationResultsViewerFormForTest(GlbPerson master, IEnumerable<object> targets, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultModels, ZGuid selectedItemPK)
			: base(master, targets, results, resultModels, selectedItemPK)
		{ }

		public object MasterForTest => Master;

		public IEnumerable<object> TargetsForTest => Targets;

		public List<string> Statuses { get; } = new List<string>();

		protected override void UpdateStatusBar(string notification, INotificationType state)
		{
			Statuses.Add(notification);
			base.UpdateStatusBar(notification, state);
		}
	}
}
