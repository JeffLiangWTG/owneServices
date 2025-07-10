
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(DeclarationLevelPackingGroupCollection))]
	public class DeclarationLevelPackingGroupCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackingGroupCollectionTest<JobDeclaration>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeclarationLevelPackingGroupCollection((JobDeclaration)Declaration);
		}
	}
}
