using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.AES;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SEDIdentificationAndNumberDeciderTest : DeclarationTestHelper
	{
		public void TestIdentificationTypeAndIdentificationNumber()
		{
			var org = CreateOrganisation("Dummy", AUSYD.Code);
			var decider = new SEDIdentificationAndNumberDecider(null, null, null);
			AssertEquals("IdentificationType", "", decider.IdentificationType);
			AssertEquals("IdentificationNumber", "", decider.IdentificationNumber);

			decider = new SEDIdentificationAndNumberDecider(org, null, null);
			AssertEquals("IdentificationType", "", decider.IdentificationType);
			AssertEquals("IdentificationNumber", "", decider.IdentificationNumber);

			UpdateOrAddCustomsRegNo(org, "123 123 12312", OrgCusCode.CodeTypes.CustomsClientCode, GlbCompany.CurrentCompany.Country);
			decider = new SEDIdentificationAndNumberDecider(org, null, null);
			AssertEquals("IdentificationType", "", decider.IdentificationType);
			AssertEquals("IdentificationNumber", "", decider.IdentificationNumber);

			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "2342--SDF#00");
			decider = new SEDIdentificationAndNumberDecider(org, org.MainAddress, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.DUNS, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "2342SDF00", decider.IdentificationNumber);

			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "2342--SDF#01");
			decider = new SEDIdentificationAndNumberDecider(org, org.MainAddress, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.Foreign, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "2342SDF01", decider.IdentificationNumber);

			UpdateOrAddCustomsRegNo(org, "323 234-2342", OrgCusCode.USACodeTypes.ForeignRegistrationNumber, GlbCompany.CurrentCompany.Country);
			decider = new SEDIdentificationAndNumberDecider(org, org.MainAddress, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.Foreign, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "3232342342", decider.IdentificationNumber);

			UpdateOrAddCustomsRegNo(org, "123-12-1234", OrgCusCode.USACodeTypes.SocialSecurityNumber, GlbCompany.CurrentCompany.Country);
			decider = new SEDIdentificationAndNumberDecider(org, null, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.SocialSecurityNumber, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "123121234", decider.IdentificationNumber);

			UpdateOrAddCustomsRegNo(org, "451234487", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, GlbCompany.CurrentCompany.Country);
			decider = new SEDIdentificationAndNumberDecider(org, org.MainAddress, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.EmployerIdentificationNumber, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "451234487", decider.IdentificationNumber);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var refUS = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates));
			UpdateOrAddCustomsRegNo(org, "451234487", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, refUS);
			decider = new SEDIdentificationAndNumberDecider(org, org.MainAddress, null);
			AssertEquals("IdentificationType", AESConstants.IDTypes.EmployerIdentificationNumber, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "451234487", decider.IdentificationNumber);

			org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789", Core.Constants.CountryCodes.UnitedStates);
			decider = new SEDIdentificationAndNumberDecider(org, address, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals(AESConstants.IDTypes.DUNS, decider.IdentificationType);
			AssertEquals("123456789", decider.IdentificationNumber);

			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123", Core.Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "965345", Core.Constants.CountryCodes.UnitedStates);
			decider = new SEDIdentificationAndNumberDecider(org, address, new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals(AESConstants.IDTypes.EmployerIdentificationNumber, decider.IdentificationType);
			AssertEquals("965345", decider.IdentificationNumber);

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			jobDocAddress.E2_PassportCountryOfIssue = "US";
			jobDocAddress.E2_PassportID = "123";
			var addressE2_GovRegNum = jobDocAddress.E2_GovRegNum;
			decider = new SEDIdentificationAndNumberDecider(jobDocAddress,null);
			AssertNotEquals("E2_GovRegNum", "123", addressE2_GovRegNum);
			AssertEquals("IdentificationType", AESConstants.IDTypes.Foreign, decider.IdentificationType);
			AssertEquals("IdentificationNumber", "123", decider.IdentificationNumber);
		}
	}
}
