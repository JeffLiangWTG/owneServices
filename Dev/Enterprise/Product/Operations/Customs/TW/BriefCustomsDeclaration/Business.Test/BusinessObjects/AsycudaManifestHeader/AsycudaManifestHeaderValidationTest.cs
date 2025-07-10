using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderValidation))]
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_RecipientReference()
		{
			var boxNumbers = new CusBrokerageBoxNumberCollection();
			var boxNumber = boxNumbers.AddNew();
			boxNumber.BoxNumber = "C01";
			boxNumber.CustomsOfficeArea = "C";
			boxNumber.IsDefaultBoxNumber = false;
			var boxNumber2 = boxNumbers.AddNew();
			boxNumber2.BoxNumber = "C02";
			boxNumber2.CustomsOfficeArea = "C";
			boxNumber2.IsDefaultBoxNumber = true;
			using (TWCustomsDataRegistry.Instance.CusBrokerageBoxNumber.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, boxNumbers))
			{
				manifestHeader.AMA_CustomsOffice = "CA";
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_RecipientReferenceInfo, new ZString[] { "B01", "D02" }, new ZString[] { "C01", "C02" });
			}
		}

		public void TestCheckAMA_GS_NKCustomsAgent()
		{
			var targetInfo = manifestHeader.AMA_GS_NKCustomsAgentInfo;
			var noTWBrkCertificateMessage = "The selected Customs Agent does not have a Customs Clearance Agent of Special Examination Certificate Number. Go to Staff > Human Resources > Certificates, ID and Training to create a TW-BRK certificate.";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS1";
			manifestHeader.AMA_GS_NKCustomsAgent = "GS1";
			AssertHasMessageError(targetInfo, noTWBrkCertificateMessage);

			var expiredMessage = "The Customs Clearance Agent of Special Examination Certificate Number of the selected Customs Agent has expired.";
			var cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_IssueDate = new ZDate(2019, 08, 02);
			cert.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-20);
			cert.XZ_RefNumber = "CER1";
			manifestHeader.Validation.ValidateAMA_GS_NKCustomsAgent();
			AssertHasMessageError(targetInfo, expiredMessage);

			cert.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(20);
			manifestHeader.Validation.ValidateAMA_GS_NKCustomsAgent();
			var willExpireWarning = $"The Customs Clearance Agent of Special Examination Certificate Number of the selected Customs Agent will expire on {cert.XZ_ExpiryOrDueDate.ToString("MM dd, yyyy")}.";
			AssertHasWarning(targetInfo, willExpireWarning);

			var noMailboxMessage = "The entered Customs Agent does not have a valid mailbox. Go to Staff > Credentials to create a mailbox.";
			cert.XZ_ExpiryOrDueDate = ZDate.Today.AddYears(1);
			manifestHeader.Validation.ValidateAMA_GS_NKCustomsAgent();
			AssertHasMessageError(targetInfo, noMailboxMessage);

			var password = TWGlbStaffWrapper.Get(staff).TWPasswordCollection.AddNew();
			password.GP_MailBoxID = "MB1";
			manifestHeader.Validation.ValidateAMA_GS_NKCustomsAgent();
			AssertNoMessageError(targetInfo, noTWBrkCertificateMessage);
			AssertNoMessageError(targetInfo, expiredMessage);
			AssertNoWarning(targetInfo, willExpireWarning);
			AssertNoMessageError(targetInfo, noMailboxMessage);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_GS_NKCustomsAgentInfo, new ZString[] { "XXX" }, new ZString[] { "GS1" });
		}

		public void TestCheckAMA_CustomsProfile()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var passwords = TWGlbStaffWrapper.Get(staff).TWPasswordCollection;
			var password = passwords.AddNew();
			password.GP_MailBoxID = "MB1";
			var password2 = passwords.AddNew();
			password2.GP_MailBoxID = "MB2";
			manifestHeader.AMA_GS_NKCustomsAgent = "GS1";

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_CustomsProfileInfo, new ZString[] { "XXX" }, new ZString[] { "MB1", "MB2" });
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var notificationExpectedToBeFound = "The entered Carrier does not have a Taiwan VAT code. Go to Organization > Details > Config > Registration Numbers / Codes to create a TW-VAT code.";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			manifestHeader.AMA_OA_Carrier = orgAddress.PK;
			AssertHasMessageError(manifestHeader.AMA_OA_CarrierInfo, notificationExpectedToBeFound);

			var vATCusCode = orgHeader.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			manifestHeader.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError(manifestHeader.AMA_OA_CarrierInfo, notificationExpectedToBeFound);
		}

		public void TestCheckAMA_OA_CarrierMandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(manifestHeader.AMA_OA_CarrierInfo);
		}

		public void TestCheckAMA_VehicleRegistration()
		{
			var notificationExpectedToBeFound = "Vessel Registration Number is exactly 6 characters long.";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.AMA_VehicleRegistration = "AAAAA";
			AssertHasMessageError(manifestHeader.AMA_VehicleRegistrationInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_VehicleRegistration = "AAAAAA";
			AssertNoMessageError(manifestHeader.AMA_VehicleRegistrationInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_VehicleRegistration = ZString.Empty;
			AssertNoMessageError(manifestHeader.AMA_VehicleRegistrationInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_VehicleRegistration = "AAAAA";
			AssertNoMessageError(manifestHeader.AMA_VehicleRegistrationInfo, notificationExpectedToBeFound);
		}

		public void TestCheckAMA_ManifestNumber()
		{
			var notificationExpectedToBeFound = "Manifest Number is exactly 4 characters long.";
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			manifestHeader.AMA_ManifestNumber = "AAAA";
			AssertNoMessageError(manifestHeader.AMA_ManifestNumberInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_ManifestNumber = "AAA";
			AssertHasMessageError(manifestHeader.AMA_ManifestNumberInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_ManifestNumber = ZString.Empty;
			AssertNoMessageError(manifestHeader.AMA_ManifestNumberInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			manifestHeader.AMA_ManifestNumber = "AAA";
			AssertNoMessageError(manifestHeader.AMA_ManifestNumberInfo, notificationExpectedToBeFound);

			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			manifestHeader.AMA_ManifestNumber = "AA";
			AssertNoMessageError(manifestHeader.AMA_ManifestNumberInfo, notificationExpectedToBeFound);
		}

		public void TestCheckAMA_Nature()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_NatureInfo, new ZString[] { "XXX" }, manifestHeader.Lookups.Natures.ToList<ZString>().ToArray());
		}

		public void TestCheckAMA_TransportMode()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_TransportModeInfo, new ZString[] { "XX" }, manifestHeader.Lookups.TransportModeList.ToList<ZString>().ToArray());
		}

		public void TestCheckAMA_CustomsOffice()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_CustomsOfficeInfo, new ZString[] { "XX" }, manifestHeader.Lookups.CustomsOffices.ToList<ZString>().ToArray());
		}

		public void TestCheckAMA_PaymentMethod()
		{
			var codes = manifestHeader.Lookups.PaymentMethods.ToList<ZString>().ToArray();
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(manifestHeader.AMA_PaymentMethodInfo, new ZString[] { "X" }, codes);
			manifestHeader.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			ValidationTestHelper.AssertInvalidCodeMessageError(manifestHeader.AMA_PaymentMethodInfo, new ZString[] { "X" }, codes);
		}

		public void TestCheckDeclarationNumberDisplay()
		{
			manifestHeader.AMA_CustomsOffice = "AR";
			manifestHeader.AMA_RecipientReference = "600";
			manifestHeader.DeclarationDate = new ZDateTime(2020, 08, 05);
			var targetInfo = manifestHeader.DeclarationNumberDisplayInfo;

			manifestHeader.DeclarationNumber = EntryNumberGenerator.New(manifestHeader).GenerateEntryNumber();
			AssertEquals("AR  0960000001", manifestHeader.DeclarationNumber);
			var errorMessage = ValidationConstants.AllocateNumber.KeyComponentValuesChanged;
			manifestHeader.AMA_CustomsOffice = "BE";
			manifestHeader.AMA_RecipientReference = "600";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			manifestHeader.AMA_CustomsOffice = "AR";
			manifestHeader.AMA_RecipientReference = "600";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			manifestHeader.DeclarationDate = new ZDateTime(2021, 08, 05);
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			manifestHeader.DeclarationDate = new ZDateTime(2020, 08, 05);
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();

			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			manifestHeader.AMA_RecipientReference = "456";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			manifestHeader.AMA_RecipientReference = "600";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			manifestHeader.BagNumber = "ABCDE";
			manifestHeader.DeclarationNumber = EntryNumberGenerator.New(manifestHeader).GenerateEntryNumber();
			AssertEquals("AR  09600ABCDE", manifestHeader.DeclarationNumber);

			manifestHeader.BagNumber = "EDCBA";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			manifestHeader.BagNumber = "ABCDE";
			manifestHeader.Validation.ValidateDeclarationNumberDisplay();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader manifestHeader;
	}
}
