using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Module.Testing;

[TestedType(typeof(AuthorisationRuleModule))]
sealed class AuthorisationRuleModuleTest : ZModuleBasherTest
{
	public void TestGetNewController_ReturnType()
	{
		using (var module = new AuthorisationRuleModule())
		{
			AssertType<AuthorisationRuleController>("Controller must be of type AuthorisationRuleController", module.GetNewController());
		}
	}

	public void TestGetNewFilterControl_ReturnType()
	{
		using (var module = new AuthorisationRuleModule())
		{
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<AuthorisationRuleControl>(filterControl);
			}
		}
	}

	public void TestGetFilterStripBusinessObject_ReturnType()
	{
		using (var module = new AuthorisationRuleModule())
		{
			var filterStipBizO = module.FilterBusinessObject;
			AssertType<AuthorisationRuleFilterBusinessObject>(filterStipBizO);
		}
	}

	public void TestGridCollection_ReturnType()
	{
		using (var module = new AuthorisationRuleModule())
		{
			AssertEquals(typeof(CusAuthorisationRuleCollection), module.GridCollection.GetType());
		}
	}

	public void TestShowRecentItems()
	{
		using (var module = new AuthorisationRuleModule())
		{
			AssertEquals("module.ShowRecentItems", false, module.ShowRecentItems);
		}
	}

	public void TestHasActions()
	{
		using (var module = new AuthorisationRuleModule())
		{
			AssertEquals(false, module.HasActions);
		}
	}

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.PL.AuthorisationRule;
}
