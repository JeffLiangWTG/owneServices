using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eManifest.Business.Testing
{
	[TestedType(typeof(SupplierBookingLineDependentCollection))]
	class SupplierBookingLineDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SupplierBookingLineDependentCollection(Factory.New<SupplierBookingHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<SupplierBookingLine>();
		}
	}
}
