using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusEntryHeaderMessageProviderTest : TestCaseWithFactory
	{
		public void TestCusEntryHeaderImportMessageMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 1 | 3-24", () =>
				{
					AssertEquals("DeclarationNo", "21340300IM123456", declaration.DeclarationNo);
					AssertEquals("Nature", "4000", declaration.Nature);
					AssertEquals("CustomsOffice", "341200", declaration.CustomsOffice);
					AssertEquals("SimplifiedProcedure", "1", declaration.SimplifiedProcedure);
					AssertEquals("CountOfLoadingDocuments", 2, declaration.CountOfLoadingDocuments);
					AssertEquals("PackQuantity", 20, declaration.PackQuantity);
					AssertEquals("CountryOfTrade", "003", declaration.CountryOfTrade);
					AssertEquals("ReferenceNo", "ORD1234", declaration.ReferenceNo);
					AssertEquals("UnionRecordNumber", ZString.Empty, declaration.UnionRecordNumber);
					AssertEquals("UnionCryptoNumber", ZString.Empty, declaration.UnionCryptoNumber);
					AssertEquals("CountryOfDeparture", "052", declaration.CountryOfDeparture);
					AssertEquals("CountryOfDestination", ZString.Empty, declaration.CountryOfDestination);
					AssertEquals("CountryOfDispatch", "003", declaration.CountryOfDispatch);
					AssertEquals("TypeOfVehicleOnExit", "4", declaration.TypeOfVehicleOnExit);
					AssertEquals("PlateOfVehicleOnExit", "TK321", declaration.PlateOfVehicleOnExit);
					AssertEquals("CountryOfVehicleOnExit", "660", declaration.CountryOfVehicleOnExit);
					AssertEquals("TypeOfDelivery", "FOB", declaration.TypeOfDelivery);
					AssertEquals("PlaceOfDelivery", "ISTANBUL", declaration.PlaceOfDelivery);
					AssertEquals("IsContainer", "EVET", declaration.IsContainer);
					AssertEquals("TypeOfVehicleAtBorder", "3", declaration.TypeOfVehicleAtBorder);
					AssertEquals("PlateOfVehicleAtBorder", "TK123", declaration.PlateOfVehicleAtBorder);
					AssertEquals("CountryOfVehicleAtBorder", "006", declaration.CountryOfVehicleAtBorder);
				});

				CombineAssertions("Declaration level case 1 | 25-56", () =>
				{
					AssertEquals("TotalInvoiceAmount", 1000m, declaration.TotalInvoiceAmount);
					AssertEquals("TotalInvoiceAmountCurrency", "EUR", declaration.TotalInvoiceAmountCurrency);
					AssertEquals("TotalFreight", 100m, declaration.TotalFreight);
					AssertEquals("TotalFreightCurrency", "USD", declaration.TotalFreightCurrency);
					AssertEquals("TypeOfTransportAtBorder", "30", declaration.TypeOfTransportAtBorder);
					AssertEquals("BuyerSellerRelationship", "0", declaration.BuyerSellerRelationship);
					AssertEquals("TotalInsurance", 30m, declaration.TotalInsurance);
					AssertEquals("TotalInsuranceCurrency", "USD", declaration.TotalInsuranceCurrency);
					AssertEquals("LoadingUnloadingPlace", "ERENKÖY GÜMRÜK MÜDÜRLÜĞÜ", declaration.LoadingUnloadingPlace);
					AssertEquals("TotalAbroadExpenditure", 100m, declaration.TotalAbroadExpenditure);
					AssertEquals("TotalAbroadExpenditureCurrency", "EUR", declaration.TotalAbroadExpenditureCurrency);
					AssertEquals("TotalDomesticExpenditure", 2100m, declaration.TotalDomesticExpenditure);
					AssertEquals("BankCode", "000100007026", declaration.BankCode);
					AssertEquals("GoodsLocation", "ZEYTİNBURNU", declaration.GoodsLocation);
					AssertEquals("CustomsOfficeOfDestination", string.Empty, declaration.CustomsOfficeOfDestination);
					AssertEquals("BondedWarehouseCode", "G002", declaration.BondedWarehouseCode);
					AssertEquals("PlannedRoute", string.Empty, declaration.PlannedRoute);
					AssertEquals("CounterVailingDuty", 0m, declaration.CounterVailingDuty);
					AssertEquals("CustomsOfficeOfEntry", "341200", declaration.CustomsOfficeOfEntry);
					AssertEquals("QualificationOfProcess", string.Empty, declaration.QualificationOfProcess);
					AssertEquals("Descriptions", "desc1 desc2 desc3", declaration.Descriptions);
					AssertEquals("UserId", "20201224104", declaration.UserId);
					AssertEquals("ReferenceDate", string.Empty, declaration.ReferenceDate);
					AssertEquals("Payment", "PESIN", declaration.Payment);
					AssertEquals("InstrumentOfPayment", "C", declaration.InstrumentOfPayment);
					AssertEquals("ReferenceOfBrokers", "ULU-2021IM/00000005", declaration.ReferenceOfBrokers);
					AssertEquals("ShipperTaxNo", "8890024379", declaration.ShipperTaxNo);
					AssertEquals("ConsigneeTaxNo", "8890024399", declaration.ConsigneeTaxNo);
					AssertEquals("DeclarantTaxNo", "8890024444", declaration.DeclarantTaxNo);
					AssertEquals("AdvisorTaxNo", "8890024555", declaration.AdvisorTaxNo);
					AssertEquals("PrincipleResponsibleTaxNo", string.Empty, declaration.PrincipleResponsibleTaxNo);
					AssertEquals("Traders", 2, declaration.Traders.Count);
				});

				CombineAssertions("Declaration level case 1 | 316-323", () =>
				{
					AssertEquals("Mail1", "ilker@ulukom.com.tr", declaration.Mail1);
					AssertEquals("Mail2", "mehmet@ulukom.com.tr", declaration.Mail2);
					AssertEquals("Mail3", "yusuf@ulukom.com.tr", declaration.Mail3);
					AssertEquals("Mobil1", "+905322173033", declaration.Mobil1);
					AssertEquals("Mobil2", string.Empty, declaration.Mobil2);
					AssertEquals("OvertimeWorkID", string.Empty, declaration.OvertimeWorkID);
					AssertEquals("PortCode", "TR01M-004", declaration.PortCode);
					AssertEquals("AgencyDispatchNotificationNo", string.Empty, declaration.AgencyDispatchNotificationNo);
				});

				headerJobDeclaration.JE_TransportMode = "SEA";
				headerJobDeclaration.JE_RN_NKTransportNationality = "TR";
				headerJobDeclaration.JE_VesselName = "TK333";
				headerJobDeclaration.JE_TransportMeans = "4";
				headerJobDeclaration.ZG_Box18TransportID = "TK125";
				headerJobDeclaration.ZG_Box18TransportNationality = "AF";
				headerJobDeclaration.Invoices[0].Charges[TRIncotermChargeCodeList.Codes.INT].J7_Amount = 77;
				headerJobDeclaration.Invoices[0].Charges[TRIncotermChargeCodeList.Codes.LocalTotalCharges].J7_Amount = 88;
				headerJobDeclaration.Invoices[0].JZ_RelatedIndicator = "N";
				headerJobDeclaration.JE_PaymentMethod = "P";

				var orgSupplier = headerJobDeclaration.Factory.New<OrgHeader>();
				orgSupplier.OH_Code = "xSupplier2";
				orgSupplier.OH_FullName = "xSupplier2 Full Name";
				orgSupplier.OH_RL_NKClosestPort = "TR";
				var addressSupplier = orgSupplier.MainAddress;
				addressSupplier.OA_OH = orgSupplier.PK;
				addressSupplier.CompanyName = "xSupplier2 Company Name";
				addressSupplier.Address1 = "xSupplierAdress1";
				addressSupplier.Address2 = "xSupplierAdress2";
				addressSupplier.OA_Phone = "02122122691";
				addressSupplier.OA_Fax = "02122122692";
				addressSupplier.City = "ISTANBUL";
				addressSupplier.Postcode = "340301";
				addressSupplier.OA_RN_NKCountryCode = "TR";
				addressSupplier.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, "1212121212121");
				headerJobDeclaration.JE_OH_Supplier = orgSupplier.PK;

				var entryInstruction = headerJobDeclaration.CusEntryInstruction;
				headerJobDeclaration.CusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
				headerJobDeclaration.CusEntryHeader.EntryInstruction.CEI_DateForDuty = new ZDateTime(2020, 7, 21);

				headerJobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc2";

				var declaration2 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 2 | 3-24", () =>
				{
					AssertEquals("TypeOfVehicleOnExit", "3", declaration.TypeOfVehicleOnExit);
					AssertEquals("PlateOfVehicleOnExit", "TK333", declaration.PlateOfVehicleOnExit);
					AssertEquals("CountryOfVehicleOnExit", "052", declaration.CountryOfVehicleOnExit);
					AssertEquals("TypeOfVehicleAtBorder", "4", declaration2.TypeOfVehicleAtBorder);
					AssertEquals("PlateOfVehicleAtBorder", "TK125", declaration2.PlateOfVehicleAtBorder);
					AssertEquals("CountryOfVehicleAtBorder", "660", declaration2.CountryOfVehicleAtBorder);
				});

				CombineAssertions("Declaration level case 2 | 25-56", () =>
				{
					AssertEquals("TotalAbroadExpenditure", 157m, declaration2.TotalAbroadExpenditure);
					AssertEquals("TotalDomesticExpenditure", 1838m, declaration2.TotalDomesticExpenditure);
					AssertEquals("ReferenceDate", "2020-07-21", declaration2.ReferenceDate);
					AssertEquals("InstrumentOfPayment", "P", declaration2.InstrumentOfPayment);
					AssertEquals("BuyerSellerRelationship", "6", declaration.BuyerSellerRelationship);
					AssertEquals("Descriptions", "desc1 desc2 desc3", declaration.Descriptions);
					AssertEquals("ShipperTaxNo", ZString.Empty, declaration.ShipperTaxNo);
					AssertEquals("Traders", 2, declaration.Traders.Count);
				});

				CombineAssertions("Declaration level case 3 | 3-24", () =>
				{
					AssertDifferentProcedureCode(headerJobDeclaration.CusEntryHeader, "8000");
					AssertDifferentProcedureCode(headerJobDeclaration.CusEntryHeader, "8100");
					AssertDifferentProcedureCode(headerJobDeclaration.CusEntryHeader, "8200");
				});
			}
		}

		void AssertDifferentProcedureCode(CusEntryHeader entryHeader, ZString procedureCode)
		{
			var invoiceLine = entryHeader.InvoiceLines.FirstOrDefault();
			invoiceLine.JI_Procedure = procedureCode;
			var declaration = new CusEntryHeaderMessageProvider(entryHeader, TRMessageTypes.Codes.DKO);
			AssertEquals("CountOfLoadingDocuments When Procedure Code:" + procedureCode, ZInt.Zero, declaration.CountOfLoadingDocuments);
			AssertEquals("CountryOfTrade When Procedure Code:" + procedureCode, ZString.Empty, declaration.CountryOfTrade);
		}

		public void TestCusEntryHeaderExportMessageMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetExportProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 1 | 3-24", () =>
				{
					AssertEquals("DeclarationNo", "21340300IM123456", declaration.DeclarationNo);
					AssertEquals("Nature", "4000", declaration.Nature);
					AssertEquals("CustomsOffice", "341200", declaration.CustomsOffice);
					AssertEquals("SimplifiedProcedure", "1", declaration.SimplifiedProcedure);
					AssertEquals("CountOfLoadingDocuments", 2, declaration.CountOfLoadingDocuments);
					AssertEquals("PackQuantity", 20, declaration.PackQuantity);
					AssertEquals("CountryOfTrade", "003", declaration.CountryOfTrade);
					AssertEquals("ReferenceNo", "ORD1234", declaration.ReferenceNo);
					AssertEquals("UnionRecordNumber", "ABC", declaration.UnionRecordNumber);
					AssertEquals("UnionCryptoNumber", "DEF", declaration.UnionCryptoNumber);
					AssertEquals("CountryOfDeparture", ZString.Empty, declaration.CountryOfDeparture);
					AssertEquals("CountryOfDestination", "004", declaration.CountryOfDestination);
					AssertEquals("CountryOfDispatch", "003", declaration.CountryOfDispatch);
					AssertEquals("TypeOfVehicleOnExit", "4", declaration.TypeOfVehicleOnExit);
					AssertEquals("PlateOfVehicleOnExit", "TK321", declaration.PlateOfVehicleOnExit);
					AssertEquals("CountryOfVehicleOnExit", "660", declaration.CountryOfVehicleOnExit);
					AssertEquals("TypeOfDelivery", "FOB", declaration.TypeOfDelivery);
					AssertEquals("PlaceOfDelivery", "ISTANBUL", declaration.PlaceOfDelivery);
					AssertEquals("IsContainer", "EVET", declaration.IsContainer);
					AssertEquals("TypeOfVehicleAtBorder", "3", declaration.TypeOfVehicleAtBorder);
					AssertEquals("PlateOfVehicleAtBorder", "TK123", declaration.PlateOfVehicleAtBorder);
					AssertEquals("CountryOfVehicleAtBorder", "006", declaration.CountryOfVehicleAtBorder);
				});

				CombineAssertions("Declaration level case 1 | 25-56", () =>
				{
					AssertEquals("TotalInvoiceAmount", 1000m, declaration.TotalInvoiceAmount);
					AssertEquals("TotalInvoiceAmountCurrency", "EUR", declaration.TotalInvoiceAmountCurrency);
					AssertEquals("TotalFreight", 100m, declaration.TotalFreight);
					AssertEquals("TotalFreightCurrency", "USD", declaration.TotalFreightCurrency);
					AssertEquals("TypeOfTransportAtBorder", "30", declaration.TypeOfTransportAtBorder);
					AssertEquals("BuyerSellerRelationship", "0", declaration.BuyerSellerRelationship);
					AssertEquals("TotalInsurance", 30m, declaration.TotalInsurance);
					AssertEquals("TotalInsuranceCurrency", "USD", declaration.TotalInsuranceCurrency);
					AssertEquals("LoadingUnloadingPlace", "ERENKÖY GÜMRÜK MÜDÜRLÜĞÜ", declaration.LoadingUnloadingPlace);
					AssertEquals("TotalAbroadExpenditure", 100m, declaration.TotalAbroadExpenditure);
					AssertEquals("TotalAbroadExpenditureCurrency", "EUR", declaration.TotalAbroadExpenditureCurrency);
					AssertEquals("TotalDomesticExpenditure", 2100m, declaration.TotalDomesticExpenditure);
					AssertEquals("BankCode", "000100007026", declaration.BankCode);
					AssertEquals("GoodsLocation", "ZEYTİNBURNU", declaration.GoodsLocation);
					AssertEquals("CustomsOfficeOfDestination", string.Empty, declaration.CustomsOfficeOfDestination);
					AssertEquals("BondedWarehouseCode", "G002", declaration.BondedWarehouseCode);
					AssertEquals("PlannedRoute", string.Empty, declaration.PlannedRoute);
					AssertEquals("CounterVailingDuty", 0m, declaration.CounterVailingDuty);
					AssertEquals("CustomsOfficeOfEntry", "341200", declaration.CustomsOfficeOfEntry);
					AssertEquals("QualificationOfProcess", string.Empty, declaration.QualificationOfProcess);
					AssertEquals("Descriptions", "desc1 desc2 desc3", declaration.Descriptions);
					AssertEquals("UserId", "20201224104", declaration.UserId);
					AssertEquals("ReferenceDate", string.Empty, declaration.ReferenceDate);
					AssertEquals("Payment", "PESIN", declaration.Payment);
					AssertEquals("InstrumentOfPayment", "C", declaration.InstrumentOfPayment);
					AssertEquals("ReferenceOfBrokers", "ULU-2021IM/00000005", declaration.ReferenceOfBrokers);
					AssertEquals("ShipperTaxNo", "8890024379", declaration.ShipperTaxNo);
					AssertEquals("ConsigneeTaxNo", ZString.Empty, declaration.ConsigneeTaxNo);
					AssertEquals("DeclarantTaxNo", "8890024444", declaration.DeclarantTaxNo);
					AssertEquals("AdvisorTaxNo", "8890024555", declaration.AdvisorTaxNo);
					AssertEquals("PrincipleResponsibleTaxNo", string.Empty, declaration.PrincipleResponsibleTaxNo);
					AssertEquals("Traders", 3, declaration.Traders.Count);
				});

				CombineAssertions("Declaration level case 1 | 316-323", () =>
				{
					AssertEquals("Mail1", "ilker@ulukom.com.tr", declaration.Mail1);
					AssertEquals("Mail2", "mehmet@ulukom.com.tr", declaration.Mail2);
					AssertEquals("Mail3", "yusuf@ulukom.com.tr", declaration.Mail3);
					AssertEquals("Mobil1", "+905322173033", declaration.Mobil1);
					AssertEquals("Mobil2", string.Empty, declaration.Mobil2);
					AssertEquals("OvertimeWorkID", string.Empty, declaration.OvertimeWorkID);
					AssertEquals("PortCode", "TR01M-005", declaration.PortCode);
					AssertEquals("AgencyDispatchNotificationNo", string.Empty, declaration.AgencyDispatchNotificationNo);
				});

				headerJobDeclaration.JE_TransportMode = "SEA";
				headerJobDeclaration.JE_RN_NKTransportNationality = "TR";
				headerJobDeclaration.JE_VesselName = "TK333";
				headerJobDeclaration.JE_TransportMeans = "4";
				headerJobDeclaration.ZG_Box18TransportID = "TK125";
				headerJobDeclaration.ZG_Box18TransportNationality = "AF";
				headerJobDeclaration.Invoices[0].Charges[TRIncotermChargeCodeList.Codes.INT].J7_Amount = 77;
				headerJobDeclaration.Invoices[0].Charges[TRIncotermChargeCodeList.Codes.LocalTotalCharges].J7_Amount = 88;
				headerJobDeclaration.Invoices[0].JZ_RelatedIndicator = "N";
				headerJobDeclaration.JE_PaymentMethod = "P";

				headerJobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc2";
				headerJobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc4";

				var entryInstruction = headerJobDeclaration.CusEntryInstruction;
				headerJobDeclaration.CusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
				headerJobDeclaration.CusEntryHeader.EntryInstruction.CEI_DateForDuty = new ZDateTime(2020, 7, 21);

				var declaration2 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 2 | 3-24", () =>
				{
					AssertEquals("TypeOfVehicleOnExit", "3", declaration.TypeOfVehicleOnExit);
					AssertEquals("PlateOfVehicleOnExit", "TK333", declaration.PlateOfVehicleOnExit);
					AssertEquals("CountryOfVehicleOnExit", "052", declaration.CountryOfVehicleOnExit);
					AssertEquals("TypeOfVehicleAtBorder", "4", declaration2.TypeOfVehicleAtBorder);
					AssertEquals("PlateOfVehicleAtBorder", "TK125", declaration2.PlateOfVehicleAtBorder);
					AssertEquals("CountryOfVehicleAtBorder", "660", declaration2.CountryOfVehicleAtBorder);
				});

				CombineAssertions("Declaration level case 2 | 25-56", () =>
				{
					AssertEquals("TotalAbroadExpenditure", 157m, declaration2.TotalAbroadExpenditure);
					AssertEquals("TotalDomesticExpenditure", 1838m, declaration2.TotalDomesticExpenditure);
					AssertEquals("ReferenceDate", "2020-07-21", declaration2.ReferenceDate);
					AssertEquals("InstrumentOfPayment", "P", declaration2.InstrumentOfPayment);
					AssertEquals("BuyerSellerRelationship", "6", declaration.BuyerSellerRelationship);
					AssertEquals("Descriptions", "desc1 desc2 desc3 desc4", declaration.Descriptions);
					AssertEquals("ShipperTaxNo", "8890024379", declaration.ShipperTaxNo);
					AssertEquals("Traders", 3, declaration.Traders.Count);
				});

				headerJobDeclaration.JE_TransportMode = "AIR";
				headerJobDeclaration.JE_TransportModeInland = "10";
				var declaration3 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 3 | AIR - 10 Airline", () =>
				{
					AssertEquals("JE_MessageType", "EXP", headerJobDeclaration.JE_MessageType);
					AssertEquals("JE_TransportMode", "AIR", headerJobDeclaration.JE_TransportMode);
					AssertEquals("JE_TransportModeInland", "10", headerJobDeclaration.JE_TransportModeInland);
					AssertEquals("AgencyDispatchNotificationNo", ZString.Empty, declaration3.AgencyDispatchNotificationNo);
				});

				headerJobDeclaration.JE_TransportModeInland = "40";
				var declaration4 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Declaration level case 4 | AIR - 40 Airline", () =>
				{
					AssertEquals("JE_MessageType", "EXP", headerJobDeclaration.JE_MessageType);
					AssertEquals("JE_TransportMode", "AIR", headerJobDeclaration.JE_TransportMode);
					AssertEquals("JE_TransportModeInland", "40", headerJobDeclaration.JE_TransportModeInland);
					AssertEquals("AgencyDispatchNotificationNo", "24066666SB000001", declaration4.AgencyDispatchNotificationNo);
				});

				headerJobDeclaration.JE_TransportModeInland = "10";
				var declaration5 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DTE);

				CombineAssertions("Declaration level case 3 | AIR - 10 Airline", () =>
				{
					AssertEquals("JE_MessageType", "EXP", headerJobDeclaration.JE_MessageType);
					AssertEquals("JE_TransportMode", "AIR", headerJobDeclaration.JE_TransportMode);
					AssertEquals("JE_TransportModeInland", "10", headerJobDeclaration.JE_TransportModeInland);
					AssertEquals("AgencyDispatchNotificationNo", ZString.Empty, declaration5.AgencyDispatchNotificationNo);
				});

				headerJobDeclaration.JE_TransportModeInland = "40";
				var declaration6 = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DTE);

				CombineAssertions("Declaration level case 4 | AIR - 40 Airline", () =>
				{
					AssertEquals("JE_MessageType", "EXP", headerJobDeclaration.JE_MessageType);
					AssertEquals("JE_TransportMode", "AIR", headerJobDeclaration.JE_TransportMode);
					AssertEquals("JE_TransportModeInland", "40", headerJobDeclaration.JE_TransportModeInland);
					AssertEquals("AgencyDispatchNotificationNo", "24066666SB000001", declaration6.AgencyDispatchNotificationNo);
				});
			}
		}

		public void TestPlateOfVehicleOnExit()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				CombineAssertions(() =>
				{
					var headerJobDeclaration = helper.GetProviderHeader();
					headerJobDeclaration.JE_TransportMode = "AIR";
					headerJobDeclaration.JE_VoyageFlightNo = "TK333";
					headerJobDeclaration.JE_VesselName = "MSC LEVANTE";
					headerJobDeclaration.JE_MessageType = "IMP";

					var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
					AssertEquals("PlateOfVehicleOnExit", "TK333", declaration.PlateOfVehicleOnExit);

					headerJobDeclaration.JE_TransportMode = "SEA";
					headerJobDeclaration.JE_VoyageFlightNo = "TK333";
					headerJobDeclaration.JE_VesselName = "MSC LEVANTE";
					headerJobDeclaration.JE_MessageType = "EXP";
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
					AssertEquals("PlateOfVehicleOnExit", "MSC LEVANTE TK333", declaration.PlateOfVehicleOnExit);

					headerJobDeclaration.JE_TransportMode = "ROA";
					headerJobDeclaration.JE_MessageType = "IMP";
					headerJobDeclaration.JE_VoyageFlightNo = "TK333";
					headerJobDeclaration.JE_VesselName = "MSC LEVANTE";
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
					AssertEquals("PlateOfVehicleOnExit", "MSC LEVANTE", declaration.PlateOfVehicleOnExit);

					headerJobDeclaration.JE_MessageType = string.Empty;
					headerJobDeclaration.JE_VoyageFlightNo = "TK333";
					headerJobDeclaration.JE_VesselName = "MSC LEVANTE";
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
					AssertEquals("PlateOfVehicleOnExit", string.Empty, declaration.PlateOfVehicleOnExit);
				});
			}
		}

		public void TestCusEntryHeaderMessageSubMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				CombineAssertions("Message DKO", () =>
				{
					AssertEquals("Traders", 2, declaration.Traders.Count);
					AssertEquals("Guarantees", 2, declaration.Guarantees.Count);
					AssertEquals("QuestionsAndAnswers", 0, declaration.QuestionsAndAnswers.Count);
					AssertEquals("Documents", 3, declaration.Documents.Count);
					AssertEquals("Taxes", 2, declaration.Taxes.Count);
					AssertEquals("DeclarationOfValue", 1, declaration.DeclarationOfValue.Count);
					AssertEquals("SummaryDeclaration", 3, declaration.SummaryDeclaration.Count);
					AssertEquals("EntryLines", 2, declaration.EntryLines.Count);
				});

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DTE);
				CombineAssertions("Message DTE", () =>
				{
					AssertEquals("Traders", 2, declaration.Traders.Count);
					AssertEquals("Guarantees", 2, declaration.Guarantees.Count);
					AssertEquals("QuestionsAndAnswers", 3, declaration.QuestionsAndAnswers.Count);
					AssertEquals("Documents", 3, declaration.Documents.Count);
					AssertEquals("Taxes", 2, declaration.Taxes.Count);
					AssertEquals("DeclarationOfValue", 1, declaration.DeclarationOfValue.Count);
					AssertEquals("SummaryDeclaration", 3, declaration.SummaryDeclaration.Count);
					AssertEquals("EntryLines", 2, declaration.EntryLines.Count);
				});
			}
		}

		public void TestGetCurrency()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var invoiceLines = headerJobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Single();

				CombineAssertions(() =>
				{
					invoiceLines.Charges.RemoveAll();
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10m);
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m);
					var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
					AssertEquals("When TotalFreight Currency Has Default value", "EUR", declaration.TotalFreightCurrency);
					AssertEquals("When TotalInsurance Currency Has Default value", "EUR", declaration.TotalInsuranceCurrency);

					invoiceLines.Charges.RemoveAll();
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, ZDecimal.Zero, "USD");
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, ZDecimal.Zero, "EUR");
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

					AssertEquals("When TotalFreight Is Empty", ZString.Empty, declaration.TotalFreightCurrency);
					AssertEquals("When TotalInsurance Is Empty", ZString.Empty, declaration.TotalInsuranceCurrency);

					invoiceLines.Charges.RemoveAll();
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 50m, "EUR");
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, "USD");
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

					AssertEquals("When TotalFreight Is Not Empty", "EUR", declaration.TotalFreightCurrency);
					AssertEquals("When TotalInsurance Is Not Empty", "USD", declaration.TotalInsuranceCurrency);

					invoiceLines.Charges.RemoveAll();
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, ZString.Empty);
					invoiceLines.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, ZString.Empty);
					declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

					AssertEquals("When TotalFreight Currency Is Empty", ZString.Empty, declaration.TotalFreightCurrency);
					AssertEquals("When TotalInsurance Currency Is Empty", ZString.Empty, declaration.TotalInsuranceCurrency);
				});
			}
		}
	}
}
