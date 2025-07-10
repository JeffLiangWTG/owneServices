using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USACEFDAAddInfoInvoiceLineValidationTest : USACEFDAAddInfoValidationTest
	{
		public void TestFDAValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "CA";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			FDAValidateState(FDA, FDA.US_ManufacturerAddressInfo);
			FDAValidateState(FDA, FDA.US_DeliverToPartyAddressInfo);
			FDAValidateState(FDA, FDA.US_FDAImporterAddressInfo);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDAValidateState(FDA, FDA.US_ProducerAddressInfo);
		}

		void FDAValidateState(ACEFDA header, ZPropertyInfo propertyInfo)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_RL_NKRelatedPortCode = "CABLA";
			address.OA_State = "XXXX";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");
		}

		public void TestCheckUS_QtyForSection804ImportationProgram()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			fda1.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda1.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			fda1.AddInfoValidation.ValidateAll();
			AssertHasMessageError(fda1.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.QuantityRequiredForSection804);
			fda1.US_Qty1 = 1000m;
			fda1.US_UQ1 = "KG";
			fda1.AddInfoValidation.ValidateAll();
			AssertNoMessageError(fda1.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.QuantityRequiredForSection804);
		}

		public void TestCheckUS_UnitValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_UnitValue = 1000m;
			AssertHasMessageError(fda2.US_UnitValueInfo, USACEFDAAddInfoInvoiceLineValidation.UnitValueNotRequiredForPriorNotice);

			fda2.US_UnitValue = 0m;
			AssertNoMessageError(fda2.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.UnitValueNotRequiredForPriorNotice);

			declaration.US_EnableSPN = false;
			var fda3 = invoiceLine.ACE_FDALines.AddNew();
			fda3.US_InvCurrValue = 1000m;
			fda3.US_Qty1 = 1200m;
			fda3.US_UQ1 = "KG";
			fda3.US_Qty2 = 10m;
			fda3.US_UQ2 = "AE";

			fda3.US_UnitValue = 83.33m;
			AssertHasWarningContaining(fda3.US_UnitValueInfo, "Unit Value should be based on the Base Quantity.");

			fda3.US_UnitValue = fda3.GetDefaultUnitValueFromBaseQty().Value;
			AssertNoWarningContaining(fda2.US_UnitValueInfo, "Unit Value should be based on the Base Quantity.");
		}

		public void TestEnsureThatAtLeastOneContainerSelectedForPN()
		{
			FDA.InvoiceLine.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;

			FDA.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(FDA, USACEFDAAddInfoInvoiceLineValidation.AtLeastOneContainerRequiredForPN);

			FDA.US_ProcessingCode = ZString.Empty;
			FDA.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError(FDA, USACEFDAAddInfoInvoiceLineValidation.AtLeastOneContainerRequiredForPN);

			var container1 = FDA.InvoiceLine.Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";

			var container2 = FDA.InvoiceLine.Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX2345672";

			var npContainer1 = FDA.InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1);
			npContainer1.IsForInvoiceLine = true;
			var npContainer2 = FDA.InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2);
			npContainer2.IsForInvoiceLine = true;

			FDA.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(FDA, USACEFDAAddInfoInvoiceLineValidation.AtLeastOneContainerRequiredForPN);
		}

		public void TestCheckUS_PFR()
		{
			FDA.InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.US_FDAForcePN = true;
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			FDA.US_PFR = ZString.Empty;
			AssertNoMessageError(FDA.US_PFRInfo, ValidationConstants.PriorNotice.FoodFacilityRegistrationNumber);

			FDA.US_ProcessingCode = ZString.Empty;
			FDA.US_PFR = ZString.Empty;
			AssertHasMessageError(FDA.US_PFRInfo, ValidationConstants.PriorNotice.FoodFacilityRegistrationNumber);

			FDA.US_PFR = ZString.Empty;
			AssertNoMessageError(FDA.US_PFRInfo, USACEFDAAddInfoInvoiceLineValidation.FoodRegNoShouldBe11Digits);

			FDA.US_PFR = "123AAA77788";
			AssertHasMessageError(FDA.US_PFRInfo, USACEFDAAddInfoInvoiceLineValidation.FoodRegNoShouldBe11Digits);

			FDA.US_PFR = "1234567891";
			AssertHasMessageError(FDA.US_PFRInfo, USACEFDAAddInfoInvoiceLineValidation.FoodRegNoShouldBe11Digits);
			FDA.US_PFR = "12366677788";
			AssertNoMessageErrorContaining(FDA.US_PFRInfo, USACEFDAAddInfoInvoiceLineValidation.FoodRegNoShouldBe11Digits);
		}

		public override void TestCheckUS_ProgramCode()
		{
			base.TestCheckUS_ProgramCode();
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			AssertHasWarningContaining(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredForRAD);
			FDA.AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.RA1;
			FDA.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoWarningContaining(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredForRAD);

			FDA.US_ProgramCode = ZString.Empty;
			AssertHasMessageErrorContaining(FDA.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			AssertNoMessageErrorContaining(FDA.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "FD4";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(5);
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FD4MustUseFOOD);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FD4MustUseFOOD);
		}

		public override void TestCheckUS_ProcessingCode()
		{
			base.TestCheckUS_ProcessingCode();
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			AssertNoMessageError(FDA.US_ProcessingCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.GovernmentAgencyProcessingCodeRequired, FDA.AddInfoLookups.ProgramCodeList.GetDescriptionFromCode(FDAProgramCodeList.Codes.FOO)));

			FDA.US_ProcessingCode = ZString.Empty;
			AssertHasMessageError(FDA.US_ProcessingCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.GovernmentAgencyProcessingCodeRequired, FDA.AddInfoLookups.ProgramCodeList.GetDescriptionFromCode(FDAProgramCodeList.Codes.FOO)));

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageError(FDA.US_ProcessingCodeInfo, USACEFDAAddInfoInvoiceLineValidation.GovernmentAgencyProcessingCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageError(FDA.US_ProcessingCodeInfo, USACEFDAAddInfoInvoiceLineValidation.GovernmentAgencyProcessingCodeRequired);
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			AssertNoMessageError(FDA.US_ProcessingCodeInfo, USACEFDAAddInfoInvoiceLineValidation.GovernmentAgencyProcessingCodeRequired);
		}

		public void TestValidateLimitAOCCodeByIntendedUseCodeForMedicalDevices()
		{
			string includeRADCodes = ", RA1, RA2, RA3, RA4, RA5, RA6, RA7, RB1, RB2, RC1, RC2, RD1, RD2, RD3, ACC, ANC, MDL, ERR, IFE, CCM";

			#region 081.001
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081001;

			var aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "KIT", FDA.US_IntendedUseCode, "DEV, DFE, LST, IRC, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.KIT;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "KIT", FDA.US_IntendedUseCode, "DEV, DFE, LST, IRC, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DEV, DFE, LST, IRC, LWC, PM#, DI, ERR"));
			#endregion

			#region 081.003
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081003;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, KIT, LST, IRC, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, KIT, LST, IRC, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DDM, DFE, KIT, LST, IRC, LWC, PM#, DI, ERR"));
			#endregion

			#region 081.004
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081004;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "KIT, DEV, DFE, LST, PM#, LWC, IRC, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "KIT, DEV, DFE, LST, PM#, LWC, IRC, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "KIT, DEV, DFE, LST, PM#, LWC, IRC, DI, ERR"));
			#endregion

			#region 081.005
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081005;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "DEV, DFE, LST, DA, IND, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "DEV, DFE, LST, DA, IND, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DEV, DFE, LST, DA, IND, DI, ERR"));
			#endregion

			#region 081.007
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081007;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "CPT, LST, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "CPT, LST, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "CPT, LST, PM#, DI, ERR"));

			#endregion

			#region 081.008
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081008;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "CPT, DA, IND, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "CPT, DA, IND, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "CPT, DA, IND, DI, ERR"));
			#endregion

			#region 180.015
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180015;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "IDE, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DDM", FDA.US_IntendedUseCode, "IDE, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "IDE, DI, ERR"));

			#endregion

			#region 170.000
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._170000;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DI", FDA.US_IntendedUseCode, "DFE, LST, IRC, LWC, PM#, DDM" + includeRADCodes.Replace(", IFE", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DI;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DI", FDA.US_IntendedUseCode, "DFE, LST, IRC, LWC, PM#, DDM" + includeRADCodes.Replace(", IFE", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DFE, LST, IRC, LWC, PM#, DDM"));
			#endregion

			#region 920.001
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._920001;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR"));
			#endregion

			#region 950.002
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._950002;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR" + includeRADCodes.Replace(", ERR", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DDM, DFE, IRC, LST, LWC, PM#, DI, ERR"));
			#endregion

			#region 970.000
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._970000;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "IRC", FDA.US_IntendedUseCode, "DEV, DFE, IFE, LST, DI, ERR" + includeRADCodes.Replace(", ERR", "").Replace(", IFE", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.IRC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "IRC", FDA.US_IntendedUseCode, "DEV, DFE, IFE, LST, DI, ERR" + includeRADCodes.Replace(", ERR", "").Replace(", IFE", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "DEV, DFE, IFE, LST, DI, ERR"));
			#endregion

			#region 970.001
			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._970001;

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "IFE, CPT, DDM, LST, DI, ERR" + includeRADCodes.Replace(", ERR", "").Replace("IFE, ", "")));

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDA.US_IntendedUseCode, "IFE, CPT, DDM, LST, DI, ERR" + includeRADCodes.Replace(", ERR", "").Replace("IFE, ", "")));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageErrorContaining(FDA.US_IntendedUseCodeInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "ACC", FDA.US_IntendedUseCode, "IFE, CPT, DDM, LST, DI, ERR"));
			#endregion
		}

		public void TestValidateAffirmationOfComplianceCodesForFood()
		{
			var invoiceLine = FDA.InvoiceLine;
			var declaration = invoiceLine.Declaration;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;

			var fda2 = invoiceLine.ACE_FDALines.AddNew();

			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.RNORequired);

			fda2.US_ProcessingCode = ZString.Empty;
			AssertHasMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.RNORequired);
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ContainerMeasurementsORVOLIsRequited);

			fda2.AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.RNO;
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoMessageError(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.RNORequired);

			fda2.AffirmationCodes.RemoveAndDeleteAll();
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ContainerMeasurementsORVOLIsRequited);

			fda2.US_ProductCode = "2123E56";
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertHasMessageErrorContaining(fda2.US_ProgramCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
			AssertHasMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ContainerMeasurementsORVOLIsRequited);

			fda2.AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.FCE;
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
			AssertHasMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ContainerMeasurementsORVOLIsRequited);

			fda2.AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.VOL;
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ContainerMeasurementsORVOLIsRequited);

			fda2.AffirmationCodes.RemoveAll();
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_EnableSPN = true;
			fda2.US_ProductCode = "2123E56";
			fda2.AddInfoValidation.ValidateUS_ProgramCode();
			AssertNoMessageErrorContaining(fda2.US_ProgramCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.FCEOrSIDRequited, ACE_AffirmationOfComplianceList.Codes.FCE));
		}

		public void TestValidateAffirmationOfComplianceCodesForMedicalDevices()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_NED;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081005;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081001AndUNK);

			var aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081001AndUNK);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081001AndUNK);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081003;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081003);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081003);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.KIT;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081003);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081004;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081004);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.KIT;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081004);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081004);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180015;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor180015);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.IDE;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor180015);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._920001;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920001_950001);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920001_950001);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920001_950001);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._920002;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920002);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920002);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor920002);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._970001;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970001);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970001);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.CPT;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DDM;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970001);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._970000;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970000);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970000);

			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DFE;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.LST;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor970000);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081007;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081007_081008);
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.CPT;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081007_081008);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081008;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081007_081008);
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.CPT;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.AOCRequiredFor081007_081008);
		}

		public void TestValidateAffirmationCodesLimitIntendedUseCodeVME()
		{
			CreateFDAIntendedUseCode(FDAIntendedUseCodesHelper.Codes._980000, "", FDAProgramCodeList.Codes.VME, FDAProcessingCodeList.Codes.VME_ADR);

			var message = string.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "RD3", FDAIntendedUseCodesHelper.Codes._980000, "REG, NDC, VAN, VNA, VFL, VFD");
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._980000;

			var aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.RD3;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, message);

			FDA.AffirmationCodes.RemoveAndDeleteAll();
			aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, message);
			AssertNoMessageErrors(FDA.US_IntendedUseCodeInfo);
		}

		public void TestValidateAffirmationOfComplianceCodesForDrugs_804()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;

			var aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDAIntendedUseCodesHelper.Codes._080012, "REG, DLS, DA, FSR, PRN"));

			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "DEV", FDAIntendedUseCodesHelper.Codes._080012, "REG, DLS, DA, FSR, PRN"));

			aoc.CY_Code = ACE_AffirmationOfComplianceList.Codes.PLR;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "PLR", FDAIntendedUseCodesHelper.Codes._080012, "REG, DLS, DA, FSR, PRN"));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, string.Format(USACEFDAAddInfoInvoiceLineValidation.LimitAOCCodeByIntendedUseCodeMessage, "PLR", FDAIntendedUseCodesHelper.Codes._080012, "REG, DLS, DA, FSR, PRN"));
		}

		public override void TestCheckUS_IntendedUseCode()
		{
			base.TestCheckUS_IntendedUseCode();
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150007;
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_IntendedUseCode = ZString.Empty;
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PHN;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_IntendedUseCode = ZString.Empty;
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_INV;
			FDA.InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			FDA.AddInfoValidation.ValidateAll();
			AssertHasMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_IntendedUseCode = "180.200";
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.US_IntendedUseCode = ZString.Empty;
			FDA.Validation.ValidateAll();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			FDA.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageError(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes.UNK;
			AssertHasWarning(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeUNKWarning);

			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080000;
			AssertNoWarning(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeUNKWarning);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_HCT;
			FDA.US_ProductCode = "57K12";
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._081002;
			AssertHasWarning(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeBIO_HCTWarning);

			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._082000;
			AssertNoWarning(FDA.US_IntendedUseCodeInfo, USACEFDAAddInfoInvoiceLineValidation.IntendedUseCodeBIO_HCTWarning);
		}

		[TestDate(2017, 02, 01)]
		public override void TestFDAProductCode()
		{
			base.TestFDAProductCode();
			FDA.US_ProductCode = ZString.Empty;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeRequired);

			var productCode = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode, ZDateTime.Now).FirstOrDefault();

			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeRequired);
		}

		public void TestCheckProductCodeFormatForPriorNotice()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;

			var newFactory = new BusinessObjectFactory();
			var testHelper = new UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var productCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "24DCS18", Core.Constants.CountryCodes.UnitedStates, listType, ZDateTime.Now);
			productCode.ZZD_Code = "8123457";

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			FDA.US_ProcessingCode = ZString.Empty;
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "0723456";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "4923456";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "0123456";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "0323456";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "1223456";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "50N0001";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "50C0001";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "52D0000";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "54AY100";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertHasMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);

			productCode.ZZD_Code = "54YL100";
			FDA.US_ProductCode = productCode.ZZD_Code;
			AssertNoMessageError(FDA.US_ProductCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ProductCodeFormatForPriorNotice);
		}

		public override void TestCheckCountries()
		{
			base.TestCheckCountries();
			FDA.AddInfoValidation.ValidateUS_ProdCountry();
			AssertNoMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProdCountry = ZString.Empty;
			FDA.US_SourceCountry = ZString.Empty;
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_SourceCountry = ZString.Empty;
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			FDA.AddInfoValidation.ValidateUS_SourceCountry();
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			FDA.US_ProdCountry = "FR";
			FDA.AddInfoValidation.ValidateUS_SourceCountry();
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProdCountry = ZString.Empty;
			AssertHasMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.US_SourceCountry = "";
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);
			FDA.AddInfoValidation.ValidateUS_ProdCountry();
			AssertHasMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequired);

			FDA.US_ProdCountry = "CA";
			FDA.US_SourceCountry = "";
			FDA.Validation.ValidateAll();
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);
			AssertNoMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequired);
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryNotAllowed);

			FDA.US_ProdCountry = "";
			FDA.US_SourceCountry = "CA";
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryNotAllowed);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_SourceCountry = "";
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryNotAllowed);
			FDA.US_SourceCountry = "CA";
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryNotAllowed);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_SourceCountry = ZString.Empty;
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_SourceCountry();
			AssertHasMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);
			FDA.US_ProdCountry = "FR";
			FDA.AddInfoValidation.ValidateUS_SourceCountry();
			AssertNoMessageError(FDA.US_SourceCountryInfo, USACEFDAAddInfoInvoiceLineValidation.SourceCountryRequired);

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";

			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.US_ShipmentCountry = ZString.Empty;
			AssertNoMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShipmentCountryRequired);

			FDA.US_ProcessingCode = ZString.Empty;
			FDA.US_ShipmentCountry = ZString.Empty;
			AssertHasMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShipmentCountryRequired);

			FDA.InvoiceLine.US_UC_NKCountryOfExport = "FR";
			FDA.AddInfoValidation.ValidateUS_ShipmentCountry();
			AssertNoMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShipmentCountryRequired);

			FDA.US_ShipmentCountry = "AD";
			AssertNoMessageError(FDA.US_ShipmentCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShipmentCountryRequired);

			FDA.US_ProdCountry = "";
			AssertHasMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequiredForFood);
			FDA.US_ProdCountry = "AD";
			AssertNoMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ProdCountryRequiredForFood);
		}

		public void TestCheckProdCountryShouldSameCountryOfManufacturer()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;

			FDA.US_ManufacturerAddress = ZGuid.Empty;
			mainAddress.OA_RL_NKRelatedPortCode = "PR123";
			FDA.US_ManufacturerAddress = mainAddress.PK;
			FDA.US_ProdCountry = "PR";
			AssertHasMessageError(FDA.US_ManufacturerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);
			AssertHasMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "US";
			AssertNoMessageError(FDA.US_ManufacturerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);
			AssertNoMessageError(FDA.US_ProdCountryInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);
		}

		public void TestValidteQtyAndUQ()
		{
			FDA.US_Qty1 = 23m;
			FDA.US_UQ1 = "";
			AssertHasMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.UQRequired);
			FDA.US_UQ1 = "KG";
			AssertNoMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.UQRequired);

			FDA.US_UQ2 = "KG";
			FDA.US_Qty2 = 0m;
			AssertHasMessageError(FDA.US_Qty2Info, USACEFDAAddInfoInvoiceLineValidation.QTYRequired);
			FDA.US_Qty2 = 10m;
			AssertNoMessageError(FDA.US_Qty2Info, USACEFDAAddInfoInvoiceLineValidation.QTYRequired);

			FDA.US_Qty3 = -1m;
			FDA.US_UQ3 = "KG";
			AssertHasMessageError(FDA.US_Qty3Info, USACEFDAAddInfoInvoiceLineValidation.QTYShoudGreaterThanZero);
			FDA.US_Qty3 = 1m;
			AssertNoMessageError(FDA.US_Qty3Info, USACEFDAAddInfoInvoiceLineValidation.QTYShoudGreaterThanZero);
		}

		[TestDate(2016, 01, 04)]
		public void TestValidateBaseQty()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.US_UQ1 = "ML";
			AssertHasMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.BaseUQRequiredPCSForRAD);

			FDA.US_UQ1 = "PCS";
			AssertNoMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.BaseUQRequiredPCSForRAD);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_UQ1 = "BOL";
			AssertHasMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.UQRequiredForTOB);

			FDA.US_UQ1 = "ML";
			AssertNoMessageError(FDA.US_UQ1Info, USACEFDAAddInfoInvoiceLineValidation.UQRequiredForTOB);

			FDA.InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.US_FDAForcePN = true;
			Assert(FDA.IsPriorNotice);

			FDA.US_Qty1 = 0m;
			AssertNoMessageError(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyRequired);

			FDA.US_ProcessingCode = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_Qty1();
			AssertHasMessageError(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_Qty1 = 0m;
			var aoc = FDA.AffirmationCodes.AddNew();
			aoc.CY_Code = "ACC";
			FDA.AddInfoValidation.ValidateUS_Qty1();
			AssertHasMessageError(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyRequiredFor2877);

			FDA.US_Qty1 = 10m;
			AssertNoMessageError(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyRequiredFor2877);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			aoc.CY_Code = "DDD";
			FDA.US_Qty1 = 0m;
			AssertHasWarning(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyWarning);

			FDA.US_Qty1 = 10m;
			AssertNoWarning(FDA.US_Qty1Info, USACEFDAAddInfoInvoiceLineValidation.BaseQtyWarning);
		}

		public override void TestCheckUS_ManufacturerAddress()
		{
			base.TestCheckUS_ManufacturerAddress();
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_ManufacturerAddress = ZGuid.Empty;
			var errorText = string.Format(USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequiredPattern, "Manufacturer");
			AssertHasMessageError(FDA.US_ManufacturerAddressInfo, errorText);

			FDA.US_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_ManufacturerAddressInfo, errorText);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProducerType = "L";
			FDA.US_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_ManufacturerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForProducerType);

			FDA.US_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_ManufacturerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForProducerType);
		}

		public void TestCheckUS_DeliverToPartyAddressNoStateCheckWhenCountryIsMX()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = ZString.Empty;

			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_DeliverToPartyAddressInfo, "State should not be empty.");
		}

		public void TestDimensionsUQ()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ContainerDimType = CylindricalRectangularList.Codes.Rectangular;
			FDA.AddInfoValidation.ValidateUS_DimUQ();
			AssertNoMessageErrorContaining(FDA.US_DimUQInfo, MandatoryValidation.YouHaveNotEntered);

			FDA.US_CanDim2 = 100m;
			FDA.AddInfoValidation.ValidateUS_DimUQ();
			AssertHasMessageErrorContaining(FDA.US_DimUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestCheckUS_DeliverToPartyAddress()
		{
			base.TestCheckUS_DeliverToPartyAddress();
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_DeliverToPartyAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_DeliverToPartyAddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);
			FDA.US_DeliverToPartyAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_DeliverToPartyAddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);
		}

		[TestDate(2016, 01, 04)]
		public void TestCheckUS_BrandName()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BDP;
			FDA.AddInfoValidation.ValidateUS_BrandName();
			AssertHasMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);
			FDA.US_BrandName = "Plasmanate";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DEV_RED;
			FDA.US_BrandName = ZString.Empty;
			AssertHasWarningContaining(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameWarning);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			FDA.US_BrandName = "";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);
			AssertNoWarningContaining(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameWarning);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_BrandName = "";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_CSU;
			FDA.US_BrandName = "";
			AssertHasMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);
			FDA.US_BrandName = "Plasmanate";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_RND;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150007;
			FDA.US_BrandName = "";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PHN;
			FDA.US_BrandName = "";
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_RND;
			FDA.AddInfoValidation.ValidateUS_BrandName();
			AssertNoMessageError(FDA.US_BrandNameInfo, USACEFDAAddInfoInvoiceLineValidation.BrandNameRequired);
		}

		public override void TestCheckUS_FDAImporterAddress()
		{
			base.TestCheckUS_FDAImporterAddress();
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";

			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_FDAImporterAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_FDAImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

			FDA.US_FDAImporterAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_FDAImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

			FDA.InvoiceLine.Declaration.US_EnableSPN = true;
			FDA.InvoiceLine.Declaration.US_EnableENS = false;
			FDA.InvoiceLine.Declaration.US_EnableCRL = false;
			FDA.US_FDAImporterAddress = ZGuid.Empty;
			AssertNoMessageError(FDA.US_FDAImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.US_FDAImporterAddress = mainAddress.PK;
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, errorMsg);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAImporterAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, errorMsg);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.US_FDAImporterAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, errorMsg);
		}

		public void TestCheckUS_FDAImporterAddressNoStateCheckWhenCountryIsMX()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = ZString.Empty;

			FDA.US_FDAImporterAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			FDA.US_FDAImporterAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_FDAImporterAddressInfo, "State should not be empty.");
		}

		public void TestCheckUS_OwnerAddressNoStateCheckWhenCountryIsMX()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = ZString.Empty;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.US_OwnerAddress = mainAddress.PK;

			AssertHasMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			FDA.US_OwnerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_OwnerAddressInfo, "State should not be empty.");
		}

		public void TestCheckUS_ProducerAddressNoStateCheckWhenCountryIsMX()
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			mainAddress.OA_State = ZString.Empty;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_IntendedUseCode = ZString.Empty;

			FDA.US_ProducerAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");

			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			FDA.US_ProducerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(FDA.US_ProducerAddressInfo, "State should not be empty.");
		}

		[ExpectNoExceptions]
		public override void TestCheckUS_FSVPImporterAddress()
		{
			base.TestCheckUS_FSVPImporterAddress();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_FSVPImporterAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertNoMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_FDAForcePN = true;
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_FSVPImporterAddress = mainAddress.PK;

			AssertNoMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_FSVPImporterAddress = ZGuid.NewZGuid();
			AssertHasMessageErrorContaining(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_FSVPImporterAddress = mainAddress.PK;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			FDA.US_ProductCode = "3223E56";
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.FSX);
			FDA.US_FSVPImporterAddress = mainAddress.PK;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.RNE);
			FDA.US_FSVPImporterAddress = mainAddress.PK;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			FDA.US_PNC = "";
			FDA.US_PND = true;
			FDA.US_FDAForcePN = true;
			AssertHasMessageError(FDA.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);
		}

		public void TestFSVPNotRequiredForStandAlone()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_FSVPImporterAddress = ZGuid.Empty;
			AssertHasMessageError(fda.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			declaration.US_EnableENS = false;
			declaration.US_EnableSPN = true;
			fda.US_FSVPImporterAddress = ZGuid.Empty;
			AssertNoMessageError(fda.US_FSVPImporterAddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);
		}

		public void TestFSVPContactEmailOnlyRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_FSVPImporterAddress = mainAddress.PK;

			var emailRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email");
			var contactNameRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name");
			var phoneNumberRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone");

			AssertHasMessageErrorContaining(fda.US_FSVPImporterAddressInfo, emailRequired);
			Assert(!fda.US_FSVPImporterAddressInfo.HasMessageError(contactNameRequired));
			Assert(!fda.US_FSVPImporterAddressInfo.HasMessageError(phoneNumberRequired));

			DeclarationTestHelper.AddFSVPContact(party.MainAddress, null, null, null, "test@test.com", null);
			fda.US_FSVPImporterAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(fda.US_FSVPImporterAddressInfo, emailRequired);
		}

		public override void TestCheckUS_ProducerType()
		{
			base.TestCheckUS_ProducerType();
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProducerType = ZString.Empty;
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.FirmTypeRequired);

			FDA.US_FDAForcePN = true;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_ProducerType = ZString.Empty;
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeRequired);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeRequired);
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.NSFMismatchManufacturer);

			FDA.US_FDAForcePN = false;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_PNC = "123456789111";
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.C;
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeForNonPriorNotice);
			AssertNoMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.NSFMismatchManufacturer);
			AssertNoMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ConsolidatorMustUseNSF);

			FDA.US_PNC = ZString.Empty;
			FDA.AddInfoValidation.ValidateUS_ProducerType();
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeForNonPriorNotice);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertNoMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeForNonPriorNotice);
			AssertNoMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.FirmTypeRequired);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.G;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.AddInfoValidation.ValidateUS_ProducerType();
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeShouldBeManufacturer);
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertNoMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ProducerTypeShouldBeManufacturer);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.C;
			AssertHasMessageError(FDA.US_ProducerTypeInfo, USACEFDAAddInfoInvoiceLineValidation.ConsolidatorMustUseNSF);
		}

		public override void TestCheckUS_OwnerAddress()
		{
			base.TestCheckUS_OwnerAddress();
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_OwnerAddress = ZGuid.Empty;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertNoMessageError(FDA.US_OwnerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;

			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertHasMessageError(FDA.US_OwnerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);

			FDA.US_OwnerAddress = mainAddress.PK;
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertNoMessageError(FDA.US_OwnerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);

			FDA.US_OwnerAddress = ZGuid.Empty;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.AddInfoValidation.ValidateUS_OwnerAddress();
			AssertNoMessageError(FDA.US_OwnerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);
		}

		public void TestCheckUS_InitialImporterAddress()
		{
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_RN_NKCountryCode = "US";

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.US_IntendedUseCode = ZString.Empty;
			FDA.US_ProducerAddress = address.PK;
			AssertNoMessageError(FDA.US_ProducerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);
			AssertHasMessageErrorContaining(FDA.US_ProducerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
			FDA.US_ProducerAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_ProducerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.AddInfoValidation.ValidateUS_ProducerAddress();
			AssertHasMessageError(FDA.US_ProducerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufRequiredForTob);
			address.OA_PostCode = "1003";
			FDA.US_ProducerAddress = address.PK;
			AssertNoMessageError(FDA.US_ProducerAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufRequiredForTob);
			AssertNoMessageErrorContaining(FDA.US_ProducerAddressInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
		}

		public void TestCheckPriorNoticeProperties()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_PND = true;
			AssertHasMessageError(fda.US_PNDInfo, ValidationConstants.PriorNotice.DisclaimedAndNotFD3);

			tariff.UE_PGACodes = "FD3";
			fda.AddInfoValidation.ValidateUS_PND();
			AssertNoMessageError(fda.US_PNDInfo, ValidationConstants.PriorNotice.DisclaimedAndNotFD3);
			AssertNoMessageError(fda.US_PNDInfo, ValidationConstants.PriorNotice.ForcedFDAIsDisclaimed);

			fda.US_FDAForcePN = true;
			fda.AddInfoValidation.ValidateUS_PND();
			AssertHasMessageError(fda.US_PNDInfo, ValidationConstants.PriorNotice.ForcedFDAIsDisclaimed);

			fda.AddInfoValidation.ValidateUS_PNC();
			AssertNoMessageError(fda.US_PNCInfo, ValidationConstants.PriorNotice.ConfirmationNumberFormat);

			fda.US_PNC = "123456";
			AssertHasMessageError(fda.US_PNCInfo, ValidationConstants.PriorNotice.ConfirmationNumberFormat);

			fda.US_PNC = "123456789012";
			AssertNoMessageError(fda.US_PNCInfo, ValidationConstants.PriorNotice.ConfirmationNumberFormat);
		}

		public override void TestCheckUS_OA_ShipperAddress()
		{
			base.TestCheckUS_OA_ShipperAddress();
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "US";

			FDA.US_OA_ShipperAddress = ZGuid.Empty;
			AssertHasMessageError(FDA.US_OA_ShipperAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);

			FDA.US_OA_ShipperAddress = mainAddress.PK;
			AssertNoMessageError(FDA.US_OA_ShipperAddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);
		}

		public void TestCheckUS_InvCurrValue()
		{
			FDA.AddInfoValidation.ValidateUS_InvCurrValue();
			AssertHasWarning(FDA.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueRequiredNew);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.AddInfoValidation.ValidateUS_InvCurrValue();
			AssertNoWarning(FDA.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueRequiredNew);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.AddInfoValidation.ValidateUS_InvCurrValue();
			AssertHasWarning(FDA.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueRequiredNew);

			FDA.US_InvCurrValue = 300m;
			AssertNoWarning(FDA.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueRequiredNew);
			AssertNoMessageError(FDA.US_InvCurrValueInfo, ValidationConstants.NegativeAmountNotAllowed);

			FDA.US_InvCurrValue = -11m;
			AssertHasMessageError(FDA.US_InvCurrValueInfo, ValidationConstants.NegativeAmountNotAllowed);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_InvCurrValue = 1000m;
			AssertHasMessageError(fda2.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueNotRequiredForPriorNotice);

			fda2.US_InvCurrValue = 0m;
			AssertNoMessageError(fda2.US_InvCurrValueInfo, USACEFDAAddInfoInvoiceLineValidation.InvCurrValueNotRequiredForPriorNotice);
			declaration.ResumeApportionment();
			fda2.AddInfoValidation.ValidateUS_TotalValue();
			Assert(!fda2.US_TotalValueInfo.HasWarning(USACEFDAAddInfoInvoiceLineValidation.FDAValueCannotBeZero));
		}

		public void TestCheckUS_TotalValue()
		{
			var invoiceLine = FDA.InvoiceLine;
			invoiceLine.JI_LinePrice = 100.10m;
			var declaration = invoiceLine.Declaration;
			AssertEquals("ENS enabled", true, declaration.US_EnableENS);
			AssertEquals("CRL enabled", false, declaration.US_EnableCRL);

			FDA.US_InvCurrValue = 30;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertNoMessageErrors(FDA.US_TotalValueInfo);

			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			FDA.US_InvCurrValue = 130;
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertHasMessageErrorContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValueShouldBeLessThanCustomsValue);

			FDA.US_InvCurrValue = 100m;
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertNoMessageErrorContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValueShouldBeLessThanCustomsValue);
			AssertNoMessageErrorContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValuesShouldBeLessThanTenTrillion);

			FDA.US_InvCurrValue = 99999999999m;
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertHasMessageErrorContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValuesShouldBeLessThanTenTrillion);
			AssertNoWarningContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValueCannotBeZero);

			FDA.US_InvCurrValue = 0.48m;
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertNoMessageErrorContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValuesShouldBeLessThanTenTrillion);
			AssertHasWarningContaining(FDA.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValueCannotBeZero);

			var secondFDA = invoiceLine.ACE_FDALines.AddNew();
			secondFDA.US_InvCurrValue = 1000m;
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			var messageError = "The rounded total of all FDA Values ";
			AssertNoMessageErrorContaining(FDA.US_TotalValueInfo, messageError);
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertHasMessageErrorContaining(FDA.US_TotalValueInfo, messageError);
			secondFDA.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			AssertHasMessageErrorContaining(FDA.US_TotalValueInfo, messageError);
			declaration.DoMerge();
			FDA.AddInfoValidation.ValidateUS_TotalValue();
			AssertNoMessageErrorContaining(FDA.US_TotalValueInfo, messageError);

			FDA.US_InvCurrValue = 99999999999m;
			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.Invoices[0].InvoiceLines.RemoveAndDeleteAll();
			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.ACE_FDALines.AddNew();

			var fda2 = invoiceLine2.ACE_FDALines[0];
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			fda2.AddInfoValidation.ValidateUS_TotalValue();
			AssertNoMessageErrorContaining(fda2.US_TotalValueInfo, USACEFDAAddInfoInvoiceLineValidation.FDAValueShouldBeLessThanCustomsValue);
		}

		public void TestCheckMatchUQ()
		{
			const string warningMessage = "UQ does not match the invoice UQ.";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;

			Assert("PReCondition", declaration.CanHavePGAFDA);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_ProductCode = "123456";
			fdaOnProduct.US_UQ1 = "KG";
			fdaOnProduct.US_Qty1 = 5m;
			fdaOnProduct.US_UQ2 = "CA";
			fdaOnProduct.US_Qty2 = 20m;
			fdaOnProduct.US_UQ3 = "CS";
			fdaOnProduct.US_Qty3 = 0m;

			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			AssertEquals(1, invoiceLine.ACE_FDALines.Count);
			invoiceLine.ACE_FDALines[0].AddInfoValidation.ValidateAll();
			AssertHasWarning(invoiceLine.ACE_FDALines[0].US_UQ3Info, warningMessage);

			var invliceLine2 = invoice.InvoiceLines.AddNew();
			invliceLine2.JI_PartNo = "Test";
			invliceLine2.JI_InvoiceQuantity = 1000m;
			invliceLine2.JI_InvoiceUQ = "KG";
			invliceLine2.ACE_FDALines[0].AddInfoValidation.ValidateAll();
			AssertNoWarning(invliceLine2.ACE_FDALines[0].US_UQ3Info, warningMessage);

			var invliceLine3 = invoice.InvoiceLines.AddNew();
			invliceLine3.JI_PartNo = "Test";
			invliceLine3.JI_InvoiceQuantity = 1000m;
			invliceLine3.JI_InvoiceUQ = "CS";
			invliceLine3.ACE_FDALines[0].AddInfoValidation.ValidateAll();
			AssertNoWarning(invliceLine3.ACE_FDALines[0].US_UQ3Info, warningMessage);
		}

		public void TestCheckUS_FDAForcePN()
		{
			FDA.US_ProductCode = "52D0001";
			AssertNoMessageError(FDA.US_FDAForcePNInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.ForcePriorNoticeShouldBeTicked, "52D0001"));
			FDA.US_FDAForcePN = false;
			AssertHasMessageError(FDA.US_FDAForcePNInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.ForcePriorNoticeShouldBeTicked, "52D0001"));

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "FD3";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(5);
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.AddInfoValidation.ValidateUS_FDAForcePN();
			AssertNoMessageError(FDA.US_FDAForcePNInfo, ZString.Format(USACEFDAAddInfoInvoiceLineValidation.ForcePriorNoticeShouldBeTicked, "52D0001"));
		}

		public void TestCheckUS_ProgramCode_LaboratoryDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_INV;
				_ = FDA.DocAddresses.CreateWithAddressType(DocAddressType.Laboratory);
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);

				FDA.LaboratoryDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);

				FDA.LaboratoryDocAddress.E2_OA_Address = ZGuid.Empty;
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);
			}
		}

		public void TestCheckUS_ProgramCode_DeliverToPartyDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_DeliverToPartyAddress = ZGuid.Empty;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);

				FDA.DeliverToPartyDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);
			}
		}

		public void TestCheckUS_ProgramCode_FSVPImporterDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
				FDA.US_FDAForcePN = false;
				FDA.US_FSVPImporterAddress = ZGuid.Empty;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageErrorContaining(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
				FDA.US_FDAForcePN = true;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageErrorContaining(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				FDA.US_FDAForcePN = false;

				FDA.FSVPImporterDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageErrorContaining(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);
			}
		}

		public void TestCheckUS_ProgramCode_ShipperDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_OA_ShipperAddress = ZGuid.Empty;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);

				FDA.ShipperDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);
			}
		}

		public void TestCheckUS_ProgramCode_FDAImporterDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var importTariff = Factory.New<USCTariff>();
				importTariff.UE_Tariff = "0000000000";
				importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
				importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
				importTariff.UE_PGACodes = "FD4";

				FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

				FDA.US_FDAImporterAddress = ZGuid.Empty;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

				FDA.FDAImporterDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

				FDA.InvoiceLine.Declaration.US_EnableSPN = true;
				FDA.InvoiceLine.Declaration.US_EnableENS = false;
				FDA.InvoiceLine.Declaration.US_EnableCRL = false;
				FDA.FDAImporterDocAddress.E2_OA_Address = ZGuid.Empty;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);
			}
		}

		public void TestCheckUS_ProgramCode_GoodsOwnerDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsOwner);
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "00000000";
				tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
				tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
				tariff.UE_PGACodes = "FD4";
				FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);

				FDA.GoodsOwnerDocAddress.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);
			}
		}

		public void TestCheckUS_ProgramCode_InitialImporterDocAddresses()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);

				var initialImporter = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.InitialImporter);
				initialImporter.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);
			}
		}

		public void TestCheckUS_ProgramCode_ManufacturerDocAddresses()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				var manufacturer = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				manufacturer.E2_OA_Address = USAddress.PK;
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				manufacturer = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				manufacturer.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

				manufacturer.E2_OA_Address = ZGuid.Empty;
				var consolidator = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Consolidator);
				consolidator.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				consolidator.E2_OA_Address = ZGuid.Empty;
				var growner = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Grower);
				growner.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			}
		}

		public void TestCheckUS_ProgramCode_AddressTypeShouldBeValidated()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				var initialImporter = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.InitialImporter);
				initialImporter.E2_OA_Address = USAddress.PK;
				FDA.AddInfoValidation.ValidateUS_ProgramCode();
				AssertNoMessageErrorContaining(initialImporter.E2_AddressTypeInfo, ListValidation.InvalidCodeMessageError);

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				AssertHasMessageErrorContaining(initialImporter.E2_AddressTypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		OrgAddress USAddress
		{
			get
			{
				if (usAddress == null)
				{
					var party = Factory.New<OrgHeader>();
					usAddress = party.MainAddress;
					usAddress.OA_RN_NKCountryCode = "US";
				}
				return usAddress;
			}
		}
		OrgAddress usAddress;
	}
}
