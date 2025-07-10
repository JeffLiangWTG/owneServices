using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class ETradeTemporaryRegistrationMessageProviderTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestIETradeTemporaryRegistrationMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = FillTestData();
				var eTradeTempRegMP = new ETradeTemporaryRegistrationMessageProvider(header) as IETradeTemporaryRegistration;

				CombineAssertions("IETradeTemporaryRegistration Members", () =>
				{
					AssertEquals("ReferenceNoToUpdate", "REG0123456", eTradeTempRegMP.ReferenceNoToUpdate);
					AssertEquals("Declaration1", "IM", eTradeTempRegMP.Declaration1);
					AssertEquals("Declaration2", "4", eTradeTempRegMP.Declaration2);
					AssertEquals("Declaration3", "ET", eTradeTempRegMP.Declaration3);
					AssertEquals("TotalLineCount", 1, eTradeTempRegMP.TotalLineCount);
					AssertEquals("TotalBoxQty", 3, eTradeTempRegMP.TotalBoxQty);
					AssertEquals("ReferenceNo", ZString.Empty, eTradeTempRegMP.ReferenceNo);
					AssertEquals("DeclaringRepresentativeNameAndTitle", "TestCompanyName", eTradeTempRegMP.DeclaringRepresentativeNameAndTitle);
					AssertEquals("DeclaringRepresentativeTCTaxNo", "test567890", eTradeTempRegMP.DeclaringRepresentativeTCTaxNo);
					AssertEquals("TypeOfVehicleOnExit", "5", eTradeTempRegMP.TypeOfVehicleOnExit);
					AssertEquals("VehiclePlateOfVehicleOnExit", "235-5738 2474", eTradeTempRegMP.VehiclePlateOfVehicleOnExit);
					AssertEquals("CountryCodeOfVehicleOnExit", "011", eTradeTempRegMP.CountryCodeOfVehicleOnExit);
					AssertEquals("IsContainer", ZBool.False, eTradeTempRegMP.IsContainer);
					AssertEquals("TypeOfVehicleOnBorder", "5", eTradeTempRegMP.TypeOfVehicleOnBorder);
					AssertEquals("VehiclePlateOfVehicleOnBorder", "VOYAGE", eTradeTempRegMP.VehiclePlateOfVehicleOnBorder);
					AssertEquals("CountryCodeOfVehicleOnBorder", "011", eTradeTempRegMP.CountryCodeOfVehicleOnBorder);
					AssertEquals("TotalInvoiceCurrencyType", "EUR", eTradeTempRegMP.TotalInvoiceCurrencyCode);
					AssertEquals("TotalInvoiceCurrencyValue", 444.44m, eTradeTempRegMP.TotalInvoiceCurrencyValue);
					AssertEquals("TotalInvoiceExchangeRate", 9.000000m, eTradeTempRegMP.TotalInvoiceExchangeRate);
					AssertEquals("InvoiceAmountInformationTurkishLira", 3999.96m, eTradeTempRegMP.InvoiceAmountInformationTurkishLira);
					AssertEquals("StatisticalValue", 500.00m, eTradeTempRegMP.StatisticalValue);
					AssertEquals("TotalExpensesFreightCurrencyType", "EUR", eTradeTempRegMP.TotalExpensesFreightCurrencyCode);
					AssertEquals("TotalExpensesFreightCurrencyValue", 200.00m, eTradeTempRegMP.TotalExpensesFreightCurrencyValue);
					AssertEquals("TotalExpensesInsuranceCurrencyType", "EUR", eTradeTempRegMP.TotalExpensesInsuranceCurrencyCode);
					AssertEquals("TotalExpensesInsuranceCurrencyValue", 44.44m, eTradeTempRegMP.TotalExpensesInsuranceCurrencyValue);
					AssertEquals("OtherOverseasExpenditureCurrencyType", ZString.Empty, eTradeTempRegMP.OtherOverseasExpenditureCurrencyCode);
					AssertEquals("OtherOverseasExpenditureCurrencyValue", ZDecimal.Zero, eTradeTempRegMP.OtherOverseasExpenditureCurrencyValue);
					AssertEquals("DomesticExpenditures", 50.00m, eTradeTempRegMP.DomesticExpenditures);
					AssertEquals("TransportTypeCode", "40", eTradeTempRegMP.TransportTypeCode);
					AssertEquals("CustomsOfficeCodeOfPredicted", "341453", eTradeTempRegMP.CustomsOfficeCodeOfPredicted);
					AssertEquals("CustomsOfficeCodeOfEntryExit", "341453", eTradeTempRegMP.CustomsOfficeCodeOfEntryExit);
					AssertEquals("GoodsLocationCode", "G34000015", eTradeTempRegMP.GoodsLocationCode);
					AssertEquals("GoodsLocationName", "LocationDesc", eTradeTempRegMP.GoodsLocationName);
					AssertEquals("Adjustment", ZString.Empty, eTradeTempRegMP.Adjustment);
					AssertEquals("WarehouseTypeCode", "G34000015", eTradeTempRegMP.WarehouseTypeCode);
					AssertEquals("PrincipleResponsibleNameAndTitle", "CardHolderName", eTradeTempRegMP.PrincipleResponsibleNameAndTitle);
					AssertEquals("PrincipleResponsibleTCTaxNo", "1234tst890", eTradeTempRegMP.PrincipleResponsibleTCTaxNo);
					AssertEquals("CustomsOfficeCodeOfPredicted", "341453", eTradeTempRegMP.CustomsOfficeCodeOfPredicted);
					AssertEquals("CustomsOfficeCodeOfDestination", "341453", eTradeTempRegMP.CustomsOfficeCodeOfDestination);
					AssertEquals("TransfersCountryCode", "052", eTradeTempRegMP.TransfersCountryCode);
					AssertEquals("TransfersPlace", "TransLoc", eTradeTempRegMP.TransfersPlace);
					AssertEquals("TransfersNewVehicleReferanceNumber", "123456", eTradeTempRegMP.TransfersNewVehicleReferanceNumber);
					AssertEquals("TransfersNewCountryCode", "052", eTradeTempRegMP.TransfersNewCountryCode);
					AssertEquals("TransfersContainer", ZBool.False, eTradeTempRegMP.IsTransfersContainer);
					AssertEquals("TransfersPreviousContainerNo", "PreContNo", eTradeTempRegMP.TransfersPreviousContainerNo);
					AssertEquals("TransfersNewContainerNo", "NewContNo", eTradeTempRegMP.TransfersNewContainerNo);
					AssertEquals("Explanations", ZString.Empty, eTradeTempRegMP.Explanations);
					AssertEquals("TotalGuaranteesType", "BANKA", eTradeTempRegMP.TotalGuaranteesType);
					AssertEquals("TotalGuaranteesValue", 1.23m, eTradeTempRegMP.TotalGuaranteesValue);
				});

				foreach (var bol in eTradeTempRegMP.Bills)
				{
					AssertIBillMembers(bol);
				}
			}
		}

		void AssertIBillMembers(IBill bill)
		{
			CombineAssertions(() =>
			{
				AssertEquals("BillOfLadingLineNo", "1", bill.BillOfLadingLineNo);
				AssertEquals("BillOfLadingNumber", "ABC111222333", bill.BillOfLadingNumber);
				AssertEquals("SummaryDeclarationNo", "TR340300OZ123456", bill.SummaryDeclarationNo);
				AssertEquals("PackType", "BI", bill.PackType);
				AssertEquals("PackQuantity", 3, bill.PackQuantity);
				AssertEquals("GrossWeight", 500m, bill.GrossWeight);
				AssertEquals("NetWeight", 450m, bill.NetWeight);
				AssertEquals("ForwarderNameAndTitle", "xShipper Name", bill.ForwarderNameAndTitle);
				AssertEquals("ForwarderTCTaxNo", "1234567893", bill.ForwarderTCTaxNo);
				AssertEquals("ConsigneeName", "xConsignee Name", bill.ConsigneeName);
				AssertEquals("ConsigneeTCTaxNo", "1234567890", bill.ConsigneeTCTaxNo);
				AssertEquals("ConsigneeStreetNumber", "Address1Address2", bill.ConsigneeStreetNumber);
				AssertEquals("ConsigneeCityCode", "ISTANBUL", bill.ConsigneeCityCode);
				AssertEquals("ConsigneeTown", "Afyon", bill.ConsigneeTown);
				AssertEquals("ConsigneePostalCode", "340300", bill.ConsigneePostalCode);
				AssertEquals("MarketPlaceNameAndTitle", "Amazon78901234567890123456789012345", bill.MarketPlaceNameAndTitle);
				AssertEquals("MarketPlaceTCTaxNo", "1234567878", bill.MarketPlaceTCTaxNo);
				AssertEquals("ReferralDestinationCountryCode", "052", bill.DestinationCountryCode);
				AssertEquals("DestinationCountryCode", "052", bill.DestinationCountryCode);
				AssertEquals("TradeCountryCode", "052", bill.TradeCountryCode);
				AssertEquals("ExportCountryCode", "001", bill.ExportCountryCode);
				AssertEquals("DeliveryMethod", "CIF", bill.DeliveryMethod);
				AssertEquals("DeliverLocation", "G34000015", bill.DeliverLocation);
				AssertEquals("TransactionNature", "12", bill.TransactionNature);
				AssertEquals("RegimeCode", "4000", bill.RegimeCode);
				AssertEquals("FinancialBankingCode", ZString.Empty, bill.FinancialBankingCode);
				AssertEquals("FinancialBankingPaymentType", "B", bill.FinancialBankingPaymentType);
				AssertEquals("FinancialBankingAmount", ZDecimal.Zero, bill.FinancialBankingAmount);
				AssertEquals("InvoiceCurrencyCode", "EUR", bill.InvoiceCurrencyCode);
				AssertEquals("InvoiceAmount", 444.44m, bill.InvoiceAmount);
				AssertEquals("InvoiceExchangeRate", 9.000000m, bill.InvoiceExchangeRate);
				AssertEquals("TaxPaymentTotalTaxPaymentAmount", 898.20m, bill.TaxPaymentTotalTaxPaymentAmount);
				AssertEquals("TaxPaymentTaxAmountToBeBonded", 5.00m, bill.TaxPaymentTaxAmountToBeBonded);
				AssertEquals("TaxPaymentTotalTaxAmountPayableLater", 3.00m, bill.TaxPaymentTotalTaxAmountPayableLater);
				AssertEquals("TaxPaymentTotal", 1023.20m, bill.TaxPaymentTotal);
				AssertEquals("FreightInformationType", "EUR", bill.FreightInformationCurrencyCode);
				AssertEquals("FreightInformationValue", 200.00m, bill.FreightInformationValue);
				AssertEquals("InsuranceInformationType", "USD", bill.InsuranceInformationCurrencyCode);
				AssertEquals("InsuranceInformationValue", 50.00m, bill.InsuranceInformationValue);
				AssertEquals("OtherOverseasExpansesType", ZString.Empty, bill.OtherOverseasExpansesCurrencyCode);
				AssertEquals("OtherOverseasExpansesValue", ZDecimal.Zero, bill.OtherOverseasExpansesValue);
				AssertEquals("DomesticExpanses", ZDecimal.Zero, bill.DomesticExpanses);
				AssertEquals("ContainerBrand", "BRANDNAME", bill.ContainerBrand);
				AssertEquals("ContainerNo", "CNTR000001", bill.ContainerNo);
				AssertEquals("ContainerPackType", "BI", bill.ContainerPackType);
				AssertEquals("ContainerPackQuantity", 3, bill.ContainerPackQuantity);
				AssertEquals("ExceptionCode1", "HK18", bill.ExceptionCode1);
				AssertEquals("ExceptionCode2", "KTP8", bill.ExceptionCode2);
				AssertEquals("TradeType", "ET", bill.TradeType);
			});

			foreach (var tax in bill.Taxes)
			{
				var taxCode = tax.Code;
				switch (taxCode)
				{
					case "10":
						AssertITaxMembersForTaxCode10(tax);
						break;
					case "89":
						AssertITaxMembersForTaxCode89(tax);
						break;
					case "40":
						AssertITaxMembersForTaxCode40(tax);
						break;
				}
			}

			foreach (var doc in bill.Documents)
			{
				if (doc.Code == "0100")
				{
					AssertIDocumentMembersForCode0100(doc);
				}
				else
				{
					AssertIDocumentMembersForCode0200(doc);
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals("GuaranteeType", "BANKA", bill.GuaranteeType);
				AssertEquals("GuaranteeAmount", 1.24m, bill.GuaranteeAmount);
				AssertEquals("GuaranteeReferenceNo", "4545656", bill.GuaranteeReferenceNo);
			});

			foreach (var pack in bill.Packs)
			{
				AssertIPackMembers(pack);
			}
		}

		void AssertIDocumentMembersForCode0100(IDocument doc)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", "0100", doc.Code);
				AssertEquals("DocumentDate", ZDateTime.Today, doc.DocumentDate);
				AssertEquals("ReferanceNo", "123654", doc.ReferenceNo);
				AssertEquals("Verfication", "V", doc.Verfication);
			});
		}

		void AssertIDocumentMembersForCode0200(IDocument doc)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", "0200", doc.Code);
				AssertEquals("DocumentDate", ZDateTime.Today, doc.DocumentDate);
				AssertEquals("ReferanceNo", "123650", doc.ReferenceNo);
				AssertEquals("Verfication", "Y", doc.Verfication);
			});
		}

		void AssertITaxMembersForTaxCode10(ITax tax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", "10", tax.Code);
				AssertEquals("Description", "Gümrük Vergisi", tax.Description);
				AssertEquals("Base", 50.00m, tax.Base);
				AssertEquals("Rate", 10.00m, tax.Rate);
				AssertEquals("Amount", 5.00m, tax.Amount);
				AssertEquals("PaymentType", "P", tax.PaymentType);
			});
		}

		void AssertITaxMembersForTaxCode89(ITax tax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", "89", tax.Code);
				AssertEquals("Description", "Damga Vergisi", tax.Description);
				AssertEquals("Base", ZDecimal.Zero, tax.Base);
				AssertEquals("Rate", ZDecimal.Zero, tax.Rate);
				AssertEquals("Amount", 898.20m, tax.Amount);
				AssertEquals("PaymentType", "P", tax.PaymentType);
			});
		}

		void AssertITaxMembersForTaxCode40(ITax tax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", "40", tax.Code);
				AssertEquals("Description", "Katma Değer Vergisi", tax.Description);
				AssertEquals("Base", 30.00m, tax.Base);
				AssertEquals("Rate", 10.00m, tax.Rate);
				AssertEquals("Amount", 3.00m, tax.Amount);
				AssertEquals("PaymentType", "P", tax.PaymentType);
			});
		}

		void AssertIPackMembers(IPack pack)
		{
			CombineAssertions(() =>
			{
				AssertEquals("PackNo", "1", pack.PackNo);
				AssertEquals("CommercialDescription", "XGOOD1", pack.CommercialDescription);
				AssertEquals("ItemsSerialNo", "SERIAL1", pack.ItemsSerialNo);
				AssertEquals("ItemsQuantity", 10, pack.ItemsQuantity);
				AssertEquals("ItemsBrand", "BRAND1", pack.ItemsBrand);
				AssertEquals("ItemsModel", "MODEL1", pack.ItemsModel);
				AssertEquals("UsedItemCode", "USED1", pack.UsedItemCode);
				AssertEquals("ItemCode1", "12345678", pack.ItemCode1);
				AssertEquals("ItemCode2", "90", pack.ItemCode2);
				AssertEquals("ItemCode3", "12", pack.ItemCode3);
				AssertEquals("PreferentialTariffCode1", ZString.Empty, pack.PreferentialTariffCode1);
				AssertEquals("PreferentialTariffCode2", ZString.Empty, pack.PreferentialTariffCode2);
				AssertEquals("ValueStatementForm", "DECFORM1", pack.ValueStatementForm);
				AssertEquals("AgriculturePolicy", "AGRIPOL1", pack.AgriculturePolicy);
				AssertEquals("IsQuota", ZBool.False, pack.IsQuota);
				AssertEquals("BillAmountCurrencyCode", "USD", pack.BillAmountCurrencyCode);
				AssertEquals("BillAmountValue", 500.00m, pack.BillAmountValue);
				AssertEquals("CalculationMethod", "B", pack.CalculationMethod);
				AssertEquals("StatisticalValue", 500.00m, pack.StatisticalValue);
				AssertEquals("CountryCodeOfOrigin", "052", pack.CountryCodeOfOrigin);
				AssertEquals("SupplementaryMeasuresType1", "KG", pack.SupplementaryMeasuresType1);
				AssertEquals("SupplementaryMeasuresQuantity1", 10.00m, pack.SupplementaryMeasuresQuantity1);
				AssertEquals("SupplementaryMeasuresType2", "KG", pack.SupplementaryMeasuresType2);
				AssertEquals("SupplementaryMeasuresQuantity2", 20.00m, pack.SupplementaryMeasuresQuantity2);
			});
		}

		public void TestCountryCodesWhenNatureChanges()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			header.AMA_Nature = "IMP";
			var bill = header.Bills.AddNew();
			bill.ExportCountry = "FR";
			bill.TradeCountry = "DE";
			bill.DepartureCountry = "ES";
			bill.ArrivalCountry = "TR";

			var proivder = new ETradeTemporaryRegistrationMessageProvider(header) as IETradeTemporaryRegistration;

			foreach (var b in proivder.Bills)
			{
				CombineAssertions(() =>
				{
					AssertEquals("ReferralDestinationCountryCode", "011", b.ReferralDestinationCountryCode);
					AssertEquals("TradeCountryCode", "004", b.TradeCountryCode);
					AssertEquals("ExportCountryCode", "001", b.ExportCountryCode);
					AssertEquals("DestinationCountryCode", "052", b.DestinationCountryCode);
				});
			}

			header.AMA_Nature = "EXP";
			proivder = new ETradeTemporaryRegistrationMessageProvider(header);

			foreach (var b in proivder.Bills)
			{
				CombineAssertions(() =>
				{
					AssertEquals("ReferralDestinationCountryCode", "001", b.ReferralDestinationCountryCode);
					AssertEquals("TradeCountryCode", "004", b.TradeCountryCode);
					AssertEquals("ExportCountryCode", "011", b.ExportCountryCode);
					AssertEquals("DestinationCountryCode", "052", b.DestinationCountryCode);
				});
			}
		}

		public void TestFreightValueAndCurrencyCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetExistingRefSysConfigType("TREPREFRCO", "TR E-Trade Precedent Freight Cost Value", "There is a Precedent Freight Cost Value for TR Customs E-Trade. The limit currency code is EUR");
			helper.CreateOrUpdateExistingRefSysConfig("TREPREFRCO", 3, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			header.AMA_Nature = "IMP";
			var bill = header.Bills.AddNew();
			bill.ABL_Incoterm = "FOB";
			bill.ABL_RX_NKTransportValueCurrency = ZString.Empty;
			bill.ABL_TransportValue = ZDecimal.Zero;

			var provider = new ETradeTemporaryRegistrationMessageProvider(header) as IETradeTemporaryRegistration;
			var iBill = provider.Bills.FirstOrDefault();
			CombineAssertions("Not Entered Freight Value", () =>
			{
				AssertEquals("FreightInformationType", "EUR", iBill.FreightInformationCurrencyCode);
				AssertEquals("FreightInformationValue", 3m, iBill.FreightInformationValue);
			});

			bill.ABL_RX_NKTransportValueCurrency = "EUR";
			bill.ABL_TransportValue = 10m;
			provider = new ETradeTemporaryRegistrationMessageProvider(header);
			iBill = provider.Bills.FirstOrDefault();
			CombineAssertions("Entered Freight Value", () =>
			{
				AssertEquals("FreightInformationType", "EUR", iBill.FreightInformationCurrencyCode);
				AssertEquals("FreightInformationValue", 10m, iBill.FreightInformationValue);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "TR", "052", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "FR", "001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "ES", "011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "DE", "004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateTaxOrFee("89", 898.20m, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Stamp Duty");
			Factory.Save();
		}

		AsycudaManifestHeader FillTestData()
		{
			#region CompanyAndUser

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			glbCompany.GC_Code = "t1";
			glbCompany.GC_Name = "TestGCName";
			glbCompany.CompanyName = "TestCompanyName";
			glbCompany.Address1 = "Test adress 1";
			glbCompany.Address2 = "Test adress 2";
			glbCompany.GC_BusinessRegNo = "test567890";

			var glbBranch = glbCompany.Branches.AddNew();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_Code = "Ts1";

			var loggedInUser = Factory.NewWithValidTestData<GlbStaff>();
			loggedInUser.GS_Code = "ULU";
			loggedInUser.GS_FullName = "CardHolderName";
			var extPassword = Factory.New<GlbExternalPassword_TR>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TRK;
			extPassword.GP_GC = glbCompany.PK;
			extPassword.GP_UserID = "1234tst890";
			extPassword.GP_GS = loggedInUser.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Env.SetUserContext(new UserContext(loggedInUser, glbBranch.PK.ToGuid(), Env.CurrentDepartment.PK));

			#endregion

			#region AsycudaManifestHeader

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.AMA_GB = glbBranch.PK;
			header.TempRegNo = "REG0123456";
			header.AMA_JobReference = "MAN0000001";
			header.AMA_TransportMode = "AIR";
			header.AMA_Nature = "IMP";
			header.ProcedureCode = "4000";
			header.DepartureFlight = "235-5738 2474";
			header.DepartureCountryCode = "TR";
			header.AMA_ContainerMode = "";
			header.AMA_Voyage = "VOYAGE";
			header.AMA_RN_NKConveyanceNationality = "ES";
			header.DischargeLoadingCustomsOffice = "TR341453";
			header.ImportExportCustomsOffice = "TR341453";
			header.GoodsLocationCode = "G34000015";
			header.LocationInformation = "LocationDesc";
			header.PresentationCustomsOffice = "TR341453";
			header.AMA_CustomsOffice = "TR341453";
			header.TransshipmentCountry = "TR";
			header.TransshipmentLocation = "TransLoc";
			header.TransshipmentReference = "123456";
			header.TransshipmentConveyanceCountry = "TR";
			header.PreviousContainerNo = "PreContNo";
			header.NewContainerNo = "NewContNo";
			header.GuaranteeType = "BANKA";
			header.GuaranteeAmount = 1.234567890;

			var helper = new CurrencyTestHelper(Factory);
			helper.SetExchangeRate(helper.EURCurrency, 9m, ZDateTime.Today);
			helper.SetExchangeRate(helper.USDCurrency, 8m, ZDateTime.Today);
			helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

			header.Branch.Company.GC_IsReciprocal = true;

			#endregion

			#region Bills

			var orgHeaderMarketPlace = Factory.New<OrgHeader>();
			orgHeaderMarketPlace.OH_Code = "AMZCODE";
			orgHeaderMarketPlace.OH_FullName = "Amazon7890123456789012345678901234567890";
			var orgAddressMarketPlace = orgHeaderMarketPlace.MainAddress;
			orgAddressMarketPlace.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567878");

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "ABC111222333";
			bill.CustomsEntryNumber = "TR340300OZ123456";
			bill.ABL_ManifestQty = 3;
			bill.ABL_ShipperRegNo = "1234567893";
			bill.ABL_ShipperName = "xShipper Name";
			bill.ABL_ConsigneeName = "xConsignee Name";
			bill.ABL_ConsigneeRegNo = "1234567890";
			bill.ABL_ConsigneeStreet1 = "Address1";
			bill.ABL_ConsigneeStreet2 = "Address2";
			bill.ABL_RN_NKConsigneeCountry = "TR";
			bill.ABL_ConsigneeCity = "ISTANBUL";
			bill.ABL_ConsigneeState = "03";
			bill.ABL_ConsigneePostcode = "340300";
			bill.ABL_OA_NotifyParty = orgAddressMarketPlace.PK;
			bill.ArrivalCountry = "TR";
			bill.TradeCountry = "TR";
			bill.ExportCountry = "FR";
			bill.DepartureCountry = "TR";
			bill.ABL_Incoterm = "CIF";
			bill.ABL_GoodsLocation = "G34000015";
			bill.NatureOfBusiness = "12";
			bill.ABL_Procedure = "4000";
			bill.PaymentMethod = "B";
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			bill.ABL_GoodsValue = 500;
			bill.ABL_RX_NKCustomsValueCurrency = "EUR";
			bill.ABL_CustomsValue = 500;
			bill.ABL_RX_NKTransportValueCurrency = "EUR";
			bill.ABL_TransportValue = 200;
			bill.TaxAmount = 125;
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.ABL_InsuranceValue = 50;
			bill.ABL_OtherValue = 50;
			bill.ExemptionCode1 = "HK18";
			bill.ExemptionCode2 = "KTP8";
			bill.GuaranteeType = "BANKA";
			bill.GuaranteeAmount = 1.23987654;
			bill.GuaranteeRefNo = "4545656";
			bill.ABL_MarksAndNumbers = "BRANDNAME";
			bill.ContainerNumber = "CNTR000001";
			bill.ABL_GrossWeight = 500;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_NetWeight = 450;
			bill.ABL_NetWeightUQ = "KG";
			bill.ABL_SpecialCargoCode = "ET";

			#endregion

			#region Packs

			var packs = bill.Packs.AddNew();
			packs.APA_LineNo = 1;
			packs.APA_GoodsDescription = "XGOOD1";
			packs.PackedItem.SerialNo = "SERIAL1";
			packs.PackedItem.API_CustomsQty = 10;
			packs.PackedItem.API_Brand = "BRAND1";
			packs.PackedItem.API_Model = "MODEL1";
			packs.PackedItem.UsedGoodsCode = "USED1";
			packs.PackedItem.API_Tariff = "1234567890123456";
			packs.PackedItem.ValueDeclarationForm = "DECFORM1";
			packs.PackedItem.AgriculturePolicy = "AGRIPOL1";
			packs.PackedItem.API_CustomsUQ2 = "KG";
			packs.PackedItem.API_CustomsQty2 = 10;
			packs.PackedItem.API_CustomsUQ3 = "KG";
			packs.PackedItem.API_CustomsQty3 = 20;
			packs.PackedItem.API_RX_NKGoodsValueCurrency = "USD";
			packs.PackedItem.API_GoodsValue = 500;
			packs.PackedItem.CalculationMethod = "B";
			packs.PackedItem.API_RN_NKGoodsOrigin = "TR";

			#endregion

			#region Taxs

			header.CalculateDuties();

			var taxs1 = bill.AsycudaTaxes.AddNew();
			taxs1.AET_ChargeType = "10";
			taxs1.AET_BaseValue = 50;
			taxs1.AET_Rate = 10;
			taxs1.AET_ChargeAmount = 5;
			taxs1.AET_MethodOfPayment = "GUA";

			var taxs2 = bill.AsycudaTaxes.AddNew();
			taxs2.AET_ChargeType = "40";
			taxs2.AET_BaseValue = 30;
			taxs2.AET_Rate = 10;
			taxs2.AET_ChargeAmount = 3;
			taxs2.AET_MethodOfPayment = "DFF";

			#endregion

			#region Docs

			var docs1 = bill.SupportingDocumentsForBill.AddNew();
			docs1.CSI_Code = "0100";
			docs1.CSI_DateOfIssue = ZDateTime.Today;
			docs1.CSI_ReferenceNumber = "123654";
			docs1.CSI_Status = "EXS";

			var docs2 = bill.SupportingDocumentsForBill.AddNew();
			docs2.CSI_Code = "0200";
			docs2.CSI_DateOfIssue = ZDateTime.Today;
			docs2.CSI_ReferenceNumber = "123650";
			docs2.CSI_Status = ZString.Empty;

			#endregion

			Factory.Save();
			return header;
		}
	}
}
