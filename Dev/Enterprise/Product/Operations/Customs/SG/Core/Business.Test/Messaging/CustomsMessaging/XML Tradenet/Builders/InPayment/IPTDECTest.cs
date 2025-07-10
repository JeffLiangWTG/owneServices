using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Customs.SG.V4.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class IPTDECTest : TestCaseWithFactory
	{
		public void TestIPTDECMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.IPTDEC, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, Message.MessageSubType);
			DataProvider.JobNumber = "CD2020";
			var dec = new TradenetDeclaration();
			AssertNull(dec.InboundMessage);
			var testIPTDECBuilder = new IPTDEC(DataProvider);
			testIPTDECBuilder.Build(dec);
			AssertType<InPayment>(dec.InboundMessage.InPayment);
			AssertNull(dec.InboundMessage.InPaymentUpdate);
			AssertEquals("WTGCD2020", dec.InboundMessage.InPayment.Header.MessageReference);
		}

		public void TestCargo_BlanketStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			DataProvider.StartDateOfBlanket = new ZDate(2015, 01, 26);
			Message.BuildCargo(InPayment);
			AssertNull(InPayment.Cargo.BlanketStartDate);
			AssertNull(InPayment.Cargo.ExhibitionTemporaryImportPeriod);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.StartDateOfBlanket = ZDate.Empty;
			DataProvider.PlaceOfStorage = ZString.Empty;
			Message.BuildCargo(InPayment);
			AssertNull(InPayment.Cargo.BlanketStartDate);
			AssertNull(InPayment.Cargo.ExhibitionTemporaryImportPeriod);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.StartDateOfBlanket = new ZDate(2015, 01, 26);
			Message.BuildCargo(InPayment);
			AssertEquals("20150126", InPayment.Cargo.BlanketStartDate);
			AssertNull(InPayment.Cargo.ExhibitionTemporaryImportPeriod);
		}

		public void TestExhibitionTemporaryImportPeriodStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.StartDateOfBlanket = new ZDate(2011, 05, 19);
			Message.BuildCargo(InPayment);
			AssertNull("Should be null.", InPayment.Cargo.ExhibitionTemporaryImportPeriod);
		}

		public void TestCargo_StorageLocation()
		{
			DataProvider.PlaceOfStorage = ZString.Empty;
			Message.BuildCargo(InPayment);
			AssertNull(InPayment.Cargo.StorageLocation);
			DataProvider.PlaceOfStorage = "MYPLACE";
			Message.BuildCargo(InPayment);
			AssertNull(InPayment.Cargo.StorageLocation);
		}

		public void TestTransport_OutwardTransport()
		{
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = false;
			Message.BuildTransport(InPayment);
			AssertNull(InPayment.Transport);
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = false;
			Message.BuildTransport(InPayment);
			AssertNotNull(InPayment.Transport.InwardTransport);
			AssertNull(InPayment.Transport.OutwardTransport);
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			Message.BuildTransport(InPayment);
			AssertNotNull(InPayment.Transport.InwardTransport);
			AssertNull(InPayment.Transport.OutwardTransport);
			DataProvider.HasInwardTransport = false;
			DataProvider.HasOutwardTransport = true;
			inPayment = new InPayment();
			Message.BuildTransport(InPayment);
			AssertNull(InPayment.Transport);
		}

		public void TestParty_Exporter_OutwardCarrierAgent_Consignee()
		{
			DataProvider.Importer = null;
			DataProvider.Exporter = null;
			DataProvider.InwardCarrierAgent = null;
			DataProvider.OutwardCarrierAgent = null;
			DataProvider.Consignee = null;
			DataProvider.Claimant = null;
			Message.BuildParty(InPayment);
			AssertNull("ImporterParty", InPayment.Party.ImporterParty);
			AssertNull("ExporterParty", InPayment.Party.ExporterParty);
			AssertNull("InwardCarrierAgentParty", InPayment.Party.InwardCarrierAgentParty);
			AssertNull("OutwardCarrierAgentParty", InPayment.Party.OutwardCarrierAgentParty);
			AssertNull("ConsigneeParty", InPayment.Party.ConsigneeParty);
			AssertNull("ClaimantParty", InPayment.Party.ClaimantParty);
			DataProvider.Importer = new OrganisationTestClass();
			DataProvider.Exporter = new OrganisationTestClass();
			DataProvider.InwardCarrierAgent = new OrganisationTestClass();
			DataProvider.OutwardCarrierAgent = new OrganisationTestClass();
			DataProvider.Consignee = new OrganisationTestClass();
			DataProvider.Claimant = new OrganisationTestClass();
			Message.BuildParty(InPayment);
			AssertNotNull("ImporterParty", InPayment.Party.ImporterParty);
			AssertNull("ExporterParty", InPayment.Party.ExporterParty);
			AssertNotNull("InwardCarrierAgentParty", InPayment.Party.InwardCarrierAgentParty);
			AssertNull("OutwardCarrierAgentParty", InPayment.Party.OutwardCarrierAgentParty);
			AssertNull("ConsigneeParty", InPayment.Party.ConsigneeParty);
			AssertNotNull("ClaimantParty", InPayment.Party.ClaimantParty);
		}

		public void TestInvoice_InvoiceNumber()
		{
			var invoice = new ItemsTestClass();
			invoice.InvoicePK = ZGuid.NewZGuid();
			invoice.InvoiceNumber = ZString.Empty;
			DataProvider.Invoices = new ICusInvoice[] { invoice };
			Message.BuildInvoice(InPayment);
			AssertNull("InvoiceNumber", InPayment.Invoice[0].InvoiceNumber);
			invoice.InvoiceNumber = "1234";
			Message.BuildInvoice(InPayment);
			AssertEquals("InvoiceNumber", "1234", InPayment.Invoice[0].InvoiceNumber);
		}

		public void TestItem_OutHAWBHUCRHBLNumber()
		{
			var item = new ItemsTestClass();
			item.InwardHAWB = ZString.Empty;
			item.OutwardHAWB = ZString.Empty;
			DataProvider.Items = new ICusItem[] { item };
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildItem(InPayment);
			AssertNull("InHAWBHUCRHBLNumber", InPayment.Item[0].InHAWBHUCRHBLNumber);
			AssertNull("OutHAWBHUCRHBLNumber", InPayment.Item[0].OutHAWBHUCRHBLNumber);
			item.InwardHAWB = "INHAWB";
			item.OutwardHAWB = "OUTHAWB";
			Message.BuildItem(InPayment);
			AssertEquals("InHAWBHUCRHBLNumber", "INHAWB", InPayment.Item[0].InHAWBHUCRHBLNumber);
			AssertNull("OutHAWBHUCRHBLNumber", InPayment.Item[0].OutHAWBHUCRHBLNumber);
		}

		public void TestItem_MotorVehicle()
		{
			var item = new ItemsTestClass();
			item.IsMotorVehicle = ZBool.False;
			item.EngineCapacity = 1980.0m;
			item.EngineCapacityUnit = "cc";
			item.DateOfFirstRegistration = new ZDate(2020, 02, 24);
			DataProvider.Items = new ICusItem[] { item };
			DataProvider.IsShortPayment = ZBool.True;
			Message.BuildItem(InPayment);
			AssertNull("MotorVehicle", InPayment.Item[0].MotorVehicle);
			item.IsMotorVehicle = ZBool.True;
			Message.BuildItem(InPayment);
			AssertNull("MotorVehicle", InPayment.Item[0].MotorVehicle);
			DataProvider.IsShortPayment = ZBool.False;
			Message.BuildItem(InPayment);
			AssertEquals("EngineCapacity.Value", 1980.0m, InPayment.Item[0].MotorVehicle.EngineCapacity.Value);
			AssertEquals("EngineCapacity.unitCode", "cc", InPayment.Item[0].MotorVehicle.EngineCapacity.unitCode);
			AssertEquals("OriginalRegistrationDate", "20200224", InPayment.Item[0].MotorVehicle.OriginalRegistrationDate);
			item.EngineCapacity = ZDecimal.Zero;
			item.EngineCapacityUnit = ZString.Empty;
			item.DateOfFirstRegistration = ZDate.Empty;
			inPayment = null;
			Message.BuildItem(InPayment);
			AssertNull("MotorVehicle.EngineCapacity", InPayment.Item[0].MotorVehicle.EngineCapacity);
			AssertEquals("OriginalRegistrationDate", string.Empty, InPayment.Item[0].MotorVehicle.OriginalRegistrationDate);
		}

		public void TestOtherTariff()
		{
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { DutyRateUnit = SGConstants.LPA, OtherTaxPercentageRate = 5.35m, OtherTaxUnitRate = 15.80m, OtherTaxAmount = 5098.35m } };
			Message.BuildItem(InPayment);
			var otherTariff = InPayment.Item[0].Tariff.OtherTax;
			AssertEquals(5098.35m, otherTariff.DutyAmount);
			AssertEquals(5.35m, otherTariff.DutyRate);
			AssertEquals(SGConstants.LPA, otherTariff.DutyRateUnit);
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { DutyRateUnit = SGConstants.LPA, OtherTaxUnitRate = 15.80m, OtherTaxAmount = 5098.35m } };
			Message.BuildItem(InPayment);
			otherTariff = InPayment.Item[0].Tariff.OtherTax;
			AssertEquals(5098.35m, otherTariff.DutyAmount);
			AssertEquals(15.80m, otherTariff.DutyRate);
			AssertEquals(SGConstants.LPA, otherTariff.DutyRateUnit);
		}

		public void TestUnitPriceExchangeRateForSGDIsSentInMessage()
		{
			var items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].CustomsValue = 17500m;
			items[0].UnitPrice = 1500m;
			items[0].LSPValue = 720m;
			items[0].InvoiceCurrency = "SGD";
			items[0].IsMotorVehicle = false;
			DataProvider.Items = items;
			Message.BuildItem(InPayment);
			CombineAssertions(() =>
			{
				var transactionValue = InPayment.Item[0].TransactionValue;
				AssertEquals(17500m, transactionValue.ItemCIFFOBValue);
				Assert(transactionValue.ItemCIFFOBValueSpecified);
				AssertEquals(720m, transactionValue.LastSellingPriceValue);
				Assert(transactionValue.LastSellingPriceValueSpecified);
				AssertNull("transactionValue.UnitPriceValue", transactionValue.UnitPriceValue);
			});

			items[0].IsMotorVehicle = true;
			Message.BuildItem(InPayment);
			CombineAssertions(() =>
			{
				var transactionUnitPriceValue = InPayment.Item[0].TransactionValue.UnitPriceValue;
				AssertEquals(1500m, transactionUnitPriceValue.Amount.Value);
				AssertEquals("SGD", transactionUnitPriceValue.Amount.currencyID);
				AssertEquals("ExchangeRateSpecified", true, transactionUnitPriceValue.ExchangeRateSpecified);
				AssertEquals("ExchangeRate", 1.00m, transactionUnitPriceValue.ExchangeRate);
			});
		}

		IPTDEC Message => message ?? (message = new IPTDEC(DataProvider));
		IPTDEC message;
		IPTDECTestClass DataProvider => dataProvider ?? (dataProvider = new IPTDECTestClass());
		IPTDECTestClass dataProvider;
		InPayment InPayment => inPayment ?? (inPayment = new InPayment());
		InPayment inPayment;
	}
}
