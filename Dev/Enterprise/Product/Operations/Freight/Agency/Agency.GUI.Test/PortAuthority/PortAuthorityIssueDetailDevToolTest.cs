using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class PortAuthorityIssueDetailDevToolTest : BaseAgencyTest
	{
		public void TestIssueDetails()
		{
			SetPortAuthoritySettings("AUBNE");
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = OverseasPort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			PortAuthority filter = new PortAuthority(voyage);
			PortAuthorityIssueDetailDevTool tool = new PortAuthorityIssueDetailDevTool();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				Application.DoEvents();
				tool.Show(dialog);
				AssertEquals("Question No issue selected", UnitTestUserNotification.Instance.LastMessage.ToString());
				PortMessageIssue issue1 = filter.Issues.AddNew(voyage.PK, "JV", "Issue1", "Detail 1");
				PortMessageIssue issue2 = filter.Issues.AddNew(voyage.PK, "JV", "Issue2", "Detail 2");
				PortMessageIssue issue3 = filter.Issues.AddNew(voyage.PK, "JV", "Issue3", "Detail 3");
				SelectIssue(dialog, issue2);
				tool.Show(dialog);
				AssertEquals("Question Detail 2", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation
		void SelectIssue(PortAuthorityFilterDialog dialog, PortMessageIssue issue)
		{
			object control = typeof(PortAuthorityFilterDialog).InvokeMember("filterControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, dialog, null);
			ZGrid grid = (ZGrid)typeof(PortAuthorityFilterControl).InvokeMember("issuesGrid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, control, null);
			CurrencyManager manager = grid.ListManager;
			manager.Position = manager.List.IndexOf(issue);
		}
		#endregion
	}
}
