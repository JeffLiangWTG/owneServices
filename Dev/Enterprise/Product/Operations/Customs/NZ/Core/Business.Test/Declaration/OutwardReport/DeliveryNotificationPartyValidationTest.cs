using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Express;
	using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
	using Enterprise.Freight.Business;
	using static Enterprise.Integration.Customs.ASYCUDA;

	public class DeliveryNotificationPartyValidation : OutwardReportManifestStatusValidationTest
	{
		public void TestCheckOrgOrPartyAndEmailShouldBeEntered()
		{
			var message = "Either Delivery Notification Organization, Delivery Notification Port or both Delivery Notification Party and email should be entered to ensure that the CCA receives delivery advices.";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = string.Empty;
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = string.Empty;

			var validation = new OutwardReportManifestStatusValidation(ManifestStatus);
			var errors = validation.CheckErrorsBeforeGeneratingMessage(MessageBuilder.MessageTypes.Original);
			AssertCollectionContains("Should have the expected warning as all values are empty.", message, errors);

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			validation = new OutwardReportManifestStatusValidation(ManifestStatus);
			errors = validation.CheckErrorsBeforeGeneratingMessage(MessageBuilder.MessageTypes.Original);
			AssertCollectionNotContains("Should not have the expected warning as PartyPK is not empty.", message, errors);

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = "TEST PARTY";
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "TESTPARTY@MAIL.COM";

			validation = new OutwardReportManifestStatusValidation(ManifestStatus);
			errors = validation.CheckErrorsBeforeGeneratingMessage(MessageBuilder.MessageTypes.Original);
			AssertCollectionNotContains("Should not have the expected warning as PartyName And PartyEmail are all not empty.", message, errors);
		}

		public void TestNotificationPartyName()
		{
			SetUpValidAIRConsol();
			Transport.JW_VoyageFlight = "QF120";
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "john.smith@testco.org.au";
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = "";
			AssertHasMessageError("DeliveryNotificationPartyName", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyNameInfo, "You have entered a notification email address. You must state the name of a Delivery notification party to be notified by TSW when the OCR is accepted.");

			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = "JOHN SMITH INDUSTRIES P/L";
			AssertNoMessageError("DeliveryNotificationPartyName", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyNameInfo, "You have entered a notification email address. You must state the name of a Delivery notification party to be notified by TSW when the OCR is accepted.");

			ValidateAreAllNotEmpty(ManifestStatus.DeliveryNotificationParty, ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyNameInfo);

			var manifestHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			var deliveryNotificationParty = new DeliveryNotificationParty(manifestHeader);
			ValidateAreAllNotEmpty(deliveryNotificationParty, deliveryNotificationParty.DeliveryNotificationPartyEmailInfo);
		}

		public void TestOutwardReportNotificationPartyAddress()
		{
			var expectedMessageErrorForOrgAddress = "Delivery Notification Organization does not have a valid TSW Identification code configured.\r\n\r\nAn Approved Transitional Facility Code (ATF), Customs Client Code (CCD) or a Customs Controlled Premises Code (CCP) must be sent.\r\nPlease either select an appropriate organization with a valid ATF, CCD or CCP code or update this organization's config details to include their appropriate TSW Registered code.";

			var notifyOrg = OrgHeader.New(Factory);
			notifyOrg.OH_Code = "NZAKLBOND";
			notifyOrg.OH_FullName = "AUCKLAND BOND STORE";
			notifyOrg.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			notifyOrg.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			notifyOrg.OH_RL_NKClosestPort = "NZAKL";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.MainAddress.PK;
			AssertHasMessageError("DeliveryNotificationParty Address must have a CCP or ATF code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorForOrgAddress);

			var cusCode = notifyOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = notifyOrg.MainAddress.PK;
			cusCode.OK_CustomsRegNo = "79458278";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.MainAddress.PK;
			AssertNoMessageError("DeliveryNotificationParty Address has CCP code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorForOrgAddress);

			notifyOrg.CustomsCodes.RemoveAndDeleteAll();
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.MainAddress.PK;
			AssertHasMessageError("DeliveryNotificationParty Address must have a CCP or ATF code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorForOrgAddress);

			cusCode = notifyOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCode.OK_OA_PremisesAddress = notifyOrg.MainAddress.PK;
			cusCode.OK_CustomsRegNo = "1234Z";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.MainAddress.PK;
			AssertNoMessageError("DeliveryNotificationParty Address has ATF code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorForOrgAddress);
		}

		public void TestOutwardReportNotificationParty()
		{
			var expectedMessageErrorOrgHeader = "Delivery Notification Organization does not have a valid TSW Identification code configured.\r\n\r\nAn Approved Transitional Facility Code (ATF), Customs Client Code (CCD) or a Customs Controlled Premises Code (CCP) must be sent.\r\nPlease either select an appropriate organization with a valid ATF, CCD or CCP code or update this organization's config details to include their appropriate TSW Registered code.";

			var notifyOrg = OrgHeader.New(Factory);
			notifyOrg.OH_Code = "NZAKLBOND";
			notifyOrg.OH_FullName = "AUCKLAND BOND STORE";
			notifyOrg.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			notifyOrg.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			notifyOrg.OH_RL_NKClosestPort = "NZAKL";

			var cusCode = notifyOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			cusCode.OK_CustomsRegNo = "79458278";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty_ZAddress.OrgPK = notifyOrg.PK;
			AssertNoMessageError("DeliveryNotificationParty has CCP code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorOrgHeader);

			notifyOrg.CustomsCodes.RemoveAndDeleteAll();
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.PK;
			AssertHasMessageError("DeliveryNotificationParty must have a CCP, CCD or ATF code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorOrgHeader);

			cusCode = notifyOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			cusCode.OK_CustomsRegNo = "1234Z";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.PK;
			AssertNoMessageError("DeliveryNotificationParty has CCD code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorOrgHeader);

			notifyOrg.CustomsCodes.RemoveAndDeleteAll();

			cusCode = notifyOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			cusCode.OK_CustomsRegNo = "1234Z";
			cusCode.OK_RN_NKCodeCountry = "NZ";

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = notifyOrg.PK;
			AssertNoMessageError("DeliveryNotificationParty has ATF code", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, expectedMessageErrorOrgHeader);
			AssertNoError("DeliveryNotificationParty code is not invalid", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, "Enter a valid selection.");

			ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Invalid;
			AssertHasError("DeliveryNotificationParty code is invalid", ManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationPartyInfo, "Enter a valid selection.");

			ValidateAreAllNotEmpty(ManifestStatus.DeliveryNotificationParty, ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyNameInfo);

			var manifestHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			var deliveryNotificationParty = new DeliveryNotificationParty(manifestHeader);
			ValidateAreAllNotEmpty(deliveryNotificationParty, deliveryNotificationParty.DeliveryNotificationPartyEmailInfo);
		}

		public void TestNotificationPartyEmail()
		{
			SetUpValidAIRConsol();
			Transport.JW_VoyageFlight = "QF120";
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = "JOHN SMITH INDUSTRIES P/L";
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "";
			AssertHasMessageError("DeliveryNotificationPartyEmail", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmailInfo, "Must be transmitted to state the email of the Delivery Notification Party where delivery notification is required to a non TSW registered party.");

			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "john.smith@testco.org.au";
			AssertNoMessageError("DeliveryNotificationPartyEmail", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmailInfo, "Must be transmitted to state the email of the Delivery Notification Party where delivery notification is required to a non TSW registered party.");

			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "john.smith";
			AssertHasErrors("DeliveryNotificationPartyEmail", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmailInfo);

			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "john.smith@testco.org.au";
			AssertNoErrors("DeliveryNotificationPartyEmail", ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmailInfo);

			ValidateAreAllNotEmpty(ManifestStatus.DeliveryNotificationParty, ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmailInfo);

			var manifestHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			var deliveryNotificationParty = new DeliveryNotificationParty(manifestHeader);
			ValidateAreAllNotEmpty(deliveryNotificationParty, deliveryNotificationParty.DeliveryNotificationPartyEmailInfo);
		}

		public void TestNotificationPartyPort()
		{
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort = string.Empty;
			AssertNoMessageErrors(ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPortInfo);
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort = "ABC01";
			AssertHasMessageError(ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPortInfo, ListValidation.InvalidCodeMessageError);
			ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort = "NZAKL";
			AssertNoMessageError(ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPortInfo, ListValidation.InvalidCodeMessageError);

			ValidateAreAllNotEmpty(ManifestStatus.DeliveryNotificationParty, ManifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyNameInfo);

			var manifestHeader = Factory.New<IAsycudaManifestHeader>() as BusinessObject;
			var deliveryNotificationParty = new DeliveryNotificationParty(manifestHeader);
			ValidateAreAllNotEmpty(deliveryNotificationParty, deliveryNotificationParty.DeliveryNotificationPartyEmailInfo);
		}

		void ValidateAreAllNotEmpty(DeliveryNotificationParty deliveryNotificationParty, ZPropertyInfo info)
		{
			deliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			deliveryNotificationParty.DeliveryNotificationPartyName = string.Empty;
			deliveryNotificationParty.DeliveryNotificationPartyEmail = string.Empty;
			deliveryNotificationParty.DeliveryNotificationPartyPort = string.Empty;
			deliveryNotificationParty.Validation.ValidateAll();

			var message = "Either Delivery Notification Organization, Delivery Notification Port or both Delivery Notification Party and email should be entered to ensure that the CCA receives delivery advices.";
			if (deliveryNotificationParty.Parent is IAsycudaManifestHeader)
			{
				AssertHasMessageError("Should have the expected message error as all values are empty.", info, message);
			}
			else
			{
				AssertHasWarning("Should have the expected warning as all values are empty.", info, message);
			}

			deliveryNotificationParty.E2_OA_DeliveryNotificationParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNoWarning("Should not have the expected warning as PartyPK is not empty.", info, message);
			AssertNoMessageError("Should not have the expected message error as PartyPK is not empty.", info, message);

			deliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			deliveryNotificationParty.DeliveryNotificationPartyPort = "NZAKL";

			AssertNoWarning("Should not have the expected warning as Port is not empty.", info, message);
			AssertNoMessageError("Should not have the expected message error as Port is not empty.", info, message);

			deliveryNotificationParty.DeliveryNotificationPartyPort = string.Empty;
			deliveryNotificationParty.DeliveryNotificationPartyName = "TEST PARTY";
			deliveryNotificationParty.DeliveryNotificationPartyEmail = "TESTPARTY@MAIL.COM";

			AssertNoWarning("Should not have the expected warning as PartyName And PartyEmail are all not empty.", info, message);
			AssertNoMessageError("Should not have the expected message error as PartyName And PartyEmail are all not empty.", info, message);

			var seaCargo = Factory.New<CusSCAOceanBill>();
			var manifestStatus = new OutwardReportManifestStatus(seaCargo);
			manifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = ZGuid.Empty;
			manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName = string.Empty;
			manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail = string.Empty;
			manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort = string.Empty;
			manifestStatus.DeliveryNotificationParty.Validation.ValidateAll();
			AssertNoWarning("Should have the expected warning as all values are empty.", info, message);
		}
	}
}
