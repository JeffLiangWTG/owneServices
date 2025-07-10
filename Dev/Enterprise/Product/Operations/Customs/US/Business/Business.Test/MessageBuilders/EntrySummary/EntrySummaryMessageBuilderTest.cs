using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryMessageBuilderTest : EntrySummaryMessageBuilderAbstractTest
	{
		[TestDate(2008, 12, 31)]
		public void Test14()
		{
			/*
			14)  6106100010                 TARIFF NUMBER
					 XO                         PROVINCE OF ORIGIN
					 YCANNNNNN                  CANADIAN TPL CERTIFICATE NUMBER

					 NOTE:  PUT CURRENT YEAR (I.E. '7' FOR 2007) IN "Y"
					 AND RANDOM NUMBERS IN "N".
			*/

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceHeader.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.US_SupTariff = "99990050";  // as per email with Phyllis
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			//line price was set to be zero
			manufacturerCode.OK_CustomsRegNo = "XOUBFFOO2421OAK";
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerCode.OK_OA_PremisesAddress;

			invoiceLine.US_MiscPermitNo = (ZDate.Today.Year % 10).ToString() + "CA123456";
			invoiceLine.JI_Tariff = "6106100010";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("14");

			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];
			AssertEquals("Total Duty", 1970m, ens90.TotalEstimatedDuty);
			AssertEquals("Total Tax", 0m, ens90.GrandTotalEstimatedTax);
			AssertEquals("Total Fee", 37.5m, ens90.GrandTotalFeeAmount);
			AssertEquals("Total Entry Value", 10000m, ens90.TotalValueOfEntrySummary);
		}

		public void Test21()
		{
			/*21)  1701125000          TARIFF NUMBER                                 
			CO                         COUNTRY OF ORIGIN 
			*/
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_SupTariff = "99041711";
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CO";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "1701125000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 58000m;
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_Weight = 10m;
			declaration.JE_MasterBill = "OBL21";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			MergeAndSend("21", true, false);

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = (MQEDIMessage)entry.Messages[0];
			List<MessageBlock> blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(AENSOI));
			AssertEquals("FDA should be declared under ENS 70", 1, blocks.Count);

			int indexOf70 = message.MessageBlock.MessageBlocks.FindIndex(x => x.GetType() == typeof(ENS70));
			int indexOfOI = message.MessageBlock.MessageBlocks.FindIndex(x => x.GetType() == typeof(AENSOI));

			Assert(indexOfOI > indexOf70);
		}

		[TestDate(2021, 12, 01)]
		public void Test27()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;

			invoiceHeader.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SupTariff = "99130410";
			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			invoiceLine.US_MiscPermitNo = (ZDate.Today.Year % 10).ToString() + "AU123456";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceLine.JI_Tariff = "0405102000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 100m;
			declaration.JE_MasterBill = "OBL27";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			MergeAndSend("27");
		}

		[TestDate(2016, 03, 27)]
		public void Test29()
		{
			/*
29) 99034105 TARIFF NUMBER 1
	4107111020 TARIFF NUMBER 2
	10000 ENTERED VALUE
	11 MODE OF TRANSPORTATION
	1800 M2 QUANTITY
	JP COUNTRY OF ORIGIN/EXPORT
			 */

			invoiceLine.US_SupTariff = "99034105";
			invoiceLine.JI_Tariff = "4107111020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.JI_CustomsQuantity = 1800m;
			invoiceLine.JI_Weight = 100m;

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_RL_NKPortOfLoading = "JPTYO";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			MergeAndSend("29");

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = (MQEDIMessage)ensEntry.Messages[0];
			Assert(!message.MessageBlock.MessageBlocks.OfType<ENS51>().Any());
		}

		[TestDate(2008, 12, 31)]
		public void Test37()
		{
			/*
			37) 99034110 TARIFF NUMBER 1
			6404191560 TARIFF NUMBER 2
			JP COUNTRY OF ORIGIN
			1000 QUANTITY
			*/
			invoiceLine.US_SupTariff = "99034110";
			invoiceLine.JI_Tariff = "6404191560";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			MergeAndSend("37");
			AssertEquals("Duty", 5050m, helper.GetTypes<ENS90>(declaration)[0].TotalEstimatedDuty);
		}

		[TestDate(2016, 03, 27)]
		public void Test68()
		{
			/*--------------------------------------------------------------------
68) 9802005060 TARIFF NUMBER 1, LINE 1
210150 ENTERED VALUE TARIFF NUMBER 1
9106100000 TARIFF NUMBER 2, LINE 1
110000 ENTERED VALUE TARIFF NUMBER 2
12000 QUANTITY 1 TARIFF NUMBER 2
48000 QUANTITY 2 TARIFF NUMBER 2
40 MODE OF TRANSPORTATION*/

			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.US_98GoodsValue = 210150m;
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.JI_Tariff = "9106100000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 110000m;
			invoiceLine.JI_CustomsQuantity = 12000m;
			invoiceLine.JI_CustomsSecondQuantity = 48000m;
			invoiceLine.JI_CustomsThirdQuantity = 10m;
			invoiceLine.JI_Weight = 100m;
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirContainer;

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "630";

			scacCode.OK_CustomsRegNo = "QF";
			declaration.US_UI_NKCarrierSCAC = "QF";
			declaration.Bills[0].US_UI_NKBillIssuerSCAC = "QF";

			MergeAndSend("68");
			ENS40 ens40 = helper.GetTypes<ENS40>(declaration)[0];
			ENS50 ens50 = helper.GetTypes<ENS50>(declaration)[0];
			ENS70 ens70 = helper.GetTypes<ENS70>(declaration)[0];
			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];

			AssertEquals("ens40.Value", 210150m, ens40.Value);
			AssertEquals("ens50.Duty", 0m, ens50.Duty);
			ens50.CountryOfExport = "FR";

			AssertEquals("ens70.Quantity1", 12000m, ens70.Quantity1);
			AssertEquals("ens70.Quantity2", 48000m, ens70.Quantity2);
			AssertEquals("ens70.Value", 110000m, ens70.Value);
			AssertEquals("ens70.Duty", 7973.90m, ens70.Duty);

			AssertEquals("Total Duty", 7973.90m, ens90.TotalEstimatedDuty);
			AssertEquals("Total Tax", 0m, ens90.GrandTotalEstimatedTax);
			AssertEquals("Total Fee", 0m, ens90.GrandTotalFeeAmount);
			AssertEquals("Total Entry Value", 320150m, ens90.TotalValueOfEntrySummary);
		}

		[TestDate(2016, 03, 27)]
		public void Test72()
		{
			/*---------------------------------------------------------------------
72) 98191127 TARIFF NUMBER ONE
6304920000 TARIFF NUMBER 2
ZM COUNTRY OF ORIGIN*/

			invoiceLine.US_SupTariff = "98191127";
			invoiceLine.JI_Tariff = "6304920000";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceHeader.US_UC_NKCountryOfOrigin = "ZM";
			var cv = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "ZM");
			cv.UC_SpecialTradeProgramsIndicator = "D";
			cv.UC_SpecialTradeProgramsBeginDate = ZDateTime.Today.AddYears(-1);
			cv.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddYears(1);
			//Extra bits to make test pass
			declaration.JE_RL_NKPortOfLoading = "ZMBBZ";
			invoiceLine.US_VisaNo = "9ZM123456";
			invoiceLine.JI_LinePrice = 10000m;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "ZMBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);

			MergeAndSend("72");
		}

		[TestDate(2008, 12, 31)]
		public void Test90()
		{
			/*---------------------------------------------------------------------
90) 98191124 TARIFF NUMBER
ZA COUNTRY OF ORIGIN
6204624021 ALTERNATE TARIFF NUMBER
4564 ENTERED VALUE*/

			invoiceHeader.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_SupTariff = "98191124";//AGOA Textile Claims
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 4564m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.US_VisaNo = "8ZA123456";//should start with 8
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "ZABEREQU6LON", Core.Constants.CountryCodes.UnitedStates);

			MergeAndSend("90");
		}

		[TestDate(2016, 03, 27)]
		public void Test93()
		{
			/*---------------------------------------------------------------------
93) YOU ARE PREPARING AN ENTRY FOR 1000 WATCHES THAT WERE EXPORTED TO
GREENLAND FOR REPAIRS(9802004040). THE WATCHES ARE COMPOSED OF
MOVEMENTS(9102111010), CASES(9102111020), BRACELETS(9102111030)
AND BATTERIES(9102111040). REPAIRS WERE PERFORMED ONLY ON THE
MOVEMENTS. REPAIRS TO THE MOVEMENTS ARE VALUED AT $3406 OUT OF
A TOTAL VALUE OF $5258. THE CASES ARE VALUED AT $2619, THE
BRACELETS AT $1345 AND THE BATTERIES AT $204. SEND THE ENTRY
SUMMARY TO ABI.*/

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.US_SupTariff = "9802004040";    // repairs
				invoiceLine.US_98GoodsValue = 3406m;
				invoiceLine.US_UC_NKCountryOfOrigin = "FR";
				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine.JI_LinePrice = 5258m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				invoiceLine.JI_Weight = 500m;

				JobComInvoiceLine secondLine = invoiceLine.AddSecondaryInvoiceLine();
				secondLine.US_SupTariff = "9802004040"; // repairs
				secondLine.US_98GoodsValue = 2619m;
				secondLine.JI_Tariff = "9102111020";
				secondLine.JI_Description = "COMMERCIAL DESCRIPTION";
				secondLine.JI_CustomsQuantity = 1000m;
				secondLine.JI_Weight = 500m;
				secondLine.JI_LinePrice = 1m;

				JobComInvoiceLine thirdLine = invoiceLine.AddSecondaryInvoiceLine();
				thirdLine.US_SupTariff = "9802004040";  // repairs
				thirdLine.US_98GoodsValue = 1345m;
				thirdLine.JI_Tariff = "9102111030";
				thirdLine.JI_Description = "COMMERCIAL DESCRIPTION";
				thirdLine.JI_CustomsQuantity = 1000m;
				thirdLine.JI_Weight = 500m;
				thirdLine.JI_LinePrice = 1m;

				JobComInvoiceLine fourthLine = invoiceLine.AddSecondaryInvoiceLine();
				fourthLine.US_SupTariff = "9802004040"; // repairs
				fourthLine.US_98GoodsValue = 204m;
				fourthLine.JI_Tariff = "9102111040";
				fourthLine.JI_Description = "COMMERCIAL DESCRIPTION";
				fourthLine.JI_CustomsQuantity = 1000m;
				fourthLine.JI_Weight = 100m;
				fourthLine.JI_LinePrice = 1m;

				MergeAndSend("93");
			}

			ENS40 ens40 = helper.GetTypes<ENS40>(declaration)[0];
			AssertEquals(3406m, ens40.Value);

			ENS50 ens50 = helper.GetTypes<ENS50>(declaration)[0];
			AssertEquals("9802004040", ens50.TariffNumber1);
			AssertEquals(0m, ens50.Duty);

			List<ENS70> ens70s = helper.GetTypes<ENS70>(declaration);
			AssertEquals(1, ens70s.Count);
			AssertEquals("9102111010", ens70s[0].TariffNumber2);
			AssertEquals(5258m, ens70s[0].Value);
			AssertEquals(1000m, ens70s[0].Quantity1);
			AssertEquals(326.26m, ens70s[0].Duty);

			List<ENS80> ens80s = helper.GetTypes<ENS80>(declaration);
			AssertEquals(1, ens80s.Count);
			AssertEquals("9802004040", ens80s[0].TariffNumber3);
			AssertEquals(2619m, ens80s[0].Value);
			AssertEquals(0m, ens80s[0].Quantity1);

			List<ENS81> ens81s = helper.GetTypes<ENS81>(declaration);
			AssertEquals(5, ens81s.Count);
			AssertEquals("9102111020", ens81s[0].AdditionalTariffNumber);
			AssertEquals("9802004040", ens81s[1].AdditionalTariffNumber);
			AssertEquals(0m, ens81s[1].Quantity1);

			AssertEquals(1345m, ens81s[1].Value);
			AssertEquals(0m, ens81s[1].Duty);
			AssertEquals("9102111030", ens81s[2].AdditionalTariffNumber);
			AssertEquals("9802004040", ens81s[3].AdditionalTariffNumber);
			AssertEquals(0m, ens81s[3].Quantity1);
			AssertEquals(204m, ens81s[3].Value);
			AssertEquals(0m, ens81s[3].Duty);
			AssertEquals(0m, ens81s[3].Quantity1);

			AssertEquals("9102111040", ens81s[4].AdditionalTariffNumber);
			AssertEquals(1m, ens81s[4].Value);
			AssertEquals(0.06m, ens81s[4].Duty);
			AssertEquals(1000m, ens81s[4].Quantity1);

			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];
			AssertEquals("Total Duty", 326.44m, ens90.TotalEstimatedDuty);
			AssertEquals("Total Tax", 0m, ens90.GrandTotalEstimatedTax);
			AssertEquals("Total Fee", 16.04m, ens90.GrandTotalFeeAmount);
			AssertEquals("Total Entry Value", 12835m, ens90.TotalValueOfEntrySummary);
		}

		[TestDate(2016, 03, 27)]
		public void Test95()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("L888");
			/*---------------------------------------------------------------------
95) 98130075 TARIFF NUMBER 1
8703230090 TARIFF NUMBER 2
23 ENTRY TYPE CODE
11 MODE OF TRANSPORTATION
500000 ENTERED VALUE
1 QUANTITY*/

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "L888";
			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(1);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceLine.US_SupTariff = "98130075";
			invoiceLine.JI_Tariff = "8703230090";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 500000m;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = "01";
			dot.US_DOTClarCode = "E";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("95");
		}

		[TestDate(2008, 12, 31)]
		public void TestWithSPI_P()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_Tariff = "0406.30.2800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			invoiceLine.US_UC_NKCountryOfExport = "HN";
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.CountryOfOrigin_US.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.JE_MasterBill = "OBLSPIP";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("SPI P");
			AssertEquals("Total Duty Calculated", 184.05m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2007, 3, 19)]
		public void Test01()
		{
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AddFCCData(invoiceLine);
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 3, 19);
			MergeAndSend("1");
		}

		[TestDate(2008, 12, 31)]
		public void Test02()
		{
			/*
02) 6203424051                  TARIFF NUMBER
	EG                          COUNTRY OF ORIGIN/EXPORT
	YES                         ELIGIBLE FOR US-ISRAEL FREE TRADE IMPLEMENTATION ACT
			 */

			invoiceLine.JI_Tariff = "6203424051";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceHeader.US_UC_NKCountryOfExport = "EG";
			invoiceLine.US_UC_NKCountryOfOrigin = "EG";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.N;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "EGBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("2");
		}

		[TestDate(2016, 03, 27)]
		public void Test03()
		{
			/*
03) YES                         LIVE ENTRY (INDICATE LIVE ENTRY)
	HK                          COUNTRY OF ORIGIN
	3506990000                  TARIFF NUMBER
			 */

			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			MergeAndSend("3");
		}

		[TestDate(2016, 03, 27)]
		public void Test04()
		{
			/*
	04) 91-013199000                IMPORTER NUMBER
			891                         SURETY CODE
			9                           BOND TYPE CODE
			150,000                     BOND AMOUNT
			AB12345678                  BOND ACCOUNT CODE
			*/

			// Importer set in setup 91-013199000
			declaration.US_BondType = "9";
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			declaration.US_SuretyCode = "891";
			declaration.US_BondAmount = 150000m;
			declaration.US_BondProducerAccNo = "AB12345678";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("4");
		}

		[TestDate(2008, 12, 31)]
		public void Test05()
		{
			/*05) 99025115                    TARIFF NUMBER 1
						5112113060                  TARIFF NUMBER 2
						W#####A##                   WOOL LICENSE NUMBER
			 
						FILL IN NUMBERS IN LICENSE NUMBER WHERE # APPEAR*/

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceLine.US_SupTariff = "99025115";
			invoiceLine.US_WoolLicenceNo = "W" + (ZDate.Today.Year % 100).ToString().PadLeft(2, '0') + "345A67";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_Tariff = "5112113060";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 13000m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_CustomsQuantity = 555.56m;
			invoiceLine.JI_Weight = 100m;
			MergeAndSend("5");
			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];
			AssertEquals("Total Duty", 0m, ens90.TotalEstimatedDuty);
			AssertEquals("Total Tax", 0m, ens90.GrandTotalEstimatedTax);
			AssertEquals("Total Fee", 43.55m, ens90.GrandTotalFeeAmount);
			AssertEquals("Total Entry Value", 13000m, ens90.TotalValueOfEntrySummary);
		}

		[TestDate(2016, 03, 27)]
		public void Test06()
		{
			/*
06) CN                         COUNTRY OF ORIGIN
	5701104000                 TARIFF NUMBER
	"M"                        SPECIAL PROGRAM INDICATOR
			*/
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "5701104000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			invoiceLine.US_SecondarySPI = "M";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "CNBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("6");
		}

		[TestDate(2007, 6, 1)]
		public void Test08()
		{
			/*08) CO                    COUNTRY OF ORIGIN
			98211119                    TARIFF NUMBER 1
			6212109020                  TARIFF NUMBER 2
			//#AN######                   ATPDEA CERTIFICATE NUMBER */

			//invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			//invoiceHeader.US_UC_NKCountryOfExport = "CA";
			//manufacturerCode.OK_CustomsRegNo = "XCKOAMOU381VAN";
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "COBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			declaration.JE_MasterBill = "OBL8";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			declaration.JE_RL_NKPortOfLoading = "COABC";
			invoiceLine.US_UC_NKCountryOfOrigin = "CO";
			invoiceLine.US_SupTariff = "98211119";
			invoiceLine.US_MiscPermitNo = "1AN234567";
			invoiceLine.JI_Tariff = "6212109020";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 6, 1);
			MergeAndSend("8");
			helper.MessageMustContainElement<ENS70>(declaration);
			AssertEquals("Duty", 0m, helper.GetTypes<ENS90>(declaration)[0].TotalEstimatedDuty);
		}

		[TestDate(2016, 03, 27)]
		public void Test10()
		{
			declaration.JE_MasterBill = "OBL10";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			declaration.JE_RL_NKPortOfLoading = "HNGJA";
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.CountryOfOrigin_US.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			MergeAndSend("10");
		}

		[TestDate(2016, 03, 27)]
		public void Test12()
		{
			/*
12) 01/25/YY                    DATE OF IMPORT
	02/10/YY                    ESTIMATED DATE OF ARRIVAL
	02/01/YY                    IT DATE
	115581395                   I.T. NUMBER
	YY=CURRENT YEAR
			 */

			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			declaration.JE_DateOfArrival = new ZDateTime(ZDate.Today.Year, 1, 25);
			declaration.US_EntryDate = new ZDateTime(ZDate.Today.Year, 2, 10);
			declaration.US_ITDate = new ZDateTime(ZDate.Today.Year, 2, 1);
			declaration.PrimaryMasterBill.ITNumber = "115581395";
			declaration.US_SchDEntry = "1101";

			declaration.JE_ExportDate = new ZDateTime(ZDate.Today.Year, 1, 15);
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("12");
		}

		[TestDate(2008, 03, 27)]
		public void Test13()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("L888");
			/*
		13) 0804506040                  TARIFF NUMBER
				PE                          COUNTRY OF ORIGIN AND EXPORT
				THIS TARIFF NUMBER REQUIRES PRIOR NOTICE DATA.
			*/
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_Tariff = "0804506040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 7, 2);//to satisfy the entry date restriction
			invoiceLine.US_UC_NKCountryOfOrigin = "PE";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.US_US_NKLocationOfGoods = "L888";
			invoiceHeader.US_UC_NKCountryOfExport = "PE";

			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.JE_MasterBill = "OBL13";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			invoiceLine.FDAs[0].US_UC_NKFDAProduction = "PE";
			invoiceLine.FDAs[0].US_FDAProductCode = "21SYB05";

			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			manufacturerCode.OK_CustomsRegNo = "PE" + manufacturerCode.OK_CustomsRegNo.SubstringSafe(2);

			invoiceHeader.US_FDAContactName = "John";

			OrgHeader importer = invoiceHeader.Importer;
			OrgHeader supplier = manufacturer;
			if (supplier.BuyerLinks.Count == 0)
			{
				OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew();
				link.OL_OH_Buyer = importer.PK;
				link.OL_RelatedParty = "Y";
			}

			ZString oldCountryOfOrigin = invoiceHeader.US_UC_NKCountryOfOrigin;
			ZString oldCountryOfExport = invoiceHeader.US_UC_NKCountryOfExport;
			invoiceHeader.JZ_OH_Supplier = manufacturer.PK;
			invoiceHeader.US_UC_NKCountryOfOrigin = oldCountryOfOrigin;
			invoiceHeader.US_UC_NKCountryOfExport = oldCountryOfExport;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("13", true, false);

			helper.MessageMustContainElement<OGAFD01>(declaration);
			helper.MessageMustContainElement<OGAFD02>(declaration);
			helper.MessageMustContainElement<OGAFD03>(declaration);
			helper.MessageMustContainElement<OGAFD04>(declaration);
			helper.MessageMustContainElement<OGAFD05>(declaration);
		}

		[TestDate(2009, 6, 1)]
		public void Test15()
		{
			/*
15) 8206000000 TARIFF NUMBER 1, PART 1
8203206030 TARIFF NUMBER 2, PART 2
6880 ENTERED VALUE
TW COUNTRY OF ORIGIN
55000 PCS QUANTITY
			 */

			invoiceLine.JI_Tariff = "8206000000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.JI_InvoiceQuantity = 55000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 55000m;
			invoiceLine.JI_Weight = 500m;
			invoiceLine.JI_LinePrice = 0m;

			JobComInvoiceLine secondaryTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = "8203206030";
			secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			secondaryTariff.JI_CustomsQuantity = 4583m; // 55000 / 12
			secondaryTariff.JI_Weight = 100m;
			secondaryTariff.JI_LinePrice = 6880m;
			MergeAndSend("15");

			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];
			AssertEquals("Total Duty", 928.36m, ens90.TotalEstimatedDuty);
			AssertEquals("Fee calculated", 14.45m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2016, 03, 27)]
		public void Test16()
		{
			/*16) 111271845 I.T. NUMBER
			9786543 BILL OF LADING (MASTER)
			15075 HOUSE BILL
			H273 SUB HOUSE BILL(1)
			5 CTNS SUB HOUSE BILL(1) QUANTITY
			J878 SUB HOUSE BILL(2)
			10 CTNS SUB HOUSE BILL(2) QUANTITY*/

			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			declaration.PrimaryMasterBill.CU_MasterBill = "9786543";
			declaration.JE_HouseBill = "15075";
			declaration.JE_MasterBillIssuerSCAC = "MAEU";
			declaration.JE_HouseBillIssuerSCAC = "MAEU";
			declaration.US_ITDate = ZDateTime.Today.AddDays(-1);
			declaration.US_SchDEntry = "1101";

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill1.CU_BillNum = "H273";
			bill1.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;
			bill1.CU_NoOfPacks = 5;
			bill1.CU_PackType = ShippingOrPackingingUnitList.Codes.Package;
			bill1.ITNumber = "111271845";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill2.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;
			bill2.CU_BillNum = "J878";
			bill2.CU_NoOfPacks = 10;
			bill2.CU_PackType = ShippingOrPackingingUnitList.Codes.Package;
			bill2.ITNumber = "342342346";

			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			invoiceHeader2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m);
			invoiceHeader2.JZ_OA_ManufacturerAddress = invoiceHeader.JZ_OA_ManufacturerAddress;
			invoiceHeader2.JZ_InvoiceAmount = invoiceHeader.JZ_InvoiceAmount;
			invoiceHeader2.US_UC_NKCountryOfOrigin = invoiceHeader.US_UC_NKCountryOfOrigin;
			invoiceHeader2.US_UC_NKCountryOfExport = invoiceHeader.US_UC_NKCountryOfExport;
			invoiceHeader2.US_TransactionsRelated = "N";
			JobComInvoiceLine line2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			line2.JI_InvoiceQuantity = 10m;

			line2.JI_Tariff = invoiceLine.JI_Tariff;
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_LinePrice = invoiceLine.JI_LinePrice;
			line2.US_UC_NKCountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
			line2.JI_CustomsQuantity = invoiceLine.JI_CustomsQuantity;
			line2.JI_Weight = 500m;
			line2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("16");
			ZString messageText = declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText;
			Assert("Message must contain IT number", messageText.Contains("111271845"));
			Assert("Message must contain master bill", messageText.Contains("9786543"));
			Assert("Message must contain house bill", messageText.Contains("15075"));
			Assert("Message must contain sub house bill 1", messageText.Contains("H273"));
			Assert("Message must contain sub house bill 2", messageText.Contains("J878"));
		}

		[TestDate(2016, 03, 27)]
		public void Test18()
		{
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirContainer;

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "QF210";
			declaration.Bills[0].US_UI_NKBillIssuerSCAC = "*F";
			declaration.US_UI_NKCarrierSCAC = "*F";
			scacCode.OK_CustomsRegNo = "*F";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("18");
		}

		[TestDate(2016, 03, 27)]
		public void Test19()
		{
			/*
			19) 4823600020 TARIFF NUMBER
					200 VALUE
					95-002725100 IMPORTER NUMBER
					8 BOND TYPE
					353 SURETY
					11 ENTRY TYPE
					YES CERTIFICATION FOR PAPERLESS
			*/
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);

			while (declaration.US_PreliminaryStatementPrintDateInfo.HasMessageErrors())
			{
				declaration.US_PreliminaryStatementPrintDate = declaration.US_PreliminaryStatementPrintDate.AddDays(1);
			}

			invoiceLine.JI_Tariff = "4823690020";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 200m;
			declaration.JE_MasterBill = "OBL19";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			importerCustomsCode.OK_CustomsRegNo = "95-002725100";
			declaration.US_BondType = "8";
			declaration.US_SuretyCode = "353";
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_IsInvoiceByRequest = true;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("19", false, false);
		}

		[TestDate(2016, 03, 27)]
		public void Test20()
		{
			/*
			20)  1602501020                 TARIFF NUMBER
					 YES                        QUALIFIES FOR ORGANIC EXEMPTION

					 (SHIPMENT QUALIFIES FOR ORGANIC EXEMPTION AS OUTLINED IN ADMIN
					 MESSAGE 04-2257)
			*/
			invoiceLine.JI_Tariff = "1602501020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 10m;
			//			invoiceLine.US_CottonCertificateNo = "ORGANIC99";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("20");
		}

		[TestDate(2016, 03, 27)]
		public void Test24()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("Z104");
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "Z104";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_Tariff = "8408909010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_Weight = 10m;
			manufacturerCode.OK_CustomsRegNo = "XYBEREQU6LON";
			MergeAndSend("24");

			AssertEquals(12.5m, declaration.CustomsEntryHeaders[0].CH_TotalPaid);   //TODO: Work out correct payment amount
		}

		[TestDate(2008, 12, 31)]
		public void Test25()
		{
			invoiceLine.JI_Tariff = "6203122010";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			manufacturerCode.OK_CustomsRegNo = "HKKOAMOU381VAN";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			MergeAndSend("25");
		}

		public void Test26()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("Z104");
			/*
			 26)  2208204000                 TARIFF NUMBER
			21                         ENTRY TYPE
			FTZ222                     YES, FROM FTZ INTO A WAREHOUSE
			32CCT04                    FDA PRODUCT CODE

			NOTE: GOODS GOING FROM A FTZ INTO A WAREHOUSE ARE NOT
			SUBJECT TO BTA
			 */
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "FTZ222";

			declaration.JE_MasterBill = "OBL26";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "Z104";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			declaration.JE_VesselName = "FTZ222";
			//declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			//TODO: Support Free Trade Zone into Warehouse (FTZ222) And remove SetUpFDARequiredData() and do something about validation
			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"32CCT04", "COGNAC (LIQUORS);GLASS;PACKAGED FOOD (NOT COMMERCIALLY STERI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			invoiceLine.FDAs[0].US_FDAProductCode = "32CCT04";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("26");
			ICusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals(249.64m, entryLine.ExciseTax);
			Assert("Message contains excise", declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText.Contains("24964"));
		}

		[TestDate(2008, 12, 31)]
		public void Test28()
		{
			declaration.JE_RL_NKPortOfLoading = "CLCUR";
			/*
			28) 99119585 TARIFF NUMBER 1
					0811908080 TARIFF NUMBER 2
					CL COUNTRY OF ORIGIN
			 */

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.US_SupTariff = "99119585";
				invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
				invoiceLine.JI_CustomsQuantity = 10m;
				invoiceLine.JI_Weight = 10m;
			}

			invoiceLine.JI_Tariff = "0811908080";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 100m;
			declaration.JE_MasterBill = "OBL28";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.US_SPI = "CL";
			MergeAndSend("28");
		}

		[TestDate(2009, 6, 1)]
		public void Test30()
		{
			/*
30) YOU ARE SENDING TO ABI INFORMATION ABOUT A SHIPMENT OF SPAGHETTI
MEALS WHICH HAS BEEN DETERMINED TO BE A SET. THE ESSENTIAL
CHARACTERISTIC IS THE SPAGHETTI-1902194000.THE SET ALSO CONTAINS
DRIED MUSHROOMS (0712311000) AND TOMATO PASTE(2002908020).
THE TOTAL VALUE OF THE SHIPMENT IS $5000. THE VALUE OF THE
SPAGHETTI IS $2400.THE MUSHROOMS& PASTE ARE EACH VALUED AT $1300
THE SPAGHETTI WAS MADE IN SWITZERLAND. THE MUSHROOMS ARE
FROM FRANCE AND THE TOMATO PASTE WAS MADE IN ITALY THE SHIPMENT
WAS EXPORTED FROM SWITZERLAND.*/

			declaration.JE_MasterBill = "OBL30";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceHeader.JZ_InvoiceAmount = 5000m;

			invoiceLine.JI_Tariff = "1902194000";   //Spaghetti
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfExport = "CH";
			invoiceLine.US_UC_NKCountryOfOrigin = "CH";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_LinePrice = 1m;
			invoiceLine.US_SPI = "N/A";

			JobComInvoiceLine line1a = invoiceLine.AddSecondaryInvoiceLine();
			line1a.JI_Tariff = "1902194000";    //Spaghetti
			line1a.JI_CustomsQuantity = 1m;
			line1a.JI_Description = "COMMERCIAL DESCRIPTION";
			line1a.JI_LinePrice = 2400M;
			line1a.US_UC_NKCountryOfOrigin = "CH";
			line1a.JI_CustomsQuantity = 5m;
			line1a.JI_Weight = 50m;
			line1a.US_SPI = "N/A";
			line1a.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "0712311000"; // Mushrooms
			line2.JI_CustomsQuantity = 1m;
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_LinePrice = 1300m;
			line2.JI_CustomsQuantity = 5m;
			line2.US_UC_NKCountryOfOrigin = "FR";
			line2.JI_Weight = 50m;
			line2.US_SPI = "N/A";
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			helper.SetUpFDARequiredData(line2, manufacturer, Factory);

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "2002908020"; // Tomato Paste
			line3.JI_CustomsQuantity = 1m;
			line3.JI_Description = "COMMERCIAL DESCRIPTION";
			line3.JI_LinePrice = 1300m;
			line3.JI_CustomsQuantity = 5m;
			line3.US_UC_NKCountryOfOrigin = "IT";
			line3.JI_Weight = 50m;
			line3.US_SPI = "N/A";
			line3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			helper.SetUpFDARequiredData(line3, manufacturer, Factory);

			invoiceHeader.US_UC_NKCountryOfExport = "CH";
			MergeAndSend("30");
			List<ENS50> ens50s = helper.GetTypes<ENS50>(declaration);
			AssertEquals("ENS50 Line Count", 4, ens50s.Count);
			AssertEquals("Tariff 1", "1902194000", ens50s[0].TariffNumber1);
			AssertEquals("Duty Line 1", 320m, ens50s[0].Duty);
			AssertEquals("Tariff 2", "1902194000", ens50s[1].TariffNumber1);
			AssertEquals("Tariff 3", "0712311000", ens50s[2].TariffNumber1);
			AssertEquals("Tariff 4", "2002908020", ens50s[3].TariffNumber1);
			List<ENS62> ens62s = helper.GetTypes<ENS62>(declaration);
			AssertEquals("ENS62 Line Count", 1, ens62s.Count);
			AssertEquals("Line 1 MPF", 10.50m, ens62s[0].UserFeeAmount);
			AssertEquals("Duty", 320m, helper.GetTypes<ENS90>(declaration)[0].TotalEstimatedDuty);
			AssertEquals("Total MPF", 25m, helper.GetTypes<ENS90>(declaration)[0].GrandTotalFeeAmount);
		}

		/*
		 * 32) SCENARIO: YOU HAVE TRANSMITTED AN ENTRY WITH A COMPUTER THAT
IS REGULATED BY FDA EVEN THOUGH THE TARIFF NUMBER IS CODED FD1.
AFTER ABI SELECTIVITY WAS PERFORMED BUT BEFORE ON-LINE
SELECTIVITY WAS PERFORMED YOU DISCOVER THAT THE TARIFF NUMBER
ORIGINALLY SENT WAS INCORRECT. YOU REQUEST THAT CBP PERFORM
A "PEN AND INK" CHANGE TO CHANGE THE TARIFF NUMBER TO
8471300000 WHICH IS ALSO CODED FD1 AND REQUIRED SUBMISSION OF
FDA DATA. YOU RECEIVE A 'DT' FDA REJECT. USING THE FOLLOWING
INFORMATION SEND THE CORRECT TRANSACTION TO ABI TO CORRECT
THE FDA REJECT.

95L--32 FDA PRODUCT CODE
NOTEBOOK COMPUTER FDA DESCRIPTION
1 FDA QUANTITY
24780 FDA VALUE
		 */
		[TestDate(2007, 9, 11)]
		public void Test32()
		{
			declaration.JE_MasterBill = "OBL32";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "8471300000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_LinePrice = 24780m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Weight = 1m;
			invoiceLine.JI_Description = "NOTEBOOK COMPUTER";

			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType, "95L--32", "CD PLAYER COMBINATION SYSTEMS;;", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			invoiceLine.FDAs[0].US_FDAProductCode = "95L--32";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("32");
		}

		[TestDate(2016, 03, 27)]
		public void Test33()
		{
			/*33) XC/CA COUNTRY OF ORIGIN/EXPORT
8703310000 TARIFF NUMBER
B SPECIAL PROGRAM INDICATOR
25000 ENTERED VALUE
2 QUANTITY
REDUCED MPF CLAIM YES*/

			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceHeader.US_UC_NKCountryOfExport = "CA";
			manufacturerCode.OK_CustomsRegNo = "XCKOAMOU381VAN";

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTPassport = "PASS";
			DOTVIN dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "Dodge";
			dotvin.US_DOTModel = "Ute";
			dotvin.US_DOTYear = 2005;
			dotvin.US_DOTVIN = "200532131231";
			invoiceLine.JI_Tariff = "8703310000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			invoiceLine.JI_LinePrice = 25000m;
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.JI_Weight = 2m;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			MergeAndSend("33");
		}

		[TestDate(2016, 03, 27)]
		public void Test34()
		{
			/*34) 2208202000 TARIFF NUMBER
297900 VALUE
125000 QUANTITY
XC/CA COUNTRY OF ORIGIN/ SPI CLAIM
DEFERRED TAX INDICATOR YES(NON-ACH PAYMENT)*/

			declaration.JE_MasterBill = "OBL34";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.FDAs[0].US_UC_NKFDAProduction = "XC";
			invoiceLine.JI_Tariff = "2208202000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 297900m;
			invoiceLine.JI_CustomsQuantity = 125000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceHeader.US_UC_NKCountryOfOrigin = "XC";
			manufacturerCode.OK_CustomsRegNo = "XCKOAMOU381VAN";

			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceHeader.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_SPI = "CA";
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			MergeAndSend("34");
		}

		[TestDate(2006, 9, 6)]
		public void Test35()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A580844004";
			uscCase.U5_ISOCountryCode = "KR";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "72142000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "7222110050";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "7228606000";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.02m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			/*35) A580844004 ANTIDUMPING CASE NUMBER
