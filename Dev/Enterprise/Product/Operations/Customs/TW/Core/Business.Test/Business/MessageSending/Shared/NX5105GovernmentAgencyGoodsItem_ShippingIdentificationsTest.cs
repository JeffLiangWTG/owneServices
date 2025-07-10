using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GovernmentAgencyGoodsItem_ShippingIdentificationsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShippingIdentifications()
		{
			var data = new ShippingIdentificationWrapper("1234", new ZDateTime(2020, 05, 01), 20m, new ZDateTime(2020, 04, 01));
			NUnit.Framework.Assert.That(data.LotNumberID, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 05, 01)));
			NUnit.Framework.Assert.That(data.ProductLotNumberAmount, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 04, 01)));
		}
	}
}
