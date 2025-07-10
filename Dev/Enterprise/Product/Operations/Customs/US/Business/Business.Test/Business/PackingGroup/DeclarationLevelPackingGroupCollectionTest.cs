using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeclarationLevelPackingGroupCollection))]
	sealed class DeclarationLevelPackingGroupCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new DeclarationLevelPackingGroupCollection((JobDeclaration)base.Declaration);
	}
}
