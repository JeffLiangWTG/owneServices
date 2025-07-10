using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class GlbAccreditationGroupingModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestDetachButton_Click()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "BBB";
			accreditation.HAC_CertificateCode = "BBC";
			var accreditationRefresher = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditationRefresher.HAC_Code = "RBB";
			accreditationRefresher.HAC_CertificateCode = "BBC";
			accreditationRefresher.HAC_IsRefresher = true;
			accreditationRefresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			group.Accreditations.Add(accreditation);
			Factory.Save();
			AssertEquals("Precondition", true, accreditation.IsInDatabase);
			AssertEquals("Precondition", true, accreditationRefresher.IsInDatabase);
			AssertEquals("Precondition", accreditationRefresher, accreditation.RefresherAccreditation);
			AssertEquals("Precondition: Group's Accreditations should consist of both accreditation and accreditationRefresher", 2, group.Accreditations.Count);
			using (var form = new ZForm(group))
			using (var groupingModuleButtonGrid = new GlbAccreditationGroupingModuleButtonGrid())
			{
				form.Controls.Add(groupingModuleButtonGrid);
				groupingModuleButtonGrid.BindToFindBoxList = "Lookups.MainAccreditationList";
				groupingModuleButtonGrid.ModuleID = ModuleIDs.GlbAccreditation;
				groupingModuleButtonGrid.InnerGrid.DataSource = group.Accreditations;
				var dummyColumn = new ZTextBoxColumnStyleInfo { CaptionResourceString = new NoResourceStringData("Code"), ColumnName = "HAC_Code" };
				groupingModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				groupingModuleButtonGrid.InnerGrid.SetDataBinding(group, "Accreditations");
				form.Show();
				groupingModuleButtonGrid.InnerGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessages();
				groupingModuleButtonGrid.DetachButtonForTest.PerformClick();
				AssertEquals("Error message should have shown when trying to detach all records (including refresher)", "Refresher Accreditations should not be selected for detach. Detach the Refresher's Main Accreditation and the Refresher will automatically be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
				groupingModuleButtonGrid.InnerGrid.SelectSingleElement(accreditationRefresher);
				UnitTestUserNotification.Instance.ClearMessages();
				groupingModuleButtonGrid.DetachButtonForTest.PerformClick();
				AssertEquals("Error message should have shown when trying to detach refresher", "Refresher Accreditations should not be selected for detach. Detach the Refresher's Main Accreditation and the Refresher will automatically be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
				groupingModuleButtonGrid.InnerGrid.SelectSingleElement(accreditation);
				UnitTestUserNotification.Instance.ClearMessages();
				groupingModuleButtonGrid.DetachButtonForTest.PerformClick();
				AssertNotEquals("No error message should show when trying to detach main accreditation", "Refresher Accreditations should not be selected for detach. Detach the Refresher's Main Accreditation and the Refresher will automatically be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
