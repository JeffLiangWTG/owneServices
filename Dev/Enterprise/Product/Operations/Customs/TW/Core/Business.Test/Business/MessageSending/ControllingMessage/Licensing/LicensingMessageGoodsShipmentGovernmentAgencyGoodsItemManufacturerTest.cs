using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer))]
	sealed class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			var manufacturerDocAddress = ((JobComInvoiceLine)Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew()).ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.FRICode = "52889317";
			IPartyDetails goodsShipmentGovernmentAgencyGoodsItemManufacturer = new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer(manufacturerDocAddress);
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemManufacturer.ID, NUnit.Framework.Is.EqualTo("52889317").Using(CustomComparers.TypeComparison));
		}
	}
}
