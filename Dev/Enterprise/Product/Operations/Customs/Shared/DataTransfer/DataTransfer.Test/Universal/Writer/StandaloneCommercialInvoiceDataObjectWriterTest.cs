using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestExportStandaloneCommercialInvoiceData()
		{
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_BranchName = "BOB'S BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var invoiceMock = CreateInvoiceMock();
			var invoice = SetupJobComInvoiceHeader(invoiceMock.Object);
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_GB = branch2.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			var charge1 = SetupJobComInvHeaderCharge(invoice.Charges.AddNew(), ZBool.False, 4000m, Common.CustomsChargeTypeList.Codes.DeductionCharge, Core.Constants.CurrencyCodes.NewZealand, ChargeDistributeByList.Codes.Volume, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.10m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, Core.Constants.PaymentType.Prepaid);
			var charge2 = SetupJobComInvHeaderCharge(invoice.Charges.AddNew(), ZBool.True, 6000m, Common.CustomsChargeTypeList.Codes.PackingCost, Core.Constants.CurrencyCodes.Singapore, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, Core.Constants.PaymentType.Collect);
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLine1 = invoice.JobComInvoiceLines[0];
			var invoiceLine1Charge1 = SetupJobComInvHeaderCharge(invoiceLine1.Charges.AddNew(), ZBool.False, 400m, Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 0.9m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, Core.Constants.PaymentType.Prepaid);
			var invoiceLine1Charge2 = SetupJobComInvHeaderCharge(invoiceLine1.Charges.AddNew(), ZBool.True, 600m, Common.CustomsChargeTypeList.Codes.Discount, Core.Constants.CurrencyCodes.Indonesia, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, Core.Constants.PaymentType.Collect);
			declaration.ResumeApportionment();

			var invoiceLine2 = invoice.JobComInvoiceLines[1];
			var invoiceLine2Charge1 = SetupJobComInvHeaderCharge(invoiceLine2.Charges.AddNew(), ZBool.False, 400m, Common.CustomsChargeTypeList.Codes.OtherCharges, Core.Constants.CurrencyCodes.Australia, ChargeDistributeByList.Codes.Value, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 0.9m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, Core.Constants.PaymentType.Prepaid);
			var invoiceLine2Charge2 = SetupJobComInvHeaderCharge(invoiceLine2.Charges.AddNew(), ZBool.True, 600m, Common.CustomsChargeTypeList.Codes.LandingCharges, Core.Constants.CurrencyCodes.Indonesia, ChargeDistributeByList.Codes.Value, ZString.Empty, 1m, ApportionmentTypeList.Codes.FullApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, Core.Constants.PaymentType.Collect);

			var container1 = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.CN, "CONT123ABC");
			var container2 = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.CN, "CONT456DEF");
			var masterBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.MB, "MB123ABC");
			var houseBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.HB, "HB123ABC");
			var subHouseBill = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.SH, "SH123ABC");
			var responsibleParty = SetupInvoiceHeaderRefs(invoice.InvoiceHeaderRefs.AddNew(), InvoiceHeaderRefsTypeList.Codes.RP, "RP123ABC");

			var transport1 = SetupTransport(invoice.Transports.AddNew(), Core.Constants.TransportModes.Rail, "", "SEK323", SeaLocalPort1.RL_Code, SeaLocalPort2.RL_Code);
			var transport2 = SetupTransport(invoice.Transports.AddNew(), Core.Constants.TransportModes.Sea, "APL EMERALD", "V23W", SeaLocalPort2.RL_Code, SeaForeignPort1.RL_Code);

			var note1 = invoice.Notes.AddNew(true, "SILLY DATA 2", "GOODBYE WORLD");
			var note2 = invoice.Notes.AddNew(true, "SILLY DATA 1", "HELLO WORLD");
			declaration.ResumeApportionment();
			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)));
			var declarationDataObject = writer.GetDataObject(invoice);
			CombineAssertions(delegate
			{
				AssertEquals("declarationDataObject.MessageType.Code", JobMessageTypeList.Codes.Import, declarationDataObject.MessageType.Code);
				AssertEquals("declarationDataObject.MessageType.Description", JobMessageTypeList.Descriptions.Import, declarationDataObject.MessageType.Description);
				AssertEquals("declarationDataObject.Branch.Code", branch2.GB_Code, declarationDataObject.Branch.Code);
				AssertEquals("declarationDataObject.Branch.Name", branch2.GB_BranchName, declarationDataObject.Branch.Name);
				AssertNull("declarationDataObject.WayBillNumber", declarationDataObject.WayBillNumber);
				AssertNull("declarationDataObject.WayBillNumber", declarationDataObject.WayBillType);
			});
			AssertEquals("declarationDataObject.TransportLegCollection.Count", 2, declarationDataObject.TransportLegCollection.Count);
			AssertContents(declarationDataObject.TransportLegCollection[0], TransportMode.Rail, "", "SEK323", GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName));
			AssertContents(declarationDataObject.TransportLegCollection[1], TransportMode.Sea, "APL EMERALD", "V23W", GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName));

			AssertEquals("declarationDataObject.ContainerCollection.Count", 2, declarationDataObject.ContainerCollection.Count);
			AssertEquals(CollectionContent.Partial, declarationDataObject.ContainerCollection.Content);
			AssertNotNull(declarationDataObject.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT123ABC" && !x.Seal.HasValue));
			AssertNotNull(declarationDataObject.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT456DEF" && !x.Seal.HasValue));

			AssertEquals("declarationDataObject.AdditionalBillCollection.Count", 3, declarationDataObject.AdditionalBillCollection.Count);
			AssertNotNull(declarationDataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "MB123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master));
			AssertNotNull(declarationDataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "HB123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House));
			AssertNotNull(declarationDataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.GetValueOrDefault() == "SH123ABC" && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.SubHouse));

			AssertEquals("declarationDataObject.AdditionalReferenceCollection.Count", 1, declarationDataObject.AdditionalReferenceCollection.Count);
			AssertNotNull(declarationDataObject.AdditionalReferenceCollection.FirstOrDefault(x => x.ReferenceNumber.GetValueOrDefault() == "RP123ABC" && x.Type.Code.GetValueOrDefault() == InvoiceHeaderRefsTypeList.Codes.RP));

			var invoiceData = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0];
			AssertContents(invoiceData);

			var noteCollection = invoiceData.NoteCollection;
			AssertNotNull("noteCollection", noteCollection);
			AssertEquals("noteCollection.Count", 2, noteCollection.Count);
			AssertContents(noteCollection[0], true, "SILLY DATA 1", "HELLO WORLD");
			AssertContents(noteCollection[1], true, "SILLY DATA 2", "GOODBYE WORLD");

			var invoiceChargeCollection = invoiceData.CommercialChargeCollection;
			AssertNotNull(invoiceChargeCollection);
			AssertEquals(6, invoiceChargeCollection.Count);
			AssertContents(invoiceChargeCollection[0], ZBool.False, 57134.76m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.10m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(invoiceChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[2], ZBool.False, 444.44m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Common.CustomsChargeTypeList.Descriptions.ForeignInlandFreight, 35), GetCodeDescriptionPair(invoice.LocalCurrencyCode, invoice.LocalCurrency.RX_Desc), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[3], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.LandingCharges, Common.CustomsChargeTypeList.Descriptions.LandingCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[4], ZBool.False, 444.44m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), GetCodeDescriptionPair(invoice.LocalCurrencyCode, invoice.LocalCurrency.RX_Desc), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceChargeCollection[5], ZBool.True, 68561.71m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));

			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertNotNull(commercialInvoiceLineCollection);
			var commercialInvoiceLine1 = commercialInvoiceLineCollection[0];
			var commercialInvoiceLine1ChargeCollection = commercialInvoiceLine1.CommercialChargeCollection;
			AssertNotNull(commercialInvoiceLine1ChargeCollection);
			AssertEquals(4, commercialInvoiceLine1ChargeCollection.Count);
			AssertContents(commercialInvoiceLine1ChargeCollection[0], ZBool.False, 23932.18m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine1ChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine1ChargeCollection[2], ZBool.False, 400m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, Common.CustomsChargeTypeList.Descriptions.ForeignInlandFreight, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 0.9m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.1m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine1ChargeCollection[3], ZBool.True, 28718.61m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));

			var commercialInvoiceLine2 = commercialInvoiceLineCollection[1];
			var commercialInvoiceLine2ChargeCollection = commercialInvoiceLine2.CommercialChargeCollection;
			AssertNotNull(commercialInvoiceLine2ChargeCollection);
			AssertEquals(4, commercialInvoiceLine2ChargeCollection.Count);
			AssertContents(commercialInvoiceLine2ChargeCollection[0], ZBool.False, 33202.58m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.DeductionCharge, Common.CustomsChargeTypeList.Descriptions.DeductionCharge, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.5m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine2ChargeCollection[1], ZBool.True, 600m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.LandingCharges, Common.CustomsChargeTypeList.Descriptions.LandingCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Indonesia, "Indonesian Rupiah"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 0.92d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.1m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(commercialInvoiceLine2ChargeCollection[2], ZBool.False, 400m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 0.9m, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZBool.False, 0.2m, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(commercialInvoiceLine2ChargeCollection[3], ZBool.True, 39843.10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeTypeList.Descriptions.PackingCost, 35), GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 2.04d, GetCodeDescriptionPair(ApportionmentTypeList.Codes.FullApportionment, ApportionmentTypeList.Descriptions.FullApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, 0.6m, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
		}

		JobComInvoiceHeaderRefs SetupInvoiceHeaderRefs(JobComInvoiceHeaderRefs reference, ZString type, ZString number)
		{
			reference.J2_ReferenceType = type;
			reference.J2_ReferenceNumber = number;
			return reference;
		}
	}
}
