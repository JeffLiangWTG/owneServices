using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFDocAddressDependentCollection))]
	sealed class ISFDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new ISFDocAddressDependentCollection(Factory.New<CusISFHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ISFDocAddress>();
	}
}
