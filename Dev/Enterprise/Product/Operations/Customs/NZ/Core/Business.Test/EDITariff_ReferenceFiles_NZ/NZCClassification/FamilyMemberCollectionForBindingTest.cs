using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(FamilyMemberCollectionForBinding))]
	public class FamilyMemberCollectionForBindingTest : NonPersistentBusinessObjectCollectionTestCase<FamilyMemberCollectionForBinding>
	{
		protected override FamilyMemberCollectionForBinding GetCollectionToTest()
		{
			return new FamilyMemberCollectionForBinding();
		}
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FamilyMemberDummy();
		}
	}

	[TestedType(typeof(FamilyMemberDummy))]
	public class IFamilyMemberDummyTest : NonPersistentBusinessObjectTestCase
	{
	}
}
