using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USACEFDAAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFDAValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			FDA.US_ManufacturerAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_ManufacturerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_ManufacturerAddressInfo, addressCodeWarning);
			FDA.US_ManufacturerAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_ManufacturerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_ManufacturerAddressInfo, addressCodeWarning);

			FDA.US_DeliverToPartyAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_DeliverToPartyAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_DeliverToPartyAddressInfo, addressCodeWarning);
			FDA.US_DeliverToPartyAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_DeliverToPartyAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_DeliverToPartyAddressInfo, addressCodeWarning);

			FDA.US_FDAImporterAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_FDAImporterAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_FDAImporterAddressInfo, addressCodeWarning);
			FDA.US_FDAImporterAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_FDAImporterAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_FDAImporterAddressInfo, addressCodeWarning);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProducerAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_ProducerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_ProducerAddressInfo, addressCodeWarning);
			FDA.US_ProducerAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_ProducerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_ProducerAddressInfo, addressCodeWarning);

			FDA.US_OwnerAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_OwnerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_OwnerAddressInfo, addressCodeWarning);
			FDA.US_OwnerAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_OwnerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_OwnerAddressInfo, addressCodeWarning);

			FDA.US_OA_ShipperAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_OA_ShipperAddressInfo, addressCodeWarning);
			FDA.US_OA_ShipperAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_OA_ShipperAddressInfo, addressCodeWarning);

			FDA.US_LocationOfGoodsAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_LocationOfGoodsAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_LocationOfGoodsAddressInfo, addressCodeWarning);
			FDA.US_LocationOfGoodsAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_LocationOfGoodsAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_LocationOfGoodsAddressInfo, addressCodeWarning);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_FSVPImporterAddress = orgAddress1.PK;
			AssertHasWarning(FDA.US_FSVPImporterAddressInfo, addressDescriptionWarning);
			AssertHasWarning(FDA.US_FSVPImporterAddressInfo, addressCodeWarning);
			FDA.US_FSVPImporterAddress = orgAddress2.PK;
			AssertNoWarning(FDA.US_FSVPImporterAddressInfo, addressDescriptionWarning);
			AssertNoWarning(FDA.US_FSVPImporterAddressInfo, addressCodeWarning);
		}

		public virtual void TestCheckUS_ProgramCode()
		{
			FDA.US_ProgramCode = "ABC";
			AssertHasMessageError(FDA.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			AssertNoMessageError(FDA.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public virtual void TestCheckUS_ProcessingCode()
		{
			FDA.US_ProcessingCode = "ABC";
			AssertHasMessageError(FDA.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLD;
			AssertNoMessageError(FDA.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateFDAProductCodeForIntendedUseCode081006()
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType, "80O--SS", "Des", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType, "80O--UG", "Des", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			FDA.US_IntendedUseCode = "081.006";
			FDA.US_ProductCode = "80O--SS";
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidProductCodeForIntendedCode081006);

			FDA.US_ProductCode = "80O--UG";
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidProductCodeForIntendedCode081006);

			FDA.US_IntendedUseCode = "081.006";
			FDA.US_ProductCode = "80O--SS";
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidProductCodeForIntendedCode081006);

			FDA.US_IntendedUseCode = "";
			FDA.AddInfoValidation.ValidateUS_ProductCode();
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidProductCodeForIntendedCode081006);
		}

		public virtual void TestCheckUS_IntendedUseCode()
		{
			CreateFDAIntendedUseCode(FDAIntendedUseCodesHelper.Codes._180009, "", FDAProgramCodeList.Codes.BIO, FDAProcessingCodeList.Codes.BIO_ALG);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_IntendedUseCode = "555.555";
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180009;
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public virtual void TestFDAProductCode()
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			FDA.US_ProductCode = "24DCS18";
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidFDAProductCodeLength);
			AssertNoWarning(FDA.US_ProductCodeInfo, USFDAAddInfoValidation.UnknownFDAProductCode);

			FDA.US_ProductCode = "01234";
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.InvalidFDAProductCodeLength);

			FDA.US_ProductCode = "somecp1";
			AssertHasWarning(FDA.US_ProductCodeInfo, USFDAAddInfoValidation.UnknownFDAProductCode);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_RND;

			var fdaProduct = testHelper.CreateNewOrGetExistingCusCodeList("US", listType, "50AKT01", "Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fdaProduct.ZZD_Code = "61ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForBIO);

			fdaProduct.ZZD_Code = "57ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForBIO);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			fdaProduct.ZZD_Code = "61ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForCOS);

			fdaProduct.ZZD_Code = "50ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForCOS);

			fdaProduct.ZZD_Code = "53ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForCOS);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			fdaProduct.ZZD_Code = "58ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForDEV);

			fdaProduct.ZZD_Code = "73ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForDEV);

			fdaProduct.ZZD_Code = "92ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForDEV);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			fdaProduct.ZZD_Code = "68ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForRAD);

			fdaProduct.ZZD_Code = "94ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForRAD);

			fdaProduct.ZZD_Code = "97ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForRAD);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			fdaProduct.ZZD_Code = "61ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForTOB);

			fdaProduct.ZZD_Code = "98ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForTOB);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADE;
			fdaProduct.ZZD_Code = "98ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchADE);
			fdaProduct.ZZD_Code = "68ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchADE);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			fdaProduct.ZZD_Code = "18ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchADR);

			fdaProduct.ZZD_Code = "56ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchADR);

			fdaProduct.ZZD_Code = "58ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchADR);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fdaProduct.ZZD_Code = "18ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_RND;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU);

			fdaProduct.ZZD_Code = "58ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU804);

			fdaProduct.ZZD_Code = "58ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU804);

			fdaProduct.ZZD_Code = "54ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndSubClassCodeMismatchDRU804);

			FDA.US_IntendedUseCode = "180.009";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			fdaProduct.ZZD_Code = "60ALT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_INV);

			fdaProduct.ZZD_Code = "60AIT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_INV);

			FDA.US_IntendedUseCode = "100.000";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fdaProduct.ZZD_Code = "56AIT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_PRE);

			fdaProduct.ZZD_Code = "56ACT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_PRE);

			FDA.US_IntendedUseCode = "100.000";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			fdaProduct.ZZD_Code = "56AIT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_OTC);

			fdaProduct.ZZD_Code = "56ABT01";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.SubClassCodeMismatchDRU_OTC);

			FDA.US_IntendedUseCode = "150.007";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fdaProduct.ZZD_Code = "56ACT02";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.PicMismatchPREOTC);

			fdaProduct.ZZD_Code = "56ACS02";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.PicMismatchPREOTC);

			FDA.US_IntendedUseCode = "150.013";
			FDA.AddInfoValidation.ValidateUS_ProductCode();
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.PicMismatchPREOTC150013);

			fdaProduct.ZZD_Code = "56ACT02";
			FDA.US_ProductCode = fdaProduct.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.PicMismatchPREOTC150013);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.US_ProductCode = "XXALT01";
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeMismatchForCOS);
			AssertHasWarningContaining(FDA.US_ProductCodeInfo, USFDAAddInfoValidation.UnknownFDAProductCode);
		}

		public void TestValidateFOO_CCW()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;

			var validIndustryCode = "52";
			string[] validClassesWithoutSubclassCheck = { "A" };
			string[] validClassesWithSubclassCheck = { "B", "E", "Y" };
			string[] validSubclass = { "Y" };
			string[] validProcessIndicatorCode = { "Y" };

			string[] invalidIndustryCodes = { "51", "53", "54" };
			string[] invalidClasses = { "C", "D", "F", "G", "H", "I", "Z" };
			string[] invalidSubclasses = { "J", "K", "L", "M", "N", "X", "Z" };
			string[] invalidProcessIndicatorCodes = { "N", "X", "Z" };

			TestValidIndustryAndClassCode(validIndustryCode, validClassesWithoutSubclassCheck);
			TestValidIndustryClassSubclassAndPIC(validIndustryCode, validClassesWithSubclassCheck, validSubclass, validProcessIndicatorCode);

			TestInvalidIndustryCode(invalidIndustryCodes);
			TestInvalidIndustryAndClassCode(validIndustryCode, invalidClasses);
			TestInvalidIndustryClassSubclassAndPIC(validIndustryCode, validClassesWithSubclassCheck, invalidSubclasses, invalidProcessIndicatorCodes);

			void TestValidIndustryAndClassCode(string industry, string[] classes)
			{
				foreach (var cls in classes)
				{
					var code = $"{industry}{cls}AT01";
					FDA.US_ProductCode = code;
					AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
				}
			}

			void TestValidIndustryClassSubclassAndPIC(string industry, string[] classes, string[] subclasses, string[] processIndicatorCodes)
			{
				foreach (var cls in classes)
				{
					foreach (var subclass in subclasses)
					{
						foreach (var processIndicatorCode in processIndicatorCodes)
						{
							var code = $"{industry}{cls}{subclass}{processIndicatorCode}01";
							FDA.US_ProductCode = code;
							AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
						}
					}
				}
			}

			void TestInvalidIndustryCode(string[] industries)
			{
				foreach (var industry in industries)
				{
					var code = $"{industry}AAT01";
					FDA.US_ProductCode = code;
					AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
				}
			}

			void TestInvalidIndustryAndClassCode(string industry, string[] classes)
			{
				foreach (var cls in classes)
				{
					var code = $"{industry}{cls}AT01";
					FDA.US_ProductCode = code;
					AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
				}
			}

			void TestInvalidIndustryClassSubclassAndPIC(string industry, string[] classes, string[] subclasses, string[] processIndicatorCodes)
			{
				foreach (var cls in classes)
				{
					foreach (var subclass in subclasses)
					{
						foreach (var processIndicatorCode in processIndicatorCodes)
						{
							var code = $"{industry}{cls}{subclass}{processIndicatorCode}01";
							FDA.US_ProductCode = code;
							AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
						}
					}
				}
			}
		}

		public void TestValidateSubClassesForPREAndOTCAndINV()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;

			string[] validIndustryCode = { "56", "58", "60", "61", "62", "63", "64", "65", "66" };
			string[] validSubclassesForPRE = { "C", "D" };
			string[] validSubclassesForOTC = { "A", "B" };
			string[] validSubclassesForINV = { "I" };

			string[] validIndustryCodeFor54 = { "54" };
			string[] validSubclassesForIndustryCode54ForPRE = { "F", "G" };
			string[] validSubclassesForIndustryCode54ForOTC = { "D", "E" };
			string[] validSubclassesForIndustryCode54ForINV = { "I" };

			string[] invalidSubclasses = { "J", "K", "L", "M", "N", "X", "Z" };

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			TestValidSubclasses(validIndustryCode, validSubclassesForPRE, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchPRE);
			TestValidSubclasses(validIndustryCodeFor54, validSubclassesForIndustryCode54ForPRE, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchPRE);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			TestValidSubclasses(validIndustryCode, validSubclassesForOTC, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchOTC);
			TestValidSubclasses(validIndustryCodeFor54, validSubclassesForIndustryCode54ForOTC, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchOTC);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			TestValidSubclasses(validIndustryCode, validSubclassesForINV, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchINV);
			TestValidSubclasses(validIndustryCodeFor54, validSubclassesForIndustryCode54ForINV, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchINV);

			foreach (var industry in validIndustryCode)
			{
				foreach (var subclass in invalidSubclasses)
				{
					var code = $"{industry}B{subclass}T01";
					FDA.US_ProductCode = code;

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchPRE);

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchOTC);

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCodeAndSubClassCodeMismatchINV);
				}
			}

			foreach (var industry in validIndustryCodeFor54)
			{
				foreach (var subclass in invalidSubclasses)
				{
					var code = $"{industry}B{subclass}T01";
					FDA.US_ProductCode = code;

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchPRE);

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchOTC);

					FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
					AssertHasMessageErrorContaining(FDA.US_ProductCodeInfo, USACEFDAAddInfoValidation.IndustryCode54AndSubClassCodeMismatchINV);
				}
			}

			void TestValidSubclasses(string[] industries, string[] subclasses, string validationText)
			{
				foreach (var industry in industries)
				{
					foreach (var subclass in subclasses)
					{
						var code = $"{industry}B{subclass}T01";
						FDA.US_ProductCode = code;
						AssertNoMessageError(FDA.US_ProductCodeInfo, validationText);
					}
				}
			}
		}

		public virtual void TestCheckCountries()
		{
			FDA.US_ProdCountry = "!w";
			AssertHasMessageError(FDA.US_ProdCountryInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_ProdCountry = "CA";
			AssertNoMessageError(FDA.US_ProdCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_ProdCountry = "XA";
			AssertHasMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_SourceCountry = "XA";
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_SourceCountry = "!w";
			AssertHasMessageError(FDA.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_SourceCountry = "CA";
			AssertNoMessageError(FDA.US_SourceCountryInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_RefusedCountry = "!w";
			AssertHasMessageError(FDA.US_RefusedCountryInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_RefusedCountry = "CA";
			AssertNoMessageError(FDA.US_RefusedCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(FDA.US_RefusedCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_RefusedCountry = "XA";
			AssertHasMessageError(FDA.US_RefusedCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_ShipmentCountry = "!w";
			AssertHasMessageError(FDA.US_ShipmentCountryInfo, ListValidation.InvalidCodeMessageError);
			FDA.US_ShipmentCountry = "CA";
			AssertNoMessageError(FDA.US_ShipmentCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);

			FDA.US_ShipmentCountry = "XA";
			AssertHasMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoValidation.CountryShouldBeCA);
		}

		public virtual void TestCheckUS_ManufacturerAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "AU34567");

			FDA.US_ManufacturerAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_ManufacturerAddressInfo, OrganisationValidation.FEICodeFormat);
			AssertHasMessageErrorContaining(FDA.US_ManufacturerAddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "12345678950";
			FDA.AddInfoValidation.ValidateUS_ManufacturerAddress();
			AssertHasMessageErrorContaining(FDA.US_ManufacturerAddressInfo, OrganisationValidation.FEICodeFormat);

			cusCode.OK_CustomsRegNo = "1234567895";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.AddInfoValidation.ValidateUS_ManufacturerAddress();
			AssertNoMessageErrorContaining(FDA.US_ManufacturerAddressInfo, "State should not be empty.");
			AssertNoMessageErrorContaining(FDA.US_ManufacturerAddressInfo, OrganisationValidation.FEICodeFormat);
			AssertHasMessageErrorContaining(FDA.US_ManufacturerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			mainAddress.OA_PostCode = "1023";
			FDA.AddInfoValidation.ValidateUS_ManufacturerAddress();
			AssertNoMessageErrorContaining(FDA.US_ManufacturerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_ManufacturerAddress();
			AssertHasMessageErrorContaining(FDA.US_ManufacturerAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.AddInfoValidation.ValidateUS_ManufacturerAddress();
			AssertNoMessageErrorContaining(FDA.US_ManufacturerAddressInfo, "State should not be empty.");
		}

		public virtual void TestCheckUS_DeliverToPartyAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "123456789";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");

			mainAddress.OA_PostCode = "1123";
			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			mainAddress.OA_RL_NKRelatedPortCode = "AU111";
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			mainAddress.OA_RL_NKRelatedPortCode = "US111";
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			mainAddress.OA_RL_NKRelatedPortCode = "PR111";
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, USACEFDAAddInfoValidation.DeliveryToPartyShouldBeUSAddress);

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.AddInfoValidation.ValidateUS_DeliverToPartyAddress();
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");
		}

		public void TestValidateOthersUQ()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.US_UQ2 = "ML";
			AssertHasMessageError(FDA.US_UQ2Info, USACEFDAAddInfoValidation.UQValidationForSpecificPrograms);
			FDA.US_UQ2 = "CS";
			AssertNoMessageError(FDA.US_UQ2Info, USACEFDAAddInfoValidation.UQValidationForSpecificPrograms);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_UQ2 = "ML";
			AssertHasMessageError(FDA.US_UQ2Info, USACEFDAAddInfoValidation.UQValidationForSpecificPrograms);
			FDA.US_UQ2 = "CS";
			AssertNoMessageError(FDA.US_UQ2Info, USACEFDAAddInfoValidation.UQValidationForSpecificPrograms);
		}

		public virtual void TestCheckUS_FDAImporterAddress()
		{
			var usZip = Factory.New<USCZipCode>();
			usZip.UZ_BeginZipCodeRange = "15000";
			usZip.UZ_EndZipCodeRange = "19699";
			usZip.UZ_State = "NY";
			Factory.Save();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var nlOrg = Factory.New<OrgHeader>();
			var nlOrgAddress = nlOrg.MainAddress;
			nlOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			nlOrgAddress.OA_State = "NH";
			nlOrgAddress.OA_PostCode = "11";

			FDA.US_FDAImporterAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");

			FDA.US_FDAImporterAddress = ZGuid.Empty;
			mainAddress.OA_State = USStateList.Codes.NewYork;
			mainAddress.OA_PostCode = "19444";
			FDA.US_FDAImporterAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertNoMessageError(FDA.US_FDAImporterAddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_PostCode = "88881";
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertHasMessageError(FDA.US_FDAImporterAddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");

			mainAddress.OA_State = USStateList.Codes.Missouri;
			mainAddress.OA_PostCode = "19444";
			FDA.US_FDAImporterAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertHasMessageError(FDA.US_FDAImporterAddressInfo, "The zip code entered for MO is not valid. The first five numbers of the zip code should fall between 63000 and 65899");

			mainAddress.OA_State = USStateList.Codes.NewYork;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertNoMessageError(FDA.US_FDAImporterAddressInfo, "The zip code entered for NY is not valid. The first five numbers of the zip code should fall between 15000 and 19699,\r\nor between 00400 and 00599,\r\nor between 09000 and 14999");
			mainAddress.OA_PostCode = "15123";
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = USStateList.Codes.NewHampshire;
			mainAddress.OA_PostCode = "11";
			FDA.US_FDAImporterAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899");

			FDA.US_FDAImporterAddress = nlOrgAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FDAImporterAddress();
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "The zip code entered for NH is not valid. The first five numbers of the zip code should fall between 03000 and 03899");
		}

		public virtual void TestCheckUS_FSVPImporterAddress()
		{
			var usZip = Factory.New<USCZipCode>();
			usZip.UZ_BeginZipCodeRange = "15000";
			usZip.UZ_EndZipCodeRange = "19699";
			usZip.UZ_State = "NY";
			Factory.Save();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			FDA.US_FSVPImporterAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_State = "NY";
			mainAddress.OA_PostCode = "19444";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertNoMessageError(FDA.US_FSVPImporterAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, "State should not be empty.");

			mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "12345678");
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, OrganisationValidation.DUNSCodeFormat);

			mainAddress.CustomsCodes[0].OK_CustomsRegNo = "123456789";
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertNoMessageError(FDA.US_FSVPImporterAddressInfo, OrganisationValidation.DUNSCodeFormat);

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, "FSVP Importer must have a US Address");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertNoMessageError(FDA.US_FSVPImporterAddressInfo, "FSVP Importer must have a US Address");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.AddInfoValidation.ValidateUS_FSVPImporterAddress();
			AssertNoMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, "State should not be empty.");
		}

		public virtual void TestCheckUS_ProducerType()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProducerType = "!";
			AssertHasMessageError(FDA.US_ProducerTypeInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertNoMessageError(FDA.US_ProducerTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public virtual void TestCheckUS_OwnerAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_OwnerAddress = mainAddress.PK;
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertHasMessageErrorContaining(FDA.US_OwnerAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(FDA.US_OwnerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");

			cusCode.OK_CustomsRegNo = "123456789";
			mainAddress.OA_PostCode = "1123";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertNoMessageErrorContaining(FDA.US_OwnerAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(FDA.US_OwnerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertHasMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertNoMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");
		}

		public void TestCheckUS_FME()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FME = FDAPriorNoticeExemptCodeList.Codes.G;
			AssertHasMessageError(FDA.US_FMEInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_FME = FDAPriorNoticeExemptCodeList.Codes.K;
			AssertNoMessageError(FDA.US_FMEInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestFSVPDUNSNumberValidation()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_FSVPImporterAddress = mainAddress.PK;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoValidation.DUNSCodeRequired);

			var dunsNumber = FDA.FSVPImporterAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, USACEFDAAddInfoValidation.DUNSUnknown, "US");
			FDA.FSVPImporterAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "123456", "US");

			FDA.US_FSVPImporterAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoValidation.DUNSCodeRequired);

			dunsNumber.OK_CustomsRegNo = "423";
			FDA.US_FSVPImporterAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, OrganisationValidation.DUNSCodeFormat);
		}

		public virtual void TestCheckUS_OA_ShipperAddressShouldBeCanada()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_OA_ShipperAddress = mainAddress.PK;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			FDA.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertHasMessageError(FDA.US_OA_ShipperAddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			FDA.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageError(FDA.US_OA_ShipperAddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			AssertNoMessageError(FDA.US_OA_ShipperAddressInfo, USACEFDAAddInfoValidation.DRU_804_ShouldBeCanadaAddress);
		}

		public virtual void TestCheckUS_OA_ShipperAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AU34567");
			FDA.US_OA_ShipperAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_OA_ShipperAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertHasMessageErrorContaining(FDA.US_OA_ShipperAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageError(FDA.US_OA_ShipperAddressInfo, "State should not be empty.");

			mainAddress.OA_PostCode = "2008";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			cusCode.OK_CustomsRegNo = "123456789";
			FDA.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageErrorContaining(FDA.US_OA_ShipperAddressInfo, OrganisationValidation.DUNSCodeFormat);
			AssertNoMessageErrorContaining(FDA.US_OA_ShipperAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageError(FDA.US_OA_ShipperAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertHasMessageError(FDA.US_OA_ShipperAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.NL;
			FDA.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageError(FDA.US_OA_ShipperAddressInfo, "State should not be empty.");
		}

		public void TestCheckUS_LocationOfGoodsAddressWhenDev()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_LocationOfGoodsAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_LocationOfGoodsAddress();
			AssertHasMessageError("Goods Location should not be entered when Program Code is 'DEV'", FDA.US_LocationOfGoodsAddressInfo, USACEFDAAddInfoValidation.GoodsLocationNotRequired);
			FDA.US_LocationOfGoodsAddress = ZGuid.Empty;
			FDA.AddInfoValidation.ValidateUS_LocationOfGoodsAddress();
			AssertNoMessageError("Goods Location should not be entered when Program Code is 'DEV'", FDA.US_LocationOfGoodsAddressInfo, USACEFDAAddInfoValidation.GoodsLocationNotRequired);
		}

		public void TestCheckUS_LocationOfGoodsAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_LocationOfGoodsAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_LocationOfGoodsAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertHasMessageError(FDA.US_LocationOfGoodsAddressInfo, "State should not be empty.");

			mainAddress.OA_PostCode = "2008";
			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.AddInfoValidation.ValidateUS_LocationOfGoodsAddress();
			AssertNoMessageErrorContaining(FDA.US_LocationOfGoodsAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			AssertNoMessageError(FDA.US_LocationOfGoodsAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.US_LocationOfGoodsAddress = mainAddress.PK;
			AssertHasMessageError(FDA.US_LocationOfGoodsAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.NL;
			FDA.US_LocationOfGoodsAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_LocationOfGoodsAddressInfo, "State should not be empty.");
		}

		public void TestCheckUS_ProducerAddress()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = ZString.Empty;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_ProducerAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");

			mainAddress.OA_State = USStateList.Codes.Illinois;
			FDA.US_ProducerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			mainAddress.OA_State = ZString.Empty;
			FDA.US_ProducerAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");

			mainAddress.OA_State = CanadaStatesList.Codes.AB;
			FDA.US_ProducerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");
		}

		public void TestValidatePGAContact()
		{
			var org1 = Factory.New<OrgHeader>();
			var orgAdd1 = org1.Addresses.AddNew();
			var org1Wrapper = OrgHeaderWrapper.New(org1);

			foreach (var propertyInfo in new[] { FDA.US_ManufacturerAddressInfo, FDA.US_DeliverToPartyAddressInfo, FDA.US_OwnerAddressInfo, FDA.US_LocationOfGoodsAddressInfo })
			{
				propertyInfo.Value = orgAdd1.PK;
				Assert("Not required because FDA has a point of contact", !propertyInfo.HasMessageError(string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name")));
				Assert("Not required because FDA has a point of contact", !propertyInfo.HasMessageError(string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone")));
				Assert("Not required because FDA has a point of contact", !propertyInfo.HasMessageError(string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email/Fax")));
			}
		}

		protected void CreateFDAIntendedUseCode(ZString code, ZString description, ZString program, ZString process)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency,
				"", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "US");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram,
				"", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "US");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProcess,
				"", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "US");

			if (description.IsEmpty)
			{
				description = "A";
			}

			var pk = helper.CreateNewOrGetExistingCusCodeList("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode,
				code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime).PK;

			helper.CreateNewOrGetExistingCusCodeListAttribute(pk, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency,
				Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.FDA);
			if (!program.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(pk, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProgram, program);
			}
			if (!process.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(pk, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPGAIntendUseCodeProcess, process);
			}

			Factory.Save();
		}

		protected virtual ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fda = invoiceLine.ACE_FDALines.AddNew();
				}
				return fda;
			}
		}
		ACEFDA fda;
	}
}
