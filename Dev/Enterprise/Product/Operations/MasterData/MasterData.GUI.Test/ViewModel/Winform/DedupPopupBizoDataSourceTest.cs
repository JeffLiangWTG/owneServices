using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DedupPopupBizoDataSourceTest : TestCaseWithFactory
	{
		protected override void TearDown()
		{
			ZFormModaliser.LastFormShownForTest?.Dispose();
			ZFormModaliser.ActiveForm?.Dispose();
			base.TearDown();
		}

		public void TestIsSelectedItemInDB()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var result = DedupPopupBizoDataSource.IsSelectedItemInDB(typeof(OrgHeader), orgHeader.PK);
			Assert(result);

			orgHeader.Delete();
			Factory.Save();
			result = DedupPopupBizoDataSource.IsSelectedItemInDB(typeof(OrgHeader), orgHeader.PK);
			Assert(!result);
		}

		public void TestOpenDetailsFormActionNotInDB()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var dataSource = new DedupPopupBizoDataSource(orgHeader, new List<ScoringResult>(), null);
			dataSource.SelectedResult = new DeduplicationResultsListDataSource();
			dataSource.OpenDetailsFormAction();
			AssertEquals("Message should be shown", "The duplicate record was removed from the system.\r\nPlease refresh the potential duplicates by pressing CTRL + G", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOpenDetailsFormActionInDB()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testTarget = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(testHeader),
				new DeduplicationOrgHeader(testTarget),
				true);
			scoringResults.Add(score);

			var isDetailsFormDisplayed = false;
			var dataSource = new DedupPopupBizoDataSource(testHeader, scoringResults, (ZGuid guid) => { isDetailsFormDisplayed = true; });
			dataSource.SelectedResult = new DeduplicationResultsListDataSource()
			{
				PK = testTarget.PK
			};

			dataSource.OpenDetailsFormAction();
			Assert(isDetailsFormDisplayed);
		}

		public void TestDataSource()
		{
			AssertOrgHeaderPropertiesAreSet();
			AssertPersonContactPropertiesAreSet();
		}

		public void TestNotThrowExceptionWhenFindNoneDupInDB()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org1 = list[0];
			var org2 = list[1];
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
					new DeduplicationOrgHeader(org1),
					new DeduplicationOrgHeader(org2),
					true);

			scoringResults.Add(score);

			var dataSource = new DedupPopupBizoDataSource(org1, scoringResults, null);
			AssertEquals(1, dataSource.DeduplicationResults.Count);

			org1.Delete();
			org2.Delete();
			Factory.Save();

			dataSource.SelectedResult = new DeduplicationResultsListDataSource() { PK = org1.PK };
			dataSource.OpenDetailsFormAction();
			AssertEquals("Message should be shown", "The duplicate record was removed from the system.\r\nPlease refresh the potential duplicates by pressing CTRL + G", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertNoExceptionThrown("Should not throw null reference exception.", () =>
			{
				AssertEquals(0, dataSource.DeduplicationResults.Count);
			});
		}

		public void TestDeduplicationResults_SetsShouldCreatePopupToFalse_OnNoResults()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var master = list[0];

			var dataSource = new DedupPopupBizoDataSource(master, new List<ScoringResult>(), null);

			var dupList = dataSource.DeduplicationResults.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(false, dupList.Any());
				AssertEquals(false, dataSource.ShouldCreatePopup);
			});
		}

		public void TestDeduplicationResults_Person()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "Michael Kheirabi";

			var scoringResults = new List<ScoringResult>();

			var result = TargetScorerController.Score(
				masterPerson.CreateIGlbPerson(),
				targetPerson.CreateIGlbPerson(),
				true);

			scoringResults.Add(result);

			var dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			var listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertPersonModel(string.Empty, null, false);

			targetPerson.PER_HomePhone_Formatted = "+61 426 829 924";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertPersonModel("+61 426 829 924", null, false);

			targetPerson.PER_EmailAddress2 = "Michael Kheirabi@qq.com";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertPersonModel("Michael Kheirabi@qq.com", "+61 426 829 924", true);

			void AssertPersonModel(string expectedMainInfo, string expectedPhone, bool expectedPhonePanelVisible)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expectedMainInfo, listModel.MainInfo);
					AssertEquals(expectedPhone, listModel.Phone);
					AssertEquals(expectedPhonePanelVisible, listModel.PhonePanelVisible);

					AssertEquals("Person", listModel.PersonType);
					AssertEquals("Michael Kheirabi", listModel.FullName);

					var padding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 28, 0, isInStandardDpi: true);
					AssertEquals(padding.Left, listModel.PersonTypePadding.Left);
					AssertEquals(padding.Top, listModel.PersonTypePadding.Top);
					AssertEquals(padding.Right, listModel.PersonTypePadding.Right);
					AssertEquals(padding.Bottom, listModel.PersonTypePadding.Bottom);

					AssertEquals(null, listModel.Email);
					AssertEquals(false, listModel.EmailPanelVisible);

					AssertEquals(string.Empty, listModel.Associations);
					AssertEquals(false, listModel.AssociationsPanelVisible);
				});
			}
		}

		public void TestDeduplicationResults_Applicant()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "Michael Kheirabi";
			var applicant = targetPerson.ApplicantCollection.AddNew();
			var jobApplicant = (IHRJobApplicant)applicant;
			Factory.Save();

			var scoringResults = new List<ScoringResult>();

			var result = TargetScorerController.Score(
				masterPerson.CreateIGlbPerson(),
				targetPerson.CreateIGlbPerson(),
				true);

			scoringResults.Add(result);

			var dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			var listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertApplicantModel(string.Empty, null, false);

			Db.Connection.ExecuteNonQuery($"update dbo.HRJobApplicant set HA_WorkPhone = '+61 426 829 924' where HA_PK ='{jobApplicant.PK}'");
			applicant.Reload();
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertApplicantModel("+61 426 829 924", null, false);

			Db.Connection.ExecuteNonQuery($"update dbo.HRJobApplicant set HA_EmailAddress = 'Michael Kheirabi@qq.com' where HA_PK ='{jobApplicant.PK}'");
			applicant.Reload();
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertApplicantModel("Michael Kheirabi@qq.com", "+61 426 829 924", true);

			void AssertApplicantModel(string expectedMainInfo, string expectedPhone, bool expectedPhonePanelVisible)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expectedMainInfo, listModel.MainInfo);
					AssertEquals(expectedPhone, listModel.Phone);
					AssertEquals(expectedPhonePanelVisible, listModel.PhonePanelVisible);

					AssertEquals("Applicant", listModel.PersonType);
					AssertEquals("Michael Kheirabi", listModel.FullName);

					var padding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 15, 0, isInStandardDpi: true);
					AssertEquals(padding.Left, listModel.PersonTypePadding.Left);
					AssertEquals(padding.Top, listModel.PersonTypePadding.Top);
					AssertEquals(padding.Right, listModel.PersonTypePadding.Right);
					AssertEquals(padding.Bottom, listModel.PersonTypePadding.Bottom);

					AssertEquals(null, listModel.Email);
					AssertEquals(false, listModel.EmailPanelVisible);

					AssertEquals(string.Empty, listModel.Associations);
					AssertEquals(false, listModel.AssociationsPanelVisible);
				});
			}
		}

		public void TestDeduplicationResults_Contact()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "Michael Kheirabi";
			var contact = targetPerson.ContactCollection.AddNew();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = string.Empty;
			contact.OC_OH = header.PK;
			targetPerson.SetPrimaryRelationship(contact);

			var scoringResults = new List<ScoringResult>();

			var result = TargetScorerController.Score(
				masterPerson.CreateIGlbPerson(),
				targetPerson.CreateIGlbPerson(),
				true);

			scoringResults.Add(result);

			var dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			var listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi", string.Empty, null, false, string.Empty, false);

			contact.OC_Title = "Development";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", string.Empty, null, false, string.Empty, false);

			contact.OC_Email = "Michael Kheirabi@google.com";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", "Michael Kheirabi@google.com", null, false, string.Empty, false);

			header.OH_FullName = "Wise Tech";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", "Wise Tech", "Michael Kheirabi@google.com", true, string.Empty, false);

			var staff = targetPerson.StaffCollection.AddNew();
			staff.GS_PER = targetPerson.PK;
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", "Wise Tech", "Michael Kheirabi@google.com", true, "Contacts (1) Staffs (1)", true);

			staff = targetPerson.StaffCollection.AddNew();
			staff.GS_PER = targetPerson.PK;
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", "Wise Tech", "Michael Kheirabi@google.com", true, "Contacts (1) Staffs (2)", true);

			var applicant = targetPerson.ApplicantCollection.AddNew();
			(applicant as IHRJobApplicant).HA_PER = targetPerson.PK;
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertContactModel("Michael Kheirabi - Development", "Wise Tech", "Michael Kheirabi@google.com", true, "Contacts (1) Staffs (2) Applicants (1)", true);

			void AssertContactModel(string expectedFullName, string expectedMainInfo, string expectedEmail, bool expectedEmailPanelVisible, string expectedAssociations, bool expectedAssociationsPanelVisible)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expectedFullName, listModel.FullName);

					AssertEquals(expectedMainInfo, listModel.MainInfo);

					AssertEquals(expectedEmail, listModel.Email);
					AssertEquals(expectedEmailPanelVisible, listModel.EmailPanelVisible);

					AssertEquals(expectedAssociations, listModel.Associations);
					AssertEquals(expectedAssociationsPanelVisible, listModel.AssociationsPanelVisible);

					AssertEquals("Contact", listModel.PersonType);
					AssertEquals("Associations: ", listModel.InfoCaption);

					var padding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 23, 0, isInStandardDpi: true);
					AssertEquals(padding.Left, listModel.PersonTypePadding.Left);
					AssertEquals(padding.Top, listModel.PersonTypePadding.Top);
					AssertEquals(padding.Right, listModel.PersonTypePadding.Right);
					AssertEquals(padding.Bottom, listModel.PersonTypePadding.Bottom);

					AssertEquals(null, listModel.Phone);
					AssertEquals(false, listModel.PhonePanelVisible);
				});
			}
		}

		public void TestDeduplicationResults_Staff()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var targetPerson = Factory.NewWithValidTestData<GlbPerson>();
			targetPerson.PER_FullName = "Michael Kheirabi";
			var staff = targetPerson.StaffCollection.AddNew();
			staff.GS_PER = targetPerson.PK;
			targetPerson.SetPrimaryRelationship(staff);

			var scoringResults = new List<ScoringResult>();

			var result = TargetScorerController.Score(
				masterPerson.CreateIGlbPerson(),
				targetPerson.CreateIGlbPerson(),
				true);

			scoringResults.Add(result);

			var dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			var listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi", string.Empty, null, false, string.Empty, false);

			staff.GS_Title = "Development";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi - Development", string.Empty, null, false, string.Empty, false);

			var emailAddress = staff.EmailAddresses.AddNew();
			emailAddress.GSE_EmailAddress = "Michael Kheirabi@google.com";
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi - Development", "Michael Kheirabi@google.com", null, false, string.Empty, false);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.Company.CompanyName = "My company";
			staff.GS_GB_HomeBranch = branch.PK;
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi - Development", "My company", "Michael Kheirabi@google.com", true, string.Empty, false);

			targetPerson.ContactCollection.AddNew();
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi - Development", "My company", "Michael Kheirabi@google.com", true, "Contacts (1) Staffs (1)", true);

			targetPerson.ContactCollection.AddNew();
			dataSource = new DedupPopupBizoDataSource(masterPerson, scoringResults, null);
			listModel = dataSource.DeduplicationResults.FirstOrDefault();
			AssertStaffModel("Michael Kheirabi - Development", "My company", "Michael Kheirabi@google.com", true, "Contacts (2) Staffs (1)", true);

			void AssertStaffModel(string expectedFullName, string expectedMainInfo, string expectedEmail, bool expectedEmailPanelVisible, string expectedAssociations, bool expectedAssociationsPanelVisible)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expectedFullName, listModel.FullName);

					AssertEquals(expectedMainInfo, listModel.MainInfo);

					AssertEquals(expectedEmail, listModel.Email);
					AssertEquals(expectedEmailPanelVisible, listModel.EmailPanelVisible);

					AssertEquals(expectedAssociations, listModel.Associations);
					AssertEquals(expectedAssociationsPanelVisible, listModel.AssociationsPanelVisible);

					AssertEquals("Staff", listModel.PersonType);
					AssertEquals("Associations: ", listModel.InfoCaption);

					var padding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 39, 0, isInStandardDpi: true);

					AssertEquals(padding.Left, listModel.PersonTypePadding.Left);
					AssertEquals(padding.Top, listModel.PersonTypePadding.Top);
					AssertEquals(padding.Right, listModel.PersonTypePadding.Right);
					AssertEquals(padding.Bottom, listModel.PersonTypePadding.Bottom);

					AssertEquals(null, listModel.Phone);
					AssertEquals(false, listModel.PhonePanelVisible);
				});
			}
		}

		void AssertPersonContactPropertiesAreSet()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var per1 = list[0];
			var per2 = list[1];
			var per3 = list[3];
			var per4 = list[4];
			var per5 = list[5];
			var org3 = per3.ContactCollection[0].Header;
			var org5 = per5.ContactCollection[0].Header;
			org3.OH_Code = "tOrg3";
			org5.OH_Code = "tOrg5";

			per3.PER_IsActive = false;

			var scoringResults = new List<ScoringResult>();
			var openDetailsMethodCalled = false;
			var selectedPK = ZGuid.Empty;

			var score1 = TargetScorerController.Score(
				per1.CreateIGlbPerson(),
				per5.CreateIGlbPerson(),
				true
			);
			var score2 = TargetScorerController.Score(
				per1.CreateIGlbPerson(),
				per2.CreateIGlbPerson(),
				true
			);

			var score3 = TargetScorerController.Score(
				per1.CreateIGlbPerson(),
				per4.CreateIGlbPerson(),
				true
			);

			scoringResults.Add(score2);
			scoringResults.Add(score1);
			scoringResults.Add(score3);

			Factory.Save();

			var dataSource = new DedupPopupBizoDataSource(per3, scoringResults, pk =>
			{
				openDetailsMethodCalled = true;
				selectedPK = pk;
			});

			var dupList = dataSource.DeduplicationResults.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(3, dupList.Length);
				AssertEquals("Associations: ", dupList[0].InfoCaption);
				AssertEquals(true, dupList[0].IsActive);
				AssertEquals("100%", dupList[0].ToolTip);
				AssertEquals(ConfidenceRating.Exact, dupList[0].Confidence);
				AssertEquals("Associations: ", dupList[0].InfoCaption);
				AssertEquals(ConfidenceRating.High, dupList[1].Confidence);
				AssertEquals("Associations: ", dupList[1].InfoCaption);
				AssertEquals("84%", dupList[2].ToolTip);
				AssertEquals(ConfidenceRating.High, dupList[2].Confidence);
			});

			dataSource.OpenDetailsFormAction();
			AssertEquals(false, openDetailsMethodCalled);

			dataSource.SelectedResult = dupList[0];
			ZFormModaliser.SetApplicationActiveForm(new Form());
			dataSource.OpenDetailsFormAction();
			CombineAssertions(() =>
			{
				AssertEquals(true, openDetailsMethodCalled);
				AssertEquals(selectedPK, dupList[0].PK);
			});
		}

		void AssertOrgHeaderPropertiesAreSet()
		{
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(Factory);
			var org1 = list[0];
			var org2 = list[1];
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;
			org3.OH_Code = "TESTO1";
			org4.OH_Code = "TEST02";
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var openDetailsMethodCalled = false;
			var selectedPK = ZGuid.Empty;

			var score1 = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org3),
				true
			);
			var score2 = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org2),
				true
			);

			var score3 = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org4),
				true
			);

			scoringResults.Add(score2);
			scoringResults.Add(score1);
			scoringResults.Add(score3);

			var dataSource = new DedupPopupBizoDataSource(org1, scoringResults, pk =>
			{
				openDetailsMethodCalled = true;
				selectedPK = pk;
			});

			var dupList = dataSource.DeduplicationResults.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(3, dupList.Length);
				AssertEquals("Code: ", dupList[0].InfoCaption);
				AssertEquals(true, dupList[0].IsActive);
				AssertEquals("94%", dupList[0].ToolTip);
				AssertEquals(ConfidenceRating.High, dupList[0].Confidence);
				AssertEquals("Code: ", dupList[0].InfoCaption);
				AssertEquals(ConfidenceRating.Low, dupList[1].Confidence);
				AssertEquals("Code: ", dupList[1].InfoCaption);
				AssertEquals(false, dupList[2].IsActive);
				AssertEquals("44%, this item is inactive", dupList[2].ToolTip);
				AssertEquals(ConfidenceRating.Low, dupList[2].Confidence);
			});

			dataSource.OpenDetailsFormAction();
			AssertEquals(false, openDetailsMethodCalled);

			dataSource.SelectedResult = dupList[0];
			ZFormModaliser.SetApplicationActiveForm(new Form());
			dataSource.OpenDetailsFormAction();
			CombineAssertions(() =>
			{
				AssertEquals(true, openDetailsMethodCalled);
				AssertEquals(selectedPK, dupList[0].PK);
			});
		}
	}
}
