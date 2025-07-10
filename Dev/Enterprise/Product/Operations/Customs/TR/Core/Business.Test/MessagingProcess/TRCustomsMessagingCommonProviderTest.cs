using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.TR.Business.MessagingProcess.Testing
{
	public class TRCustomsMessagingCommonProviderTest : TestCaseWithFactory
	{
		public static void SetupUser(TRCustomsMessagingCommonProvider provider, string email = "gone@fishing.com", string cert = "02b9572b9cad7250a906b3", string passwordStatus = "OK")
		{
			var trPassword = provider.TRBPassword;

			GlbStaff.CurrentUser.GS_EmailAddress = email;
			trPassword.GP_CertificateSerialNumber = cert;
			trPassword.GP_PasswordStatus = passwordStatus;
		}

		TRCustomsMessagingCommonProvider CreateTRCustomsMessagingCommonProvider(bool requireSigning)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child = Factory.New<DummyBizObjWithMessages>();
			var mockMessageGenerator = new Mock<ITRCustomsMessageGenerator>();
			mockMessageGenerator.Setup(m => m.IsMessageSigningRequired).Returns(requireSigning);
			var messenger = new TRCustomsMessenger(child, mockMessageGenerator.Object);
			var provider = new TRCustomsMessagingCommonProvider(new[] { messenger });

			SetupUser(provider);

			return provider;
		}

		public void TestPreSendValidation_UserProfile()
		{
			var trProvider = CreateTRCustomsMessagingCommonProvider(true);

			CombineAssertions(() =>
			{
				SetupUser(trProvider);
				var result = trProvider.RunPreSendValidation();
				AssertEquals("No validation wanrings/errors", 0, result.Count);

				SetupUser(trProvider, email: "");
				result = trProvider.RunPreSendValidation();
				AssertEquals("1 validation wanrings/errors", 1, result.Count);
				var notification = result.First();
				AssertEquals("Missing email - Is Error", true, notification.IsError);
				AssertEquals("Missing email - Msg", "Your staff profile requires an email address as this is needed for messaging.", notification.Message);

				SetupUser(trProvider, passwordStatus: "INV");
				result = trProvider.RunPreSendValidation();
				AssertEquals("1 validation wanrings/errors", 1, result.Count);
				notification = result.First();
				AssertEquals("Invalid Credentials - Is Error", true, notification.IsError);
				AssertEquals("Invalid Credentials - Msg", "Your customs credentials are marked as invalid, please update your customs credentials.", notification.Message);

				SetupUser(trProvider, cert: "");
				result = trProvider.RunPreSendValidation();
				AssertEquals("1 validation wanrings/errors", 1, result.Count);
				notification = result.First();
				AssertEquals("Invalid certificate - Is Error", true, notification.IsError);
				AssertEquals("Invalid certificate - Msg", "A valid certificate serial number is required on your staff record on the Brokerage tab", notification.Message);

				SetupUser(trProvider, "", "", "INV");
				result = trProvider.RunPreSendValidation();
				AssertEquals("3 validation wanrings/errors", 3, result.Count);
			});
		}

		public void TestRunPreSendValidationNoSigningRequired()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var intCust = new TRCustomsMessagingCommonProvider(Array.Empty<TRCustomsMessenger>());

			var result = intCust.RunPreSendValidation().ToArray();
			AssertEquals("No notifications expected", 0, result.Length);
		}

		public void TestTRBPassword()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TSN";
			staff.GS_FullName = "TR Testing User";
			var password = TRGlbStaffWrapper.Get(staff).TRBPassword;
			password.GP_UserID = "1234";
			password.CurrentDecryptedPassword = "xxx";
			password.GP_CertificateAuthority = "TÜBİTAK";
			password.TR_Chipset = "ABC";
			password.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var dummy = Factory.New<DummyBusinessObject>();
				var intCust = new TRCustomsMessagingCommonProvider(Array.Empty<TRCustomsMessenger>());

				AssertSame(password, intCust.TRBPassword);
			}
		}

		public void TestIsInTestMode()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var provider = new TRCustomsMessagingCommonProvider(Array.Empty<TRCustomsMessenger>());
				AssertEquals("Should be in test mode", true, provider.IsInTestMode);
				AssertEquals("Test mode validation", false, provider.EnableTestModeValidation);
			}

			using (TRCustomsDataRegistry.Instance.TRTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var provider = new TRCustomsMessagingCommonProvider(Array.Empty<TRCustomsMessenger>());
				AssertEquals("Should not be in test mode", false, provider.IsInTestMode);
			}
		}
	}
}
