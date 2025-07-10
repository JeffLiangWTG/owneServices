using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Module.Testing;

[TestedType(typeof(AuthorisationRuleController))]
sealed class AuthorisationRuleControllerTest : ZControllerBasherTest
{
	public void TestMakeUrlsOnlyOpenableForCurrentCompany()
	{
		AssertEquals(true, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
	}

	public void TestGetForm()
	{
		var controller = ZControllerFactory.Create(ControllerIDs.Customs.PL.AuthorisationRule);
		var header = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		var rule = header.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = "LOC";
		rule.CPR_ValueFrom = "WQW";
		Factory.Save();

		using (var form = ((ZControllerInternals)controller).GetForm(rule))
		{
			AssertType<CusAuthorisationForm>(form);
		}
	}

	public override void TestNewForm()
	{
		AssertControllerNotNull();

		var authorizationsRuleControllerForTest = new AuthorisationRuleController();
		var businessObject = Factory.NewWithValidTestData(authorizationsRuleControllerForTest.TypeOfTopLevelBusinessObject);
		Factory.Save();

		using (var form = Controller.ShowFormForNewEntity(businessObject))
		{
			AssertNotNull(form);
		}
	}

	public void TestCheckPointForView()
	{
		AssertEquals(Env.Security.AuthorisationsView, Controller.CheckPointForViewExposedForTest);
	}

	public void TestCheckPointForEdit()
	{
		AssertEquals(Env.Security.AuthorisationsEdit, Controller.CheckPointForEditExposedForTest);
	}

	public void TestCheckPointForNew()
	{
		AssertEquals(Env.Security.AuthorisationsNew, Controller.CheckPointForNewExposedForTest);
	}

	public void TestCheckPointForDelete()
	{
		AssertEquals(Env.Security.AuthorisationsDelete, Controller.CheckPointForDeleteExposedForTest);
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.PL.AuthorisationRule;
}