7222110050 TARIFF NUMBER
KR COUNTRY OF ORIGIN*/

			invoiceLine.JI_Tariff = "7222110050";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_MiscPermitNo = "CG0000123";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A580844004";
			invoiceLine.AntidumpingDutyCase.U5_CaseStatus = ACCaseStatusList.Codes.IT;
			declaration.US_EstimatedEntryDate = new ZDateTime(2006, 9, 6);
			MergeAndSend("35");
			helper.MessageMustContain(declaration, "KR", "A580844004");
		}

		[ExpectNoExceptions]
		public void Test36()
		{
			var qrBlock = new DSTQR
			{
				TransmissionDateOfStatementOrACHPaymentTransaction = ZDate.Today.AddDays(-1),
				PreliminaryStatementRequest = "Y",
				FinalStatementRequest = "Y",
				ACHPaymentRequest = "Y",
				PeriodicStatementPaymentAuthorizationRequest = "Y"
			};
			new MiscellaneousMessageRequester(Factory).RequestStatement(false, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute, qrBlock, "9999");
		}

		[TestDate(2016, 03, 27)]
		public void Test38()
		{
			/*You are correct.  The Dominican Republic lost the 'E' on 3/1/07.  Now you
could use the 'P' indicator.  I will ask about them changing this question
as well.*/

			USCCountry dO = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "DO");
			dO.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			dO.UC_MiscellaneousSPIIndicator = "P";

			invoiceLine.JI_Tariff = "9010503000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "DO";

			invoiceHeader.US_UC_NKCountryOfExport = "DO";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AddFCCData(invoiceLine);
			MergeAndSend("38");
		}

		[TestDate(2008, 12, 31)]
		public void Test39()
		{
			// Added to keep validation happy
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_Tariff = "6201134040";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			manufacturerCode.OK_CustomsRegNo = "MXKOAMOU381VAN";
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("39");
		}

		[TestDate(2016, 03, 27)]
		public void Test40()
		{
			/*
40) 20 MODE OF TRANSPORTATION
CANADIAN NATIONAL RR CARRIER
194135 MASTER BILL NUMBER
3802 PORT OF UNLADING
			*/

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3802", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameUnlading = refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			newFactory.Save();

			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.RailContainer;

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.US_UI_NKCarrierSCAC = "ATLT";

			declaration.JE_MasterBill = "194135";
			declaration.US_SchDArrival = "3802";
			declaration.JE_PrimaryITNumber = "V2365425140";
			declaration.US_ITDate = ZDateTime.Today;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("40");
		}

		[TestDate(2016, 03, 27)]
		public void Test41()
		{
			/* 41) 9010501000 TARIFF NUMBER
			$10,000 ENTERED VALUE
			AZ COUNTRY OF ORIGIN/EXPORT*/

			invoiceLine.JI_Tariff = "9010501000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "AZ";
			invoiceHeader.US_UC_NKCountryOfExport = "AZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AddFCCData(invoiceLine);
			MergeAndSend("41");
		}

		[TestDate(2016, 03, 27)]
		public void Test42()
		{
			declaration.JE_MasterBill = "OBL42";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0409000025";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_ADD_NA = true;

			MergeAndSend("42");
		}

		[TestDate(2016, 03, 27)]
		public void Test43()
		{
			declaration.JE_RL_NKPortOfLoading = "FMYAP";
			invoiceLine.JI_Tariff = "2401106130";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "FM";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Z;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 1000m;
			MergeAndSend("43");
			helper.MessageMustContain(declaration, "Z");
		}

		[TestDate(2016, 03, 27)]
		public void Test44()
		{
			invoiceLine.JI_Tariff = "1602412040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 5000m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("44");
		}

		[TestDate(2016, 03, 27)]
		public void Test45()
		{
			invoiceLine.JI_Tariff = "1602501020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 16000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("45");
		}

		[TestDate(2016, 03, 27)]
		public void Test46()
		{
			invoiceLine.JI_Tariff = "2914704000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			invoiceHeader.US_UC_NKCountryOfExport = "GB";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.L;
			MergeAndSend("46");
		}

		[TestDate(2016, 03, 27)]
		public void Test47()
		{
			/*
47)  9403509040                 TARIFF NUMBER
	 A570890072                 ANTIDUMPING DUTY CASE NUMBER
	 CN                         COUNTRY OF ORIGIN
	 100                        QUANTITY 1
			*/

			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A570890072";
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "9403509040";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "9403608080";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "9403509080";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 0.33m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_Tariff = "9403509040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A570890072";
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_Weight = 100m;
			MergeAndSend("47");
		}

		[TestDate(2016, 03, 27)]
		public void Test48()
		{
			/*
48) 0910910000 TARIFF NUMBER
	BD COUNTRY OF ORIGIN
	11 MODE OF TRANSPORTATION
	1700 VALUE
	'A' SPECIAL PROGRAMS INDICATOR
			 */
			declaration.JE_MasterBill = "OBL48";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			declaration.JE_RL_NKPortOfLoading = "BDBZL";
			invoiceLine.JI_Tariff = "0910910000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "BD";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			invoiceLine.JI_LinePrice = 1700m;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			MergeAndSend("48");
		}

		[TestDate(2009, 12, 12)]
		public void Test49()
		{
			invoiceLine.JI_Tariff = "8215990100";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 4497m;
			invoiceLine.JI_CustomsQuantity = 18000m;
			invoiceLine.JI_Weight = 6000m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("49");
		}

		[TestDate(2016, 03, 27)]
		public void Test50()
		{
			/* 50) SEND TO ABI AN ENTRY SUMMARY WITH THREE LINE ITEMS. 
			 * LINE ITEM ONE IS CLASSIFIED IN TSUSA 1205100090 VALUED AT $10,000 AND IS
ULTIMATELY DESTINED FOR BOZEMAN, MONTANA. 
			 * LINE ITEM TWO IS CLASSIFIED IN TSUSA 3306900000 VALUED AT $5,000 AND IS
ULTIMATELY DESTINED FOR ROCHESTER, NEW YORK. 
			 * LINE ITEM THREE IS CLASSIFIED IN 3702100060 VALUED AT $15,000 AND IS ULTIMATELY
DESTINED FOR HUMPTULIPS, WASHINGTON. */
			declaration.JE_MasterBill = "OBL50";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.FDAs[0].US_FDAValue = 1;
			invoiceLine.JI_Tariff = "1205100090";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_DestinationState = USStatesList.Codes.Montana;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			helper.SetUpFDARequiredData(invoiceLine2, manufacturer, Factory);
			invoiceLine2.FDAs[0].US_FDAValue = 1;
			invoiceLine2.JI_Tariff = "3306900000";
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.JI_Weight = invoiceLine.JI_Weight;
			invoiceLine2.US_DestinationState = USStatesList.Codes.NewYork;
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_Weight = 100m;
			invoiceLine2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			helper.SetUpFDARequiredData(invoiceLine3, manufacturer, Factory);
			invoiceLine3.FDAs[0].US_FDAValue = 1;
			invoiceLine3.JI_Tariff = "3702100060";
			invoiceLine3.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine3.JI_LinePrice = 15000m;
			invoiceLine3.JI_Weight = invoiceLine.JI_Weight;
			invoiceLine3.US_DestinationState = USStatesList.Codes.Washington;
			invoiceLine3.JI_CustomsQuantity = 10m;
			invoiceLine3.JI_Weight = 100m;
			invoiceLine3.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("50");
		}

		/// <summary>
		/// If this fails, then request tariffs by clicking 'Brokerage > Tariff Requests'. The current reference files do not have tariff records
		/// for the privileded status dates used in invoice lines
		/// </summary>
		[TestDate(2007, 1, 3)]
		public void Test52()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("Z104");
			/*
			 * 52) CN COUNTRY OF ORIGIN
06 ENTRY TYPE
212A FOREIGN TRADE ZONE NUMBER
P ZONE STATUS
1901909085 TARIFF NUMBER, LINE ONE
010195 PRIVILEGED STATUS DATE
6201933521 TARIFF NUMBER, LINE TWO
020193 PRIVILEGED STATUS DATE
2841610000 TARIFF NUMBER, LINE THREE.
020196 PRIVILEGED STATUS DATE
A570001000 CASE NUMBER, LINE THREE*/

			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A570001000";
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "2841610000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "2841600010";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.29m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.JE_RL_NKPortOfLoading = "CNCDO";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "FTZ212A";
			declaration.US_FTZNo = "FTZ212B";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 3);
			declaration.US_US_NKLocationOfGoods = "Z104";
			invoiceHeader.US_UC_NKCountryOfOrigin = "CN";

			invoiceLine.US_PrivilegedStatusDate = new ZDate(1995, 1, 1);
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_ManifestQty = 100;
			invoiceLine.JI_Tariff = "1901909085";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			invoiceLine.JI_LinePrice = 8000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_UC_NKCountryOfOrigin = "CN";
			line2.US_PrivilegedStatusDate = new ZDate(1993, 2, 1);
			line2.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			line2.JI_Tariff = "6201933521";
			line2.JI_CustomsQuantity = 1m;
			line2.JI_CustomsSecondQuantity = 1m;
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_CustomsQuantity = 100m;
			line2.US_SWPMIndicator = SWPMList.Codes._1;
			line2.US_ManifestQty = 100;
			line2.JI_Weight = 500m;
			line2.JI_LinePrice = 1000m;

			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			line3.US_UC_NKCountryOfOrigin = "CN";
			line3.US_PrivilegedStatusDate = new ZDate(1996, 2, 1);
			line3.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			line3.US_ManifestQty = 100;
			line3.JI_Tariff = "2841610000";
			line3.JI_CustomsQuantity = 1m;
			line3.JI_CustomsSecondQuantity = 1m;
			line3.JI_Description = "COMMERCIAL DESCRIPTION";
			line3.JI_CustomsQuantity = 100m;
			line3.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			line3.US_ADDCaseNo = "A570001000";
			line3.US_SWPMIndicator = SWPMList.Codes._1;
			line3.JI_Weight = 500m;
			line3.JI_LinePrice = 1000m;
			invoiceHeader.JZ_InvoiceAmount = invoiceHeader.JZ_Calc_LinesEntered;
			invoiceHeader.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceHeader.US_PrivilegedStatusDate = new ZDate(1995, 1, 1);

			MergeAndSend("52");
		}

		public void Test53()
		{
			/*
		53)
6405206030 TARIFF NUMBER
02 ENTRY TYPE
KN COUNTRY OF ORIGIN */

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_Tariff = "6405206030";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			manufacturer.OH_RL_NKClosestPort = "KNBAS";
			manufacturerCode.OK_CustomsRegNo = "KNBEREQU6LON";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SaintKittsAndNevis;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("53");
		}

		[TestDate(2007, 9, 11)]
		public void Test57()
		{
			/*---------------------------------------------------------------------
57) 1004000090 TARIFF NUMBER
10000 ENTERED VALUE
85000 QUANTITY
CA COUNTRY OF ORIGIN & SPI CLAIM LINE 1
01 ENTRY TYPE

YOU ARE PREPARING AN ENTRY FOR WHICH YOU ARE CLAIMING NAFTA
PREFERENTIAL TREATMENT BASED ON REGIONAL VALUE CONTENT CALCULATED
USING THE NET COST METHOD.*/

			declaration.JE_MasterBill = "OBL57";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.FDAs[0].US_UC_NKFDAProduction = "XA";
			invoiceLine.JI_Tariff = "1004000090";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 85000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.US_UC_NKCountryOfExport = "CA";

			manufacturerCode.OK_CustomsRegNo = "XASUPINT8010WET";
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			MergeAndSend("57");
		}

		[TestDate(2008, 12, 31)]
		public void Test60()
		{
			/*60) 6505900800 TARIFF NUMBER
KR COUNTRY OF ORIGIN
88895 VALUE
17800 QUANTITY 1
5528 QUANTITY 2*/

			invoiceLine.JI_Tariff = "6505900800";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			manufacturer.OH_RL_NKClosestPort = "KRADG";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			manufacturerCode.OK_CustomsRegNo = "KRBEREQU6LON";
			invoiceLine.JI_LinePrice = 88895m;
			invoiceLine.JI_CustomsQuantity = 17800m;
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			invoiceLine.JI_CustomsSecondQuantity = 5528m;
			invoiceLine.JI_Weight = 1000m;
			MergeAndSend("60");
		}

		[TestDate(2007, 9, 11)]
		public void Test62()
		{
			/*62) 8542218024 TARIFF NUMBER
KR COUNTRY OF ORIGIN
C580851000 CVD CASE NUMBER*/

			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "C580851000";
			uscCase.U5_ISOCountryCode = "KR";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "8542310000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "85438993";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 0.57m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;

			invoiceLine.JI_Tariff = "8542310000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_CVDCaseNo = "C580851000";
			invoiceLine.CountervailingDutyCase.U5_CaseStatus = ACCaseStatusList.Codes.IT;
			MergeAndSend("62");
		}

		public void Test63HandRolledValueInSecondaryTariff()
		{
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			ENS10 ens10 = new ENS10();
			ENS20 ens20 = new ENS20();
			ENS22 ens22 = new ENS22();
			ENS30 ens30 = new ENS30();
			ENS40 ens40 = new ENS40();
			ENS50 ens50 = new ENS50();
			ENS51 ens51 = new ENS51();
			ENS60 ens60 = new ENS60();
			ENS62 ens62 = new ENS62();
			ENS70 ens70 = new ENS70();
			ENS89 ens89 = new ENS89();
			ENS90 ens90 = new ENS90();

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.B.BlockNumber = 1;
			block.B.ProcessingDistrictPortCode = "8888";
			block.B.EntryFilerCode = "XJ5";
			block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.EntrySummary;

			ens10.Deserialise("10R888891-01319900091-013199000                 8         XJ500053575 01891  CO ");
			ens20.Deserialise("20     APL EMERALD         102809      63                   V123W               ");
			ens22.Deserialise("22            OBL63                               00000001PCS        OTT1       ");
			ens30.Deserialise("30                                                  1                   OTT1    ");
			ens40.Deserialise("40001AU          0000009000                    000000005060267                  ");
			ens50.Deserialise("50 8211100000          000000400000PCS                              AU071407N   ");
			ens51.Deserialise("51                                                                              ");
			ens60.Deserialise("60                                        XYBEREQU6LON                          ");
			ens62.Deserialise("62          49900005040                                                         ");
			ens70.Deserialise("708211930030 0000189600000002000000NO                               0000024000  ");
			ens89.Deserialise("8949900000005040                                                                ");
			ens90.Deserialise("9000000189600                                   00000005040                     ");
			block.Y.Deserialise("Y  8888XJ5EI00012000000189600                                                   ");

			block.MessageBlocks.Add(ens10);
			block.MessageBlocks.Add(ens20);
			block.MessageBlocks.Add(ens22);
			block.MessageBlocks.Add(ens30);
			block.MessageBlocks.Add(ens40);
			block.MessageBlocks.Add(ens50);
			block.MessageBlocks.Add(ens51);
			block.MessageBlocks.Add(ens60);
			block.MessageBlocks.Add(ens62);
			block.MessageBlocks.Add(ens70);
			block.MessageBlocks.Add(ens89);
			block.MessageBlocks.Add(ens90);

			MQEDIMessage ediMessage = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageText = block.Serialise();
			ediMessage.EM_MessageType = "EI";
			ediMessage.EM_MessageSubType = "EIR";
			Factory.Save();
			Assert(true);
		}

		public void Test63HandRolled()
		{
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.B.BlockNumber = 1;
			block.B.ProcessingDistrictPortCode = "8888";
			block.B.EntryFilerCode = "XJ5";
			block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.EntrySummary;

			ENS10 ens10 = new ENS10();
			ENS20 ens20 = new ENS20();
			ENS22 ens22 = new ENS22();
			ENS30 ens30 = new ENS30();
			ENS40 ens40 = new ENS40();
			ENS50 ens50 = new ENS50();
			ENS51 ens51 = new ENS51();
			ENS60 ens60 = new ENS60();
			ENS62 ens62 = new ENS62();
			ENS70 ens70 = new ENS70();
			ENS89 ens89 = new ENS89();
			ENS90 ens90 = new ENS90();

			ens10.Deserialise("10R888891-01319900091-013199000                 8         XJ5 0005357501891  CO ");
			ens20.Deserialise("20     APL EMERALD         102809      63                   V123W               ");
			ens22.Deserialise("22            OBL63                               00000001PCS        OTT1       ");
			ens30.Deserialise("30                                                  1                   OTT1    ");
			ens40.Deserialise("40001AU          0000009000                    000000005060267                  ");
			ens50.Deserialise("50 82111000000000189600000002000000PCS                              AU071407N   ");
			ens51.Deserialise("51                                                                              ");
			ens60.Deserialise("60                                        XYBEREQU6LON                          ");
			ens62.Deserialise("62          49900005040                                                         ");
			ens70.Deserialise("708211930030           000000400000NO                               0000008000  ");
			ens89.Deserialise("8949900000005040                                                                ");
			ens90.Deserialise("9000000189600                                   0000000504000000024000          ");
			block.Y.Deserialise("Y  8888XJ5EI00012000000189600                                                   ");

			block.MessageBlocks.Add(ens10);
			block.MessageBlocks.Add(ens20);
			block.MessageBlocks.Add(ens22);
			block.MessageBlocks.Add(ens30);
			block.MessageBlocks.Add(ens40);

			ens40.Value = 24000m;
			ens50.Quantity1 = 4000;
			ens70.Value = 0m;
			ens70.Quantity1 = 20000;
			decimal dutyFromQuantity = .03m * ens70.Quantity1;
			decimal dutyFromValue = .054m * ens40.Value;

			ens50.Duty = dutyFromQuantity + dutyFromValue;
			ens70.Duty = 0m;

			ens62.UserFeeAmount = 50.40m;
			ens89.TotalAmount = Math.Max(ens62.UserFeeAmount, 25m);

			ens90.GrandTotalFeeAmount = ens89.TotalAmount;
			ens90.TotalEstimatedDuty = ens50.Duty + ens70.Duty;
			ens90.TotalValueOfEntrySummary = 24000;

			block.Y.TotalEstimatedDuty = ens90.TotalEstimatedDuty;

			block.MessageBlocks.Add(ens50);
			block.MessageBlocks.Add(ens51);
			block.MessageBlocks.Add(ens60);
			block.MessageBlocks.Add(ens62);
			block.MessageBlocks.Add(ens70);
			block.MessageBlocks.Add(ens89);
			block.MessageBlocks.Add(ens90);

			MQEDIMessage ediMessage = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageText = block.Serialise();
			ediMessage.EM_MessageType = "EI";
			ediMessage.EM_MessageSubType = "EIR";
			Factory.Save();
			Assert(true);
		}

		[TestDate(2009, 6, 1)]
		public void Test63()    //  ADMINISTRATIVE MESSAGE 02-0703 + ADMINISTRATIVE MESSAGE 02-0911
		{
			/*63) 8211100000 TARIFF NBR(SET PROVISION FOR KNIVES)

THERE ARE 4000 SETS. EACH SET CONSISTS OF 5 KNIVES.
4 KNIVES IN THE SET ARE CLASSIFIED IN 8211929045 AND ARE VALUED
AT $1.00 EA.THE OTHER KNIFE IS CLASSIFIED IN 8211930030 AND IS
VALUED AT $2.00 EA.*/
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceHeader.JZ_InvoiceAmount = 24000m;

			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_Weight = 4000m;
			invoiceLine.JI_LinePrice = 0m;

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_Weight = 1000m;
			line2.JI_LinePrice = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_Description = "COMMERCIAL DESCRIPTION";
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_Weight = 100m;
			line3.JI_LinePrice = 8000m;

			AssertEquals("Invoice line count", 3, declaration.InvoiceLines.Count);
			MergeAndSend("63");

			List<ENS40> ens40s = helper.GetTypes<ENS40>(declaration);
			AssertEquals("40 Count", 1, ens40s.Count);
			AssertEquals("40 Value", 24000m, ens40s[0].Value);

			List<ENS50> ens50s = helper.GetTypes<ENS50>(declaration);
			AssertEquals("50 Count", 1, ens50s.Count);
			AssertEquals("50 Tariff", "8211100000", ens50s[0].TariffNumber1);
			AssertEquals("50 Duty", 1896m, ens50s[0].Duty);
			AssertEquals("50 Quantity 1", 4000m, ens50s[0].Quantity1);

			List<ENS70> ens70s = helper.GetTypes<ENS70>(declaration);
			AssertEquals("70 Count", 1, ens70s.Count);
			AssertEquals("70 Tariff", "8211930030", ens70s[0].TariffNumber2);
			AssertEquals("70 Duty", 0m, ens70s[0].Duty);
			AssertEquals("70 Quantity", 20000m, ens70s[0].Quantity1);
			AssertEquals("70 Value", 0m, ens70s[0].Value);
			List<ENS80> ens80s = helper.GetTypes<ENS80>(declaration);
			AssertEquals("80 Count", 0, ens80s.Count);

			ENS90 ens90 = helper.GetTypes<ENS90>(declaration)[0];
			AssertEquals("Total Duty", 1896m, ens90.TotalEstimatedDuty);
			AssertEquals("Total Tax", 0m, ens90.GrandTotalEstimatedTax);
			AssertEquals("Total Fee", 80.40m, ens90.GrandTotalFeeAmount);
			AssertEquals("Total Entry Value", 24000m, ens90.TotalValueOfEntrySummary);
		}

		[TestDate(2016, 03, 27)]
		public void Test64()
		{
			/*---------------------------------------------------------------------
64) 0810400028 TARIFF NUMBER
CL COUNTRY OF ORIGIN*/

			declaration.JE_MasterBill = "OBL64";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0810400028";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("64");
		}

		[TestDate(2010, 12, 1)]//1901104500 not active after 2009 in a test DB
		public void Test65()
		{
			/*---------------------------------------------------------------------
65) 12/31/XX IT DATE (XX=LAST YEAR)
12/31/XX DATE OF IMPORT (XX = LAST YEAR)
12/31/XX DATE OF EXPORT (XX = LAST YEAR)
MMDDYY ESTIMATED ENTRY DATE(USE CURRENT DATE)
40 MODE OF TRANSPORTATION
430042900 IT NUMBER
1901104500 TARIFF NUMBER
10,000 QUANTITY
SG COUNTRY OF ORIGIN & SPI CLAIM
10,000 ENTERED VALUE*/

			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2801", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			newFactory.Save();

			declaration.JE_MasterBill = "OBL65";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			declaration.JE_RL_NKPortOfLoading = "SGSIN";

			int year = ZDateTime.Today.Year - 1;

			declaration.US_ITDate = new ZDateTime(year, 12, 31);
			declaration.US_EntryDate = declaration.US_ITDate;
			declaration.JE_ExportDate = declaration.US_ITDate;
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirContainer;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_SchDArrival = "2801";
			declaration.PrimaryMasterBill.ITNumber = "430042900";
			declaration.JE_VoyageFlightNo = "UA650B";

			invoiceLine.JI_Tariff = "1901104500";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.Bills[0].US_UI_NKBillIssuerSCAC = "QF";
			declaration.US_UI_NKCarrierSCAC = "QF";

			MergeAndSend("65");
		}

		[TestDate(2016, 03, 27)]
		public void Test66()
		{
			/*---------------------------------------------------------------------
66) 7419993000 TARIFF NUMBER
HK COUNTRY OF ORIGIN
11 ENTRY TYPE
YES PERSONAL SHIPMENT
3000 VALUE*/

			invoiceLine.JI_Tariff = "7419993000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			invoiceLine.JI_LinePrice = 3000m;

			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;

			MergeAndSend("66");
		}

		[TestDate(2016, 03, 27)]
		public void Test67()
		{
			/*---------------------------------------------------------------------
67) 9014101000 TARIFF NUMBER
GL COUNTRY OF ORIGIN
C SPECIAL PROGRAM INDICATOR
8 QUANTITY*/

			invoiceLine.JI_Tariff = "9014101000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "GL";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.C;
			invoiceLine.JI_CustomsQuantity = 8m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AddFCCData(invoiceLine);
			MergeAndSend("67");
		}

		public void Test68HandRolled()
		{
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(@"
B018888XJ5EI                                               1005376              
10R888891-01319900091-013199000                 8         XJ5 0005454001891  CO 
20                         402801      68                   V123W               
22            OBL68                               00000001PCS                   
30                                                  1                   QF      
40001AU00002101500000009000                    000000005060267                  
50 9802005060                                                       AU071507N   
51                                                                              
60                                        XYBEREQU6LON                          
709106100000           000001200000NO 000004800000JWL               0000110000  
90                                                         00000320150          
Y  8888XJ5EI00010                                                               ".Replace("\r\n", ""));
			block.B.UserData = EDIMessage.MessageNumberPlaceHolder;

			ENS40 ens40 = (ENS40)block.MessageBlocks[4];
			ENS50 ens50 = (ENS50)block.MessageBlocks[5];
			ENS70 ens70 = (ENS70)block.MessageBlocks[8];
			ENS90 ens90 = (ENS90)block.MessageBlocks[9];

			ens40.Value = 210150m;// 210150m;
			ens40.CountryOfOrigin = "FR";
			ens50.Duty = 7974.15m;
			ens50.CountryOfExport = "FR";

			ens70.Quantity1 = 12000m;// 12000m;
			ens70.Quantity2 = 48000m;// 48000m;
			ens70.Value = 110000m;// 110000;
			ens70.Duty = 0m;// 23208.32m;

			ens90.GrandTotalFeeAmount = 0;
			ens90.TotalEstimatedDuty = ens50.Duty + ens70.Duty;
			ens90.TotalValueOfEntrySummary = ens40.Value + ens70.Value;

			block.Y.TotalEstimatedDuty = ens90.TotalEstimatedDuty;

			MQEDIMessage ediMessage = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageText = block.Serialise();
			ediMessage.EM_MessageType = "EI";
			ediMessage.EM_MessageSubType = "EIR";
			Factory.Save();
			Assert(true);
		}

		[TestDate(2008, 12, 31)]
		public void Test69()
		{
			/*---------------------------------------------------------------------
69) 6205202015 TARIFF NUMBER
JP COUNTRY OF ORIGIN
225 ENTERED VALUE
11 ENTRY TYPE
YES COMMERCIAL SAMPLE*/

			invoiceLine.JI_Tariff = "6205202016";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_LinePrice = 225m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "JPBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("69");
		}

		//TODO: Get replacement tariffs from Phyllis When she replies, remove the TestDate attribute
		[TestDate(2006, 1, 2)]
		public void Test70()
		{
			/*---------------------------------------------------------------------
70) 6203219015 TARIFF NUMBER 1 LINE ITEM 1
6203315010 TARIFF NUMBER 2 LINE ITEM 1
TW COUNTRY OF ORIGIN LINE ITEM 1
20000 ENTERED VALUE LINE ITEM 1
100 QUANTITY 1(BOTH TARIFF NUMBERS,LINE 1
2000 QUANTITY 2(BOTH TARIFF NUMBERS,LINE 1

6203219020 TARIFF NUMBER 1 LINE ITEM 2
6203410510 TARIFF NUMBER 2 LINE ITEM 2
TW COUNTRY OF ORIGIN LINE ITEM 2
10000 ENTERED VALUE LINE ITEM 2
100 QUANTITY 1(BOTH TARIFF NUMBERS,LINE 1
1000 QUANTITY 2(BOTH TARIFF NUMBERS,LINE 2*/

			invoiceLine.JI_Tariff = "6203219015";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 0m;
			JobComInvoiceLine secondaryTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = "6203315010";
			secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			secondaryTariff.JI_LinePrice = 20000m;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondQuantity = 2000m;
			invoiceLine.JI_Weight = 10m;
			secondaryTariff.US_UC_NKCountryOfOrigin = "TW";

			secondaryTariff.JI_CustomsQuantity = 100m;
			secondaryTariff.JI_CustomsSecondQuantity = 2000m;
			secondaryTariff.JI_Weight = 100m;

			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "6203219020";
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			JobComInvoiceLine line2secondaryTariff = line2.AddSecondaryInvoiceLine();
			line2secondaryTariff.JI_Tariff = "6203410510";
			line2secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.US_UC_NKCountryOfOrigin = "TW";
			line2.JI_LinePrice = 0m;
			line2.JI_CustomsQuantity = 100m;
			line2.JI_CustomsSecondQuantity = 1000m;
			line2.JI_Weight = 500m;
			line2secondaryTariff.JI_LinePrice = 10000m;
			line2secondaryTariff.JI_CustomsQuantity = 100m;
			line2secondaryTariff.JI_CustomsSecondQuantity = 1000m;
			line2secondaryTariff.JI_Weight = 100m;
			line2secondaryTariff.US_UC_NKCountryOfOrigin = "TW";
			declaration.US_EstimatedEntryDate = new ZDateTime(2006, 1, 2);
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "TWBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);

			MergeAndSend("70");
		}

		[TestDate(2009, 12, 12)]
		public void Test71()
		{
			/*---------------------------------------------------------------------
71) 9101118010 TARIFF NUMBER 1
1490200 ENTERED VALUE TARIFF 1
			 * 
9101118020 TARIFF NUMBER 2
601690 ENTERED VALUE TARIFF 2
			 * 
9101118030 TARIFF NUMBER 3
790840 ENTERED VALUE TARIFF 3
			 * 
9101118040 TARIFF NUMBER 4
500612 ENTERED VALUE TARIFF 4

59600 COMPLETE WATCHES.ENTER THIS QUANTITY FOR EACH TARIFF NBR.*/
			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.JI_Tariff = "9101118010";
				invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
				invoiceLine.JI_LinePrice = 1490200m;
				invoiceLine.JI_CustomsQuantity = 59600m;
				invoiceLine.JI_Weight = 1000m;
				invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

				JobComInvoiceLine secondTariff = invoiceLine.AddSecondaryInvoiceLine();
				secondTariff.JI_Tariff = "9101118020";
				secondTariff.JI_Description = "COMMERCIAL DESCRIPTION";
				secondTariff.JI_LinePrice = 601690m;
				secondTariff.JI_CustomsQuantity = 59600m;
				secondTariff.JI_Weight = 1000m;

				JobComInvoiceLine thirdTariff = invoiceLine.AddSecondaryInvoiceLine();
				thirdTariff.JI_Tariff = "9101118030";
				thirdTariff.JI_Description = "COMMERCIAL DESCRIPTION";
				thirdTariff.JI_LinePrice = 790840m;
				thirdTariff.JI_CustomsQuantity = 59600m;
				thirdTariff.JI_Weight = 1000m;

				JobComInvoiceLine fourthTariff = invoiceLine.AddSecondaryInvoiceLine();
				fourthTariff.JI_Tariff = "9101118040";
				fourthTariff.JI_Description = "COMMERCIAL DESCRIPTION";
				fourthTariff.JI_LinePrice = 500612m;
				fourthTariff.JI_CustomsQuantity = 59600m;
				fourthTariff.JI_Weight = 1000m;
			}

			MergeAndSend("71");
		}

		[TestDate(2008, 12, 31)]
		public void Test73()
		{
			/*---------------------------------------------------------------------
73) 9404902000 TARIFF NUMBER
MOROCCO COUNTRY OF ORIGIN AND EXPORT
SPECIAL AGREEMENT APPLIES YES*/

			invoiceLine.JI_Tariff = "9404902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			manufacturer.OH_RL_NKClosestPort = "MAAGA";
			manufacturerCode.OK_CustomsRegNo = "MABEREQU6LON";
			invoiceHeader.US_UC_NKCountryOfOrigin = "MA";
			invoiceHeader.US_UC_NKCountryOfExport = "MA";
			invoiceLine.US_SPI = "MA";
			MergeAndSend("73");
		}

		[ExpectNoExceptions]
		public void Test74()
		{
			/*---------------------------------------------------------------------
74) MAEUCCU826298 AMS BILL OF LADING

SEND THE PROPER QUERY TO DETERMINE THE CARGO STATUS OF
THIS BILL OF LADING.*/

			declaration.PrimaryMasterBill.CU_BillNum = "CCU826298";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "MAEU";

			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;

			var sendingObj = new CargoManifestQuerySendingObject(sendingHeader, declaration.PrimaryMasterBill);

			new CargoManifestStatusQueryMessageBuilder(declaration, sendingObj).GenerateMessages();
			AssertEquals("Message", true, declaration.Messages[0].EM_MessageText.Contains("UCCU826298"));
			AssertEquals("Message", true, declaration.Messages[0].EM_MessageText.Contains("MAEU"));
		}

		[TestDate(2008, 12, 31)]
		public void Test75()
		{
			/*---------------------------------------------------------------------
75) 8206000000 TARIFF NUMBER 1 PART 1
8203204000 TRAIFF NUMBER 1 PART 2*/

			invoiceLine.JI_Tariff = "8206000000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			JobComInvoiceLine secondTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondTariff.JI_Tariff = "8203204000";
			secondTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			secondTariff.JI_LinePrice = 10000m;
			MergeAndSend("75");
		}

		[TestDate(2016, 03, 27)]
		public void Test76()
		{
			/*---------------------------------------------------------------------
76) 2204216000 TARIFF NUMBER
112800 ENTERED VALUE
56000 QUANTITY
VA COUNTRY OF ORIGIN*/

			declaration.JE_MasterBill = "OBL76";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2204216000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 112800m;
			invoiceLine.JI_CustomsQuantity = 56000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfOrigin = "VA";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			MergeAndSend("76");
		}

		[TestDate(2016, 03, 27)]
		public void Test78()
		{
			/*---------------------------------------------------------------------
78) 7101223000 TARIFF NUMBER
GU COUNTRY OF ORIGIN
Y SPECIAL PROGRAM INDICATOR*/

			invoiceLine.JI_Tariff = "7101223000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "GU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;

			declaration.JE_RL_NKPortOfLoading = "GUGUM";

			MergeAndSend("78");
		}

		[TestDate(2007, 9, 11)]
		public void Test79()
		{
			CreateNewOrGetExistingCusCodeTypeForFIRMSCode("Z104");
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "ADD";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.MinSmallDateTimeValue;
			addCase.U5_ManufacturerMID = ZString.Empty;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "3926909880";
			var rate1 = addCase.CaseRates.AddNew();
			rate1.U6_EffectiveDate = ZDateTime.Today;
			rate1.U6_AdValoremRate = 1m;
			Factory.Save();
			/*---------------------------------------------------------------------
79) 06 ENTRY TYPE
2709002000 TARIFF NUMBER
P ZONE STATUS
MM/DD/YY PRIVILEGED STATUS FILING DATE (USE OU
PREVIOUS YEAR)
MM/DD/YY ESTIMATED ENTRY DATE-USE CURRENT YEAR
$2500 ENTERED VALUE
20,000 BBL QUANTITY*/

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "FTZ002A";
			declaration.US_FTZNo = "FTZ002B";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_Tariff = "3926909880";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today.AddYears(-1);
			declaration.US_US_NKLocationOfGoods = "Z104";

			invoiceLine.JI_LinePrice = 2500m;
			invoiceLine.JI_CustomsQuantity = 20000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.US_ManifestQty = 100;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "ADD";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			invoiceHeader.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceHeader.US_PrivilegedStatusDate = ZDateTime.Today.AddYears(-1);
			MergeAndSend("79");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens20 = message.MessageBlock.MessageBlocks.OfType<ENS20>().FirstOrDefault();
			AssertEquals("FTZ002B", ens20.ImportingVesselName);
		}

		[TestDate(2016, 03, 27)]
		public void Test81()
		{
			/*--------------------------------------------------------------------
81) 9005908000 TARIFF NUMBER 1, LINE 1
9005804040 TARIFF NUMBER 2, LINE 1
10000 ENTERED VALUE
TW COUNTRY OF ORIGIN*/
			invoiceLine.JI_Tariff = "9005908000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "9005804040";
			secondaryLine.JI_Description = "COMMERCIAL DESCRIPTION";
			secondaryLine.JI_LinePrice = 10000m;

			MergeAndSend("81");
		}

		[TestDate(2008, 12, 31)]
		public void Test82()
		{
			/*--------------------------------------------------------------------
82) 0804400010 TARIFF NUMBER
MX COUNTRY OF ORIGIN*/

			declaration.JE_MasterBill = "OBL82";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0804400010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("82");
		}

		[TestDate(2016, 03, 27)]
		public void Test83()
		{
			/*--------------------------------------------------------------------
83) 9102119510 TARIFF NUMBER 1, LINE 1
1189 ENTERED VALUE TARIFF 1
9102119520 TARIFF NUMBER 2, LINE 1
100 ENTERED VALUE TARIFF 2
9102119530 TARIFF NUMBER 3, LINE 1
85 ENTERED VALUE TARIFF 3
9102119540 TARIFF NUMBER 4, LINE 1
42 ENTERED VALUE TARIFF 4
50 MODE OF TRANSPORTATION

QUANTITY FOR ALL TARIFF NUMBERS IS 120*/

			invoiceLine.JI_Tariff = "9102119510";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 1189m;
			invoiceLine.JI_CustomsQuantity = 120m;
			invoiceLine.JI_Weight = 100m;

			JobComInvoiceLine secondTariff = invoiceLine.SecondaryTariffLines.ElementAt(0);
			secondTariff.JI_Tariff = "9102119520";
			secondTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			secondTariff.JI_LinePrice = 100m;
			secondTariff.JI_CustomsQuantity = 120m;
			secondTariff.JI_Weight = 120m;

			JobComInvoiceLine thirdTariff = invoiceLine.SecondaryTariffLines.ElementAt(1);
			thirdTariff.JI_Tariff = "9102119530";
			thirdTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			thirdTariff.JI_LinePrice = 85m;
			thirdTariff.JI_CustomsQuantity = 120m;
			thirdTariff.JI_Weight = 80m;

			JobComInvoiceLine fourthTariff = invoiceLine.SecondaryTariffLines.ElementAt(2);
			fourthTariff.JI_Tariff = "9102119540";
			fourthTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			fourthTariff.JI_LinePrice = 42m;
			fourthTariff.JI_CustomsQuantity = 120m;
			fourthTariff.JI_Weight = 120m;

			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.Mail;
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;

			AssertEquals("Invoice Line Count", 4, declaration.InvoiceLines.Count);
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("83");
		}

		[TestDate(2016, 03, 27)]
		public void Test85()
		{
			/*--------------------------------------------------------------------
85) 8538908080 TARIFF NUMBER LINE 1
$2000 ENTERED VALUE, LINE 1
CA COUNTRY OF ORIGIN/EXPORT, LINE 1
CA SPI CLAIM,LINE 1
8538908080 TARIFF NUMBER LINE 2
$1500 ENTERED VALUE, LINE 2
TW COUNTRY OF ORIGIN, LINE 2
CA COUNTRY OF EXPORT, LINE 2*/

			invoiceLine.JI_Tariff = "8538908080";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;

			manufacturerCode.OK_CustomsRegNo = "XASUPINT8010WET";

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "8538908080";
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_LinePrice = 1500;
			line2.US_UC_NKCountryOfOrigin = "TW";
			line2.US_UC_NKCountryOfExport = SpecialProgramList.Codes.CA;
			line2.US_SPI = "N/A";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			line2.JI_Weight = 500m;

			MergeAndSend("85");
		}

		[TestDate(2016, 03, 27)]
		public void Test86()
		{
			/*--------------------------------------------------------------------
86) 0210992000 TARIFF NUMBER
10000 QUANTITY
VC COUNTRY OF ORIGIN
W SPECIAL PROGRAM INDICATOR*/

			invoiceHeader.US_UC_NKCountryOfExport = "VC";
			invoiceLine.JI_Tariff = "0210992000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "VC";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.W;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			MergeAndSend("86");
		}

		[TestDate(2016, 03, 27)]
		public void Test87()
		{
			/*---------------------------------------------------------------------
87) 3103100010 TARIFF NUMBER
IT COUNTRY OF ORIGIN
15,698,587 VALUE(FOREIGN CURRENCY/ITALY)
100 QUANTITY*/

			invoiceLine.JI_Tariff = "3103100010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_LinePrice = 15698587m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Italy;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = invoiceLine.JI_CustomsUnitQty;
			MergeAndSend("87");
		}

		[TestDate(2016, 03, 27)]
		public void Test88()
		{
			/*---------------------------------------------------------------------
88) 3103100020 TARIFF NUMBER, LINE 1
TW COUNTRY OF ORIGIN AND EXPORT
10 QUANTITY, LINE 1
3103100020 TARIFF NUMBER, LINE 2
KR COUNTRY OF ORIGIN AND EXPORT
10 QUANTITY, LINE 2*/

			invoiceLine.JI_Tariff = "3103100020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.US_UC_NKCountryOfExport = "TW";
			invoiceLine.US_SPI = "N/A";
			invoiceHeader.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = invoiceLine.JI_CustomsUnitQty;
			manufacturerCode.OK_CustomsRegNo = "TWSTRTAP4068TAI";

			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			invoiceHeader2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m);
			invoiceHeader2.US_UC_NKCountryOfExport = "KR";
			invoiceHeader2.US_UC_NKCountryOfOrigin = "KR";

			OrgHeader manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			manufacturer2.OH_FullName = "Mr MANUFACTURER 2";
			manufacturer2.OH_IsConsignor = true;
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "KRJUHCOR3325KIM");

			invoiceHeader2.JZ_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			invoiceHeader2.US_TransactionsRelated = "N";

			JobComInvoiceLine line2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "8538908080";
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_LinePrice = 1500;
			line2.US_UC_NKCountryOfOrigin = "KR";
			line2.JI_CustomsQuantity = 10m;
			line2.JI_Weight = 500m;
			line2.US_SPI = "N/A";
			MergeAndSend("88");
		}

		[TestDate(2016, 03, 27)]
		public void Test91()
		{
			/*---------------------------------------------------------------------
91) 0710100000 HTS NUMBER
GB COUNTRY OF ORIGIN AND EXPORT
10000 KG QUANTITY
$2000 ENTERED VALUE
11 MODE OF TRANSPORTATION*/

			declaration.JE_MasterBill = "OBL91";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0710100000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.JI_Weight = 10000m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			MergeAndSend("91");
		}

		public void Test93HandRolled()
		{
			string message =
@"
B018888XJ5EI                                               1005428              
10R888891-01319900091-013199000                 8         XJ5 0005505901891  CO 
20     APL EMERALD         102809      93                   V123W               
22            OBL93                               00000001PCS        OTT1       
30                                                  1                   OTT1    
40001AU00000034060000009000                    000000005060267                  
50 98020040400000026703                                             AU071607N   
51                                                                              
60                                        XYBEREQU6LON                          
709102111010           000000100000NO                               0000005258  
809802004040                                                                    
819102111020           000000100000NO                               0000002619  
819802004040                                                                    
819102111030           000000100000NO                               0000001344  
819802004040                                                                    
819102111040           000000100000NO                               0000000204  
9000000026703                                              00000012831          
Y  8888XJ5EI00016000000026703                                                   ";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(message.Replace("\r\n", ""));

			block.B.UserData = EDIMessage.MessageNumberPlaceHolder;

			// Movement Repair Line 9802004040
			ENS40 ens40 = (ENS40)block.MessageBlocks[4];
			ens40.Value = 3406m;
			ens40.CountryOfOrigin = "FR";

			ENS50 ens50 = (ENS50)block.MessageBlocks[5];
			ens50.Duty = 0m;
			ens50.CountryOfExport = "FR";

			// Movement Tariff Line 9102111010 (40¢ each movement)
			ENS70 ens70 = (ENS70)block.MessageBlocks[8];
			ens70.Value = 5258m;
			ens70.Quantity1 = 1000;
			ens70.Duty = 326m;

			// Second Repair Line
			ENS80 ens80 = (ENS80)block.MessageBlocks[9];
			ens80.Value = 2619m;
			ens80.Quantity1 = 0m;
			ens80.Quantity2 = 0m;
			ens80.Quantity3 = 0m;
			ens80.Duty = 0m;

			// Case Tariff Line
			ENS81 ens811 = (ENS81)block.MessageBlocks[10];
			ens811.Value = 0m;
			ens811.Duty = 0m;
			ens811.Quantity1 = 1000m;
			ens811.Quantity2 = 0m;
			ens811.Quantity3 = 0m;

			#region Not involved yet (secondary 81's with no value)
			// Third Repair Line
			ENS81 ens812 = (ENS81)block.MessageBlocks[11];
			ens812.Value = 1344m;
			ens812.Duty = 0m;
			ens812.Quantity1 = 0m;
			ens812.Quantity2 = 0m;
			ens812.Quantity3 = 0m;

			// Bracelet
			ENS81 ens813 = (ENS81)block.MessageBlocks[12];
			ens813.Value = 0m;
			ens813.Duty = 0m;
			ens813.Quantity1 = 1000m;
			ens813.Quantity2 = 0m;
			ens813.Quantity3 = 0m;

			// Fourth Repair Line
			ENS81 ens814 = (ENS81)block.MessageBlocks[13];
			ens814.Value = 204m;
			ens814.Duty = 0m;
			ens814.Quantity1 = 0m;
			ens814.Quantity2 = 0m;
			ens814.Quantity3 = 0m;

			// Battery
			ENS81 ens815 = (ENS81)block.MessageBlocks[14];
			ens815.Value = 0m;
			ens815.Duty = 0m;
			ens815.Quantity1 = 1000m;
			ens815.Quantity2 = 0m;
			ens815.Quantity3 = 0m;

			#endregion

			ENS90 ens90 = (ENS90)block.MessageBlocks[15];

			ens90.TotalEstimatedDuty = ens50.Duty + ens70.Duty + ens80.Duty + ens811.Duty + ens812.Duty + ens813.Duty + ens814.Duty;
			ens90.TotalValueOfEntrySummary = ens40.Value + ens70.Value + ens80.Value + ens811.Value + ens812.Value + ens813.Value + ens814.Value;
			block.Y.TotalEstimatedDuty = ens90.TotalEstimatedDuty;

			MQEDIMessage ediMessage = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(MQEDIMessage));
			ediMessage.EM_MessageText = block.Serialise();
			ediMessage.EM_MessageType = "EI";
			ediMessage.EM_MessageSubType = "EIR";
			Factory.Save();
			Assert(true);
		}

		[TestDate(2016, 03, 27)]
		public void Test94()
		{
			/*---------------------------------------------------------------------
94) 8536490055 TARIFF NUMBER
17100 ENTERED VALUE
JP COUNTRY OF ORIGIN
JPMATELE288OSA MANUFACTURER/SUPPLIER ID CODE
9000 QUANTITY
832264 CLASSIFICATION RULINGS NUMBER
95-156732200 IMPORTER OF RECORD NUMBER*/

			declaration.US_BondType = "9";
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = 20000m;
			declaration.US_SuretyCode = "891";
			declaration.US_BondProducerAccNo = "AB12345678";
			invoiceLine.JI_Tariff = "8536490055";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 17100m;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			manufacturer.OH_RL_NKClosestPort = "JPTYO";
			manufacturerCode.OK_CustomsRegNo = "JPMATELE288OSA";
			invoiceLine.JI_CustomsQuantity = 9000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_PIRPRulingType = "C";
			invoiceLine.US_PIRPRulingNo = "832264";
			importerCustomsCode.OK_CustomsRegNo = "95-156732200";
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			AddFCCData(invoiceLine);
			MergeAndSend("94");
		}

		[TestDate(2016, 03, 27)]
		public void Test96()
		{
			/*---------------------------------------------------------------------
96) 2401106130 TARIFF NUMBER
CO COUNTRY OF ORIGIN
CO COUNTRY OF EXPORT
J SPECIAL PROGRAMS INDICATOR*/

			invoiceLine.JI_Tariff = "2401106130";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			USCCountry columbia = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, Core.Constants.CountryCodes.Colombia);
			columbia.UC_SPIEndDate = ZDateTime.Empty;

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Colombia;
			invoiceHeader.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Colombia;

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.J;
			MergeAndSend("96");
		}

		[TestDate(2016, 03, 27)]
		public void Test97()
		{
			/*---------------------------------------------------------------------
97) 6111305070 TSUSA
TW COUNTRY OF ORIGIN
TW COUNTRY OF EXPORT
963 KILOS (QUANTITY)
90 DOZEN (UNIT OF MEASURE)*/
			invoiceLine.JI_Tariff = "6111305070";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceHeader.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_CustomsQuantity = 90m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_CustomsSecondQuantity = 963m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("97");

			AssertEquals(0m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(0m, invoiceLine.CusEntryLine.Header.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2016, 03, 27)]
		public void Test98()
		{
			/*---------------------------------------------------------------------
98) 6206900040 TARIFF NUMBER
TW COUNTRY OF ORIGIN
TW COUNTRY OF EXPORT
1800 KG QUANTITY

NOTE: ARTICLES CONTAIN NO COTTON*/

			invoiceLine.JI_Tariff = "6206900040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceHeader.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_Weight = 1800m;
			invoiceLine.JI_CustomsSecondQuantity = 1800m;//affects the cotton fee
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;//exempt
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "TWBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("98");

			AssertNull("Invoice line should not have Cotton Fees as it was exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			helper.MessageMustContain(declaration, "1800", "999999999");
		}

		[TestDate(2016, 03, 27)]
		public void Test99()
		{
			/*---------------------------------------------------------------------
99) ZA COUNTRY OF ORIGIN
D SPI
4113903000 HTS NUMBER*/

			invoiceHeader.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.D;
			invoiceLine.JI_Tariff = "4113903000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "N/A";
			MergeAndSend("99");
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			base.PopulateMessage(entryHeader, certifyCargoRelease);
			entryHeader.CH_BGMReference = "Passing " + entryHeader.CH_BGMReference;
		}

		void AddFCCData(JobComInvoiceLine invoiceLine)
		{
			FCC fcc = invoiceLine.FCCs.AddNew();
			fcc.US_FCCImpCondNo = "02";
			fcc.US_FCCModel = "MODEL";
			fcc.US_FCCQty = 1;
			fcc.US_FCCTradeName = "TRADE NAME";
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		void CreateNewOrGetExistingCusCodeTypeForFIRMSCode(ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, code, "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
