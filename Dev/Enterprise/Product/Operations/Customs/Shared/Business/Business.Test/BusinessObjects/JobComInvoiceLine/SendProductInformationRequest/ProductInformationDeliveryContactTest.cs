using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProductInformationDeliveryContact))]
	sealed class ProductInformationDeliveryContactTest : NonPersistentBusinessObjectTestCase
	{
		ProductInformationDeliveryContact deliveryContact;

		ProductInformationDeliveryContact DeliveryContact => deliveryContact ??= new ProductInformationDeliveryContact(Factory, new ZArchitecture.Core.CodeDescriptionPairList());

		public void TestNotifyModes()
		{
			AssertEquals($"E-Mail", DeliveryContact.NotifyModes.CodesAsString);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProductInformationDeliveryContact(Factory, new ZArchitecture.Core.CodeDescriptionPairList());
		}
	}
}
