using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageConsignmentGovernmentAgencyGoodsItem))]
	sealed class LicensingMessageConsignmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.IDCode = "96944491";
			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new LicensingMessageConsignmentGovernmentAgencyGoodsItem(invoiceLine);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.TypeOf<LicensingMessageManufacturer>());
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("96944491").Using(CustomComparers.TypeComparison), "Manufacturer ID");

				governmentAgencyGoodsItem = new LicensingMessageConsignmentGovernmentAgencyGoodsItem(null);
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Manufacturer should be null when FirstInvoiceLine is null - should be [null]");
			});
		}
	}
}
