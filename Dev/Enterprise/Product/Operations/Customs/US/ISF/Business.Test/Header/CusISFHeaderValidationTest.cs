using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBF_ConsigneeFullName()
		{
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			Header.BF_ConsigneeFullName = "*TIM*";
			var message = @"Full Legal Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertHasWarningContaining(Header.BF_ConsigneeFullNameInfo, message);
			Header.BF_ConsigneeFullName = "TIM";
			AssertNoWarningContaining(Header.BF_ConsigneeFullNameInfo, message);
			Header.BF_ConsigneeFullName = "ÌTIM";
			AssertHasWarningContaining(Header.BF_ConsigneeFullNameInfo, message);
		}

		public void TestCheckBF_CustomsReference()
		{
			ZGuid companyPK = GlbCompany.CurrentCompany.PK;
			ZString entryFilerCode = "ABC";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler()
			{ EntryFilerCode = "ABC" });
			Header.BF_CustomsReference = "1234567890";
			AssertHasErrorContaining(header.BF_CustomsReferenceInfo, ValidationConstants.Header.CustomsReferenceFirst3CharsShouldBeSameAsEntryFilerCode(entryFilerCode));
			Header.BF_CustomsReference = "ABC1234567890";
			AssertHasErrorContaining(header.BF_CustomsReferenceInfo, ValidationConstants.Header.CustomsReferenceFormat);
			Header.BF_CustomsReference = "ABC12345678901";
			AssertHasErrorContaining(header.BF_CustomsReferenceInfo, ValidationConstants.Header.CustomsReferenceFormat);
			Header.BF_CustomsReference = "ABC-12345678901";
			AssertNoErrorContaining(header.BF_CustomsReferenceInfo, ValidationConstants.Header.CustomsReferenceFormat);
		}

		public void TestConvertISFBondActivityCodeToCommonActivityCode()
		{
			AssertEquals(ActivityCodeList.Codes._1, CusISFHeaderValidation.ConvertISFBondActivityCodeToCommonActivityCode(ISFBondActivityCodeList.Codes.ImporterOrBroker));
			AssertEquals(ActivityCodeList.Codes._2, CusISFHeaderValidation.ConvertISFBondActivityCodeToCommonActivityCode(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise));
			AssertEquals(ActivityCodeList.Codes._3, CusISFHeaderValidation.ConvertISFBondActivityCodeToCommonActivityCode(ISFBondActivityCodeList.Codes.InternationalCarrier));
			AssertEquals(ActivityCodeList.Codes._4, CusISFHeaderValidation.ConvertISFBondActivityCodeToCommonActivityCode(ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator));
			AssertEquals(ActivityCodeList.Codes._16, CusISFHeaderValidation.ConvertISFBondActivityCodeToCommonActivityCode(ISFBondActivityCodeList.Codes.ISFBond16));
		}

		public void CheckIfBondTypeIsOnTheFile()
		{
			var importer = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			var bondDetail = importer.BondDetails.AddNew();
			bondDetail.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondDetail.PW_BondType = BondTypeList.Codes.ContinuousBond;
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator;
			header.BF_BondType = BondTypeList.Codes.ContinuousBond;
			AssertHasWarning(header.BF_BondTypeInfo, ValidationConstants.Header.NoValidBondOnFile(header.BF_BondActivityCode, header.BF_BondType, ZDateTime.Today));
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.InternationalCarrier;
			AssertNoWarning(header.BF_BondTypeInfo, ValidationConstants.Header.NoValidBondOnFile(header.BF_BondActivityCode, header.BF_BondType, ZDateTime.Today));
			importer.BondDetails.RemoveAndDeleteAll();
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator;
			AssertNoWarning(header.BF_BondTypeInfo, ValidationConstants.Header.NoValidBondOnFile(header.BF_BondActivityCode, header.BF_BondType, ZDateTime.Today));
		}

		public void TestCheckBF_SCAC()
		{
			USCarrierCombined carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ZZZZ";
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.OceanVesselNonContainerized;
			Factory.Save();
			AssertEquals("Carrier 'ZBZZ' should not exists", 0, Factory.Load<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "ZBZZ")).Length);
			Header.BF_SCAC = "ZBZZ";
			AssertHasWarningContaining(Header.BF_SCACInfo, ListValidation.InvalidCodeMessage);
			Header.BF_SCAC = "ZZZZ";
			AssertNoWarningContaining(Header.BF_SCACInfo, ListValidation.InvalidCodeMessage);
			AssertNoMessageError(Header.BF_SCACInfo, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			Header.BF_SCAC = "ZZ1Z";
			AssertHasMessageError(Header.BF_SCACInfo, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			Header.BF_SCAC = ZString.Empty;
			AssertNoMessageError(Header.BF_SCACInfo, CusISFHeaderValidation.InvalidSCACFormat);
			Header.BF_SCAC = "ZZ";
			AssertHasMessageError(Header.BF_SCACInfo, CusISFHeaderValidation.InvalidSCACFormat);
			Header.BF_SCAC = "ZZZZ";
			AssertNoMessageError(Header.BF_SCACInfo, CusISFHeaderValidation.InvalidSCACFormat);
		}

		public void TestCheckBF_GB_RestrictsISFJobs()
		{
			var nonUSCompany = Factory.New<GlbCompany>();
			nonUSCompany.GC_Code = "Z!Z";
			nonUSCompany.GC_Name = "BOB THE BUILDER";
			nonUSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonUSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var nonUSBranch = nonUSCompany.Branches.AddNew();
			nonUSBranch.GB_Code = "Z!Z";
			nonUSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1C";
			uSCompany.GC_Name = "WENDY THE DESTROYER";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch1 = uSCompany.Branches.AddNew();
			uSBranch1.GB_Code = "Z1Z";
			uSBranch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSBranch2 = uSCompany.Branches.AddNew();
			uSBranch2.GB_Code = "Z2Z";
			uSBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			GlbGroup group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "CD1";
			GlbStaff staff1 = group1.Staff.AddNew();
			staff1.GS_Code = "S1";
			staff1.GS_LoginName = "S1";
			staff1.GS_EmailAddress = "S1@pretend.email.com";
			staff1.GS_GB_HomeBranch = uSBranch1.PK;
			Factory.Save();
			ISFRegistry.Instance.ISFUSRestrictISFJobs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Header.BF_GB = nonUSBranch.PK;
			AssertNoErrorContaining(Header.BF_GBInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrors(Header.BF_GBInfo);
			Header.BF_GB = ZGuid.Empty;
			AssertHasErrorContaining(Header.BF_GBInfo, MandatoryValidation.MustBeEntered);
			ISFRegistry.Instance.ISFUSRestrictISFJobs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Header.BF_GB = nonUSBranch.PK;
			Header.Validation.ValidateBF_GB();
			AssertHasErrors(CusISFHeaderValidation.InvalidateBranchForISF, Header.BF_GBInfo);
		}

		public void TestCheckBF_GB()
		{
			var nonUSCompany = Factory.New<GlbCompany>();
			nonUSCompany.GC_Code = "Z!Z";
			nonUSCompany.GC_Name = "BOB THE BUILDER";
			nonUSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonUSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var nonUSBranch = nonUSCompany.Branches.AddNew();
			nonUSBranch.GB_Code = "Z!Z";
			nonUSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_Name = "WENDY THE DESTROYER";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch1 = uSCompany.Branches.AddNew();
			uSBranch1.GB_Code = "Z1Z";
			uSBranch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSBranch2 = uSCompany.Branches.AddNew();
			uSBranch2.GB_Code = "Z2Z";
			uSBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			Header.BF_GB = ZGuid.Empty;
			AssertHasErrors(Header.BF_GBInfo);
			AssertHasErrorContaining(Header.BF_GBInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(Header.BF_GBInfo, ListValidation.InvalidCodeError);
			Header.BF_GB = ZGuid.Invalid;
			AssertNoErrorContaining(Header.BF_GBInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(Header.BF_GBInfo, ListValidation.InvalidCodeError);
			AssertNoMessageErrors(Header.BF_GBInfo);
			Header.BF_GB = nonUSBranch.PK;
			AssertNoErrorContaining(Header.BF_GBInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrors(Header.BF_GBInfo);
		}

		public void TestCheckBF_ConsigneeDateOfBirth()
		{
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			Header.BF_ConsigneeDateOfBirth = ZDateTime.Today.AddYears(-11);
			AssertNoMessageErrors(Header.BF_ConsigneeDateOfBirthInfo);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ConsigneeDateOfBirth = ZDateTime.Today.AddYears(1);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			AssertHasMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			Header.BF_ConsigneeDateOfBirth = ZDateTime.Empty;
			AssertHasMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.CBPAssignedNumber;
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ConsigneeDateOfBirth = ZDateTime.Today.AddYears(1);
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			AssertHasMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			Header.BF_ConsigneeDateOfBirth = ZDateTime.Empty;
			AssertHasMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
			Header.BF_ConsigneeDateOfBirth = ZDateTime.BrettsBirthday;
			AssertNoMessageError(Header.BF_ConsigneeDateOfBirthInfo, ValidationConstants.Header.ConsigneeDateOfBirthIsRequired);
		}

		public void TestCheckBF_ConsigneeCountryOfIssue()
		{
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			Header.BF_ConsigneeCountryOfIssue = Core.Constants.CountryCodes.Australia;
			AssertNoMessageError(Header.BF_ConsigneeCountryOfIssueInfo, ValidationConstants.Header.ConsigneeCountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(Header.BF_ConsigneeCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_ConsigneeCountryOfIssue = "Z!";
			AssertHasMessageErrorContaining(Header.BF_ConsigneeCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_ConsigneeCountryOfIssue = ZString.Empty;
			AssertNoMessageErrorContaining(Header.BF_ConsigneeCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(Header.BF_ConsigneeCountryOfIssueInfo, ValidationConstants.Header.ConsigneeCountryOfIssueIsRequiredForPassport);
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.CBPAssignedNumber;
			AssertNoMessageError(Header.BF_ConsigneeCountryOfIssueInfo, ValidationConstants.Header.ConsigneeCountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(Header.BF_ConsigneeCountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBF_ActionReasonCode()
		{
			Header.BF_ActionReasonCode = "ZZ";
			AssertHasMessageErrorContaining(Header.BF_ActionReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_ActionReasonCode = "";
			AssertHasMessageErrorContaining(Header.BF_ActionReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);
			foreach (ICodeDescription pair in Header.Lookups.ActionReasonCodeList)
			{
				Header.BF_ActionReasonCode = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ActionReasonCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(Header.BF_ActionReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckBF_BondActivityCode()
		{
			Header.BF_BondActivityCode = "ZZ";
			AssertHasMessageErrorContaining(Header.BF_BondActivityCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in Header.Lookups.BondActivityCodeList)
			{
				Header.BF_BondActivityCode = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_BondActivityCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			Header.BF_BondActivityCode = "";
			AssertNoMessageErrorContaining(Header.BF_BondActivityCodeInfo, ListValidation.InvalidCodeMessageError);
			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (string validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertHasWarning(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				Header.BF_BondActivityCode = ZString.Empty;
				AssertNoWarning(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
			}

			foreach (string validCode in validList)
			{
				Header.BF_ShipmentType = validCode;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertNoWarning(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				Header.BF_BondActivityCode = ZString.Empty;
				AssertNoWarning(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondActivityCodeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondActivityCodeInfo.HumanReadableName));
			}
		}

		public void TestCheckBF_BondType()
		{
			Header.BF_BondType = "Z";
			AssertHasMessageErrorContaining(Header.BF_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_BondType = "";
			AssertNoMessageErrors(Header.BF_BondTypeInfo);
			foreach (ICodeDescription pair in new ImporterBondTypeList())
			{
				Header.BF_BondType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (string validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				AssertHasWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondType = BondTypeList.Codes.ContinuousBond;
				AssertHasWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondType = ZString.Empty;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
			}

			foreach (string validCode in validList)
			{
				Header.BF_ShipmentType = validCode;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondType = BondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				Header.BF_BondType = ZString.Empty;
				AssertNoWarning(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondType9MayOnlyUsedWithBondActivityCode16);
				AssertHasMessageError(Header.BF_BondTypeInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondTypeInfo.HumanReadableName));
			}
		}

		public void TestCheckBF_EntryNumber()
		{
			Header.BF_EntryNumber = "ZZ12322";
			AssertHasMessageError(Header.BF_EntryNumberInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
			Header.BF_EntryNumber = "XJF23469874";
			AssertNoMessageError(Header.BF_EntryNumberInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
			Header.BF_EntryNumber = ZString.Empty;
			AssertNoMessageError(Header.BF_EntryNumberInfo, ValidationConstants.Bill.CBPEntryNumberRightFormat);
		}

		public void TestBF_SuretyCodeRightFormat()
		{
			Header.BF_SuretyCode = ZString.Empty;
			AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			Header.BF_SuretyCode = "7891";
			AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			Header.BF_SuretyCode = "789";
			AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			Header.BF_SuretyCode = "7 9";
			AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
			Header.BF_SuretyCode = "78A";
			AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeRightFormat);
		}

		public void TestBF_HouseBillMaxLength()
		{
			AssertMaxLength((ZPropertyInfoString)Header.BF_HouseBillInfo, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.HouseBillOfLading, 16));
		}

		public void TestBF_OceanBillMaxLength()
		{
			AssertMaxLength((ZPropertyInfoString)Header.BF_OceanBillInfo, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.OceanBillOfLading, 16));
		}

		public void TestBF_MasterBillMaxLength()
		{
			AssertMaxLength((ZPropertyInfoString)Header.BF_MasterBillInfo, 16, ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.MasterBillOfLading, 16));
		}

		void AssertMaxLength(ZPropertyInfoString info, int maxLength, ZString messageError)
		{
			info.Value = ZString.Empty;
			AssertNoMessageError(info, messageError);
			info.Value = new ZString('A', maxLength + 1);
			AssertHasMessageError(info, messageError);
			info.Value = new ZString('A', maxLength);
			AssertNoMessageError(info, messageError);
		}

		public void TestCheckBF_LineMergeStyle()
		{
			Header.BF_LineMergeStyle = "ZZ";
			AssertHasWarningContaining(Header.BF_LineMergeStyleInfo, ValidationConstants.Header.LineMergeStyleIsInvalid);
			AssertNoWarning(Header.BF_LineMergeStyleInfo, ValidationConstants.Header.LineMergeStyleNotSpecified);
			foreach (ICodeDescription pair in new MergeStyleList())
			{
				Header.BF_LineMergeStyle = pair.Code;
				AssertNoWarningContaining(Header.BF_LineMergeStyleInfo, ValidationConstants.Header.LineMergeStyleIsInvalid);
			}

			Header.BF_LineMergeStyle = ZString.Empty;
			AssertHasWarning(Header.BF_LineMergeStyleInfo, ValidationConstants.Header.LineMergeStyleNotSpecified);
		}

		public void TestCheckBF_NumOfHarmChars()
		{
			Header.Lines.DeleteAll();
			Header.BF_NumOfHarmChars = ZString.Empty;
			AssertHasWarning(Header.BF_NumOfHarmCharsInfo, ValidationConstants.Header.NumberOfHarmonizedDigitsToReport);
			AssertHasMessageError(Header.BF_NumOfHarmCharsInfo, ValidationConstants.Header.AtLeastOneHarmonizedTariffScheduleIsEntered);
			CusISFLine line = Header.Lines.AddNew();
			foreach (ICodeDescription pair in new NumberOfHarmonizedDigitsList())
			{
				Header.BF_NumOfHarmChars = pair.Code;
				AssertNoWarning(Header.BF_NumOfHarmCharsInfo, ValidationConstants.Header.NumberOfHarmonizedDigitsToReport);
				AssertNoMessageError(Header.BF_NumOfHarmCharsInfo, ValidationConstants.Header.AtLeastOneHarmonizedTariffScheduleIsEntered);
			}
		}

		public void TestCheckBF_MasterBill()
		{
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_MasterBill = "MWB2134234";
			CusISFHeader header1 = Factory.New<CusISFHeader>();
			header1.BF_OceanBill = "OC23423";
			header1.BF_MasterBill = "MWB2134234";
			AssertNoWarnings(header1.BF_MasterBillInfo);
			AssertNoMessageError(header1.BF_MasterBillInfo, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			CusISFBill bill = header1.ReferenceDatas.AddNew();
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill.BB_BillNum = "MB23432434";
			header1.BF_MasterBill = "MB23432434";
			AssertNoWarnings(header1.BF_MasterBillInfo);
			AssertHasMessageError(header1.BF_MasterBillInfo, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertHasMessageError(header1.BF_MasterBillInfo, ValidationConstants.Bill.OnlySpecifyMasterBillWhenThereIsHouseBill);
			header1.BF_MasterBill = "SCAP";
			var errorMessage1 = ValidationConstants.Bill.BillNumberMinLength(BillTypeList.Descriptions.MasterBillOfLading, 5);
			var errorMessage2 = ValidationConstants.Bill.BillNumberSCACCodeValidation(BillTypeList.Descriptions.MasterBillOfLading);
			AssertHasMessageError(header1.BF_MasterBillInfo, errorMessage1);
			AssertHasMessageError(header1.BF_MasterBillInfo, errorMessage2);
		}

		public void TestCheckBF_HouseBill()
		{
			Header.BF_OceanBill = ZString.Empty;
			Header.BF_HouseBill = ZString.Empty;
			AssertHasMessageError(Header.BF_HouseBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_HouseBill = "SCACHB21423";
			AssertNoMessageError(Header.BF_HouseBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_OceanBill = "OB212321";
			AssertHasMessageError(Header.BF_HouseBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_HouseBill = ZString.Empty;
			AssertNoMessageError(Header.BF_HouseBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_HouseBill = "SCAP";
			var errMessage1 = ValidationConstants.Bill.BillNumberMinLength(BillTypeList.Descriptions.HouseBillOfLading, 5);
			var errMessage2 = ValidationConstants.Bill.BillNumberSCACCodeValidation(BillTypeList.Descriptions.HouseBillOfLading);
			AssertHasMessageError(Header.BF_HouseBillInfo, errMessage1);
			AssertHasMessageError(Header.BF_HouseBillInfo, errMessage2);
			Header.BF_HouseBill = "SCACHB21423";
			Header.BF_MasterBill = "MWB2134234";
			Header.BF_JobReference = "ISF23424345";
			Factory.Save();
			string messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.HouseBillOfLading, "SCACHB21423", Header.HumanReadableName);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_MasterBill = "MWB2134234";
			header2.BF_HouseBill = "SCACHB21423";
			AssertHasMessageError(header2.BF_HouseBillInfo, messageError);
			header2.BF_MasterBill = "";
			AssertHasMessageError(header2.BF_HouseBillInfo, messageError);
			header2.BF_MasterBill = "MWB2134234";
			AssertHasMessageError(header2.BF_HouseBillInfo, messageError);
			header2.BF_HouseBill = "HB3234232";
			AssertNoMessageError(header2.BF_HouseBillInfo, messageError);
		}

		public void TestCheckBF_OceanBill()
		{
			Header.BF_HouseBill = ZString.Empty;
			Header.BF_OceanBill = ZString.Empty;
			AssertHasMessageError(Header.BF_OceanBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_OceanBill = "OB212321";
			AssertNoMessageError(Header.BF_OceanBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_HouseBill = "HB21423";
			Header.BF_OceanBill = ZString.Empty;
			AssertNoMessageError(Header.BF_OceanBillInfo, ValidationConstants.Header.HouseOrOceanBillIsRequired);
			Header.BF_OceanBill = "SCAP";
			var errMessage1 = ValidationConstants.Bill.BillNumberMinLength(BillTypeList.Descriptions.OceanBillOfLading, 5);
			var errMessage2 = ValidationConstants.Bill.BillNumberSCACCodeValidation(BillTypeList.Descriptions.OceanBillOfLading);
			AssertHasMessageError(Header.BF_OceanBillInfo, errMessage1);
			AssertHasMessageError(Header.BF_OceanBillInfo, errMessage2);
			Header.BF_OceanBill = "OB212321";
			Header.BF_JobReference = "ISF23424345";
			string messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.OceanBillOfLading, "OB212321", Header.HumanReadableName);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_OceanBill = "OB212321";
			AssertHasMessageError(header2.BF_OceanBillInfo, messageError);
			header2.BF_OceanBill = "OB2156845";
			AssertNoMessageError(header2.BF_OceanBillInfo, messageError);
		}

		public void TestCheckBF_DateOfBirth()
		{
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			Header.BF_DateOfBirth = ZDateTime.Today.AddYears(-11);
			AssertNoMessageErrors(Header.BF_DateOfBirthInfo);
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_DateOfBirth = ZDateTime.Today.AddYears(1);
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			AssertHasMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Carnet;
			Header.BF_DateOfBirth = ZDateTime.Empty;
			AssertHasMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.CBPAssignedNumber;
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			Header.BF_DateOfBirth = ZDateTime.Today.AddYears(1);
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			AssertHasMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsInvalid);
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			Header.BF_DateOfBirth = ZDateTime.Empty;
			AssertHasMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
			Header.BF_DateOfBirth = ZDateTime.BrettsBirthday;
			AssertNoMessageError(Header.BF_DateOfBirthInfo, ValidationConstants.Header.DateOfBirthIsRequired);
		}

		public void TestCheckBF_ImporterFullName()
		{
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			Header.BF_ImporterFullName = ZString.Empty;
			AssertHasMessageError(Header.BF_ImporterFullNameInfo, ValidationConstants.Header.ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber);
			Header.BF_ImporterFullName = "SMITH, BOB";
			AssertNoMessageError(Header.BF_ImporterFullNameInfo, ValidationConstants.Header.ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber);
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			AssertNoMessageError(Header.BF_ImporterFullNameInfo, ValidationConstants.Header.ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber);
			Header.BF_ImporterFullName = ZString.Empty;
			AssertHasMessageError(Header.BF_ImporterFullNameInfo, ValidationConstants.Header.ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber);
			Header.BF_ImporterFullName = new string('*', 36);
			AssertHasWarning(Header.BF_ImporterFullNameInfo, "Importer Name is too long. Only the first 35 characters will be sent.");
			Header.BF_ImporterFullName = @"BÌB";
			var message = @"Full Legal Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertHasWarningContaining(Header.BF_ImporterFullNameInfo, message);
		}

		public void TestCheckBF_TransportMode()
		{
			Header.BF_TransportMode = ZString.Empty;
			AssertNoMessageErrors(Header.BF_TransportModeInfo);
			Header.BF_TransportMode = "ZZ";
			AssertHasMessageErrorContaining(Header.BF_TransportModeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			AssertNoMessageErrorContaining(Header.BF_TransportModeInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2015, 1, 10)]
		public void TestCheckBF_EntryType()
		{
			Header.BF_EntryType = ZString.Empty;
			AssertHasMessageErrorContaining(Header.BF_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_EntryType = "Z";
			AssertNoMessageErrorContaining(Header.BF_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.BF_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			AssertNoMessageErrorContaining(Header.BF_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			AssertNoMessageErrorContaining(Header.BF_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(Header.BF_EntryTypeInfo, CusISFHeaderValidation.InvalidLateType);
			Header.BF_EntryType = SubmissionTypeList.Codes.LateISF5;
			AssertHasMessageErrorContaining(Header.BF_EntryTypeInfo, CusISFHeaderValidation.InvalidLateType);
			Header.BF_EntryType = SubmissionTypeList.Codes.LateISF10;
			AssertHasMessageErrorContaining(Header.BF_EntryTypeInfo, CusISFHeaderValidation.InvalidLateType);
		}

		public void TestCheckBF_ShipmentType()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			Header.BF_ShipmentType = ZString.Empty;
			AssertNoMessageErrorContaining(Header.BF_ShipmentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			AssertHasMessageErrorContaining(Header.BF_ShipmentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_ShipmentType = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_ShipmentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			ShipmentTypeList list = new ShipmentTypeList();
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			}

			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			list.RemoveCode(ShipmentTypeList.Codes.StandardOrRegularFilings);
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
				AssertHasMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			}

			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			AssertNoMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			Header.BF_ShipmentType = "ZZ";
			AssertHasMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			AssertHasMessageErrorContaining(Header.BF_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Header.StandardOrRegularFilingsIsRequiredWhenISF5);
			Header.ReferenceDatas.DeleteAll();
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Carnet;
			AssertHasMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Bill.CarnetReferenceIsRequired);
			CusISFBill carnet = Header.ReferenceDatas.AddNew();
			carnet.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			AssertNoMessageError(Header.BF_ShipmentTypeInfo, ValidationConstants.Bill.CarnetReferenceIsRequired);
		}

		public void TestCheckBF_ImporterCodeType()
		{
			Header.BF_ImporterCodeType = ZString.Empty;
			AssertHasMessageErrorContaining(Header.BF_ImporterCodeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_ImporterCodeType = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_ImporterCodeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.BF_ImporterCodeTypeInfo, ListValidation.InvalidCodeMessageError);
			ImporterCodeTypeList list = new ImporterCodeTypeList();
			list.RemoveCode(ImporterCodeTypeList.Codes.SCAC);
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ImporterCodeType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ImporterCodeTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SCAC;
			AssertHasMessageErrorContaining(Header.BF_ImporterCodeTypeInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			ShipmentTypeList shipmentTypeList = new ShipmentTypeList();
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.DiplomaticShipment);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.Carnet);
			foreach (ICodeDescription pair in shipmentTypeList)
			{
				Header.BF_ShipmentType = pair.Code;
				AssertHasMessageError(Header.BF_ImporterCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			}

			Header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			AssertNoMessageError(Header.BF_ImporterCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.DiplomaticShipment;
			AssertNoMessageError(Header.BF_ImporterCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Carnet;
			AssertNoMessageError(Header.BF_ImporterCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.CBPAssignedNumber;
			AssertNoMessageError(Header.BF_ImporterCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
		}

		public void TestCheckBF_ImporterCode()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			PowerOfAttorneyValidator poa = new PowerOfAttorneyValidator();
			Header.BF_OH_Importer = ZGuid.Empty;
			Header.BF_ImporterCode = ZString.Empty;
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			var messageError = ValidationConstants.Header.ValueIsRequiredForEntity(ImporterCodeTypeList.Descriptions.IRS);
			AssertHasMessageError(Header.BF_ImporterCodeInfo, messageError);
			Header.BF_ImporterCode = "ZZ";
			string receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poa.CountrySpecificNameForPOA, "ISF");
			AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertNoMessageErrorContaining(Header.BF_ImporterCodeInfo, receivedDateRequired);
			JobRequiredDocument poaDocument = Header.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			Header.Validation.ValidateBF_ImporterCode();
			AssertHasMessageErrorContaining(Header.BF_ImporterCodeInfo, receivedDateRequired);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			Header.Validation.ValidateBF_ImporterCode();
			AssertNoMessageErrorContaining(Header.BF_ImporterCodeInfo, receivedDateRequired);
			OrgHeader importer = Factory.New<OrgHeader>();
			Header.BF_OH_Importer = importer.PK;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			Header.Validation.ValidateBF_ImporterCode();
			AssertNoMessageErrorContaining(Header.BF_ImporterCodeInfo, receivedDateRequired);
			Header.BF_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(Header.BF_ImporterCodeInfo, receivedDateRequired);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			ImporterCodeTypeList list = Header.Lookups.ImporterCodeTypes;
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ImporterCode = ZString.Empty;
				Header.BF_ImporterCodeType = pair.Code;
				messageError = ValidationConstants.Header.ValueIsRequiredForEntity(pair.Description);
				AssertHasMessageError(Header.BF_ImporterCodeInfo, messageError);
				Header.BF_ImporterCode = "ZV323";
				AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			}

			importer.OH_Code = "TSTIMP123";
			OrgCusCode einCusCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123US");
			OrgCusCode cbnCusCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "093801-04778");
			OrgHeaderWrapper orgWrapper = OrgHeaderWrapper.New(importer);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = ZString.Empty;
			Factory.Save();
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			Header.BF_ImporterCode = "00-1234123US";
			messageError = ValidationConstants.Header.OrganisationShouldBeRegisteredInCustoms(ImporterCodeTypeList.Codes.IRS, "TSTIMP123");
			string messageError2 = ValidationConstants.Header.EntityMightNotBeRegisteredInCustoms(ImporterCodeTypeList.Codes.IRS);
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			Header.BF_ImporterCode = "00-1234123ZZ";
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError);
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "N";
			Factory.Save();
			Header.BF_ImporterCode = "00-1234123US";
			AssertHasMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			Header.BF_ImporterCode = "00-1234123ZZ";
			AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "Y";
			Factory.Save();
			Header.BF_ImporterCode = "00-1234123US";
			AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			messageError = ValidationConstants.Header.OrganisationShouldBeRegisteredInCustoms(ImporterCodeTypeList.Codes.CBPAssignedNumber, "TSTIMP123");
			messageError2 = ValidationConstants.Header.EntityMightNotBeRegisteredInCustoms(ImporterCodeTypeList.Codes.CBPAssignedNumber);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = ZString.Empty;
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.CBPAssignedNumber;
			Factory.Save();
			Header.BF_ImporterCode = "093801-04778";
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			Header.BF_ImporterCode = "093801-04799";
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError);
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "N";
			Factory.Save();
			Header.BF_ImporterCode = "093801-04778";
			AssertHasMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			Header.BF_ImporterCode = "093801-04799";
			AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertHasWarning(Header.BF_ImporterCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "Y";
			Factory.Save();
			Header.BF_ImporterCode = "093801-04778";
			AssertNoMessageError(Header.BF_ImporterCodeInfo, messageError);
			AssertNoWarning(Header.BF_ImporterCodeInfo, messageError2);
			if (ErrorReporter.LastKeyReported == "Validation:BF_ImporterCode")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCheckBF_OH_Importer()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			PowerOfAttorneyValidator poav = new PowerOfAttorneyValidator();
			OrgHeader importer = Factory.New<OrgHeader>();
			JobRequiredDocument poaDocument = importer.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			string receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, "organization");
			string expiryDateRequiredForPeriodicDocument = PowerOfAttorneyValidator.GetExpiryDateRequiredForPeriodicDocumentString(poav.CountrySpecificNameForPOA, "organization");
			Header.BF_OH_Importer = importer.PK;
			AssertHasMessageErrorContaining(Header.BF_OH_ImporterInfo, receivedDateRequired);
			AssertNoMessageErrorContaining(Header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			Header.Validation.ValidateBF_OH_Importer();
			AssertHasMessageErrorContaining(Header.BF_OH_ImporterInfo, receivedDateRequired);
			AssertHasMessageErrorContaining(Header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			poaDocument.EQ_ValidToDate = ZDateTime.Today;
			string pOAWillExpireSoon = PowerOfAttorneyValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), "organization", poav.CountrySpecificNameForPOA);
			Header.Validation.ValidateBF_OH_Importer();
			AssertNoMessageErrorContaining(Header.BF_OH_ImporterInfo, receivedDateRequired);
			AssertNoMessageErrorContaining(Header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
			AssertHasWarningContaining(Header.BF_OH_ImporterInfo, pOAWillExpireSoon);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
			Header.Validation.ValidateBF_OH_Importer();
			AssertNoWarningContaining(Header.BF_OH_ImporterInfo, pOAWillExpireSoon);
			string pOAExpired = PowerOfAttorneyValidator.GetPOAExpiredString(ZDate.Today.AddDays(-1).ToString(), "organization", poav.CountrySpecificNameForPOA);
			poaDocument.EQ_ValidToDate = ZDateTime.Today;
			Header.Validation.ValidateBF_OH_Importer();
			AssertNoMessageErrorContaining(Header.BF_OH_ImporterInfo, pOAExpired);
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
			Header.Validation.ValidateBF_OH_Importer();
			AssertHasMessageErrorContaining(Header.BF_OH_ImporterInfo, pOAExpired);
			importer.RequiredDocuments.RemoveAndDeleteAll();
			Header.Validation.ValidateBF_OH_Importer();
			AssertHasMessageErrorContaining(Header.BF_OH_ImporterInfo, PowerOfAttorneyValidator.GetNoPOADocumentForImporterString(poav.CountrySpecificNameForPOA));
			AssertNoMessageError(Header.BF_OH_ImporterInfo, receivedDateRequired);
			poaDocument = Header.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			Header.Validation.ValidateBF_OH_Importer();
			receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, "ISF");
			AssertNoMessageErrorContaining(Header.BF_OH_ImporterInfo, PowerOfAttorneyValidator.GetNoPOADocumentForImporterString(poav.CountrySpecificNameForPOA));
			AssertHasMessageError(Header.BF_OH_ImporterInfo, receivedDateRequired);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
			Header.Validation.ValidateBF_OH_Importer();
			AssertNoMessageError(Header.BF_OH_ImporterInfo, receivedDateRequired);
			AssertNoMessageError(Header.BF_OH_ImporterInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			Header.BF_OH_Importer = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(Header.BF_OH_ImporterInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
		}

		public void TestCheckBF_CountryOfIssue()
		{
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			Header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			AssertNoMessageError(Header.BF_CountryOfIssueInfo, ValidationConstants.Header.CountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(Header.BF_CountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_CountryOfIssue = "Z!";
			AssertHasMessageErrorContaining(Header.BF_CountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_CountryOfIssue = ZString.Empty;
			AssertNoMessageErrorContaining(Header.BF_CountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(Header.BF_CountryOfIssueInfo, ValidationConstants.Header.CountryOfIssueIsRequiredForPassport);
			Header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.CBPAssignedNumber;
			AssertNoMessageError(Header.BF_CountryOfIssueInfo, ValidationConstants.Header.CountryOfIssueIsRequiredForPassport);
			AssertNoMessageErrorContaining(Header.BF_CountryOfIssueInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBF_ConsigneeCode()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			ConsigneeCodeTypeList list = Header.Lookups.ConsigneeCodeTypes;
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ConsigneeCode = ZString.Empty;
				Header.BF_ConsigneeCodeType = pair.Code;
				var messageErrorDescription = ValidationConstants.Header.ValueIsRequiredForEntity(pair.Description);
				AssertHasMessageError(Header.BF_ConsigneeCodeInfo, messageErrorDescription);
				Header.BF_ConsigneeCode = "ZV323";
				AssertNoMessageError(Header.BF_ConsigneeCodeInfo, messageErrorDescription);
			}

			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			foreach (ICodeDescription pair in list)
			{
				Header.BF_ConsigneeCode = ZString.Empty;
				Header.BF_ConsigneeCodeType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ConsigneeCodeInfo, ValidationConstants.Header.ValueIsRequiredForEntity(pair.Description));
			}

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP123";
			OrgCusCode einCusCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123US");
			OrgCusCode cbnCusCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "093801-04778");
			OrgHeaderWrapper orgWrapper = OrgHeaderWrapper.New(importer);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = ZString.Empty;
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			Factory.Save();
			Header.BF_ConsigneeCodeType = ImporterCodeTypeList.Codes.IRS;
			Header.BF_ConsigneeCode = "00-1234123US";
			string messageError = ValidationConstants.Header.OrganisationShouldBeRegisteredInCustoms(ImporterCodeTypeList.Codes.IRS, "TSTIMP123");
			string messageError2 = ValidationConstants.Header.EntityMightNotBeRegisteredInCustoms(ImporterCodeTypeList.Codes.IRS);
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			Header.BF_ConsigneeCode = "00-1234123ZZ";
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError);
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "N";
			Factory.Save();
			Header.BF_ConsigneeCode = "00-1234123US";
			AssertHasMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			Header.BF_ConsigneeCode = "00-1234123ZZ";
			AssertNoMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "Y";
			Factory.Save();
			Header.BF_ConsigneeCode = "00-1234123US";
			AssertNoMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			messageError = ValidationConstants.Header.OrganisationShouldBeRegisteredInCustoms(ImporterCodeTypeList.Codes.CBPAssignedNumber, "TSTIMP123");
			messageError2 = ValidationConstants.Header.EntityMightNotBeRegisteredInCustoms(ImporterCodeTypeList.Codes.CBPAssignedNumber);
			Header.BF_ConsigneeCodeType = ImporterCodeTypeList.Codes.CBPAssignedNumber;
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = ZString.Empty;
			Factory.Save();
			Header.BF_ConsigneeCode = "093801-04778";
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			Header.BF_ConsigneeCode = "093801-04799";
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError);
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "N";
			Factory.Save();
			Header.BF_ConsigneeCode = "093801-04778";
			AssertHasMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			Header.BF_ConsigneeCode = "093801-04799";
			AssertNoMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertHasWarning(Header.BF_ConsigneeCodeInfo, messageError2);
			orgWrapper.ZO_IsEINNumberVerifiedIndicator = "Y";
			Factory.Save();
			Header.BF_ConsigneeCode = "093801-04778";
			AssertNoMessageError(Header.BF_ConsigneeCodeInfo, messageError);
			AssertNoWarning(Header.BF_ConsigneeCodeInfo, messageError2);
		}

		public void TestCheckBF_ConsigneeCodeType()
		{
			Header.BF_ConsigneeCodeType = "ZZ";
			AssertHasMessageErrorContaining(Header.BF_ConsigneeCodeTypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in Header.Lookups.ConsigneeCodeTypes)
			{
				Header.BF_ConsigneeCodeType = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_ConsigneeCodeTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			ShipmentTypeList shipmentTypeList = new ShipmentTypeList();
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.DiplomaticShipment);
			shipmentTypeList.RemoveCode(ShipmentTypeList.Codes.Carnet);
			foreach (ICodeDescription pair in shipmentTypeList)
			{
				Header.BF_ShipmentType = pair.Code;
				AssertHasMessageError(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			}

			Header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			AssertNoMessageError(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.DiplomaticShipment;
			AssertNoMessageError(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Carnet;
			AssertNoMessageError(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.IRS;
			AssertNoMessageError(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.PassportIsOnlyUsedFor03Or05Or06ShipmentType);
			Header.BF_ImporterCodeType = ConsigneeCodeTypeList.Codes.CBPAssignedNumber;
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.CBPAssignedNumber;
			AssertHasWarning(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.ForeignBasedConsigneeWithForeignBasedImporter);
			Header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			AssertNoWarning(Header.BF_ConsigneeCodeTypeInfo, ValidationConstants.Header.ForeignBasedConsigneeWithForeignBasedImporter);
		}

		public void TestCheckBF_RL_NKPlaceOfDelivery()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			Header.BF_RL_NKPlaceOfDelivery = ZString.Empty;
			AssertHasMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_RL_NKPlaceOfDelivery = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_RL_NKPlaceOfDelivery = "AUSYD";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			Header.BF_RL_NKPlaceOfDelivery = ZString.Empty;
			AssertNoMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_RL_NKPlaceOfDelivery = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBF_RL_NKPortOfUnload()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			Header.BF_RL_NKPortOfUnload = ZString.Empty;
			AssertHasMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_RL_NKPortOfUnload = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_RL_NKPortOfUnload = "AUSYD";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			Header.BF_RL_NKPortOfUnload = ZString.Empty;
			AssertNoMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, MandatoryValidation.YouHaveNotEntered);
			Header.BF_RL_NKPortOfUnload = "ZZ";
			AssertNoMessageErrorContaining(Header.BF_RL_NKPortOfUnloadInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBF_BondNumberOrHolder()
		{
			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (string validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				Header.BF_BondNumberOrHolder = "123-12-1234";
				AssertHasWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = ZString.Empty;
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "12-3456789XY";
				AssertHasWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "061234-12345";
				AssertHasWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "BND1234567";
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
			}

			foreach (string validCode in validList)
			{
				Header.BF_ShipmentType = validCode;
				Header.BF_BondNumberOrHolder = "123-12-1234";
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = ZString.Empty;
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "12-3456789XY";
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "061234-12345";
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
				Header.BF_BondNumberOrHolder = "BND1234567";
				AssertNoWarning(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsOnlyRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertNoMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondDetailIsRequiredForShipmentTypes(Header.BF_BondNumberOrHolderInfo.HumanReadableName));
				AssertHasMessageError(Header.BF_BondNumberOrHolderInfo, ValidationConstants.Header.BondHolderIsInvalid);
			}
		}

		public void TestCheckBF_SuretyCode()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (string validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
				Header.BF_SuretyCode = ZString.Empty;
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				Header.BF_SuretyCode = "798";
				AssertHasWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
			}

			foreach (string validCode in validList)
			{
				Header.BF_ShipmentType = validCode;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
				Header.BF_SuretyCode = ZString.Empty;
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				Header.BF_SuretyCode = "798";
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
				Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertNoWarning(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Header.BondSuretyCodeIsRequiredForISFBondSingleTransaction);
				AssertHasMessageError(Header.BF_SuretyCodeInfo, ValidationConstants.Bill.SuretyCodeIsOnlyRequired);
			}
		}

		public void TestCheckBF_BondReferenceNumber()
		{
			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (var validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			foreach (ICodeDescription pair in list)
			{
				Header.BF_ShipmentType = pair.Code;
				Header.BF_BondReferenceNumber = "798";
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				AssertHasWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				Header.BF_BondType = BondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				Header.BF_BondReferenceNumber = ZString.Empty;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
			}

			foreach (string validCode in validList)
			{
				Header.BF_ShipmentType = validCode;
				Header.BF_BondReferenceNumber = "798";
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
				Header.BF_BondType = BondTypeList.Codes.ContinuousBond;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
				Header.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
				Header.BF_BondReferenceNumber = ZString.Empty;
				AssertNoWarning(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequiredForShipmentTypes);
				AssertNoMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsOnlyRequired);
				AssertHasMessageError(Header.BF_BondReferenceNumberInfo, ValidationConstants.Bill.BondReferenceNumberIsRequired);
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<CusISFHeader>();
			header2.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			header2.BF_SuretyCode = "971";
			header2.BF_BondReferenceNumber = "BF12664";
			var header3 = newFactory.New<CusISFHeader>();
			header3.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			header3.BF_SuretyCode = "971";
			header3.BF_BondReferenceNumber = "BF12664";
			newFactory.Save();
			var messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.BondReferenceNumber, "BF12664", header3.HumanReadableName);
			Header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			Header.BF_SuretyCode = "971";
			Header.BF_BondReferenceNumber = "BF12664";
			AssertHasMessageError(Header.BF_BondReferenceNumberInfo, messageError);
			Header.BF_SuretyCode = "978";
			AssertNoMessageError(Header.BF_BondReferenceNumberInfo, messageError);
			Header.BF_SuretyCode = "971";
			Header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			AssertNoMessageError(Header.BF_BondReferenceNumberInfo, messageError);
		}

		public void TestCheckBF_EstimatedValue()
		{
			Header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			Header.BF_EstimatedValue = ZDecimal.Zero;
			AssertNoMessageError(Header.BF_EstimatedValueInfo, ValidationConstants.Header.EstimatedValueIsRequiredForInformalShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			AssertHasMessageError(Header.BF_EstimatedValueInfo, ValidationConstants.Header.EstimatedValueIsRequiredForInformalShipmentType);
			Header.BF_EstimatedValue = 10m;
			AssertNoMessageError(Header.BF_EstimatedValueInfo, ValidationConstants.Header.EstimatedValueIsRequiredForInformalShipmentType);
		}

		public void TestCheckBF_EstimatedQuantity()
		{
			Header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			Header.BF_EstimatedQuantity = ZInt.Zero;
			AssertNoMessageError(Header.BF_EstimatedQuantityInfo, ValidationConstants.Header.EstimatedQuantityIsRequiredForInformalShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			AssertHasMessageError(Header.BF_EstimatedQuantityInfo, ValidationConstants.Header.EstimatedQuantityIsRequiredForInformalShipmentType);
			Header.BF_EstimatedQuantity = 10;
			AssertNoMessageError(Header.BF_EstimatedQuantityInfo, ValidationConstants.Header.EstimatedQuantityIsRequiredForInformalShipmentType);
		}

		public void TestCheckBF_EstimatedQuantityUQ()
		{
			Header.BF_ShipmentType = ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects;
			Header.BF_EstimatedQuantityUQ = ZString.Empty;
			AssertNoMessageError(Header.BF_EstimatedQuantityUQInfo, ValidationConstants.Header.EstimatedQuantityUQIsRequiredForInformalShipmentType);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			AssertHasMessageError(Header.BF_EstimatedQuantityUQInfo, ValidationConstants.Header.EstimatedQuantityUQIsRequiredForInformalShipmentType);
			Header.BF_EstimatedQuantityUQ = "Z!";
			AssertNoMessageError(Header.BF_EstimatedQuantityUQInfo, ValidationConstants.Header.EstimatedQuantityUQIsRequiredForInformalShipmentType);
			AssertHasMessageErrorContaining(Header.BF_EstimatedQuantityUQInfo, ListValidation.InvalidCodeMessageError);
			Header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			AssertNoMessageError(Header.BF_EstimatedQuantityUQInfo, ValidationConstants.Header.EstimatedQuantityUQIsRequiredForInformalShipmentType);
			AssertNoMessageErrorContaining(Header.BF_EstimatedQuantityUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBF_EstimatedWeight()
		{
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			Header.BF_EstimatedWeightUQ = Core.Constants.Weight.Pounds;
			Header.BF_EstimatedWeight = ZInt.Zero;
			AssertHasMessageError(Header.BF_EstimatedWeightInfo, Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			AssertNoMessageError(Header.BF_EstimatedWeightInfo, Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			Header.BF_EstimatedWeightUQ = Core.Constants.Weight.Pounds;
			Header.BF_EstimatedWeight = 10;
			AssertNoMessageError(Header.BF_EstimatedWeightInfo, Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			Header.BF_EstimatedWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError(Header.BF_EstimatedWeightInfo, Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			Header.BF_EstimatedWeight = ZInt.Zero;
			AssertHasMessageError(Header.BF_EstimatedWeightInfo, Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			AssertNoNotifications(Header.BF_EstimatedWeightInfo);
		}

		public void TestCheckBF_EstimatedWeightUQ()
		{
			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			Header.BF_EstimatedWeight = 1;
			Header.BF_EstimatedWeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.WeightUQRequiredForInformal);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			AssertNoMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.WeightUQRequiredForInformal);
			Header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			Header.BF_EstimatedWeight = 1;
			Header.BF_EstimatedWeightUQ = "Z!";
			AssertNoMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.WeightUQRequiredForInformal);
			AssertHasMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.ValidWeightUQRequired); //should validate even if it is not an informal shipment
			foreach (CodeDescriptionPair pair in new CodeDescriptionPairList(OLookUpEditType.Weight))
			{
				Header.BF_EstimatedWeightUQ = pair.Code;
				AssertNoMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.WeightUQRequiredForInformal);
				AssertNoMessageErrorContaining(Header.BF_EstimatedWeightUQInfo, CusISFHeaderValidation.ValidWeightUQRequired);
			}

			Header.BF_ShipmentType = ShipmentTypeList.Codes.Informal;
			Header.BF_EstimatedWeightUQ = Core.Constants.Weight.Hectograms;
			Header.BF_EstimatedWeight = 425;
			AssertHasWarning(Header.BF_EstimatedWeightUQInfo, Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			foreach (var code in new string[] { Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds })
			{
				Header.BF_EstimatedWeightUQ = code;
				AssertNoWarning(Header.BF_EstimatedWeightUQInfo, Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			}

			var list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			list.RemoveCode(Core.Constants.Weight.Kilograms);
			list.RemoveCode(Core.Constants.Weight.Pounds);
			foreach (CodeDescriptionPair pair in list)
			{
				Header.BF_EstimatedWeightUQ = pair.Code;
				AssertHasWarning(Header.BF_EstimatedWeightUQInfo, Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			}
		}

		public void TestValidateParties()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			Header.RunPreSaveValidation();
			AssertISF10Party(Header, DocAddressType.SellingParty, x => x.SellingParty);
			AssertISF10Party(Header, DocAddressType.BuyingParty, x => x.BuyingParty);
			AssertISF10Party(Header, DocAddressType.ScheduledContainerStuffingLocation, x => x.StuffingLocation);
			AssertISF10Party(Header, DocAddressType.Consolidator, x => x.Consolidator);
			AssertShipToPartyParty(Header);
			AssertISF5Party(Header, DocAddressType.BookingPartyDocumentaryAddress, x => x.BookingParty);
		}

		void AssertShipToPartyParty(CusISFHeader header)
		{
			AssertNotNull(header.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty));
			ISFDocAddress shipToParty = header.MainShipToParty;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, OrgHeader.UnmatchedOrganisationPK));
			string messageError = ValidationConstants.Organisation.OrganisationPKRequired(shipToParty.AddressCaption, SubmissionTypeList.Codes.ISF10);
			shipToParty.OrganisationPK = org.PK;
			AssertNoMessageError(shipToParty.OrganisationPKInfo, messageError);
			shipToParty.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(shipToParty.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			shipToParty.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(shipToParty.OrganisationPKInfo, messageError);
			AssertNoMessageError(shipToParty.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			messageError = ValidationConstants.Organisation.OrganisationPKRequired(shipToParty.AddressCaption, SubmissionTypeList.Codes.ISF5);
			shipToParty.OrganisationPK = org.PK;
			AssertNoMessageError(shipToParty.OrganisationPKInfo, messageError);
			shipToParty.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(shipToParty.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			shipToParty.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(shipToParty.OrganisationPKInfo, messageError);
			AssertNoMessageError(shipToParty.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
		}

		delegate ISFDocAddress DocAddressGetter(CusISFHeader sender);
		void AssertISF10Party(CusISFHeader header, DocAddressType addressType, DocAddressGetter getDocAddress)
		{
			AssertNotNull(header.DocAddresses.FindByDocAddressType(addressType));
			ISFDocAddress docAddress = getDocAddress(header);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			string messageError = ValidationConstants.Organisation.OrganisationPKRequired(docAddress.AddressCaption, SubmissionTypeList.Codes.ISF10);
			docAddress.OrganisationPK = Org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, messageError);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			AssertHasMessageError(docAddress.OrganisationPKInfo, messageError);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			docAddress = getDocAddress(header);
			docAddress.OrganisationPK = Org.PK;
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);
		}

		void AssertISF5Party(CusISFHeader header, DocAddressType addressType, DocAddressGetter getDocAddress)
		{
			AssertNotNull(header.DocAddresses.FindByDocAddressType(addressType));
			ISFDocAddress docAddress = getDocAddress(header);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			string messageError = ValidationConstants.Organisation.OrganisationPKRequired(docAddress.AddressCaption, SubmissionTypeList.Codes.ISF5);
			docAddress.OrganisationPK = Org.PK;
			AssertNoMessageError(docAddress.OrganisationPKInfo, messageError);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(docAddress.OrganisationPKInfo, messageError);
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			docAddress.OrganisationPK = Org.PK;
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);
			AssertNoMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
			docAddress.OrganisationPK = OrgHeader.UnmatchedOrganisationPK;
			AssertHasMessageError(docAddress.OrganisationPKInfo, ValidationConstants.Organisation.UnmatchedOrganisationNotValidForCustoms);
		}

		OrgHeader org;
		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_FullName = "CARGOWISE";
					org.MainAddress.FillWithValidTestData();
					org.MainAddress.OA_City = "SYDNEY";
					org.OH_RL_NKClosestPort = "AUSYD";
				}

				return org;
			}
		}

		CusISFHeader header;
		CusISFHeader Header => header ?? (header = Factory.New<CusISFHeader>());
	}
}
