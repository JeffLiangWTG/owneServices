using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusCodeValidity))]
	public class OrgCusCodeValidityTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestVerificationStatus()
		{
			var validity = Factory.New<OrgCusCodeValidity>();
			AssertEquals("Validity Not Supported", ZString.Empty, validity.VerificationStatus);

			validity.VerificationStatus = OrgConstants.CusCodeValidityVerification.Verified;
			AssertEquals("Validity Not Supported", ZString.Empty, validity.VerificationStatus);

			validity.VerificationStatus = OrgConstants.CusCodeValidityVerification.NotVerified;
			AssertEquals("Validity Not Supported", ZString.Empty, validity.VerificationStatus);
		}

		public virtual void TestLastVerifiedTime()
		{
			var validity = Factory.New<OrgCusCodeValidity>();
			AssertEquals("Validity Not Supported", ZDateTime.Empty, validity.LastVerifiedTime);

			validity.LastVerifiedTime = ZDateTime.Today;
			AssertEquals("Validity Not Supported", ZDateTime.Empty, validity.LastVerifiedTime);
		}

		public virtual void TestVerificationAuthority()
		{
			var validity = Factory.New<OrgCusCodeValidity>();
			AssertEquals("Validity Not Supported", ZString.Empty, validity.VerificationAuthority);

			validity.VerificationAuthority = "ABC";
			AssertEquals("Validity Not Supported", ZString.Empty, validity.VerificationAuthority);
		}

		public virtual void TestVerificationAuthorityFieldType()
		{
			var validity = Factory.New<OrgCusCodeValidity>();
			AssertEquals("Validity Not Supported", nameof(FieldType.Text), validity.VerificationAuthorityFieldType);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "213";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "I have events";
			org.OH_RL_NKClosestPort = "AU";
			org.CustomsCodes.Add(orgCusCode);

			var validity = Factory.New<OrgCusCodeValidity>();
			validity.OCV_OK_OrgCusCode = orgCusCode.PK;
			validity.OCV_Verified = true;
			Factory.Save();

			return Factory.Load<OrgCusCodeValidity>(validity.PK);
		}
	}
}
