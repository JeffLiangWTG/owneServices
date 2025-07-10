using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESAddressValidatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			AssertOrganisation(dummy.Z0_GuidInfo, (ZGuid orgAddressPK) =>
			{
				dummy.Z0_Guid = orgAddressPK;
				AESAddressValidator.Validate(dummy.Z0_GuidInfo, Factory.Load<OrgAddress>(orgAddressPK));
			});
		}

		static void AssertOrganisation(ZPropertyInfo organisationInfo, ValidateOrganisation validate)
		{
			var factory = organisationInfo.BizObj.Factory;
			var refCountryStates1 = factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "US";
			refCountryStates1.RW_Description = "KNZTEST";
			factory.Save();
			validate(ZGuid.Empty);
			AssertHasMessageError(organisationInfo, AESAddressValidator.AddressRequired);
			OrgHeader ultimateConsignee = factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "ZXC" + new Random().Next(1000000).ToString();
			validate(ultimateConsignee.PK);
			OrgAddress orgAddress = ultimateConsignee.MainAddress;
			orgAddress.OA_Address1 = "";
			orgAddress.OA_City = "";
			orgAddress.OA_RL_NKRelatedPortCode = "";
			orgAddress.OA_State = "";
			orgAddress.OA_PostCode = "";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.Address1Required);
			orgAddress.OA_Address1 = "Test Address 1";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.CityRequired);
			orgAddress.OA_City = "Test City";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.CountryRequired);
			ultimateConsignee.OH_RL_NKClosestPort = "USSFO";
			orgAddress.OA_State = "";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.StateRequired);
			orgAddress.OA_State = "CA";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.PostalCodeRequired);
			orgAddress.OA_PostCode = "A723";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			ultimateConsignee.OH_RL_NKClosestPort = "PRSJU";
			orgAddress.OA_State = "CA";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			ultimateConsignee.OH_RL_NKClosestPort = "USDAL";
			orgAddress.OA_PostCode = "TX75261";
			orgAddress.OA_State = "TX";
			validate(orgAddress.PK);
			AssertHasMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			orgAddress.OA_IsActive = false;
			validate(orgAddress.PK);
			AssertHasMessageErrorContaining(organisationInfo, "You have selected an inactive address.");
			orgAddress.OA_IsActive = true;
			orgAddress.OA_PostCode = "75261";
			orgAddress.OA_State = "TX";
			validate(orgAddress.PK);
			AssertNoMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			orgAddress.OA_PostCode = "60046-2333";
			orgAddress.OA_State = "TX";
			validate(orgAddress.PK);
			AssertNoMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			orgAddress.OA_PostCode = "600462333";
			orgAddress.OA_State = "TX";
			validate(orgAddress.PK);
			AssertNoMessageError(organisationInfo, AESAddressValidator.PostalCodeInvalidFormat);
			ultimateConsignee.OH_RL_NKClosestPort = "USDAL";
			orgAddress.OA_State = "XXX";
			validate(orgAddress.PK);
			AssertHasMessageErrorContaining(organisationInfo, "The state is not a valid");
			orgAddress.OA_State = "KNZ";
			validate(orgAddress.PK);
			AssertNoMessageErrorContaining(organisationInfo, "The state is not a valid");
			orgAddress.OA_State = "KNZTEST";
			validate(orgAddress.PK);
			AssertNoMessageErrorContaining(organisationInfo, "The state is not a valid");
			ultimateConsignee.OH_RL_NKClosestPort = "AUSYD";
			orgAddress.OA_State = "XXX";
			validate(orgAddress.PK);
			AssertNoMessageErrorContaining(organisationInfo, "The state is not a valid");
			if (CargoWise.Common.ErrorReporter.LastKeyReported == "Validation:" + organisationInfo.Name)
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		delegate void ValidateOrganisation(ZGuid orgAddressPK);
	}
}
