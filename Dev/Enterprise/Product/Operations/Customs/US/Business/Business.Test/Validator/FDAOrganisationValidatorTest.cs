using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FDAOrganisationValidatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			AssertOrganisation(dummy.Z0_GuidInfo, delegate(ZGuid organisationPK)
			{
				dummy.Z0_Guid = organisationPK;
				FDAOrganisationValidator.Validate(Factory.Load<OrgHeader>(organisationPK), dummy.Z0_GuidInfo);
			});
		}

		internal static void AssertOrganisation(ZPropertyInfo organisationInfo, ValidateOrganisation validate)
		{
			var factory = organisationInfo.BizObj.Factory;
			validate(ZGuid.Empty);
			var name = organisationInfo.HumanReadableName;
			var messageError = string.Format(ValidationConstants.PriorNotice.Organisation, name);
			AssertHasMessageError(organisationInfo, messageError);

			var ultimateConsignee = factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "ZXC" + new Random().Next(1000000).ToString();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationCity, name);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_City = "Chicago";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationCountry, name);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationStateProvince, name);
			ultimateConsignee.MainAddress.OA_State = "";
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_State = "IL";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFirmName, name);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.OH_FullName = "cargowise";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationPCode, name);
			OrgHeaderWrapper.New(ultimateConsignee);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_PostCode = "987654";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactFirstName, name);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, "Brendon", null, null, null, null);
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactLastName, name);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, "Brendon", "Paine", null, null, null);
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationAddress, name);
			ultimateConsignee.MainAddress.OA_Address1 = "";
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_Address1 = "Holland Village";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactPhone, name);
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, "987654325", null, null);
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactPhoneFormat, name);
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, "+ 1 (234) 5678955", null, null);
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, "", null, null);
			factory.Save();
			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFAXFormat, name);
			ultimateConsignee.MainAddress.OA_Fax = "+1(2)567890";
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			ultimateConsignee.MainAddress.OA_Fax = "+ 1 (234) 5678901";
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, null, null, "+1(2)567890");
			factory.Save();
			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFAXFormat, name);
			ultimateConsignee.MainAddress.OA_Fax = "";
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, null, null, "+ 1 (234) 5678901");
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, null, null, "");
			ultimateConsignee.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, "234567895", null, null);
			factory.Save();
			messageError = string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactPhoneFormat, name);
			validate(ultimateConsignee.PK);
			AssertHasMessageError(organisationInfo, messageError);
			DeclarationTestHelper.AddPGAContact(ultimateConsignee, null, null, "+ 3 (234) 56789553", null, null);
			factory.Save();
			validate(ultimateConsignee.PK);
			AssertNoMessageError(organisationInfo, messageError);

			if (CargoWise.Common.ErrorReporter.LastKeyReported == "Validation:" + organisationInfo.Name)
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
		}

		internal delegate void ValidateOrganisation(ZGuid organisationPK);
	}
}
