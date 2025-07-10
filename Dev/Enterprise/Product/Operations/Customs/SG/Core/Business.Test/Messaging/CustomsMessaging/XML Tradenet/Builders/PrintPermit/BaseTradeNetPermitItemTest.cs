using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class BaseTradeNetPermitItemTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSerialNb()
		{
			AssertEquals("SerialNb", "   01", Item.SerialNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHSCode()
		{
			AssertEquals("HSCode", "87021029", Item.HSCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBrandName()
		{
			AssertEquals("BrandName", "LVMH", Item.BrandName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHSQuantity()
		{
			AssertEquals("HSQuantity", "29.2500", Item.HSQuantity);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHSQuantityUnit()
		{
			AssertEquals("HSQuantityUnit", "LTR", Item.HSQuantityUnit);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMarking()
		{
			AssertEquals("Marking", "HW", Item.Marking);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCityOfOrigin()
		{
			AssertEquals("CityOfOrigin", "SG", Item.CityOfOrigin);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestModel()
		{
			AssertEquals("Model", "NA", Item.Model);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardMawbObl()
		{
			AssertEquals("InwardMawbObl", "MISCHAM000018754", Item.InwardMawbObl);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardHawbHbl()
		{
			AssertEquals("InwardHawbHbl", "0209513", Item.InwardHawbHbl);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardMawbObl()
		{
			AssertEquals("OutwardMawbObl", "MISCHAM000018754", Item.OutwardMawbObl);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardHawbHbl()
		{
			AssertEquals("OutwardHawbHbl", "0509518", Item.OutwardHawbHbl);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGoodsDescription()
		{
			AssertEquals("GoodsDescription", "NEW MERCEDES-BENZ MODEL SL350", Item.GoodsDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUnitPrice()
		{
			AssertEquals("UnitPrice", 98820.0000m, Item.UnitPrice);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUnitPriceCurrency()
		{
			AssertEquals("UnitPriceCurrency", "SGD", Item.UnitPriceCurrency);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDutQuantity()
		{
			AssertEquals("DutQuantity", "0.7500", Item.DutQuantity);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDutQuantityUnit()
		{
			AssertEquals("DutQuantityUnit", "LTR", Item.DutQuantityUnit);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsDutyPayable()
		{
			AssertEquals("CustomsDutyPayable", 10m, Item.CustomsDutyPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExciseDutyPayable()
		{
			AssertEquals("ExciseDutyPayable", 277.88m, Item.ExciseDutyPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOtherTaxPayable()
		{
			AssertEquals("OtherTaxPayable", 20m, Item.OtherTaxPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentLotNb()
		{
			AssertEquals("CurrentLotNb", "SNTB3132", Item.CurrentLotNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreviousLotNb()
		{
			AssertEquals("PreviousLotNb", "SNTB3130", Item.PreviousLotNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCifFobLspValue()
		{
			AssertEquals("CifFobLspValue", 1390.00m, Item.CifFobLspValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLspAmount()
		{
			AssertEquals("LspAmount", 158.75m, Item.LspAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGstAmount()
		{
			AssertEquals("GstAmount", 68.36m, Item.GstAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCASCProductCode()
		{
			AssertEquals("We dont use CASCProductCode in 4.1 version.", string.Empty, Item.CASCProductCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCASCProductCodes()
		{
			AssertArrayEqualsByElements("CASCProductCodes", new[] { "   01/CUPAUDEXE/          1.0000  NMB", "   02/ML6/" }, Item.CASCProductCodes.Select(c => $"{c.SequenceNumber}/{c.ProductCode}/{c.ProductQuantity}").ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCASCProductQty()
		{
			AssertEquals("CASCProductQty", 1m, Item.CASCProductQty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCASCProductUQ()
		{
			AssertEquals("CASCProductUQ", "NMB", Item.CASCProductUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEngineNbChassisNb()
		{
			AssertEquals("We dont use EngineNbChassisNb in 4.1 version.", string.Empty, Item.EngineNbChassisNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEngineOrChassisNumbers()
		{
			AssertArrayEqualsByElements("EngineOrChassisNumbers", new[] { "   01-27296630432170 / WDB2304562F126796" }, Item.EngineOrChassisNumbers.Select(c => $"{c.SequenceNumber}-{c.Number}").ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOuterPackQty()
		{
			AssertEquals("OuterPackQty", 1, Item.OuterPackQty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOuterPackUQ()
		{
			AssertEquals("OuterPackUQ", "CAR", Item.OuterPackUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInPackQty()
		{
			AssertEquals("InPackQty", 50, Item.InPackQty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInPackUQ()
		{
			AssertEquals("InPackUQ", "BOX", Item.InPackUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInnerPackQty()
		{
			AssertEquals("InnerPackQty", 10, Item.InnerPackQty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInnerPackUQ()
		{
			AssertEquals("InnerPackUQ", "PKT", Item.InnerPackUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInmostPackQty()
		{
			AssertEquals("InmostPackQty", 20, Item.InmostPackQty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInmostPackUQ()
		{
			AssertEquals("InmostPackUQ", "STK", Item.InmostPackUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue1()
		{
			AssertEquals("LineValue1", "1390.00", Item.LineValue1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue2()
		{
			AssertEquals("LineValue2", "158.75", Item.LineValue2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue3()
		{
			AssertEquals("LineValue3", "68.36", Item.LineValue3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue4()
		{
			AssertEquals("LineValue4", "0.7500", Item.LineValue4);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue5()
		{
			AssertEquals("LineValue5", "98820.00", Item.LineValue5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue6()
		{
			AssertEquals("LineValue6", "277.88", Item.LineValue6);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue7()
		{
			AssertEquals("LineValue7", "10.00", Item.LineValue7);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineValue8()
		{
			AssertEquals("LineValue8", "20.00", Item.LineValue8);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineUnit1()
		{
			AssertEquals("LineUnit1", "", Item.LineUnit1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineUnit2()
		{
			AssertEquals("LineUnit2", "", Item.LineUnit2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineUnit3()
		{
			AssertEquals("LineUnit3", "", Item.LineUnit3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineUnit4()
		{
			AssertEquals("LineUnit4", "", Item.LineUnit4);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLineUnit5()
		{
			AssertEquals("LineUnit5", "", Item.LineUnit5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValueFields()
		{
			AssertArrayEqualsByElements("ValueFields", new ZString[] { "1390.00", "158.75", "68.36", "0.7500", "98820.00", "277.88", "10.00", "20.00" }, Item.ValueFields);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvoice()
		{
			AssertEquals("Invoice", "INV20201008", Item.Invoice.InvoiceNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestManufacturerName()
		{
			AssertEquals("ManufacturerName", "TEST Manufacturer Party Name", Item.ManufacturerName);
		}

		BaseTradeNetPermitItem Item
		{
			get
			{
				if (item == null)
				{
					var message = Factory.New<SGXmlEDIMessage>();
					message.EM_MessageText = System.IO.File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\CommonPermit.XML");
					Factory.Save();
					var declaration = message.TradenetResponse.OutboundMessage.InNonPaymentPermit.Declaration;
					var invoices = declaration.Invoice;
					var permitItem = declaration.Item.FirstOrDefault();
					item = new BaseTradeNetPermitItem(permitItem, invoices, true);
				}

				return item;
			}
		}

		BaseTradeNetPermitItem item;
	}
}
