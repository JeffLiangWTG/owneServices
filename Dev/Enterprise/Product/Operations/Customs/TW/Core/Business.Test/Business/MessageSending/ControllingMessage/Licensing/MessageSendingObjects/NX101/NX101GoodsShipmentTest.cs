using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(IEnumerable<Messaging.IAdditionalDocument>)), "AdditionalDocuments - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty), "ExitDateTime");
				NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "ItemChargeAmount");
				NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)), "Consignee - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.Consignor, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)), "Consignor - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.EqualTo(default(Messaging.ICustomsValuation)), "CustomsValuation - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "DeliveryDestinationName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees, NUnit.Framework.Is.EqualTo(default(IEnumerable<Messaging.IGoodsShipmentDutyTaxFee>)), "DutyTaxFees - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.NotifyParty, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)), "NotifyParty - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)), "Seller - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TradeTermsConditionCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.UCR.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UCR - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(Messaging.IPartyDetails)), "Buyer - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.Exporter.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItems()
		{
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(0));

			for (var i = 0; i < 20; i++)
			{
				var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = mergedLine.PK;
			}

			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.AssignCMHeaderToInvoices(header));
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(20));

			var firstInvoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.FirstOrDefault();
			firstInvoiceLine.JI_Group = "group";
			firstInvoiceLine.JI_DeclarationGoodsDescription = new ZString('A', 512);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(21));

			firstInvoiceLine.JI_DeclarationGoodsDescription = new ZString('A', 513);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(22));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(25));
		}

		public void TestAdditionalDeclarations()
		{
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.AssignCMHeaderToInvoices(header));
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var entryHeader = (CusEntryHeader)header.EntryInstruction.EntryHeader;
			entryHeader.EntryNumber = "Test Number";
			var goodsShipmentCertificate = new NX101GoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipmentCertificate.AdditionalDeclarations.IsNullOrEmpty(), NUnit.Framework.Is.True, "AdditionalDeclaration should be null when CertificateType is 15");

			header.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			goodsShipmentCertificate = new NX101GoodsShipment(header);
			AssertContainsExactElementsInAnyOrder(new[] { "Test Number" }, goodsShipmentCertificate.AdditionalDeclarations.Select(x => x.ID));
			NUnit.Framework.Assert.That(goodsShipmentCertificate.AdditionalDeclarations.Select(x => x.SequenceNumeric), NUnit.Framework.Is.EquivalentTo(new ZDecimal[] { 1m }));

			var mergedLine2 = entryHeader.MergedLines.AddNew();
			mergedLine2.CL_LineNumber = 3;

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = mergedLine2.PK;
			invoiceLine.JI_PermitQty = 20m;
			invoiceLine.JI_PermitUQ = "TNE";
			invoiceLine.AssignCMHeaderToInvoices(header);

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = mergedLine2.PK;
			invoiceLine.JI_PermitQty = 30m;
			invoiceLine.JI_PermitUQ = "KG";
			invoiceLine.AssignCMHeaderToInvoices(header);

			var mergedLine3 = entryHeader.MergedLines.AddNew();
			mergedLine3.CL_LineNumber = 2;

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = mergedLine3.PK;
			invoiceLine.JI_PermitQty = 20m;
			invoiceLine.JI_PermitUQ = "TNE";

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = mergedLine3.PK;
			invoiceLine.JI_PermitQty = 30m;
			invoiceLine.JI_PermitUQ = "KG";
			var goodsShipment = new NX101GoodsShipment(header);
			CombineAssertions(() =>
			{
				var additionalDeclrations = goodsShipment.AdditionalDeclarations;
				AssertContainsExactElementsInAnyOrder(new[] { "Test Number", "Test Number" }, additionalDeclrations.Select(x => x.ID));
				NUnit.Framework.Assert.That(additionalDeclrations.Select(x => x.SequenceNumeric), NUnit.Framework.Is.EquivalentTo(new ZDecimal[] { 1m, 3m }));
			});

			entryHeader.EntryNumberInfo.ClearValue();
			goodsShipment = new NX101GoodsShipment(header);
			AssertNull("Should be null when declaration number is empty", goodsShipment.AdditionalDeclarations);
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations.IsNullOrEmpty(), NUnit.Framework.Is.True);

			header.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			var entryHeader = (CusEntryHeader)header.EntryInstruction.EntryHeader;
			entryHeader.CH_EntryReleaseDate = ZDate.Today.AddDays(-181);

			entryHeader.EntryNumber = "Test Number";
			Factory.Save();

			var goodsShipment1 = new NX101GoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment1.AdditionalInformations.Select(x => x.ApprovalID), NUnit.Framework.Is.EquivalentTo(new List<ZString>() { "Test Number" }));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var goodsShipment2 = new NX101GoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment2.AdditionalInformations.IsNullOrEmpty(), NUnit.Framework.Is.True);

			header.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			var goodsShipment3 = new NX101GoodsShipment(header);
			entryHeader.CH_EntryReleaseDate = ZDate.Today.AddDays(-180);
			NUnit.Framework.Assert.That(goodsShipment3.AdditionalInformations.IsNullOrEmpty(), NUnit.Framework.Is.True, "AdditionalInformation should be null when EntryReleaseDate within 180 days");

			entryHeader.CH_EntryReleaseDate = ZDate.Empty;
			NUnit.Framework.Assert.That(goodsShipment3.AdditionalInformations.IsNullOrEmpty(), NUnit.Framework.Is.True, "AdditionalInformation should be null when EntryReleaseDate is Empty");
		}

		[ExpectNoExceptions]
		public void TestConsignment()
		{
			var consignment = goodsShipment.Consignment;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.AdditionalInformations.Single().StatementDescription, NUnit.Framework.Is.EqualTo("TEST NOTES").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(consignment.BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("Flight123").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(consignment.DepartureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("AS3245").Using(CustomComparers.TypeComparison));
				var loadingLocation = consignment.LoadingLocation;
				NUnit.Framework.Assert.That(loadingLocation.ID, NUnit.Framework.Is.EqualTo("TWXXX").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(loadingLocation.LoadingDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2023, 5, 5).Date));
				NUnit.Framework.Assert.That(loadingLocation.Name, NUnit.Framework.Is.EqualTo("TAIWANG").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(loadingLocation.EstimatedLoadingCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(consignment.TransportEquipments.Select(x => x.ID), NUnit.Framework.Is.EquivalentTo(new List<ZString> { "CRXU1234569", "DRXU1234569" }));
				NUnit.Framework.Assert.That(consignment.UnloadingLocation.ID, NUnit.Framework.Is.EqualTo("AUSYD").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			var org = new TestTWCreator(Factory).CreateOrganization();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			var supplierDocumentaryAddress = header.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = org.PK;

			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierTranslatedDocumentaryAddress = supplierDocumentaryAddress.LocalAddress;
			supplierTranslatedDocumentaryAddress.E2_AddressType = "STA";
			supplierTranslatedDocumentaryAddress.E2_ParentTableCode = "TW1";
			supplierTranslatedDocumentaryAddress.CompanyName = "TW Address";
			goodsShipment = new NX101GoodsShipment(header);

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsShipment.Exporter.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._53).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsShipment.Exporter.Address.Line, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY TAIPEI 12345 TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line");
				NUnit.Framework.Assert.That(goodsShipment.Exporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
			});

			supplierDocumentaryAddress.E2_Address1 = "";
			supplierTranslatedDocumentaryAddress.E2_Address1 = "";
			goodsShipment = new NX101GoodsShipment(header);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsShipment.Exporter.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._53).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsShipment.Exporter.Address.Line, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Address.Line");
				NUnit.Framework.Assert.That(goodsShipment.Exporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Address.ChineseLine");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasures()
		{
			var goodsMeasures = goodsShipment.GoodsMeasures;
			NUnit.Framework.Assert.That(goodsMeasures, NUnit.Framework.Is.EqualTo(default(IEnumerable<Messaging.IGoodsMeasure>)));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			goodsMeasures = goodsShipment.GoodsMeasures;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsMeasures.Count(), NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(goodsMeasures.Any(x => x.TariffQuantity == 130m && x.UnitCode == "KG"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(goodsMeasures.Any(x => x.TariffQuantity == 20m && x.UnitCode == "TNE"), NUnit.Framework.Is.True);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_VoyageFlightNo = "Flight123";
			declaration.JE_VesselName = "AS3245";
			declaration.JE_ExportDate = new ZDateTime(2023, 5, 5);
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			var testContainer1 = declaration.CusContainers.AddNew();
			testContainer1.CO_ContainerNumber = "CRXU1234569";
			var testContainer2 = declaration.CusContainers.AddNew();
			testContainer2.CO_ContainerNumber = "DRXU1234569";

			invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			mergedLine = entryHeader.MergedLines.AddNew();
			mergedLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = mergedLine.PK;
			invoiceLine.JI_PermitQty = 100m;
			invoiceLine.JI_PermitUQ = "KG";

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = mergedLine.PK;
			invoiceLine2.JI_PermitQty = 20m;
			invoiceLine2.JI_PermitUQ = "TNE";

			var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = mergedLine.PK;
			invoiceLine3.JI_PermitQty = 30m;
			invoiceLine3.JI_PermitUQ = "KG";

			header = entryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_IsEstimatedLoadingDate = true;
			header.TW_Notes = "TEST NOTES";
			header.TW1_RL_NKPortOfLoading = "TWXXX";
			header.TW1_PortOfLoadingName = "TAIWANG";
			goodsShipment = new NX101GoodsShipment(header);
		}

		CusTWControllingMessageHeader header;
		NX101GoodsShipment goodsShipment;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		CusEntryLine mergedLine;
	}
}
