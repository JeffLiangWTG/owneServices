using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ShippingIdentificationDataValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			var parent = Factory.New<ShippingIdentificationData>();
			NUnit.Framework.Assert.That(parent.Validation.Parent, NUnit.Framework.Is.SameAs(parent));
		}
	}
}
