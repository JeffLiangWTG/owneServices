using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingPasswordCredential))]
	public class EInvoicingPasswordCredentialTest : GlbExternalPasswordTest<EInvoicingPasswordCredential>
	{
		public void TestCanDelete()
		{
			var passwordCredential = GetNewBusinessObjectForDeleteTest(Factory);

			AssertEquals(false, passwordCredential.CanDelete);
		}

		public void TestPasswordStatus()
		{
			var passwordCredential = Factory.New<EInvoicingPasswordCredential>();
			passwordCredential.GP_PasswordStatus = ZString.Empty;
			AssertEquals(ZString.Empty, passwordCredential.PasswordStatus);

			passwordCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			AssertEquals("Should match description from code", PasswordStatusList.Descriptions.Valid, passwordCredential.PasswordStatus);

			passwordCredential.GP_PasswordStatus = "SUS";
			AssertEquals("If code is unknown, display as it is", "SUS", passwordCredential.PasswordStatus);
		}

		public void TestPropertiesAreSetByDefinition()
		{
			var passwordLabelMock = new Mock<IMultilingualString>();
			passwordLabelMock.Setup(x => x.ToString()).Returns("Password Label");
			var usernameMock = new Mock<IMultilingualString>();
			usernameMock.Setup(x => x.ToString()).Returns("Username Label");

			var definitionMock = new Mock<IEInvoicingPasswordCredentialDefinition>();
			definitionMock.Setup(x => x.DisplayOrder).Returns(2);
			definitionMock.Setup(x => x.PasswordLabel).Returns(passwordLabelMock.Object);
			definitionMock.Setup(x => x.UsernameLabel).Returns(usernameMock.Object);

			var passwordCredential = Factory.New<EInvoicingPasswordCredential>();
			passwordCredential.CredentialDefinition = definitionMock.Object;

			AssertEquals(2, passwordCredential.DisplayOrder);
			AssertEquals("Password Label", passwordCredential.PasswordLabel);
			AssertEquals("Username Label", passwordCredential.UsernameLabel);
		}

		public void TestSetDefaultValues()
		{
			var passwordCredential = Factory.New<EInvoicingPasswordCredential>();

			AssertEquals(PasswordTypesList.Codes.EIM, passwordCredential.GP_PasswordType);
			AssertEquals(PasswordStatusList.Codes.Invalid, passwordCredential.GP_PasswordStatus);
			AssertEquals(ZGuid.Empty, passwordCredential.GP_GS);
		}

		public void TestSettingCredentialDefinition()
		{
			var passwordCredential = Factory.New<EInvoicingPasswordCredential>();

			AssertNull(passwordCredential.CredentialDefinition);

			var definition = new Mock<IEInvoicingPasswordCredentialDefinition>().Object;
			passwordCredential.CredentialDefinition = definition;

			AssertExceptionThrown<InvalidOperationException>("can only set settings once", () => passwordCredential.CredentialDefinition = definition);
		}

		public void TestSettingCredentialSettings()
		{
			var passwordCredential = Factory.New<EInvoicingPasswordCredential>();

			AssertNull(passwordCredential.CredentialSettings);

			var settings = EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingPasswordCredentialSettings>().Object;
			passwordCredential.CredentialSettings = settings;

			AssertExceptionThrown<InvalidOperationException>("can only set settings once", () => passwordCredential.CredentialSettings = settings);
		}

		#region Implementation

		protected override EInvoicingPasswordCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var credential = Factory.New<EInvoicingPasswordCredential>();
			credential.GP_GB = GlbBranch.CurrentBranch.PK;
			return credential;
		}

		#endregion
	}
}
