using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class N5167GoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItems()
		{
			var governmentAgencyGoodsItems = goodsShipment.GovernmentAgencyGoodsItems;
			NUnit.Framework.Assert.That(governmentAgencyGoodsItems, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IGovernmentAgencyGoodsItem>)));
			NUnit.Framework.Assert.That(governmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentAgencyGoodsItems.Single(), NUnit.Framework.Is.TypeOf(typeof(GovernmentAgencyGoodsItem)));
		}

		[ExpectNoExceptions]
		public void TestExitDateTime()
		{
			NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestItemChargeAmount()
		{
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestTotalCIFAmount()
		{
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestConsignee()
		{
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestConsignment()
		{
			NUnit.Framework.Assert.That(goodsShipment.Consignment, NUnit.Framework.Is.EqualTo(default(IConsignment)));
		}

		[ExpectNoExceptions]
		public void TestConsignor()
		{
			NUnit.Framework.Assert.That(goodsShipment.Consignor, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation()
		{
			NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.EqualTo(default(ICustomsValuation)));
		}

		[ExpectNoExceptions]
		public void TestDeliveryDestinationName()
		{
			NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFees()
		{
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsShipmentDutyTaxFee>)));
		}

		[ExpectNoExceptions]
		public void TestNotifyParty()
		{
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestSeller()
		{
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestTradeTermsConditionCode()
		{
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestUCR()
		{
			NUnit.Framework.Assert.That(goodsShipment.UCR, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBuyer()
		{
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasures()
		{
			NUnit.Framework.Assert.That(goodsShipment.GoodsMeasures, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsMeasure>)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			goodsShipment = new GoodsShipment(entryHeader);
		}

		IGoodsShipment goodsShipment;
	}
}
