using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchCredentialsForIndia))]
	public class GlbBranchCredentialsForIndiaTest : UserAndClientCredentialsTest<GlbBranchExternalPasswordINT, GlbBranchExternalPasswordINS>
	{
		public void TestNewGlbBranchCredentialsForIndia_WhenGlbBranchIsNull()
		{
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: branch", () => GlbBranchCredentialsForIndia.New(null));
#else
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'branch')", () => GlbBranchCredentialsForIndia.New(null));
#endif
		}

		public override void TestErrorStatusAndReason()
		{
			var branch = CreateIndiaBranch();
			branch.WithExternalPassword<GlbBranchExternalPasswordINT>(userId: "taxpayer.user", passwordStatus: PasswordStatusList.Codes.Invalid, statusReason: "error details for INT");
			branch.WithExternalPassword<GlbBranchExternalPasswordINS>(userId: "service.provider.id", passwordStatus: PasswordStatusList.Codes.Invalid, statusReason: "error details for INS");
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBranch = newFactory.Load<GlbBranch>(branch.PK);
			AssertEquals("Taxpayer password status should be error", "Error", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatus);
			AssertEquals("Service provider password status should be error", "Error", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatus);
			AssertEquals("Taxpayer password status reason should be same as database field", "error details for INT", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatusReason);
			AssertEquals("Service provider password status should be same as database field", "error details for INS", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatusReason);

			loadedBranch.BranchCredentialsIndia.Password = "newpassword";
			loadedBranch.BranchCredentialsIndia.PasswordConfirmation = "newpassword";
			loadedBranch.BranchCredentialsIndia.ClientSecret = "newsecret";

			AssertEquals("Taxpayer password status should be cleared", ZString.Empty, loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatus);
			AssertEquals("Service provider password status should be cleared", ZString.Empty, loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatus);
			AssertEquals("Taxpayer password status reason should remain the same", "error details for INT", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatusReason);
			AssertEquals("Service provider password status should remain the same", "error details for INS", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatusReason);

			newFactory.Save();

			AssertEquals("Taxpayer password status should be saved", "Saved", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatus);
			AssertEquals("Service provider password status should be saved", "Saved", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatus);
			AssertEquals("Taxpayer password status reason should be cleared after save", ZString.Empty, loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatusReason);
			AssertEquals("Service provider password status should be cleared after save", ZString.Empty, loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatusReason);
		}

		public void TestValidationIsAppliedOnLoad()
		{
			var branch = CreateIndiaBranch();
			AssertNoWarning("Taxpayer status reason should have no warning when created", branch.BranchCredentialsIndia.UserCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Password");
			AssertNoWarning("Service provider status reason should have no warning when created", branch.BranchCredentialsIndia.ClientCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Client Secret");

			var branch2 = CreateIndiaBranch(prefix: "I2");
			branch2.WithExternalPassword<GlbBranchExternalPasswordINT>(userId: "taxpayer.user", passwordStatus: PasswordStatusList.Codes.Invalid, statusReason: "error details for INT");
			branch2.WithExternalPassword<GlbBranchExternalPasswordINS>(userId: "service.provider.id", passwordStatus: PasswordStatusList.Codes.Invalid, statusReason: "error details for INS");
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBranch = newFactory.Load<GlbBranch>(branch2.PK);
			AssertHasWarning("Taxpayer status reason should have warning when loaded with error reason", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Password");
			AssertHasWarning("Service provider status reason should have warning when loaded with error reason", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatusReasonInfo, "Please correct the error and re-enter your Client Secret");
		}

		public override void TestClearingUserIdUpdatesStatus()
		{
			var branch = CreateIndiaBranch();
			branch.WithExternalPassword<GlbBranchExternalPasswordINT>(userId: "taxpayer.user", passwordStatus: PasswordStatusList.Codes.PasswordOK);
			branch.WithExternalPassword<GlbBranchExternalPasswordINS>(userId: "service.provider.id", passwordStatus: PasswordStatusList.Codes.PasswordOK);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBranch = newFactory.Load<GlbBranch>(branch.PK);
			AssertEquals("UserCredential password status should be saved", "Saved", loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatus);
			AssertEquals("ClientCredential password status should be saved", "Saved", loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatus);

			loadedBranch.BranchCredentialsIndia.Username = ZString.Empty;
			loadedBranch.BranchCredentialsIndia.ClientId = ZString.Empty;
			newFactory.Save();

			AssertEquals("UserCredential password status should be empty when username is cleared", ZString.Empty, loadedBranch.BranchCredentialsIndia.UserCredentialPasswordStatus);
			AssertEquals("ClientCredential password status should be empty when username is cleared", ZString.Empty, loadedBranch.BranchCredentialsIndia.ClientCredentialPasswordStatus);
		}

		public void TestEHubMessage_IsNotSentOnSave()
		{
			var branch = CreateIndiaBranch();
			branch.BranchCredentialsIndia.ClientId = "branch client id";
			branch.BranchCredentialsIndia.ClientSecret = "branch client secret";
			branch.BranchCredentialsIndia.Username = "taxpayer username";
			branch.BranchCredentialsIndia.Password = "taxpayer password";
			branch.BranchCredentialsIndia.PasswordConfirmation = "taxpayer password";

			AssertNull("Precondition: no credential message queued", Factory.GetLatestEHubConfigurationInterchange());

			Factory.Save();
			var interchangeMessages = Factory.GetQueuedEHubConfigurationInterchangeMessages();
			AssertNull("No credential message queued", Factory.GetLatestEHubConfigurationInterchange());
		}

		#region Implementation

		protected override UserAndClientCredentials CreateUserAndClientCredentials()
		{
			var branch = CreateIndiaBranch(Factory);
			branch.WithExternalPassword<GlbBranchExternalPasswordINT>();
			branch.WithExternalPassword<GlbBranchExternalPasswordINS>();
			return GlbBranchCredentialsForIndia.New(branch);
		}

		protected override BusinessObject GetNewBusinessObject()
			=> GlbBranchCredentialsForIndia.New(CreateIndiaBranch());

		GlbBranch CreateIndiaBranch(BusinessObjectFactory factory = null, string prefix = "IN")
			=> (factory ?? Factory).NewCompanyAndBranchWith(companyCode: prefix + "C", branchCode: prefix + "B", countryCode: Core.Constants.CountryCodes.India);

		#endregion
	}
}
