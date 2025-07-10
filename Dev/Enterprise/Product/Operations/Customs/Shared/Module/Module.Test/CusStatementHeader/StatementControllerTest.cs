using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(StatementController))]
	public class StatementControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.CustomsStatement;
		}

		public void TestCheckStatementBelongsToThisCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "HXU";
			var controllerID = GetControllerID();
			var controller = ZControllerFactory.Create(controllerID);
			var statementHeader = Factory.New<BaseCusStatementHeader>();
			statementHeader.B2_GC = company.PK;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			controller.ShowViewForm(statementHeader);
			AssertEquals("You are trying to view a statement that belongs to a different company. Please log into the company 'HXU' and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			controller.ShowEditForm(statementHeader);
			AssertEquals("You are trying to view a statement that belongs to a different company. Please log into the company 'HXU' and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}
}
