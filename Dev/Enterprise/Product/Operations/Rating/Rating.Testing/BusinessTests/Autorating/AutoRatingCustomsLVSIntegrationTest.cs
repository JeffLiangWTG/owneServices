using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using CAIntegration = Enterprise.Integration.Customs.CA;

namespace Enterprise.Rating.Testing.GUI
{
	public class AutoRatingCustomsLVSIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestLVSAutoRatingNoCurrencyOverride()
		{
			AssertLVSAutoRating(ZString.Empty);
		}

		public void TestLVSAutoRatingNoCurrencyOverrideAndDetail()
		{
			AssertLVSAutoRating(ZString.Empty, testImporterDetail: true);
		}

		public void TestLVSAutoRatingWithCurrencyOverride()
		{
			AssertLVSAutoRating(CurrencyCodes.NewZealand);
		}

		public void TestLVSAutoRatingWithCurrencyOverrideAndDetail()
		{
			AssertLVSAutoRating(CurrencyCodes.NewZealand, testImporterDetail: true);
		}

		public void AssertLVSAutoRating(ZString currencyCode, bool testImporterDetail = false)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "0497", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ON");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0821", "0821", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "BC");
			Factory.Save();

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CUSDSB");
			Helper.Factory.Save();

			var originalIsGSTRegisteredValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var zone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "HSTC"));
			zone.FZ_IsActive = false;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			{
				ObjectFactory.Get<CAIntegration.ICACustomsDataRegistry>().DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				if (!currencyCode.IsEmpty)
				{
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = currencyCode;
				}
				RatingDataRegistry.Instance.HideValueBreakdownInDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Factory.Save();

				CreateCAGSTRateCode(Factory, 16m);

				var importer1 = Factory.New<OrgHeader>();
				importer1.OH_Code = "IMP1";
				importer1.OH_IsConsignee = true;
				importer1.OH_IsDebtor = true;
				importer1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				importer1.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

				var importer2 = Factory.New<OrgHeader>();
				importer2.OH_Code = "IMP2";
				importer2.OH_IsConsignee = true;
				importer2.OH_IsDebtor = true;
				importer2.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				importer2.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				if (testImporterDetail)
				{
					var countryData = importer2.GetCountryData(Core.Constants.CountryCodes.Canada);
					importer2.RegisterEditableChildObject(countryData);
					((CAIntegration.IOrgImpAddInfo)(countryData.ImpAddInfo)).ZO_LVSInvoiceDetailCode = "DET";
				}

				var importer3 = Factory.New<OrgHeader>();
				importer3.OH_Code = "IMP3";
				importer3.OH_IsConsignee = true;
				importer3.OH_IsDebtor = true;
				importer3.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				importer3.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

				var billTo = Factory.New<OrgHeader>();
				billTo.OH_Code = "BILLTO";
				billTo.OH_IsConsignee = true;
				billTo.OH_IsDebtor = true;
				billTo.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				billTo.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

				importer3.SetRelatedParty(billTo, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, TransportModes.All, ZString.Empty);

				var agBillTo = Factory.New<OrgHeader>();
				agBillTo.OH_Code = "AGENTBT";
				agBillTo.OH_IsConsignee = true;
				agBillTo.OH_IsDebtor = true;
				agBillTo.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				agBillTo.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

				var agent = Factory.New<OrgHeader>();
				agent.OH_Code = "AGENT";
				agent.OH_IsConsignee = true;
				agent.OH_IsDebtor = true;
				agent.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
				agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

				agent.SetRelatedParty(agBillTo, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.PickupAndDelivery, TransportModes.All, ZString.Empty);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = CAJobMessageTypeList.Codes.LowValueShipments;
				declaration.JE_MessageSubType = "VAR";
				((CAIntegration.IJobDeclaration)declaration).UpdateTransactionNumber("12345", "1");
				declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "LVSIDA";
				invoice1.JZ_OH_Buyer = importer1.PK;
				invoice1.InvoiceLines.AddNew();
				invoice1.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice1).CA_PortOfClearance = "0497";
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "LVSIDB";
				invoice2.JZ_OH_Buyer = importer1.PK;
				invoice2.InvoiceLines.AddNew();
				invoice2.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice2).CA_PortOfClearance = "0497";
				var invoice3 = declaration.Invoices.AddNew();
				invoice3.JZ_InvoiceNumber = "LVSIDC";
				invoice3.JZ_OH_Buyer = importer1.PK;
				invoice3.InvoiceLines.AddNew();
				invoice3.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice3).CA_PortOfClearance = "0497";
				var invoice4 = declaration.Invoices.AddNew();
				invoice4.JZ_RX_NKInvoice_Currency = CurrencyCodes.Canada;
				invoice4.JZ_InvoiceNumber = "LVSIDD";
				invoice4.JZ_OH_Buyer = importer2.PK;
				invoice4.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice4).CA_PortOfClearance = "0497";
				FillInvoiceLine(invoice4.JobComInvoiceLines.AddNew(), 1, 1, 4000, 11, 12, 13, Weight.Kilograms, 16);
				var invoice5 = declaration.Invoices.AddNew();
				invoice5.JZ_RX_NKInvoice_Currency = CurrencyCodes.Canada;
				invoice5.JZ_InvoiceNumber = "LVSIDE";
				invoice5.JZ_OH_Buyer = importer2.PK;
				invoice5.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice5).CA_PortOfClearance = "0497";
				FillInvoiceLine(invoice5.JobComInvoiceLines.AddNew(), 1, 1, 4500, 11, 12, 13, Weight.Kilograms, 16);
				var invoice6 = declaration.Invoices.AddNew();
				invoice6.JZ_InvoiceNumber = "LVSIDF";
				invoice6.JZ_OH_Buyer = importer3.PK;
				invoice6.InvoiceLines.AddNew();
				invoice6.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice6).CA_PortOfClearance = "0497";

				var invoice7 = declaration.Invoices.AddNew();
				invoice7.JZ_RX_NKInvoice_Currency = CurrencyCodes.Canada;
				invoice7.JZ_InvoiceNumber = "LVSID1";
				invoice7.JZ_OH_Buyer = importer1.PK;
				invoice7.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice7).CA_PortOfClearance = "0497";
				FillInvoiceLine(invoice7.JobComInvoiceLines.AddNew(), 1, 1, 3000, 11, 12, 13, Weight.Kilograms, 16);
				FillInvoiceLine(invoice7.JobComInvoiceLines.AddNew(), 2, 1, 3000, 11, 12, 13, Weight.Kilograms, 16);
				var invoice8 = declaration.Invoices.AddNew();
				invoice8.JZ_RX_NKInvoice_Currency = CurrencyCodes.Canada;
				invoice8.JZ_InvoiceNumber = "LVSID2";
				invoice8.JZ_OH_Buyer = importer1.PK;
				invoice8.JZ_IncoTerm = "DDP";
				((CAIntegration.IJobComInvoiceHeader)invoice8).CA_PortOfClearance = "0497";
				FillInvoiceLine(invoice8.JobComInvoiceLines.AddNew(), 1, 1, 3500, 11, 12, 13, Weight.Kilograms, 16);
				FillInvoiceLine(invoice8.JobComInvoiceLines.AddNew(), 2, 1, 3500, 11, 12, 13, Weight.Kilograms, 16);
				var invoice9 = declaration.Invoices.AddNew();
				invoice9.JZ_RX_NKInvoice_Currency = CurrencyCodes.Canada;
				invoice9.JZ_InvoiceNumber = "LVSID3";
				invoice9.JZ_OH_Buyer = importer1.PK;
				invoice9.JZ_IncoTerm = "DDP";
				((CAIntegration.IJobComInvoiceHeader)invoice9).CA_PortOfClearance = "0497";
				FillInvoiceLine(invoice9.JobComInvoiceLines.AddNew(), 1, 1, 3500, 11, 12, 13, Weight.Kilograms, 16);
				FillInvoiceLine(invoice9.JobComInvoiceLines.AddNew(), 2, 1, 3500, 11, 12, 13, Weight.Kilograms, 16);
				var invoice10 = declaration.Invoices.AddNew();
				invoice10.JZ_InvoiceNumber = "LVSID4";
				invoice10.JZ_OH_Buyer = ZGuid.Empty;
				invoice10.InvoiceLines.AddNew();
				invoice10.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice10).CA_PortOfClearance = "0497";
				var invoice11 = declaration.Invoices.AddNew();
				invoice11.JZ_InvoiceNumber = "LVSID5";
				invoice11.JZ_OH_Buyer = importer1.PK;
				invoice11.InvoiceLines.AddNew();
				invoice11.JZ_IncoTerm = "FOB";
				((CAIntegration.IJobComInvoiceHeader)invoice11).CA_PortOfClearance = "0821";

				declaration.ResumeApportionment();
				declaration.DoMerge();

				var agencyChargeCode = Factory.New<AccChargeCode>();
				agencyChargeCode.AC_Code = "AGY";
				agencyChargeCode.AC_Desc = "AGENCY";
				agencyChargeCode.AC_RateCalculator = AgencyCalculator.Code;
				agencyChargeCode.AC_ChargeGroup = "BRK";
				agencyChargeCode.AC_ChargeType = ChargeType.ManualJobAccrual;
				agencyChargeCode.AC_DepartmentFilterList = "ALL";

				var header = Factory.NewWithValidTestData<CompanyTariff>();
				header.TH_Accepted = ZDateTime.Now.AddDays(-10);
				header.TH_GlobalRateDescription = "BASE COMPANY RATES";
				header.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
				header.TH_GlobalRateLevel = 1;
				var entry = header.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "");
				entry.TI_RH_NKCommodityCode = "GEN";
				entry.TI_RX_NKCurrency = CurrencyCodes.Canada;
				entry.TI_RateStartDate = ZDate.Today.AddDays(-10);
				entry.TI_RateEndDate = ZDate.Empty;
				var line = entry.RateLines.AddNew();
				line.TL_RateCalculator = AgencyCalculator.Code;
				line.TL_CompanyTariffLevel = 1;
				line.TL_RX_NKCurrency = CurrencyCodes.Canada;
				line.TL_AC = agencyChargeCode.PK;
				AddParityExchangeRate(line.Currency);

				var calculator = (AgencyCalculator)line.Calculator;
				calculator.AgencyFeeType = "INV";
				calculator.AgencyRate = 25;
				calculator.IncludedHeaders = 1;
				calculator.AdditionalRate = 25;
				calculator.MessageType = "LVS";
				calculator.MessageSubType = "VAR";
				calculator.AgencyLineType = "FLT";

				if (!currencyCode.IsEmpty)
				{
					var sellRate = line.Currency.ExchangeRates.AddNew();
					sellRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
					sellRate.RE_StartDate = ZDateTime.Today.AddMonths(-14);
					sellRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(14);
					sellRate.RE_RX_NKExCurrency = currencyCode;
					sellRate.RE_SellRate = 1m;

					var costRate = line.Currency.ExchangeRates.AddNew();
					costRate.RE_ExRateType = ExchangeRateTypes.Code.BuyRate;
					costRate.RE_StartDate = ZDateTime.Today.AddMonths(-14);
					costRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(14);
					sellRate.RE_RX_NKExCurrency = currencyCode;
					costRate.RE_SellRate = 1m;
				}

				Factory.Save();

				var baseExpected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 100m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSID1, LVSIDA, LVSIDB, LVSIDC
  Province of Clearance: ON",
								SellAccountCode = importer1.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 25m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSID5
  Province of Clearance: BC",
								SellAccountCode = importer1.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "CUSDSB",
								JR_OSSellAmt = 2672.16m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"Customs Disbursement Charges - 12345000000012
  LVS IDs: LVSID1",
								SellAccountCode = importer1.OH_Code,
								JR_RX_NKCostCurrency = "CAD",
								JR_OSCostAmt = 2672.16m,
								CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
							},
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 25m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSIDF
  Province of Clearance: ON",
								SellAccountCode = billTo.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 50m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSID2, LVSID3
  Importer: IMP1
  Province of Clearance: ON",
								SellAccountCode = agBillTo.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "CUSDSB",
								JR_OSSellAmt = 4443.24m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"Customs Disbursement Charges - 12345000000012
  LVS IDs: LVSID2, LVSID3
  Importer: IMP1",
								SellAccountCode = agBillTo.OH_Code,
								JR_RX_NKCostCurrency = "CAD",
								JR_OSCostAmt = 4443.24m,
								CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
							},
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 25m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSID4
  Province of Clearance: ON",
								SellAccountCode = GlbBranch.CurrentBranch.OrgProxy?.OH_Code ?? ZString.Empty,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							}
					};

				var additionalExpected = Array.Empty<AssertionCharge>();

				if (testImporterDetail)
				{
					additionalExpected = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 25m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012 - LVSIDD
  Province of Clearance: ON",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 25m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012 - LVSIDE
  Province of Clearance: ON",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "CUSDSB",
								JR_OSSellAmt = 1704.88m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"Customs Disbursement Charges - 12345000000012 - LVSIDD",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = "CAD",
								JR_OSCostAmt = 1704.88m,
								CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
							},
							new AssertionCharge
							{
								ChargeCode = "CUSDSB",
								JR_OSSellAmt = 1889.28m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"Customs Disbursement Charges - 12345000000012 - LVSIDE",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = "CAD",
								JR_OSCostAmt = 1889.28m,
								CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
							}
						};
				}
				else
				{
					additionalExpected = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "AGY",
								JR_OSSellAmt = 50m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"AGENCY - 12345000000012
  LVS IDs: LVSIDD, LVSIDE
  Province of Clearance: ON",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = currencyCode.IsEmpty ? "CAD" : "NZD",
								JR_OSCostAmt = 0m,
							},
							new AssertionCharge
							{
								ChargeCode = "CUSDSB",
								JR_OSSellAmt = 3594.16m,
								JR_RX_NKSellCurrency = "CAD",
								JR_Desc = @"Customs Disbursement Charges - 12345000000012
  LVS IDs: LVSIDD, LVSIDE",
								SellAccountCode = importer2.OH_Code,
								JR_RX_NKCostCurrency = "CAD",
								JR_OSCostAmt = 3594.16m,
								CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
							}
						};
				}

				AutorateAndAssert(baseExpected.Concat(additionalExpected), declaration, GlbBranch.CurrentBranch.OrgProxy, agent);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegisteredValue;
		}

		static void CreateCAGSTRateCode(BusinessObjectFactory factory, ZDecimal rateValue)
		{
			CreateCACClassHeader();

			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			var gst1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "001", "001 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			gst1.Attributes.Where(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate)).DeleteAll();
			var rateType = universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, "V");
			var rate = universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "5.00");
			rate.ZZE_Value = rateValue.ToString("0.00");
			factory.Save();
		}

		public static void CreateCACClassHeader()
		{
			var sqlText = string.Format(@$"
INSERT INTO dbo.RefDbEntCA_CACClassHeader (ZA_PK, ZA_ClassificationNumber, ZA_EffectiveDate, ZA_ExpiryDate, ZA_AreaCode, ZA_ClassAuthorityNumber,ZA_TariffEffectiveDate,ZA_TariffExpiryDate)
							values('0B21F0ED-7D59-4EB3-ABD5-66ABE3F48C6E', '8466939090', '2009-08-01', '2079-06-05', '900','2009-06-CPFTA','2009-08-01', '2079-06-05');
INSERT INTO dbo.RefDbEntCA_CACRateHeader (ZB_PK, ZB_ZA_ClassHeader, ZB_EffectiveDate, ZB_ExpiryDate, ZB_FreeInd, ZB_UnitOfMeasure, ZB_Inactive,ZB_DutyRateAuthorityNumber,ZB_RateType)
							values('15B44ECC-31F7-4907-8CC4-EB1B77AE238F', '0B21F0ED-7D59-4EB3-ABD5-66ABE3F48C6E', '2009-08-01', '2079-06-05', 'N','GRM','N', '', 'EXS');
INSERT INTO dbo.RefDbEntCA_CACRateHeader (ZB_PK, ZB_ZA_ClassHeader, ZB_EffectiveDate, ZB_ExpiryDate, ZB_FreeInd, ZB_UnitOfMeasure, ZB_Inactive,ZB_DutyRateAuthorityNumber,ZB_RateType)
							values('DA8464D4-0D38-432D-B9DA-005E613D16E2', '0B21F0ED-7D59-4EB3-ABD5-66ABE3F48C6E', '2009-08-01', '2079-06-05', 'N','','N', '', 'CLS');
INSERT INTO dbo.RefDbEntCA_CACRate (ZC_PK, ZC_ParentID, ZC_TreatmentCode, ZC_ParentTableCode)
							values('95722FB2-1C64-4472-B733-0BE429814FEA', '15B44ECC-31F7-4907-8CC4-EB1B77AE238F', '','ZB');
INSERT INTO dbo.RefDbEntCA_CACRate (ZC_PK, ZC_ParentID, ZC_TreatmentCode, ZC_ParentTableCode)
							values('E7E325A3-1B91-45DC-9901-68894ACBE251', 'DA8464D4-0D38-432D-B9DA-005E613D16E2', '02','ZB');
INSERT INTO dbo.RefDbEntCA_CACRateLine (ZR_PK, ZR_ZC_Rate, ZR_DutyRateType, ZR_DutyRateRegular)
							values(newid(), '95722FB2-1C64-4472-B733-0BE429814FEA', 'S','{14m}');
INSERT INTO dbo.RefDbEntCA_CACRateLine (ZR_PK, ZR_ZC_Rate, ZR_DutyRateType, ZR_DutyRateRegular)
							values(newid(), 'E7E325A3-1B91-45DC-9901-68894ACBE251', 'V','{18m}');
INSERT INTO dbo.RefDbEntCA_CACRateLine (ZR_PK, ZR_ZC_Rate, ZR_DutyRateType, ZR_DutyRateRegular)
							values(newid(), 'E7E325A3-1B91-45DC-9901-68894ACBE251', 'F','{0m}');

");
			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.ExecuteNonQuery();
			}
		}

		static void FillInvoiceLine(BaseJobComInvoiceLine line, short number, int pageNumber, decimal linePrice, decimal quantity1, decimal quantity2, decimal quantity3, string weightUQ, decimal sima = 0)
		{
			line.JI_LineNo = 1;
			line.JI_Tariff = "8466.93.90 90";
			line.JI_LinePrice = linePrice;
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			line.JI_LineNo = number;
			line.JI_CustomsUnitQty = "LTR";
			line.JI_CustomsQuantity = quantity1;
			line.JI_CustomsSecondUnitQty = "KGM";
			line.JI_CustomsSecondQuantity = quantity2;
			line.JI_CustomsThirdUnitQty = "GRM";
			line.JI_CustomsThirdQuantity = quantity3;
			line.JI_Description = "NUMBER DESCRIPTION UP TO 39 CHARACTERS1";

			if (sima > 0 && line is CAIntegration.IJobComInvoiceLine caLine)
			{
				var simaDuty = caLine.DutiesAndTaxes.FirstOrDefault() ?? caLine.DutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = "ADD";
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C31;
				simaDuty.C1_Amount = sima;
				simaDuty.B7_ParentID = line.PK;
				simaDuty.B7_ParentTableCode = line.TablePrefix;
			}
		}
	}
}
