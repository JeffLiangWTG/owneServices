using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNX101GoodsShipmentGovernmentAgencyGoodsItemProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			invoiceLine.JI_OriginCriteria = "A";
			invoiceLine.JI_PTCriteria = "B";
			invoiceLine.JI_ManufacturerRelationship = "M";
			invoiceLine.JI_PTCriteria2 = "B2";
			var header = entryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;

			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.CriteriaCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "CriteriaCode");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.PreferentialCriteria, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison), "PreferentialCriteria");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ProducerCode, NUnit.Framework.Is.EqualTo("M").Using(CustomComparers.TypeComparison), "ProducerCode");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.OtherCriteria, NUnit.Framework.Is.EqualTo("B2").Using(CustomComparers.TypeComparison), "OtherCriteria");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Packaging, NUnit.Framework.Is.TypeOf<NX101GoodsShipmentPackaging>());
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasure()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			invoiceLine.JI_PermitQty = 100M;
			invoiceLine.JI_PermitUQ = "KG";
			invoiceLine.JI_CustomPermitUQ = "SET";
			var header = entryInstruction.ControllingMessageHeaders.AddNew();

			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			var goodsMeasure = governmentAgencyGoodsItem.GoodsMeasure;
			NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsMeasure.CustomUnitCode, NUnit.Framework.Is.EqualTo("SET").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			var header = entryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));

			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.OH_FullName = "Test Org";
			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Test 1";
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.OrganisationPK = org.PK;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.IDCode = "123";
			header.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var header = entryInstruction.ControllingMessageHeaders.AddNew();

			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDeclaration)), "AdditionalDeclaration should be null when CertificateType is not 15 - should be [null]");

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			mergedLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = mergedLine.PK;
			entryHeader.EntryNumber = "CA0002";
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDeclaration)), "AdditionalDeclaration should be nul when ClearanceStatus is Empty - should be [null]");

			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			var additionalDecl = governmentAgencyGoodsItem.AdditionalDeclaration;
			NUnit.Framework.Assert.That(additionalDecl.SequenceNumeric, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "SequenceNumeric");
			NUnit.Framework.Assert.That(additionalDecl.ID, NUnit.Framework.Is.EqualTo("CA0002").Using(CustomComparers.TypeComparison), "ID");
		}

		[ExpectNoExceptions]
		public void TestCommodity()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var header = Factory.New<CusTWControllingMessageHeader>();
			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.TypeOf<NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity>());
		}
	}
}
