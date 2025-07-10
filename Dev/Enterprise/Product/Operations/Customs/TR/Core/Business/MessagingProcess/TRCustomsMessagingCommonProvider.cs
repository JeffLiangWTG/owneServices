using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public sealed class TRCustomsMessagingCommonProvider
	{
		public TRCustomsMessagingCommonProvider(IReadOnlyCollection<ITRCustomsMessenger> trMessengers, TRMessageSigner signer = null)
		{
			TRMessengers = trMessengers;
			Signer = signer ?? TRMessageSigner.New();
			IsInTestMode = TRCustomsDataRegistry.Instance.IsTRTestingSystem;
		}
		public IReadOnlyCollection<ITRCustomsMessenger> TRMessengers { get; }
		public IReadOnlyCollection<ICustomsMessenger> GetMessengers() => TRMessengers.Select(x => x.Messenger).ToList();

		public TRMessageSigner Signer { get; }
		public bool IsInTestMode { get; }
		public bool EnableTestModeValidation => false;

		public GlbExternalPassword_TR TRBPassword => trBPassword ??= TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
		GlbExternalPassword_TR trBPassword;

		public List<MessageSendingNotification> RunPreSendValidation()
		{
			var results = new List<MessageSendingNotification>();

			if (TRMessengers.Any(x => x.IsMessageSigningRequired))
			{
				results.AddRange(ValidateUserProfile());
			}

			return results;
		}

		IEnumerable<MessageSendingNotification> ValidateUserProfile()
		{
			var currentUser = TRBPassword;
			if (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty)
			{
				yield return new MessageSendingError(Res.GetString("9132578A-A87D-4C57-A68F-2D5AB8DD4973", "Your staff profile requires an email address as this is needed for messaging."));
			}

			if (currentUser.GP_PasswordStatus != PasswordStatusList.Codes.PasswordOK)
			{
				yield return new MessageSendingError(Res.GetString("5DE75409-9DB4-4279-B0E4-BBD6263A0041", "Your customs credentials are marked as invalid, please update your customs credentials."));
			}

			if (!TRMessageSigner.IsValidCertificateSerialNumber(currentUser.GP_CertificateSerialNumber))
			{
				yield return new MessageSendingError(Res.GetString("FEED961D-B4BD-4530-AF6D-459EAE001715", "A valid certificate serial number is required on your staff record on the Brokerage tab"));
			}
		}
	}
}
