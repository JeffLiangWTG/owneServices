using System.Collections.Generic;
using CargoWise.eServices.Encryption.Client.Encryptor;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Customs
{
	public static class XTMsgAttrProviderExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xT System Const")]
		public static Dictionary<string, string> GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificateThumbprint(this IxTMessageAttributeProvider obj)
		{
			if (obj is GlbExternalPassword instance)
			{
				return new Dictionary<string, string>
				{
					{ xTMessaging.Shared.Constants.xTMsgAttributes.anycertificate, "xt-certificate:" + instance.GP_UserID }
				};
			}
			return new Dictionary<string, string>();
		}

		public static Dictionary<string, string> GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(this IxTMessageAttributeProvider obj, bool encrypt)
		{
			if (obj is GlbExternalPassword instance)
			{
				return new Dictionary<string, string>
				{
					{ xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientUser, instance.GP_UserID },
					{ xTMessaging.Shared.Constants.xTMsgAttributes.HttpClientPassword, encrypt ? EhubClientEncryptor.Encrypt(instance.CurrentDecryptedPassword) : instance.GP_CurrentPassword }
				};
			}
			return new Dictionary<string, string>();
		}

		public static Dictionary<string, string> GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM(this IxTMessageAttributeProvider obj)
		{
			if (obj is GlbExternalPassword instance)
			{
				var (keyPEM, certPEM) = instance.ExportRSAKeyAndCertificateStringAsPEM();
				return new Dictionary<string, string>
				{
					{ xTMessaging.Shared.Constants.xTMsgAttributes.cw1key, keyPEM },
					{ xTMessaging.Shared.Constants.xTMsgAttributes.cw1certificate, certPEM }
				};
			}
			return new Dictionary<string, string>();
		}
	}
}

