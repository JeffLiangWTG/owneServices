using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CusAuthorisationRuleCollection))]
sealed class CusAuthorisationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAuthorisationRuleCollection>
{
	protected override CusAuthorisationRuleCollection GetCollectionToTest() => new CusAuthorisationRuleCollection(Factory);

	public void TestModuleIdAttribute()
	{
		var moduleIDAttribute = typeof(CusAuthorisationRuleCollection).GetCustomAttribute<ModuleIDAttribute>();
		AssertEquals(ModuleId.AuthorisationRule, moduleIDAttribute.ModuleId);
	}
}
