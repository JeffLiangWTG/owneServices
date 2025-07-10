using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USACEFDAAddInfoValidation : AutoUSACEFDAAddInfoValidation
	{
		public USACEFDAAddInfoValidation(AutoUSACEFDAAddInfo parent)
			: base(parent)
		{
		}

		protected new USACEFDAAddInfo Parent
		{
			get { return (USACEFDAAddInfo)base.Parent; }
		}

		public override void ValidateAll()
		{
			FDA.ClearRowNotifications();
			base.ValidateAll();
		}

		protected ACEFDA FDA
		{
			get { return Parent.Parent; }
		}

		protected bool EnableDocAddressForFDA => FDA?.EnableDocAddressForFDA ?? false;

		protected override void CheckUS_ProgramCode()
		{
			base.CheckUS_ProgramCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramCodeInfo, Parent.Lookups.ProgramCodeList);
		}

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingCodeInfo, Parent.Lookups.ProcessingCodeList);
			ValidateUS_ProductCode();
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckUS_ProductCode()
		{
			base.CheckUS_ProductCode();

			if (Parent.US_ProductCode.Length == ValidFDAProductCodeLength)
			{
				var fda = FDA;
				var effectiveDate = fda?.InvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today;
				var productNumber = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, Parent.US_ProductCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode, effectiveDate);
				if (productNumber == null)
				{
					Parent.US_ProductCodeInfo.AddWarning(USFDAAddInfoValidation.UnknownFDAProductCode);
				}

				var industryCode = fda.IndustryCode;
				var intindustryCode = ZInt.ParseSafe(industryCode, 0);

				if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.DRU)
				{
					CheckProductCodeFormatForDrugsNew(Parent.US_ProductCodeInfo);
				}

				if (fda.IsBiologics)
				{
					if (industryCode != "57")
					{
						Parent.US_ProductCodeInfo.AddMessageError(IndustryCodeMismatchForBIO);
					}
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.COS)
				{
					if (industryCode != "50" && industryCode != "53")
					{
						Parent.US_ProductCodeInfo.AddMessageError(IndustryCodeMismatchForCOS);
					}
				}
				else if (fda.IsFood && Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_CCW)
				{
					CheckProductCodeFormatForFOO_CCW(Parent.US_ProductCodeInfo);
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.DEV)
				{
					if (intindustryCode < 73 || intindustryCode > 92)
					{
						Parent.US_ProductCodeInfo.AddMessageError(IndustryCodeMismatchForDEV);
					}
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.RAD)
				{
					if (intindustryCode < 94 || intindustryCode > 97)
					{
						Parent.US_ProductCodeInfo.AddMessageError(IndustryCodeMismatchForRAD);
					}
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB)
				{
					if (industryCode != "98")
					{
						Parent.US_ProductCodeInfo.AddMessageError(IndustryCodeMismatchForTOB);
					}
				}
				else if (Parent.US_ProgramCode == FDAProgramCodeList.Codes.VME)
				{
					CheckProductCodeFormatForVME(Parent.US_ProductCodeInfo);
				}

				if (Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._081006 && !ValidProductCodesForIntendedCode081006.Split(',').Contains(Parent.US_ProductCode.ToString()))
				{
					Parent.US_ProductCodeInfo.AddMessageError(InvalidProductCodeForIntendedCode081006);
				}
			}
			else if (!Parent.US_ProductCode.IsEmpty)
			{
				Parent.US_ProductCodeInfo.AddMessageError(InvalidFDAProductCodeLength);
			}
		}
		const int ValidFDAProductCodeLength = 7;
		internal const string InvalidFDAProductCodeLength = "Invalid FDA Product Code. Valid format for FDA Product Code is 7 alphanumeric.";

		const string ValidProductCodesForIntendedCode081006 = "80O--UG,86N--FF,86N--FG,80N--XQ,90L--MB,90L--MD";
		internal const string InvalidProductCodeForIntendedCode081006 = "This is an invalid product code for the selected intended use code.";

		internal const string IndustryCodeMismatchForBIO = "The first two characters of FDA Product Code (Industry Code) should be 57 for Biologics.";
		internal const string IndustryCodeMismatchForCOS = "The first two characters of FDA Product Code (Industry Code) should be 50 or 53 for Cosmetics.";
		internal const string IndustryCodeMismatchForDEV = "The first two characters of FDA Product Code (Industry Code) should be between 73 and 92 for Medical Devices.";
		internal const string IndustryCodeMismatchForRAD = "The first two characters of FDA Product Code (Industry Code) should be between 94 and 97 for Radiation Emitting Products.";
		internal const string IndustryCodeMismatchForTOB = "The first two characters of FDA Product Code (Industry Code) should be 98 for Tobacco.";

		void CheckProductCodeFormatForFOO_CCW(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			var industryCode = fda.IndustryCode;
			var classCode = fda.ClassCode;
			var subClassCode = fda.SubClassCode;
			var picCode = fda.PIC;

			if (industryCode == "52" && new ZString("A,B,E,Y").OccurrencesIgnoringCase(classCode) == 1)
			{
				if (new ZString("B,E,Y").OccurrencesIgnoringCase(classCode) == 1 && (subClassCode != "Y" || picCode != "Y"))
				{
					propertyInfo.AddMessageError(IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
				}
			}
			else
			{
				propertyInfo.AddMessageError(IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW);
			}
		}
		internal const string IndustryAndClassCodeAndSubClassCodeAndPICMismatchFOO_CCW = "If Government Agency Processing Code is CCW then first two characters of FDA Product Code (Industry Code) should be 52 for Foods and 3th character of Product Code (Class code) should be A, B, E, Y, if Product Code (Class Code) is B, E, or Y, then 4th character of Product Code (Subclass) should be Y and 5th character of Product Code (PIC) should be Y.";

		void CheckProductCodeFormatForVME(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			var industryCode = fda.IndustryCode;
			var subClassCode = fda.SubClassCode;
			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADE)
			{
				if (industryCode != "68")
				{
					propertyInfo.AddMessageError(IndustryAndSubClassCodeMismatchADE);
				}
			}
			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADR)
			{
				var industryCodeInSpecificRange = new ZString("56,58,60,61,62,63,64,65,66,67").OccurrencesIgnoringCase(industryCode) == 1;
				var product54WithSpecificSubClassCodes = industryCode == "54" && new ZString("N,R").OccurrencesIgnoringCase(subClassCode) == 1;
				if (!industryCodeInSpecificRange && !product54WithSpecificSubClassCodes)
				{
					propertyInfo.AddMessageError(IndustryAndSubClassCodeMismatchADR);
				}
			}
		}
		internal const string IndustryAndSubClassCodeMismatchADE = "If Government Agency Processing Code is ADE then first two characters of FDA Product Code (Industry Code) should be 68";
		internal const string IndustryAndSubClassCodeMismatchADR = "If Government Agency Processing Code is ADR then first two characters of FDA Product Code (Industry Code) should be 56, 58, 60, 61, 62, 63, 64, 65, 66 or 67, or if Industry Code is 54 then 4th character of product code (Subclass) should be N or R";

		bool IsIndustryCodeInSpecificRangeForDrugs(ZString industryCode) => new ZString("56,58,60,61,62,63,64,65,66").Occurrences(industryCode) == 1;
		bool IsIndustryCodeInSpecificRangeForDRU_804(ZString industryCode) => new ZString("54,56,60,61,62,63,64,65,66").Occurrences(industryCode) == 1;
		bool IsProduct54WithSpecificSubClassCodesForDrugs(ZString industryCode, ZString subClassCode) => industryCode == "54" && new ZString("D,E,F,G,I").OccurrencesIgnoringCase(subClassCode) == 1;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckProductCodeFormatForDrugsNew(ZPropertyInfo propertyInfo)
		{
			var fda = FDA;
			var subClassCode = fda.SubClassCode;
			var industryCode = fda.IndustryCode;

			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_PRE ||
				Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_OTC ||
				Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_INV ||
				Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_RND)
			{
				if (!IsIndustryCodeInSpecificRangeForDrugs(industryCode) && !IsProduct54WithSpecificSubClassCodesForDrugs(industryCode, subClassCode))
				{
					propertyInfo.AddMessageError(IndustryAndSubClassCodeMismatchDRU);
				}
			}
			else if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804)
			{
				if (!IsIndustryCodeInSpecificRangeForDRU_804(industryCode))
				{
					propertyInfo.AddMessageError(IndustryAndSubClassCodeMismatchDRU804);
				}
			}

			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_INV)
			{
				if ((Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180009 ||
						 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180026 ||
						 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._920000) && subClassCode != "I")
				{
					propertyInfo.AddMessageError(SubClassCodeMismatchDRU_INV);
				}

				if (IsIndustryCodeInSpecificRangeForDrugs(industryCode) && subClassCode != "I")
				{
					propertyInfo.AddMessageError(IndustryCodeAndSubClassCodeMismatchINV);
				}

				if (industryCode == "54" && subClassCode != "I")
				{
					propertyInfo.AddMessageError(IndustryCode54AndSubClassCodeMismatchINV);
				}
			}

			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_PRE)
			{
				if ((Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080012 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._100000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150017 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._155009 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._920000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._970000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._980000) &&
					 subClassCode != "C" &&
					 subClassCode != "D")
				{
					propertyInfo.AddMessageError(SubClassCodeMismatchDRU_PRE);
				}

				if (IsIndustryCodeInSpecificRangeForDrugs(industryCode) && subClassCode != "C" && subClassCode != "D")
				{
					propertyInfo.AddMessageError(IndustryCodeAndSubClassCodeMismatchPRE);
				}

				if (industryCode == "54" && subClassCode != "F" && subClassCode != "G")
				{
					propertyInfo.AddMessageError(IndustryCode54AndSubClassCodeMismatchPRE);
				}
			}

			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_OTC)
			{
				if ((Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._100000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._130000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150017 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._155009 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._920000 ||
					 Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._970000) &&
					 subClassCode != "A" &&
					 subClassCode != "B")
				{
					propertyInfo.AddMessageError(SubClassCodeMismatchDRU_OTC);
				}

				if (IsIndustryCodeInSpecificRangeForDrugs(industryCode) && subClassCode != "A" && subClassCode != "B")
				{
					propertyInfo.AddMessageError(IndustryCodeAndSubClassCodeMismatchOTC);
				}

				if (industryCode == "54" && subClassCode != "D" && subClassCode != "E")
				{
					propertyInfo.AddMessageError(IndustryCode54AndSubClassCodeMismatchOTC);
				}
			}

			if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_OTC || Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_PRE)
			{
				var picCode = fda.PIC;

				if ((Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007 || Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150017) && picCode != "S")
				{
					propertyInfo.AddMessageError(PicMismatchPREOTC);
				}
				else if (Parent.US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150013 && picCode != "T")
				{
					propertyInfo.AddMessageError(PicMismatchPREOTC150013);
				}
			}
		}
		internal const string IndustryAndSubClassCodeMismatchDRU = "If Government Agency Processing Code is PRE, OTC, INV or RND then first two characters of FDA Product Code (Industry Code) should be 56, 58, 60, 61, 62, 63, 64, 65 or 66, or if Industry Code is 54 then 4th character of product code (Subclass) should be D, E, F, G or I";
		internal const string IndustryAndSubClassCodeMismatchDRU804 = "If Government Agency Processing Code is 804 then first two characters of FDA Product Code (Industry Code) should be 54, 56, 60, 61, 62, 63, 64, 65 or 66";
		internal const string SubClassCodeMismatchDRU_INV = "If Government Agency Processing Code is INV and Intended Use Code is 180.009, 180.026, or 920.000 then 4th character of product code (Subclass) should be I";
		internal const string SubClassCodeMismatchDRU_PRE = "If Government Agency Processing Code is PRE and Intended Use Code is 080.012, 100.000, 150.007, 150.017, 155.009, 920.000, 970.000 or 980.000 then 4th character of product code (Subclass) should be C or D";
		internal const string SubClassCodeMismatchDRU_OTC = "If Government Agency Processing Code is PRE and Intended Use Code is 100.000, 130.000, 150.007, 150.017, 155.009, 920.000 or 970.000 then 4th character of product code (Subclass) should be A or B";
		internal const string IndustryCodeAndSubClassCodeMismatchINV = "If Government Agency Processing Code is INV and Industry Code is 56, 58, 60, 61, 62, 63, 64, 65 or 66 then 4th character of product code (Subclass) should be I";
		internal const string IndustryCodeAndSubClassCodeMismatchPRE = "If Government Agency Processing Code is PRE and Industry Code is 56, 58, 60, 61, 62, 63, 64, 65 or 66 then 4th character of product code (Subclass) should be C or D";
		internal const string IndustryCodeAndSubClassCodeMismatchOTC = "If Government Agency Processing Code is OTC and Industry Code is 56, 58, 60, 61, 62, 63, 64, 65 or 66 then 4th character of product code (Subclass) should be A or B";
		internal const string IndustryCode54AndSubClassCodeMismatchINV = "If Government Agency Processing Code is INV and Industry Code is 54 then 4th character of product code (Subclass) should be I";
		internal const string IndustryCode54AndSubClassCodeMismatchPRE = "If Government Agency Processing Code is PRE and Industry Code is 54 then 4th character of product code (Subclass) should be F or G";
		internal const string IndustryCode54AndSubClassCodeMismatchOTC = "If Government Agency Processing Code is OTC and Industry Code is 54 then 4th character of product code (Subclass) should be D or E";
		internal const string PicMismatchPREOTC = "If Government Agency Processing Code is PRE or OTC and Intended Use Code is 150.007 or 150.017 then 5th character of product code (Pic) should be S";
		internal const string PicMismatchPREOTC150013 = "If Government Agency Processing Code is PRE or OTC and Intended Use Code is 150.013 then 5th character of product code (Pic) should be T";
		internal const string IndustryCodeMismatchDRU = "If Sub Type is PRE, OTC, INV or RND, then first two characters of FDA Product Code (Industry Code) should be 56, 60, 61, 62, 63, 64, 65, or 66.";
		internal const string IndustryCodeMismatchPHN = "If Sub Type is PHN, then first two characters of FDA Product Code (Industry Code) should be 55.";
		internal const string SubClassMismatchRxFor180009 = "If Sub Type is PRE, OTC or INV and Intended Use Code is 180.009, then 4th character of product code (Subclass) should be 'I'.";
		internal const string SubClassMismatchPRE = "If Sub Type is PRE, then 4th character of product code (Subclass) should be 'C' or 'D'.";
		internal const string SubClassMismatchOTC = "If Sub Type is OTC, then 4th character of product code (Subclass) should be 'A' or 'B'.";
		internal const string SubClassMismatchPREForPIC = "If Sub Type is PRE or OTC and Intended Use Code is 150.007 or 155.100 then 5th character of product code (Pic) should be 'S' or 'T'.";

		protected override void CheckUS_ProdCountry()
		{
			base.CheckUS_ProdCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProdCountryInfo, Parent.Lookups.USCCountryList);

			var fda = FDA;
			if (fda != null && fda.US_ProdCountry.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.US_ProdCountryInfo.AddMessageError(CountryShouldBeCA);
			}
		}
		internal const string CountryShouldBeCA = "Canadian Province is not allowed for FDA reporting, use the country code of 'CA' if Canada.";

		protected bool IsRadiationEmitting
		{
			get { return Parent.US_ProgramCode == FDAProgramCodeList.Codes.RAD; }
		}

		protected override void CheckUS_SourceCountry()
		{
			base.CheckUS_SourceCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SourceCountryInfo, Parent.Lookups.USCCountryList);

			var fda = FDA;
			if (fda != null && fda.US_SourceCountry.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.US_SourceCountryInfo.AddMessageError(CountryShouldBeCA);
			}
		}

		protected override void CheckUS_RefusedCountry()
		{
			base.CheckUS_RefusedCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RefusedCountryInfo, Parent.Lookups.USCCountryList);

			if (Parent.US_RefusedCountry.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.US_RefusedCountryInfo.AddMessageError(CountryShouldBeCA);
			}
		}

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ1Info, Parent.Lookups.FDABaseUQs);
		}

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ2Info, Parent.Lookups.FDAUQs);
			CheckUQForSpecificPrograms(Parent.US_UQ2Info);
		}

		protected override void CheckUS_UQ3()
		{
			base.CheckUS_UQ3();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ3Info, Parent.Lookups.FDAUQs);
			CheckUQForSpecificPrograms(Parent.US_UQ3Info);
		}

		protected override void CheckUS_UQ4()
		{
			base.CheckUS_UQ4();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ4Info, Parent.Lookups.FDAUQs);
			CheckUQForSpecificPrograms(Parent.US_UQ4Info);
		}

		protected override void CheckUS_UQ5()
		{
			base.CheckUS_UQ5();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ5Info, Parent.Lookups.FDAUQs);
			CheckUQForSpecificPrograms(Parent.US_UQ5Info);
		}

		protected override void CheckUS_UQ6()
		{
			base.CheckUS_UQ6();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ6Info, Parent.Lookups.FDAUQs);
			CheckUQForSpecificPrograms(Parent.US_UQ6Info);
		}

		void CheckUQForSpecificPrograms(ZPropertyInfo info)
		{
			if (IsRadiationEmitting || Parent.US_ProgramCode == FDAProgramCodeList.Codes.TOB)
			{
				var value = info.Value.ToString();
				if (!string.IsNullOrEmpty(value) && value != FDAUQList.Codes.CS && value != FDAUQList.Codes.CT && value != FDAUQList.Codes.BX && value != FDAUQList.Codes.PK)
				{
					info.AddMessageError(UQValidationForSpecificPrograms);
				}
			}
		}
		internal const string UQValidationForSpecificPrograms = "Unit Of Measure should be ‘CS’, ‘CT’, ‘BX’ or ‘PK’ when reporting Radiation Emitting Products and Tobacco to FDA.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override void CheckUS_ManufacturerAddress()
		{
			base.CheckUS_ManufacturerAddress();

			if (FDA is ACEFDA fda && !EnableDocAddressForFDA && !fda.US_ManufacturerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(Parent.US_ManufacturerAddressInfo, fda.ManufacturerNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_ManufacturerAddressInfo);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_ManufacturerAddressInfo, fda.ManufacturerAddress);
				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_ManufacturerAddressInfo, fda.ManufacturerAddress, ShouldCheckStateForAddress);
			}
		}

		protected override void CheckUS_ContainerDimType()
		{
			base.CheckUS_ContainerDimType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ContainerDimTypeInfo, Parent.Lookups.CylindricalRectangularList);
		}

		protected override void CheckUS_DimUQ()
		{
			base.CheckUS_DimUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DimUQInfo, Parent.Lookups.DimensionUQs);
		}

		protected override void CheckUS_DeliverToPartyAddress()
		{
			base.CheckUS_DeliverToPartyAddress();

			var fda = FDA;
			var parent = Parent;
			if (fda != null && !EnableDocAddressForFDA && fda.US_DeliverToPartyAddress.IsValid)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(parent.US_DeliverToPartyAddressInfo, fda.DeliveryPartyNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(parent.US_DeliverToPartyAddressInfo);
				OrganisationValidation.ValidateCountryForPGAAddress(parent.US_DeliverToPartyAddressInfo, fda.DeliverToPartyAddress, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, DeliveryToPartyShouldBeUSAddress);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_DeliverToPartyAddressInfo, fda.DeliverToPartyAddress);
				OrganisationValidation.ValidateStateForPGAAddress(fda.US_DeliverToPartyAddressInfo, fda.DeliverToPartyAddress, ShouldCheckStateForAddress);
			}
		}
		internal const string DeliveryToPartyShouldBeUSAddress = "Deliver to Party Address should be an US address.";

		protected override void CheckUS_FDAImporterAddress()
		{
			base.CheckUS_FDAImporterAddress();

			var fda = FDA;
			if (fda != null && !EnableDocAddressForFDA && !fda.US_FDAImporterAddress.IsEmpty)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(Parent.US_FDAImporterAddressInfo, fda.FDAImporterNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_FDAImporterAddressInfo);

				var fdaImporterAddress = fda.FDAImporterAddress;
				if (fdaImporterAddress != null)
				{
					string additionalZipCodeMessage = new ZipCodeValidation().ValidateForZipCode(fda.Factory, fdaImporterAddress.OA_PostCode, fdaImporterAddress.OA_State, fdaImporterAddress.OA_RN_NKCountryCode);
					if (!string.IsNullOrEmpty(additionalZipCodeMessage))
					{
						Parent.US_FDAImporterAddressInfo.AddMessageError(additionalZipCodeMessage);
					}

					OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_FDAImporterAddressInfo, fdaImporterAddress);
					OrganisationValidation.ValidateStateForPGAAddress(fda.US_FDAImporterAddressInfo, fdaImporterAddress, ShouldCheckStateForAddress);
				}
			}
		}

		protected override void CheckUS_FSVPImporterAddress()
		{
			base.CheckUS_FSVPImporterAddress();

			var fda = FDA;
			var fsvpImporterAddress = fda?.FSVPImporterAddress;
			if (!EnableDocAddressForFDA && fsvpImporterAddress != null)
			{
				if (fda.IsFSVPImpRequired)
				{
					ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_FSVPImporterAddressInfo);

					var duns = fsvpImporterAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
					if (duns.IsEmpty)
					{
						Parent.US_FSVPImporterAddressInfo.AddMessageError(DUNSCodeRequired);
					}
					else
					{
						OrganisationValidation.ValidateACEFDAOrganisationCodes(fda.US_FSVPImporterAddressInfo, fda.FSVPImporterNumber);
					}
				}
				OrganisationValidation.ValidateCountryForPGAAddress(Parent.US_FSVPImporterAddressInfo, fsvpImporterAddress, (x) => { return x == Core.Constants.CountryCodes.UnitedStates; }, FSVPMustHaveUSAddress);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_FSVPImporterAddressInfo, fsvpImporterAddress);
				OrganisationValidation.ValidateStateForPGAAddress(fda.US_FSVPImporterAddressInfo, fsvpImporterAddress, ShouldCheckStateForAddress);
			}
		}

		internal const string DUNSUnknown = "UNK";
		internal const string DUNSCodeRequired = "A DUNS Number with an address is required for PGA reporting. Please press F3 in this field and enter both a DUNS Number and an address in the Config > Registration Numbers/Codes for US.";
		internal const string FSVPMustHaveUSAddress = "FSVP Importer must have a US Address";

		protected override void CheckUS_ProducerAddress()
		{
			base.CheckUS_ProducerAddress();

			var fda = FDA;
			if (fda != null && !fda.US_ProducerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(Parent.US_ProducerAddressInfo, fda.ProducerNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_ProducerAddressInfo);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_ProducerAddressInfo, fda.ProducerAddress);
				OrganisationValidation.ValidateStateForPGAAddress(fda.US_ProducerAddressInfo, fda.ProducerAddress, ShouldCheckStateForAddress);
			}
		}
		public const string DRU_804_ShouldBeCanadaAddress = "For the Section 804 Importation Program, the Shipper must have a Canada address";

		protected override void CheckUS_ItemIdentityNumberQualifier()
		{
			base.CheckUS_ItemIdentityNumberQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ItemIdentityNumberQualifierInfo, Parent.Lookups.IdentityNumberQualifierList);
		}

		protected override void CheckUS_ShipmentCountry()
		{
			base.CheckUS_ShipmentCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ShipmentCountryInfo, Parent.Lookups.USCCountryList);

			var fda = FDA;
			if (fda != null && fda.US_ShipmentCountry.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
			{
				Parent.US_ShipmentCountryInfo.AddMessageError(CountryShouldBeCA);
			}
		}

		protected override void CheckUS_ProducerType()
		{
			base.CheckUS_ProducerType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProducerTypeInfo, Parent.Lookups.ProducerFirmTypes);
		}

		protected override void CheckUS_OwnerAddress()
		{
			base.CheckUS_OwnerAddress();

			var fda = FDA;
			if (fda != null && !fda.US_OwnerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(Parent.US_OwnerAddressInfo, fda.OwnerNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_OwnerAddressInfo);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_OwnerAddressInfo, fda.OwnerAddress);
				OrganisationValidation.ValidateStateForPGAAddress(fda.US_OwnerAddressInfo, fda.OwnerAddress, ShouldCheckStateForAddress);
			}
		}

		protected override void CheckUS_FME()
		{
			base.CheckUS_FME();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FMEInfo, Parent.Lookups.FoodFacilityRegistrationExemptionCodes);
		}

		protected override void CheckUS_OA_ShipperAddress()
		{
			base.CheckUS_OA_ShipperAddress();

			var fda = FDA;
			if (fda != null && !EnableDocAddressForFDA && !fda.US_OA_ShipperAddress.IsEmpty)
			{
				OrganisationValidation.ValidateACEFDAOrganisationCodes(Parent.US_OA_ShipperAddressInfo, fda.ShipperNumber);
				ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_OA_ShipperAddressInfo);
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_OA_ShipperAddressInfo, fda.ShipperAddress);
				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ShipperAddressInfo, fda.ShipperAddress, ShouldCheckStateForAddress);

				if (Parent.US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804)
				{
					OrganisationValidation.ValidateCountryForPGAAddress(fda.US_OA_ShipperAddressInfo, fda.ShipperAddress, (x) => { return x == Core.Constants.CountryCodes.Canada; }, DRU_804_ShouldBeCanadaAddress);
				}
			}
		}

		protected override void CheckUS_LocationOfGoodsAddress()
		{
			base.CheckUS_LocationOfGoodsAddress();

			var fda = FDA;
			if (fda != null && fda.US_LocationOfGoodsAddress.IsValid)
			{
				if (fda.US_ProgramCode == FDAProgramCodeList.Codes.DEV)
				{
					Parent.US_LocationOfGoodsAddressInfo.AddMessageError(GoodsLocationNotRequired);
				}
				else
				{
					ZipCodeValidation.ValidateForEmptyZIPForUSAddress(Parent.US_LocationOfGoodsAddressInfo);
					OrganisationValidation.ValidateCountryForPGAAddress(Parent.US_LocationOfGoodsAddressInfo, fda.LocationOfGoodsAddress, (x) => { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x) == Core.Constants.CountryCodes.UnitedStates; }, GoodsLocationShouldBeUSAddress);
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_LocationOfGoodsAddressInfo, fda.LocationOfGoodsAddress, ShouldCheckStateForAddress);
					OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_LocationOfGoodsAddressInfo, fda.LocationOfGoodsAddress);
				}
			}
		}
		internal const string GoodsLocationShouldBeUSAddress = "Goods Location Address should be an US address.";
		internal const string GoodsLocationNotRequired = "Goods Location should not be entered when Program Code is 'DEV'";

		internal static bool ShouldCheckStateForAddress(ZString countryCode)
		{
			return countryCode == Core.Constants.CountryCodes.UnitedStates || countryCode == Core.Constants.CountryCodes.Canada;
		}
	}
}
