using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderApplicantAddressRequirement))]
	sealed class ControllingMessageHeaderApplicantAddressRequirementTest : TestCaseWithFactory
	{
		public void TestLocalAddressWhenNotOverrided()
		{
			var errorMsgForLocalAddress = ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalAddressIsRequired;
			var errorMsgForLocalCompanyName = ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalCompanyNameIsRequired;
			var org = Factory.New<OrgHeader>();
			var emptyAddress = org.Addresses.AddNew();
			var fullAddress = org.Addresses.AddNew();
			var localAddress = fullAddress.TranslatedAddresses.AddNew();
			localAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			localAddress.Address1 = "TestAddress";
			localAddress.CompanyName = "TestCompany";

			var targerInfo = applicantDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				applicantDocumentaryAddress.OrganisationPK = org.PK;

				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					applicantDocumentaryAddress.E2_OA_Address = emptyAddress.PK;

					applicantDocumentaryAddress.Validation.ValidateE2_OA_Address();
					var needValidateLocalAddress1 = messageType == ControllingMessageTypeList.Codes.NX101 || messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603;
					var needValidateLocalCompanyName = messageType == ControllingMessageTypeList.Codes.NX201_01 || messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603;

					if (needValidateLocalAddress1)
					{
						if (messageType == ControllingMessageTypeList.Codes.NX201_01)
						{
							declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
							applicantDocumentaryAddress.Validation.ValidateE2_OA_Address();
							AssertHasMessageErrorContaining($"{messageType}: Local Address empty", targerInfo, errorMsgForLocalAddress);

							declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
							applicantDocumentaryAddress.Validation.ValidateE2_OA_Address();
							AssertNoMessageErrorContaining($"{messageType}: not validate Local Address when Export", targerInfo, errorMsgForLocalAddress);
						}
						else
						{
							AssertHasMessageErrorContaining($"{messageType}: Local Address empty", targerInfo, errorMsgForLocalAddress);
						}
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Local Address", targerInfo, errorMsgForLocalAddress);
					}

					if (needValidateLocalCompanyName)
					{
						AssertHasMessageErrorContaining($"{messageType}: Local Company Name empty", targerInfo, errorMsgForLocalCompanyName);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Local Company Name", targerInfo, errorMsgForLocalCompanyName);
					}

					applicantDocumentaryAddress.E2_OA_Address = fullAddress.PK;
					if (needValidateLocalAddress1)
					{
						AssertNoMessageErrorContaining($"{messageType}: Local Address is TestAddress", targerInfo, errorMsgForLocalAddress);
					}

					if (needValidateLocalCompanyName)
					{
						AssertNoMessageErrorContaining($"{messageType}: Local Company Name is TestCompany", targerInfo, errorMsgForLocalCompanyName);
					}
				}
			});
		}

		public void TestCheckCompanyName()
		{
			var errorMsg = ValidationConstants.CusTWControllingMessageHeader.Applicant.CompanyNameIsRequired;
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			var targerInfo = applicantDocumentaryAddress.E2_CompanyNameInfo;
			var targetInfoNotOverrided = applicantDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					applicantDocumentaryAddress.E2_AddressOverride = false;
					applicantDocumentaryAddress.OrganisationPK = ZGuid.Empty;
					applicantDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
					applicantDocumentaryAddress.Validation.ValidateE2_CompanyName();

					if (messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"{messageType}: Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						AssertHasMessageErrorContaining($"{messageType}: CompanyName empty", targerInfo, errorMsg);

						applicantDocumentaryAddress.E2_CompanyName = "TestCompany";
						AssertNoMessageErrorContaining($"{messageType}: CompanyName is TestCompany", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate CompanyName when Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						applicantDocumentaryAddress.E2_CompanyName = ZString.Empty;
						AssertNoMessageErrorContaining($"{messageType}: not validate CompanyName", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckTelephoneNumber()
		{
			var errorMsg = ValidationConstants.CusTWControllingMessageHeader.Applicant.TelephoneNumberIsRequired;
			var org = Factory.New<OrgHeader>();
			var addressWithPhone = org.Addresses.AddNew();
			addressWithPhone.OA_Phone = "11111111111";
			var addressWithoutPhone = org.Addresses.AddNew();
			addressWithoutPhone.OA_Phone = ZString.Empty;

			var targerInfo = applicantDocumentaryAddress.E2_PhoneInfo;
			var targetInfoNotOverrided = applicantDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					applicantDocumentaryAddress.E2_AddressOverride = false;
					applicantDocumentaryAddress.OrganisationPK = ZGuid.Empty;
					applicantDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
					applicantDocumentaryAddress.Validation.ValidateOrganisationPK();

					if (messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"{messageType}: Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.OrganisationPK = org.PK;
						applicantDocumentaryAddress.E2_OA_Address = addressWithoutPhone.PK;
						AssertHasMessageErrorContaining($"{messageType}: Phone empty", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_OA_Address = addressWithPhone.PK;
						AssertNoMessageErrorContaining($"{messageType}: Phone is 11111111111", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						applicantDocumentaryAddress.E2_Phone = ZString.Empty;
						AssertHasMessageErrorContaining($"{messageType}: Overrided Address and Phone empty", targerInfo, errorMsg);

						applicantDocumentaryAddress.E2_Phone = "22222222222";
						AssertNoMessageErrorContaining($"{messageType}: Overrided Address and Phone is 22222222222", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Phone when Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.OrganisationPK = org.PK;
						applicantDocumentaryAddress.E2_OA_Address = addressWithoutPhone.PK;
						AssertNoMessageErrorContaining($"{messageType}: not validate Phone", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						applicantDocumentaryAddress.E2_Phone = ZString.Empty;
						AssertNoMessageErrorContaining($"{messageType}: not validate Phone when overrided", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckIDCode()
		{
			var errorMsg = ValidationConstants.CusTWControllingMessageHeader.Applicant.IDIsRequired;
			var orgWithCode = Factory.New<OrgHeader>();
			var addressWithCode = orgWithCode.Addresses.AddNew();
			addressWithCode.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);

			var orgWithoutCode = Factory.New<OrgHeader>();
			var addressWithoutCode = orgWithoutCode.Addresses.AddNew();

			var targerInfo = applicantDocumentaryAddress.IDCodeInfo;
			var targetInfoNotOverrided = applicantDocumentaryAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					applicantDocumentaryAddress.E2_AddressOverride = false;
					applicantDocumentaryAddress.OrganisationPK = ZGuid.Empty;
					applicantDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
					applicantDocumentaryAddress.Validation.ValidateOrganisationPK();

					if (messageType == ControllingMessageTypeList.Codes.NX101 ||
						messageType == ControllingMessageTypeList.Codes.NX201_07 ||
						messageType == ControllingMessageTypeList.Codes.NX301 ||
						messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"{messageType}: Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.OrganisationPK = orgWithoutCode.PK;
						applicantDocumentaryAddress.E2_OA_Address = addressWithoutCode.PK;
						AssertHasMessageErrorContaining($"{messageType}: IDCode empty", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.OrganisationPK = orgWithCode.PK;
						applicantDocumentaryAddress.E2_OA_Address = addressWithCode.PK;
						AssertNoMessageErrorContaining($"{messageType}: Address has a IDCode", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						applicantDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
						applicantDocumentaryAddress.IDCode = ZString.Empty;
						AssertHasMessageErrorContaining($"{messageType}: Overrided Address and IDCode empty", targerInfo, errorMsg);

						applicantDocumentaryAddress.IDCode = "22222222";
						AssertNoMessageErrorContaining($"{messageType}: Overrided Address and has a IDCode", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate IDCode when Address not set", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.OrganisationPK = orgWithoutCode.PK;
						applicantDocumentaryAddress.E2_OA_Address = addressWithoutCode.PK;
						AssertNoMessageErrorContaining($"{messageType}: not validate IDCode", targetInfoNotOverrided, errorMsg);

						applicantDocumentaryAddress.E2_AddressOverride = true;
						applicantDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
						applicantDocumentaryAddress.IDCode = ZString.Empty;
						AssertNoMessageErrorContaining($"{messageType}: not validate IDCode when overrided", targerInfo, errorMsg);
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			cMHeader = instruction.ControllingMessageHeaders.AddNew();
			applicantDocumentaryAddress = cMHeader.ApplicantDocumentaryAddress;
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader cMHeader;
		TWJobDocAddress applicantDocumentaryAddress;
	}
}
