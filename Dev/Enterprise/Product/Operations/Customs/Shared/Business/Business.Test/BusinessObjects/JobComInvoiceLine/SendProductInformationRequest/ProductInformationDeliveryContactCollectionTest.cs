using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProductInformationDeliveryContactCollection))]
	sealed class ProductInformationDeliveryContactCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProductInformationDeliveryContactCollection>
	{
		protected override ProductInformationDeliveryContactCollection GetCollectionToTest()
		{
			return new ProductInformationDeliveryContactCollection(Factory, new ZArchitecture.Core.CodeDescriptionPairList());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProductInformationDeliveryContact(Factory, new ZArchitecture.Core.CodeDescriptionPairList());
		}
	}
}
