using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class PortAuthorityFilter3rdPartyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCorrectValidation()
		{
			AssertType(typeof(PortAuthority3rdPartyValidation), Message.Validation);
		}

		public void TestSenderId()
		{
			Message.SenderId = "BOB";
			AssertNoNotifications(Message.SenderIdInfo);

			Message.SenderId = "";
			AssertHasError(Message.SenderIdInfo, "Please enter a Sender ID.");
		}

		public void TestRecipientId()
		{
			Message.RecipientId = "BOB";
			AssertNoNotifications(Message.RecipientIdInfo);

			Message.RecipientId = "";
			AssertHasError(Message.RecipientIdInfo, "Please enter a Recipient ID.");
		}

		public void TestEmailAddress()
		{
			Message.EmailAddress = "random crap";
			AssertHasError(Message.EmailAddressInfo, "Email Address is not valid .");

			Message.EmailAddress = "bob@fread.net";
			AssertNoNotifications(Message.EmailAddressInfo);

			Message.EmailAddress = "";
			AssertHasError(Message.EmailAddressInfo, "Please enter an Email Address.");
		}

		public void TestVersion()
		{
			Message.Version = ZString.Empty;
			AssertHasError(Message.VersionInfo, "Please enter a Version.");

			Message.Version = PortAuthorityVersionList.Codes.V11;
			AssertNoNotifications(Message.VersionInfo);

			Message.Version = PortAuthorityVersionList.Codes.V20;
			AssertNoNotifications(Message.VersionInfo);

			Message.Version = "O.o";
			AssertHasError(Message.VersionInfo, "Enter a valid Version.");
		}

		#region Implementation

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
					voyage.GenerateSailings();
				}

				return voyage;
			}
		}
		JobVoyage voyage;

		PortAuthority Message
		{
			get { return message ?? (message = new PortAuthority(Voyage) { DeliverTo3rdParty = true }); }
		}
		PortAuthority message;

		#endregion
	}
}
