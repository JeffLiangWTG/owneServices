using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NX101ControllingMessageHeaderSectionBodyDocumentWrapper))]
	sealed class NX101ControllingMessageHeaderSectionBodyDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestGrouping()
		{
			invoiceLine.JI_Group = "Grouppppp";
			NUnit.Framework.Assert.That(documentWrapper.Grouping, NUnit.Framework.Is.EqualTo("Grouppppp").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTariffformat()
		{
			invoiceLine.JI_Tariff = "98257200204";
			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.NotPrinted;
			NUnit.Framework.Assert.That(documentWrapper.Tariffformat, NUnit.Framework.Is.EqualTo(ZString.Empty));

			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.One;
			var fINX101 = new NX101MessageSendingObject(header);
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
			NUnit.Framework.Assert.That(documentWrapper.Tariffformat, NUnit.Framework.Is.EqualTo("9825.72").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.TWO;
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
			NUnit.Framework.Assert.That(documentWrapper.Tariffformat, NUnit.Framework.Is.EqualTo("9825.72.00").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_TariffPrintLength = TariffPrintLengthList.Codes.THREE;
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
			NUnit.Framework.Assert.That(documentWrapper.Tariffformat, NUnit.Framework.Is.EqualTo("9825.72.00.20-4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsDescription()
		{
			invoiceLine.NX101PermitGoodsDescription = "NX101 Permit Goods Description";
			NUnit.Framework.Assert.That(documentWrapper.GoodsDescription, NUnit.Framework.Is.EqualTo("NX101 Permit Goods Description").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBrand()
		{
			invoiceLine.JI_BrandName = "Apple Watch";
			NUnit.Framework.Assert.That(documentWrapper.Brand, NUnit.Framework.Is.EqualTo("Apple Watch").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			documentWrapper = (NX101ControllingMessageHeaderSectionBodyDocumentWrapper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(documentWrapper.Brand.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Brand should be empty - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		public void TestModel()
		{
			invoiceLine.JI_Model = "Iphone 15 pro max";
			NUnit.Framework.Assert.That(documentWrapper.Model, NUnit.Framework.Is.EqualTo("Iphone 15 pro max").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			documentWrapper = (NX101ControllingMessageHeaderSectionBodyDocumentWrapper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(documentWrapper.Model.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Model should be empty - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		public void TestSpecification()
		{
			invoiceLine.JI_InnerPackDescription = "Inner pack desc";
			NUnit.Framework.Assert.That(documentWrapper.Specification, NUnit.Framework.Is.EqualTo("Inner pack desc").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			var msgSendingObj = new NX101MessageSendingObject(header);
			documentWrapper = (NX101ControllingMessageHeaderSectionBodyDocumentWrapper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(documentWrapper.Specification.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Specification should be empty - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		public void TestShippingMarks()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceLine.NX101ShippingMarks = "shipping marks test";
			NUnit.Framework.Assert.That(documentWrapper.ShippingMarks, NUnit.Framework.Is.EqualTo("shipping marks test").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(documentWrapper.Specification, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDescriptionOfGoods()
		{
			invoiceLine.NX101PermitGoodsDescription = "NX101 Permit Goods Description";
			invoiceLine.JI_InnerPackDescription = "Inner pack desc";
			NUnit.Framework.Assert.That(documentWrapper.DescriptionOfGoods, NUnit.Framework.Is.EqualTo("NX101 Permit Goods Description\r\nInner pack desc").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTariffQuantity()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_PermitQty = 10M;
				NUnit.Framework.Assert.That(documentWrapper.TariffQuantity, NUnit.Framework.Is.EqualTo("10").Using(CustomComparers.TypeComparison), "10");

				invoiceLine.JI_PermitQty = 10.1M;
				NUnit.Framework.Assert.That(documentWrapper.TariffQuantity, NUnit.Framework.Is.EqualTo("10.1").Using(CustomComparers.TypeComparison), "10.1");

				invoiceLine.JI_PermitQty = 10.12M;
				NUnit.Framework.Assert.That(documentWrapper.TariffQuantity, NUnit.Framework.Is.EqualTo("10.12").Using(CustomComparers.TypeComparison), "10.12");

				invoiceLine.JI_PermitQty = 10010.123M;
				NUnit.Framework.Assert.That(documentWrapper.TariffQuantity, NUnit.Framework.Is.EqualTo("10,010.12").Using(CustomComparers.TypeComparison), "10010.123");
			});
		}

		[ExpectNoExceptions]
		public void TestQuantity()
		{
			invoiceLine.JI_PermitQty = 15m;
			NUnit.Framework.Assert.That(documentWrapper.Quantity, NUnit.Framework.Is.EqualTo(15m));
		}

		[ExpectNoExceptions]
		public void TestUnit()
		{
			invoiceLine.JI_CustomPermitUQ = "TNE";
			NUnit.Framework.Assert.That(documentWrapper.Unit, NUnit.Framework.Is.EqualTo("TNE").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			invoiceLine.JI_CustomPermitUQ = "CUQ";
			invoiceLine.JI_PermitUQ = "TUQ";
			var fINX101 = new NX101MessageSendingObject(header);
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.Unit, NUnit.Framework.Is.EqualTo("CUQ").Using(CustomComparers.TypeComparison), "Unit for Code18: CustomPermitUQ is not empty");
				invoiceLine.JI_CustomPermitUQ = ZString.Empty;
				NUnit.Framework.Assert.That(documentWrapper.Unit, NUnit.Framework.Is.EqualTo("TUQ").Using(CustomComparers.TypeComparison), "Unit for Code18: CustomPermitUQ is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestUnitWithCertificate15()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity", Core.Constants.CountryCodes.China);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.China, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "036", "克", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceLine.JI_PermitUQ = "036";
			var fINX101 = new NX101MessageSendingObject(header);
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.Unit, NUnit.Framework.Is.EqualTo("克").Using(CustomComparers.TypeComparison), "Unit for Code15: JI_PermitUQ is 036");
				invoiceLine.JI_PermitUQ = ZString.Empty;
				NUnit.Framework.Assert.That(documentWrapper.Unit, NUnit.Framework.Is.EqualTo(ZString.Empty), "Unit for Code15: JI_PermitUQ is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestQuantityAndUnit()
		{
			invoiceLine.JI_PermitQty = 15m;
			invoiceLine.JI_CustomPermitUQ = "TNE";
			NUnit.Framework.Assert.That(documentWrapper.QuantityAndUnit, NUnit.Framework.Is.EqualTo("15 TNE").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_PermitQty = 1500m;
			NUnit.Framework.Assert.That(documentWrapper.QuantityAndUnit, NUnit.Framework.Is.EqualTo("1,500 TNE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCriteriaCode()
		{
			invoiceLine.JI_OriginCriteria = "A";
			NUnit.Framework.Assert.That(documentWrapper.CriteriaCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreferentialCriteria()
		{
			invoiceLine.JI_PTCriteria = "B";
			NUnit.Framework.Assert.That(documentWrapper.PreferentialCriteria, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOtherCriteria()
		{
			invoiceLine.JI_PTCriteria2 = "O";
			NUnit.Framework.Assert.That(documentWrapper.OtherCriteria, NUnit.Framework.Is.EqualTo("O").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestProducerCode()
		{
			invoiceLine.JI_ManufacturerRelationship = "C";
			NUnit.Framework.Assert.That(documentWrapper.ProducerCode, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEightDigitsHSTariff()
		{
			invoiceLine.JI_Tariff = "98257200204";
			NUnit.Framework.Assert.That(documentWrapper.EightDigitsHSTariff, NUnit.Framework.Is.EqualTo("98257200").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestZZZTarifff()
		{
			invoiceLine.JI_IMPTariff = "98257200204";
			NUnit.Framework.Assert.That(documentWrapper.EightDigitsZZZTariff, NUnit.Framework.Is.EqualTo("98257200").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestInoviceCurrency()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			NUnit.Framework.Assert.That(documentWrapper.InvoiceCurrency, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestInovicePrice()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceLine.JI_PermitUnitPrice = 200m;
			invoiceLine.JI_PermitQty = 4m;
			var headerDocumentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			NUnit.Framework.Assert.That(headerDocumentWrapper.InvoiceLines[0].InvoicePrice, NUnit.Framework.Is.EqualTo("800").Using(CustomComparers.TypeComparison));

			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			invoiceLine.JI_PermitUnitPrice = 10.23m;
			invoiceLine.JI_PermitQty = 1m;
			headerDocumentWrapper = new NX101ControllingMessageHeaderDocumentWrapper(header);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(headerDocumentWrapper.InvoiceLines[0].InvoicePrice, NUnit.Framework.Is.EqualTo("800.00").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(headerDocumentWrapper.InvoiceLines[1].InvoicePrice, NUnit.Framework.Is.EqualTo("10.23").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestInvoice()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceHeader.JZ_InvoiceNumber = "YX12345678";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2023, 12, 18);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentWrapper.InvoiceID, NUnit.Framework.Is.EqualTo("YX12345678").Using(CustomComparers.TypeComparison), "InvoiceID");
				NUnit.Framework.Assert.That(documentWrapper.InvoiceDate, NUnit.Framework.Is.EqualTo(invoiceHeader.JZ_InvoiceDate.Date), "InvoiceDate");
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fINX101 = new NX101MessageSendingObject(header);
			return new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			var fINX101 = new NX101MessageSendingObject(header);
			documentWrapper = new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), header.TW1_CertificateType, Factory);
		}

		CusTWControllingMessageHeader header;
		NX101ControllingMessageHeaderSectionBodyDocumentWrapper documentWrapper;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
