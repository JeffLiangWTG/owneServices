using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class SecurityDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			using (var testControl = new FormForTest())
			{
				Assert(!testControl.DateTimeOfScheduledArrivalDateEdit.Visible);
				Assert(!testControl.ChooseAdditionalSecurityInformationStatementDropEdit.Visible);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			using (var testControl = new FormForTest())
			{
				Assert(!testControl.DateTimeOfScheduledArrivalDateEdit.Visible);
				Assert("ChooseAdditionalSecurityInformationStatementDropEdit is only visible for AU", testControl.ChooseAdditionalSecurityInformationStatementDropEdit.Visible);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			using (var testControl = new FormForTest())
			{
				Assert("DateTimeOfScheduledArrivalDateEdit is only visible for UK", testControl.DateTimeOfScheduledArrivalDateEdit.Visible);
				Assert(!testControl.ChooseAdditionalSecurityInformationStatementDropEdit.Visible);
			}
		}

		class FormForTest : SecurityDeclarationUserControl
		{
			public ZDateEdit DateTimeOfScheduledArrivalDateEdit => this.Controls.Find("dateTimeOfScheduledArrivalDateEdit", true)[0] as ZDateEdit;

			public ZDropEdit ChooseAdditionalSecurityInformationStatementDropEdit => this.Controls.Find("chooseAdditionalSecurityInformationStatementDropEdit", true)[0] as ZDropEdit;
		}
	}
}
