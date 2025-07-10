using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(DeclarationLevelPackingGroupCollection))]
sealed class DeclarationLevelPackingGroupCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new DeclarationLevelPackingGroupCollection(Factory.New<JobDeclaration>());
	}
}
