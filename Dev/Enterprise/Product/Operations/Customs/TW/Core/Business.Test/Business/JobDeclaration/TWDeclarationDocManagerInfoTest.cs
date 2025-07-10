using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing;

[TestedType(typeof(TWDeclarationDocManagerInfo))]
sealed class TWDeclarationDocManagerInfoTest : MasterFiles.Business.Testing.DocManagerInfoTestCase
{
	public override BusinessObject GetEmptyParentBusinessObject()
	{
		return Factory.New<JobDeclaration>();
	}

	public override BusinessObject GetPopulatedParentBusinessObject()
	{
		return Factory.NewWithValidTestData<JobDeclaration>();
	}
}
