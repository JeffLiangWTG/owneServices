using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Shared.Crypto;
using Common.Logging;
using CargoWise.eHub.Products.ITCustoms.Configuration;

namespace CargoWise.eHub.Products.ITCustoms.Helpers
{
	public static class CertificateHelper
	{
		public static eHubCertificate GetSenderCertificate(string senderId, string userId, string node)
		{
			using (var context = GetContext())
			{
				return context.eHubCertificates.SingleOrDefault(x => x.eHubClientSystem.EH_ID == senderId &&
																	 x.CE_ID == userId + "+" + node);
			}
		}

		public static byte[] CreateSignedEncryptedMessage(string message, string senderID, string userId, string node,
			ILog logger)
		{
			var certPass = "";
			var senderCert = UserLevelConfigurationHandler.GetCertificate(senderID, userId, node, out certPass);
			if (senderCert == null)
				throw new InvalidDataException(string.Format("Could not find certificate for sender: {0}, user: {1}, node: {2}", senderID, userId, node));

			var encryptedContent = CmsHelpers.ComputeSignature(Encoding.UTF8.GetBytes(message), senderCert,
				certPass, false);
			return encryptedContent;
		}

		public static string DecryptMessage(string message)
		{
			return Encoding.UTF8.GetString(CmsHelpers.ExtractDataFromSignature(Convert.FromBase64String(message)));
		}

		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
	}
}