using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	sealed class ProtestValidationTestCase : BusinessObjectValidationTestCase
	{
		public void TestValidateRefundPartyOrganisationPK()
		{
			Protest.US_P_RefundCOPartyType = "T";
			Protest.RefundPartyAddress.OrganisationPK = ZGuid.Empty;
			Protest.RunPreSaveValidation();
			AssertHasMessageError(Protest.RefundPartyAddress.OrganisationPKInfo,
				"Refund C/O Party is required when a Refund C/O Party Type has a value.");

			var refundPartyOrgHeader = Factory.New<OrgHeader>();
			Protest.RefundPartyAddress.OrganisationPK = refundPartyOrgHeader.PK;
			Protest.RunPreSaveValidation();
			AssertHasMessageError(Protest.RefundPartyAddress.OrganisationPKInfo,
				"Refund Party Organization does not have a valid EIN, SSN or CBN Identification Number.");

			refundPartyOrgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "333");
			Protest.RunPreSaveValidation();
			AssertNoMessageErrors(Protest.RefundPartyAddress.OrganisationPKInfo);
		}

		public void TestTariffActCitation()
		{
			Protest.TariffActCitation = "";
			AssertHasMessageError(Protest.TariffActCitationInfo, ProtestValidation.TariffActCitationRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			AssertNoMessageError(Protest.TariffActCitationInfo, ProtestValidation.TariffActCitationRequired);

			Protest.TariffActCitation = "X";
			AssertHasMessageError(Protest.TariffActCitationInfo, ProtestValidation.TariffActCitation);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			AssertNoMessageError(Protest.TariffActCitationInfo, ProtestValidation.TariffActCitation);
		}

		public void TestValidateEntries()
		{
			Protest.Validation.ValidateEntries();
			AssertHasRowMessageError(Protest, ProtestValidation.EntryRequired);

			Protest.LinkedEntries.AddNew();
			Protest.Validation.ValidateEntries();
			AssertNoRowMessageError(Protest, ProtestValidation.EntryRequired);
		}

		public void TestCheckJustificationNote()
		{
			Protest.Validation.ValidateJustificationNote();
			AssertHasMessageError(Protest.JustificationNoteInfo, ProtestValidation.JustificationNoteRequired);

			Protest.JustificationNote = "For Test";
			AssertNoMessageError(Protest.JustificationNoteInfo, ProtestValidation.JustificationNoteRequired);
		}

		public void TestCheckCountry()
		{
			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			Protest.Protestant.E2_AddressOverride = true;
			Protest.Protestant.E2_RN_NKCountryCode = "AU";
			Protest.RunPreSaveValidation();
			Assert(!Protest.Protestant.OrganisationPKInfo.HasMessageError(ProtestValidation.ProtestantIsRequired));
			AssertHasMessageErrorContaining(Protest.Protestant.E2_RN_NKCountryCodeInfo, ProtestValidation.ValidProtestantCountry);

			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ImporterConsignee;
			Protest.Protestant.E2_RN_NKCountryCode = "AU";
			Protest.RunPreSaveValidation();
			AssertNoMessageErrorContaining(Protest.Protestant.E2_RN_NKCountryCodeInfo, ProtestValidation.ValidProtestantCountry);
		}

		public void TestCheckRegoNumber()
		{
			Protest.Protestant.E2_AddressOverride = false;
			Protest.RunPreSaveValidation();
			AssertHasMessageError(Protest.Protestant.OrganisationPKInfo, ProtestValidation.ProtestantIsRequired);

			Protest.Protestant.E2_AddressOverride = true;
			Protest.Protestant.E2_GovRegNum = "081";
			Protest.RunPreSaveValidation();
			Assert(!Protest.Protestant.OrganisationPKInfo.HasMessageError(ProtestValidation.ProtestantIDIsRequired));
		}

		public void TestAutoValidationType()
		{
			var validation = new ProtestValidation(Protest);
			AssertEquals(typeof(Protest), validation.AutoValidationType);
		}

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Factory.New<JobDeclaration>()));
	}
}
