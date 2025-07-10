using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ApplicantLocalAddressRequirement))]
	sealed class ApplicantLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckAddress()
		{
			var errorMsg = ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalAddressIsRequired;
			var org = Factory.New<OrgHeader>();
			applicantDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = applicantDocumentaryAddress.LocalAddress;

			var targerInfo = localAddress.E2_Address1Info;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					localAddress.Address1 = ZString.Empty;
					localAddress.Validation.ValidateE2_Address1();

					if (messageType == ControllingMessageTypeList.Codes.NX101 || messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"{messageType}: Local Address empty", targerInfo, errorMsg);

						localAddress.Address1 = "Test Address";
						localAddress.Validation.ValidateE2_Address1();
						AssertNoMessageErrorContaining($"{messageType}: Local Address is Test Address", targerInfo, errorMsg);
					}
					else if (messageType == ControllingMessageTypeList.Codes.NX201_01)
					{
						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
						localAddress.Validation.ValidateE2_Address1();
						AssertHasMessageErrorContaining($"{messageType}: Local Address empty", targerInfo, errorMsg);

						localAddress.Address1 = "Test Address";
						localAddress.Validation.ValidateE2_Address1();
						AssertNoMessageErrorContaining($"{messageType}: Local Address is Test Address", targerInfo, errorMsg);

						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
						localAddress.Validation.ValidateE2_Address1();
						AssertNoMessageErrorContaining($"{messageType}: not validate Local Address when Export", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Local Address", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckCompanyName()
		{
			var errorMsg = ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalCompanyNameIsRequired;
			var org = Factory.New<OrgHeader>();
			applicantDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = applicantDocumentaryAddress.LocalAddress;

			var targerInfo = localAddress.E2_CompanyNameInfo;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					localAddress.E2_CompanyName = ZString.Empty;
					localAddress.Validation.ValidateE2_CompanyName();

					if (messageType == ControllingMessageTypeList.Codes.NX201_01 || messageType == ControllingMessageTypeList.Codes.NX301 || messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"{messageType}: Local CompanyName empty", targerInfo, errorMsg);

						localAddress.E2_CompanyName = "TestCompany";
						localAddress.Validation.ValidateE2_CompanyName();
						AssertNoMessageErrorContaining($"{messageType}: Local CompanyName is TestCompany", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Local CompanyName", targerInfo, errorMsg);
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
