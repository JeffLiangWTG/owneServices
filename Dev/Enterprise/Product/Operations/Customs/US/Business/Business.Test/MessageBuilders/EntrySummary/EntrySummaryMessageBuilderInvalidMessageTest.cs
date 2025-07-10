using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryMessageBuilderInvalidMessageTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void Test07()
		{
			/*
			07) 11                      MODE OF TRANSPORTATION
			3205                        U.S. PORT OF UNLADING
			111176391                   I.T NUMBER
			879348                      BILL OF LADING(MASTER)
			 * */
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.PrimaryMasterBill.ITNumber = "111176391";
			declaration.US_ITDate = ZDateTime.Today;
			declaration.US_SchDArrival = "3205";
			AssertHasMessageErrors(declaration.US_SchDArrivalInfo);
			declaration.JE_MasterBill = "879348";
			MergeAndSend("7", false, true);
		}

		public void Test09()
		{
			/*
09) V1111124246                 I.T. NUMBER
287 CTN                     I.T. QUANTITY/ UNIT OF MEASURE
		*/
			declaration.PrimaryMasterBill.ITNumber = "V1111124246";
			declaration.PrimaryMasterBill.CU_NoOfPacks = 287;
			declaration.PrimaryMasterBill.CU_PackType = "CTN";
			declaration.PrimaryMasterBill.AddInfoValidation.ValidateUS_ITDate();
			MergeAndSend("9", false, true);
			AssertHasMessageErrors("An IT date must be entered when an IT number is supplied", declaration.US_ITDateInfo);
		}

		public void Test11()
		{
			/*
11) 01/21/YY                    DATE OF EXPORT
	02/01/YY                    DATE OF IMPORT
	01/22/YY                    I.T. DATE
	111271425                   I.T. NUMBER

	YY=CURRENT YEAR
			 * */
			declaration.JE_ExportDate = new ZDateTime(ZDate.Today.Year, 1, 21);
			declaration.JE_DateOfArrival = new ZDateTime(ZDate.Today.Year, 2, 1);
			declaration.US_ITDate = new ZDateTime(ZDate.Today.Year, 1, 22);
			declaration.PrimaryMasterBill.ITNumber = "111271425";
			MergeAndSend("11", false, true);
			AssertHasMessageErrors("IT Date may not be before Date of arrival", declaration.US_ITDateInfo);
		}

		public void Test17()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._06, "Diamond Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"^\w{9}$");
			Factory.Save();

			/*
17)  7102211020                 TARIFF NUMBER
	 EC000383                   DIAMOND CERTIFICATE NUMBER
			 */

			invoiceLine.JI_Tariff = "7102211020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_MiscPermitNo = "EC000383";
			MergeAndSend("17", false, true);
			AssertHasMessageErrors("Invalid permit format", invoiceLine.US_MiscPermitNoInfo);
		}

		[TestDate(2013, 1, 7)]
		public void Test22()
		{
			/*
22)  6801000000                 TARIFF NUMBER
	 2130                       ENTERED VALUE
	 11                         ENTRY TYPE
			*/

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EstimatedEntryDate = ValidationConstants.InformalEntryLimit2500StartDate.AddDays(-1);
			invoiceLine.JI_Tariff = "6801000000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 2130m;
			MergeAndSend("22", false, true);
			AssertHasMessageErrors(string.Format(FormalImportAddInfoJobDeclarationValidation.CustomsValueExceedsAmericanGoodsReturnedFreeDutiableMaxValue, FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue), declaration.US_EntryTypeInfo);
		}

		public void Test23()
		{
			/*
23)  11                         ENTRY TYPE
	 227                        VALUE
	 6203322030                 TARIFF NUMBER
	 HK                         COUNTRY OF ORIGIN
			*/

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoiceLine.JI_LinePrice = 227m;
			invoiceLine.JI_Tariff = "6203322030";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._2;

			/*Entry type 11 is an informal entry.  
			 * Tariff No. 6203.32.20.30 is subject to quota type 333 
			 * per the Harmonized Tariff of the United States of America (HTUSA). 
			 * The quota designator is found in parentheses after the tariff number in the HTUSA.   
			 * Per US Customs regulation, goods subject to quota will always be submitted via a formal entry.  			
			*/
			MergeAndSend("23", false, true);
		}

		//Phyllis confirmed that the case was revoked
		public void Test31()
		{
			/*
31) 2844200020 TARIFF NUMBER
FR COUNTRY OF ORIGIN
20000 ENTERED VALUE
20 QUANTITY
A427818001 ANTIDUMPING CASE NO.
C427819001 COUNTERVAILING CASE NO.
21000 ANTIDUMPING SPECIAL DEPOSIT VALUE
			 */
			string addCaseNo = "A427818001";
			string cvdCaseNo = "C427819001";
			string tariffNo = "2844200020";
			SetupADDCaseCVDCase(addCaseNo, tariffNo, "FR", ACCaseStatusList.Codes.IO);
			SetupADDCaseCVDCase(cvdCaseNo, tariffNo, "FR", ACCaseStatusList.Codes.IO);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			invoiceLine.JI_Tariff = tariffNo;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_InvoiceQuantity = 20m;
			invoiceLine.US_ADDCaseNo = addCaseNo;
			invoiceLine.US_CVDCaseNo = cvdCaseNo;
			invoiceLine.US_ADDDepositValue = 21000m;

			MergeAndSend("31", false, true);
			helper.MessageMustContain(declaration, addCaseNo, cvdCaseNo, "21000", "20000");
		}

		public void Test51()
		{
			/*
51)  6402195061                 TARIFF NUMBER
		 44010                      VALUE
		 6750                       QUANTITY
			*/

			invoiceLine.JI_Tariff = "6402195061";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 44010m;
			invoiceLine.JI_CustomsQuantity = 6750m;
			invoiceLine.JI_Weight = 1000m;

			/*This is a failure based upon value of the goods.  
				If you look at the tariff number you will see that this classification 
				is based upon a value of over $3 per pair, but not over $6.50 per day.  
				The value of the goods based upon this information is $6.52 per pair.  
				In this scenario, the correct tariff number would be 6402.19.70.61 value 
				over $6.50 per pair, but not over $12.00 per pair.
			 */
			MergeAndSend("51", false, true);
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);

			AssertEquals(string.Format("The unit price ${0} calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows,  > 3.0000 and <= 6.5000. Please review all the invoice lines merged to this entry line. If you have modified line prices or quantities of any invoice line, please save or click Brokerage > Merge to refresh this validation.", unitPrice), invoiceLine.JI_LinePriceInfo.GetMessageErrors().GetFirstMessage());
		}

		public void Test54()
		{
			/*54) 9801001010 TARIFF NUMBER1
3926301000 TARIFF NUMBER 2
US COUNTRY OF ORIGIN*/

			invoiceLine.JI_Tariff = "9801001010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "US";
			JobComInvoiceLine secondaryTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = "3926301000";
			secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			MergeAndSend("54", false, true);
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
		}

		public void Test55()
		{
			/*55) 3001900110 TARIFF NUMBER
GB COUNTRY OF ORIGIN
0 CHARGES*/

			invoiceLine.JI_Tariff = "3001900110";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceLine.Charges.RemoveAndDeleteAll();
			MergeAndSend("55", false, true);
			Assert(invoiceHeader.JZ_Calc_TNIInfo.HasMessageError(string.Format(JobComInvoiceHeaderValidation.NoFreightEntered, invoiceHeader.EnteredValueThresholdForCharges)));
		}

		[TestDate(2008, 12, 31)]
		public void Test56()
		{
			/*56) 0809402000 TARIFF NUMBER
MM/DD/YY DATE OF IMPORT-USE CURRENT MONTH/YEAR
MM/DD/YY ENTRY DATE-USE CURRENT MONTH/YEAR*/
			invoiceLine.JI_Tariff = "0809402000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EntryDate = ZDate.Today;
			declaration.JE_DateOfArrival = ZDate.Today;
			declaration.JE_MasterBill = "OBL56";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			declaration.RunPreSaveValidation();
			if (ZDate.Today.Month >= 6)
			{
				Assert(invoiceLine.JI_TariffInfo.HasMessageErrors());
			}
			else
			{
				MergeAndSend("56", false, false);
			}
			//AssertEquals("Some message error expected", declaration.Notifications.MessageErrors.ToUniqueMessageListString());
		}

		public void Test58()
		{
			/*58) 9802004040 TARIFF NUMBER 1
9207100065 TARIFF NUMBER 2
5000 ENTERED VALUE TARIFF 1
5000 ENTERED VALUE TARIFF 2
100 QUANTITY TARIFF 2*/

			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 5000m;
			invoiceLine.JI_Tariff = "9207100065";
			invoiceLine.US_FCCIndicator = "";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("58", false, true);

			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format("The unit price ${0} calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows", unitPrice));
		}

		//TODO
		public void Test59()
		{
			/*59) 6402191520 TARIFF NUMBER
CU COUNTRY OF ORIGIN
MX COUNTRY OF EXPORT*/

			invoiceLine.JI_Tariff = "6402191520";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";
			invoiceHeader.US_UC_NKCountryOfExport = "MX";
			MergeAndSend("59", false, true);
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, "CUBA is a restricted country");
		}

		//TODO
		[TestDate(2016, 03, 27)]
		public void Test77()
		{
			/*---------------------------------------------------------------------
77) 5701101300 TARIFF NUMBER
IN COUNTRY OF ORIGIN*/

			//Tariff requires specific country - cannot find this reference in electronic tariff
			invoiceLine.JI_Tariff = "5701101300";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "IN";
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "INBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			declaration.RunPreSaveValidation();
			MergeAndSend("77", false, false);
			//AssertEquals("Some message error expected", declaration.Notifications.MessageErrors.ToUniqueMessageListString());
		}

		public void Test80()
		{
			/*---------------------------------------------------------------------
80) 98110060 TARIFF NUMBER 1, LINE 1
6404119010 TARIFF NUMBER 2, LINE 1
2000 ENTERED VALUE TARIFF NUMBER 1
0 ENTERED VALUE TARIFF NUMBER 2
100 QUANTITY TARIFF NUMBER 2*/

			invoiceLine.JI_Tariff = "98110060";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			JobComInvoiceLine secondaryTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = "6404119010";
			secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.JI_LinePrice = 2000m;
			secondaryTariff.JI_LinePrice = 9m;
			secondaryTariff.JI_CustomsQuantity = 100m;
			secondaryTariff.JI_Weight = 10m;

			MergeAndSend("80", false, true);
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
		}

		public void Test84()
		{
			/*--------------------------------------------------------------------
84) 6704190000 TARIFF NUMBER
KR COUNTRY OF ORIGIN
US COUNTRY OF EXPORT*/

			invoiceLine.JI_Tariff = "6704190000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceHeader.US_UC_NKCountryOfExport = "US";
			MergeAndSend("84", false, true);
			Assert(invoiceHeader.US_UC_NKCountryOfExportInfo.GetMessageErrors().Contains(ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport));
		}

		//TODO: Waiting for response from Cindy
		public void Test89()
		{
			/*---------------------------------------------------------------------
89) 3926902100 TARIFF NUMBER
CI COUNTRY OF ORIGIN
CM COUNTRY OF EXPORT
A SPECIAL PROGRAM INDICATOR CLAIM*/

			invoiceLine.JI_Tariff = "3926902100";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = "CI";
			invoiceHeader.US_UC_NKCountryOfExport = "CM";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.D;
			//AssertHasMessageError(invoiceLine.US_SPIInfo, "SPI Claim invalid for tariff");
			//invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;

			MergeAndSend("89", false, true);
			//AssertHasMessageError(invoiceLine.US_SPIInfo, "origin and export must match for claim");
		}

		/// <summary>
		/// This importer(69-9999999JC) number has been voided. Will result in a rejection
		/// Using 91-013199000 results in a rejection as the consignee is not registered as DCASR consignees
		/// </summary>
		[TestDate(2016, 03, 27)]
		public void Test92()
		{
			/*---------------------------------------------------------------------
92) THIS SHIPMENT INVOLVES A DIRECT SHIPMENT OF EMERGENCY WAR
MATERIALS TO A U.S. FEDERAL GOVERNMENT AGENCY OTHER THAN THE
DCMAO(DEFENSE CONTRACT MANAGEMENT AREA OPERATIONS. SEND THE
CORRECT ENTRY TYPE. HINT: ENTRY TYPE IS NOT 01.

69-9999999JC IMPORTER OF RECORD NUMBER
9 BOND TYPE
9808003000 TARIFF NUMBER 1
9301190000 TARIFF NUMBER 2*/

			declaration.US_EntryType = EntryTypeList.Codes.DCASR;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_SuretyCode = "891";
			declaration.US_BondAmount = 10000m;
			declaration.US_BondProducerAccNo = "OTT1";
			invoiceLine.US_SupTariff = "9808003000";
			invoiceLine.JI_Tariff = "9301190000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("92");
		}

		[TestDate(2008, 1, 5)]
		public void Test9812PermanentExhibition()
		{
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			AssertHasMessageError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.PaperBasedEntryType);

			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.JI_Tariff = "9812002000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			JobComInvoiceLine secondaryTariff = invoiceLine.AddSecondaryInvoiceLine();
			secondaryTariff.JI_Tariff = "9703.00.00 00";
			secondaryTariff.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9812", false, true);
		}

		[TestDate(2008, 1, 3)]
		public void Test9818VesselPartsAndRepairs()
		{
			declaration.US_EntryType = EntryTypeList.Codes.AircraftVesselSupplyIE;
			AssertHasMessageError(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.PaperBasedEntryType);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine.US_SupTariff = "9818.00.07 00";//50%
			invoiceLine.JI_Tariff = "8903.10.00 15";//2.4%
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);
			MergeAndSend("9818", false, true);
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			base.PopulateMessage(entryHeader, certifyCargoRelease);
			entryHeader.CH_BGMReference = "Failing " + entryHeader.CH_BGMReference;
		}

		void SetupADDCaseCVDCase(string caseNo, string tariffNo, string countryCode, string caseStatus)
		{
			var acCase = Factory.LoadTop1<USCACCase>(new ZQuery(USCACCaseSchema.U5_CaseNumber, caseNo));
			if (acCase == null)
			{
				acCase = Factory.New<USCACCase>();
				acCase.U5_CaseNumber = caseNo;
				acCase.U5_ISOCountryCode = countryCode;
				acCase.U5_CaseStatus = caseStatus;
				acCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

				var acCaseTariff = acCase.CaseTariffs.AddNew();
				acCaseTariff.U9_TariffNumber = tariffNo;
				Factory.Save();
			}
		}
	}
}
