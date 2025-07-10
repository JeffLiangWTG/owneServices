using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EIDOMessagingIdentityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePrincipalPK()
		{
			OrgHeader principal1 = BaseAgencyTest.NewPrincipal(Factory);
			OrgHeader principal2 = BaseAgencyTest.NewPrincipal(Factory);
			Factory.Save();
			Identity1.PrincipalPK = ZGuid.Invalid;
			AssertHasError(Identity1.PrincipalPKInfo, "Enter a valid Principal.");
			Identity1.PrincipalPK = principal1.PK;
			AssertNoErrors(Identity1.PrincipalPKInfo);
			Identity1.PrincipalPK = ZGuid.Empty;
			AssertHasError(Identity1.PrincipalPKInfo, "Please enter a Principal.");
			Identity2.PrincipalPK = principal2.PK;
			Identity1.PrincipalPK = principal2.PK;
			AssertHasError(Identity1.PrincipalPKInfo, "This principal already exists in this list.");
			Identity1.PrincipalPK = principal1.PK;
			AssertNoErrors(Identity1.PrincipalPKInfo);
		}

		public void TestValidateSenderID()
		{
			Identity1.Validation.ValidateSenderID();
			AssertHasError(Identity1.SenderIDInfo, "Please enter a Sender ID.");
			Identity1.SenderID = "Blaticus";
			AssertNoErrors(Identity1.SenderIDInfo);
		}

		public void TestValidateRecipientID()
		{
			Identity1.Validation.ValidateRecipientID();
			AssertHasError(Identity1.RecipientIDInfo, "Please enter a Recipient ID.");
			Identity1.RecipientID = "Blaticus";
			AssertNoErrors(Identity1.RecipientIDInfo);
		}

		public void TestValidatePassword()
		{
			Identity1.Validation.ValidatePassword();
			AssertHasError(Identity1.PasswordInfo, "Please enter a Password.");
			Identity1.Password = "Blaticus";
			AssertNoErrors(Identity1.PasswordInfo);
		}

		#region Implementation
		EIDOMessagingHeader Messaging
		{
			get
			{
				return messaging ?? (messaging = new EIDOMessagingHeader());
			}
		}

		EIDOMessagingHeader messaging;
		EIDOMessagingIdentity Identity1
		{
			get
			{
				return identity1 ?? (identity1 = Messaging.Identities.AddNew());
			}
		}

		EIDOMessagingIdentity identity1;
		EIDOMessagingIdentity Identity2
		{
			get
			{
				return identity2 ?? (identity2 = Messaging.Identities.AddNew());
			}
		}

		EIDOMessagingIdentity identity2;
		#endregion
	}
}
