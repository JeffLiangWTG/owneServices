using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderImporterLocalAddressRequirement))]
	sealed class ControllingMessageHeaderImporterLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckAddress()
		{
			var errorMsg = "You have not entered an Importer Local Address.";
			var org = Factory.New<OrgHeader>();
			var targerInfo = importerLocalAddress.E2_Address1Info;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerLocalAddress.Address1 = ZString.Empty;
					importerLocalAddress.Validation.ValidateE2_Address1();

					if (cMHeader.IsNX101 ||
						cMHeader.IsNX301 ||
						cMHeader.IsNX301_AX ||
						cMHeader.IsNX301_DN ||
						cMHeader.IsNX601 ||
						cMHeader.IsNX603)
					{
						AssertHasMessageError($"{messageType}: Local Address empty", targerInfo, errorMsg);

						importerLocalAddress.Address1 = "Test Address";
						importerLocalAddress.Validation.ValidateE2_Address1();
						AssertNoMessageError($"{messageType}: Local Address is Test Address", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageError($"{messageType}: not validate Local Address", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckCompanyName()
		{
			var errorMsg = "You have not entered an Importer Local Company Name.";
			var targerInfo = importerLocalAddress.E2_CompanyNameInfo;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerLocalAddress.E2_CompanyName = ZString.Empty;
					importerLocalAddress.Validation.ValidateE2_CompanyName();

					if (cMHeader.IsNX101 ||
						cMHeader.IsNX301 ||
						cMHeader.IsNX301_AX ||
						cMHeader.IsNX301_DN ||
						cMHeader.IsNX401 ||
						cMHeader.IsNX601 ||
						cMHeader.IsNX603)
					{
						AssertHasMessageError($"{messageType}: Local CompanyName empty", targerInfo, errorMsg);
						importerLocalAddress.E2_CompanyName = "TestCompany";
						importerLocalAddress.Validation.ValidateE2_CompanyName();
						AssertNoMessageError($"{messageType}: Local CompanyName is TestCompany", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageError($"{messageType}: not validate Local CompanyName", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var targetInfo = importerLocalAddress.E2_CompanyNameInfo;
			importerLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			importerLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = importerLocalAddress.E2_Address1Info;
			importerLocalAddress.Address2 = new string('A', 50);
			importerLocalAddress.AdditionalAddressInformation = new string('A', 50);
			importerLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			importerLocalAddress.Address2 = ZString.Empty;
			importerLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.E2_RN_NKCountryCode = "AU";
			importerLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(importerLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(importerLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = importerLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.E2_RN_NKCountryCode = "78";
			importerLocalAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			importerLocalAddress.E2_RN_NKCountryCode = "DE";
			importerLocalAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerLocalAddress = importerDocumentaryAddress.LocalAddress;
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader cMHeader;
		JobDocAddress importerLocalAddress;
	}
}
