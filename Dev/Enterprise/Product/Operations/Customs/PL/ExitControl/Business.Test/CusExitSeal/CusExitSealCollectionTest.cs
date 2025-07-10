using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitSealCollection))]
sealed class CusExitSealCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var container = CusExitContainerTest.GetNewBusinessObject(Factory);
		return container.AllSealNumbers;
	}
}
