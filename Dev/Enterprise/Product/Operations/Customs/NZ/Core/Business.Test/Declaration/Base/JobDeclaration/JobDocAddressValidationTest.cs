using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using NZ.Business;
	using NZ.Business.Declaration;

	public class JobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestMessageErrorForDeliveryDestination()
		{
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.DeliveryDestinationPartyDocAddress.E2_AddressOverride = true;
			declaration.DeliveryDestinationPartyDocAddress.E2_Address1 = "Address";
			declaration.DeliveryDestinationPartyDocAddress.E2_City = "City";
			declaration.DeliveryDestinationPartyDocAddress.E2_Postcode = "2000";
			declaration.DeliveryDestinationPartyDocAddress.E2_State = "NSW";
			declaration.DeliveryDestinationPartyDocAddress.E2_RN_NKCountryCode = "AU";
			AssertNoMessageError(declaration.DeliveryDestinationPartyDocAddress.E2_Address1Info, "The country/region for the Delivery Address differs from the port of final destination country/region.");

			declaration.JE_RL_NKFinalDestination = "JPTYO";
			declaration.DeliveryDestinationPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("Declaration Deliver To address should have a message error", declaration.DeliveryDestinationPartyDocAddress.E2_OA_AddressInfo, "The country/region for the Delivery Address differs from the port of final destination country/region.");

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.DeliveryDestinationPartyDocAddress.E2_RN_NKCountryCode = "SG";
			declaration.DeliveryDestinationPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("Legacy entry should not hit this validation", declaration.DeliveryDestinationPartyDocAddress.E2_OA_AddressInfo, "The country/region for the Delivery Address differs from the port of final destination country/region.");
		}

		public void TestMessageErrorForDeliveryNotificationCCPATFDocAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var winterfell = org.Addresses.AddNew();
			winterfell.OA_Code = "WINTERFELL";
			var kingsLanding = org.Addresses.AddNew();
			kingsLanding.OA_Code = "KINGSLANDING";
			kingsLanding.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			var highGarden = org.Addresses.AddNew();
			highGarden.OA_Code = "HIGHGARDEN";
			highGarden.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "22222B");

			declaration.NotifyParty2DocumentaryAddress.OrganisationPK = org.PK;

			declaration.NotifyParty2DocumentaryAddress.E2_OA_Address = winterfell.PK;
			AssertHasMessageError("Winterfell has no CCP nor ATF code.", declaration.NotifyParty2DocumentaryAddress.E2_OA_AddressInfo, JobDocAddressValidation.OrganisationShouldHaveCCPOrATFCode);

			declaration.NotifyParty2DocumentaryAddress.E2_OA_Address = kingsLanding.PK;
			AssertNoMessageError("King's Landing has CCP code.", declaration.NotifyParty2DocumentaryAddress.E2_OA_AddressInfo, JobDocAddressValidation.OrganisationShouldHaveCCPOrATFCode);

			declaration.NotifyParty2DocumentaryAddress.E2_OA_Address = highGarden.PK;
			AssertNoMessageError("High garden has ATF code.", declaration.NotifyParty2DocumentaryAddress.E2_OA_AddressInfo, JobDocAddressValidation.OrganisationShouldHaveCCPOrATFCode);

			declaration.NotifyParty2DocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "33333C");
			AssertNoMessageError("There is no code on address, but we have one on organisation.", declaration.NotifyParty2DocumentaryAddress.E2_OA_AddressInfo, JobDocAddressValidation.OrganisationShouldHaveCCPOrATFCode);
		}

		[NUnit.Framework.ExpectNoExceptions()]
		public void TestNullReferenceError()
		{
			declaration.DeliveryDestinationPartyDocAddress.Validation.ValidateE2_OA_Address();
		}

		#region Implementation

		JobDeclaration declaration;
		OrgHeader importer;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.OH_IsConsignee = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
		}

		#endregion
	}
}
