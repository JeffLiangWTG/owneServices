using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(StatementAndACHRerouteForm))]
	sealed class StatementAndACHRerouteFormTest : ZFormBasherTest
	{
		public void TestControlsVisibility()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = US.Business.MessageBuilders.StatementTypeList.Codes.Daily;
			using (var form = new StatementAndACHRerouteForm(reroute))
			{
				form.Show();
				AssertEquals("ProcessingPortFindBox is visible when message type is 'ACS' and statement type is 'D'", true, form.ProcessingPortFindBox.Visible);
				AssertEquals("PreliminaryStatementCheckBox is visible when message type is 'ACS' and statement type is 'D'", true, form.PreliminaryStatementCheckBox.Visible);
				AssertEquals("FinalStatementCheckBox is visible when message type is 'ACS' and statement type is 'D'", true, form.FinalStatementCheckBox.Visible);
				AssertEquals("ACHPaymentCheckBox is visible when message type is 'ACS' and statement type is 'D'", true, form.ACHPaymentCheckBox.Visible);
				AssertEquals("PeriodicStatementPaymentAuthorizationCheckBox is visible when message type is 'ACS' and statement type is 'D'", true, form.PeriodicStatementPaymentAuthorizationCheckBox.Visible);
				reroute.Z9_RerouteType = US.Business.MessageBuilders.StatementTypeList.Codes.PeriodicMonthly;
				AssertEquals("ProcessingPortFindBox is visible when message type is 'ACS' and statement type is 'P'", true, form.ProcessingPortFindBox.Visible);
				AssertEquals("PreliminaryStatementCheckBox is visible when message type is 'ACS' and statement type is 'P'", true, form.PreliminaryStatementCheckBox.Visible);
				AssertEquals("FinalStatementCheckBox is visible when message type is 'ACS' and statement type is 'P'", true, form.FinalStatementCheckBox.Visible);
				AssertEquals("ACHPaymentCheckBox is NOT visible when message type is 'ACS' and statement type is 'P'", false, form.ACHPaymentCheckBox.Visible);
				AssertEquals("PeriodicStatementPaymentAuthorizationCheckBox is NOT visible when message type is 'ACS' and statement type is 'P'", false, form.PeriodicStatementPaymentAuthorizationCheckBox.Visible);
				reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
				reroute.Z9_RerouteType = US.Business.MessageBuilders.StatementTypeList.Codes.Daily;
				AssertEquals("ProcessingPortFindBox is visible when message type is 'ACE' and statement type is 'D'", true, form.ProcessingPortFindBox.Visible);
				AssertEquals("PreliminaryStatementCheckBox is visible when message type is 'ACE' and statement type is 'D'", true, form.PreliminaryStatementCheckBox.Visible);
				AssertEquals("FinalStatementCheckBox is visible when message type is 'ACE' and statement type is 'D'", true, form.FinalStatementCheckBox.Visible);
				AssertEquals("ACHPaymentCheckBox is NOT visible when message type is 'ACE' and statement type is 'D'", false, form.ACHPaymentCheckBox.Visible);
				AssertEquals("PeriodicStatementPaymentAuthorizationCheckBox is NOT visible when message type is 'ACE' and statement type is 'D'", false, form.PeriodicStatementPaymentAuthorizationCheckBox.Visible);
				reroute.Z9_RerouteType = US.Business.MessageBuilders.StatementTypeList.Codes.PeriodicMonthly;
				AssertEquals("ProcessingPortFindBox is visible when message type is 'ACE' and statement type is 'P'", true, form.ProcessingPortFindBox.Visible);
				AssertEquals("PreliminaryStatementCheckBox is visible when message type is 'ACE' and statement type is 'P'", true, form.PreliminaryStatementCheckBox.Visible);
				AssertEquals("FinalStatementCheckBox is visible when message type is 'ACE' and statement type is 'P'", true, form.FinalStatementCheckBox.Visible);
				AssertEquals("ACHPaymentCheckBox is NOT visible when message type is 'ACE' and statement type is 'P'", false, form.ACHPaymentCheckBox.Visible);
				AssertEquals("PeriodicStatementPaymentAuthorizationCheckBox is NOT visible when message type is 'ACE' and statement type is 'P'", false, form.PeriodicStatementPaymentAuthorizationCheckBox.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new StatementAndACHRerouteForm(new StatementAndACHPaymentReroute());
	}
}
