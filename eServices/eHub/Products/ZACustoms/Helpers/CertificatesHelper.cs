using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;

namespace CargoWise.eHub.Products.ZACustoms.Helpers
{
	public class CertificatesHelper
	{
		public static eHubCertificate GetZACustomsCertificate(string recipientID, string certID, ILog logger)
		{
			var zaCustomsCert = DatabaseAccessHelpers.AccessDatabaseWithRetries<eHubCertificate>(() =>
			{
				using (var context = ContextFactory())
				{
                    return SelectMostValidCertificate(context.eHubCertificates.Where(c => c.CE_Category == recipientID && c.eHubClient.CC_ID == recipientID), GetDateTimeUtcNow());
				}
			}, logger);

			if (zaCustomsCert == null)
				throw new InvalidOperationException("Unable to retrieve ZACustoms certificate.");

			return zaCustomsCert;
		}

		public static eHubCertificate GetSenderCertificate(string senderID, string recipientID, string as2From, bool retrying, ILog logger)
		{
			var senderCert = DatabaseAccessHelpers.AccessDatabaseWithRetries<eHubCertificate>(() =>
			{
				using (var context = ContextFactory())
				{
                    var certs = context.eHubCertificates.Where(c => c.CE_Category == recipientID && c.eHubClient.CC_ID == senderID && c.CE_ID == as2From && c.CE_BinaryContainer != null && c.CE_ValidFromUTC != null && c.CE_ValidToUTC != null);
					var currentCert = SelectMostValidCertificate(certs, GetDateTimeUtcNow());
					if (currentCert == null)
						throw new FatalMessageProcessingException("No certificate registered for South African Customs messaging.");

					if (retrying)
					{
						var nonCurrentCerts = certs.Where(c => c.CE_PK != currentCert.CE_PK);
						var retryCert = SelectMostValidCertificate(nonCurrentCerts, GetDateTimeUtcNow());
						if (retryCert == null)
							throw new FatalMessageProcessingException("No valid certificates registered for South African Customs messaging.");
						return retryCert;
					}
					else
						return currentCert;
				}
			}, logger);

			return senderCert;
		}

		public static int GetSenderCertificateCount(string senderID, string recipientID, string as2From, ILog logger)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<int>(() =>
			{
				using (var context = ContextFactory())
				{
                    return context.eHubCertificates.Count(c => c.CE_Category == recipientID && c.eHubClient.CC_ID == senderID && c.CE_ID == as2From);
				}
			}, logger);
		}

		public static eHubCertificate GetCertificateForMdnSigning(string senderID, string as2From, ILog logger)
		{
			using (var context = ContextFactory())
			{
                var certs = context.eHubCertificates.Include("eHubClient").Where(c => c.CE_Category == senderID && c.CE_ID == as2From && c.CE_Password != null);
				return SelectMostValidCertificate(certs, GetDateTimeUtcNow());
			}
		}

		internal static eHubCertificate SelectMostValidCertificate(IEnumerable<eHubCertificate> certs, DateTime effectiveUTC)
		{
			var orderedCerts = certs.OrderBy(c => c.CE_ActiveFromUTC ?? c.CE_ValidFromUTC ?? DateTime.MinValue)
									.ThenBy(c => c.CE_AddedUTC);

			return
				/* 
				 * Latest valid current cert
				 *  (Active From is set and <= now && Valid To is null or Valid To > now)
				 *  Or (Active From is not set and Valid From/To are set and Valid From <= now and Valid To > now)
				 */
				orderedCerts.LastOrDefault(c =>
					(c.CE_ActiveFromUTC <= effectiveUTC && (!c.CE_ValidToUTC.HasValue || c.CE_ValidToUTC > effectiveUTC))
					|| (!c.CE_ActiveFromUTC.HasValue && c.CE_ValidFromUTC <= effectiveUTC && c.CE_ValidToUTC > effectiveUTC))
				/* 
				 * Or first future valid cert
				 *  (Active From is set and >= now) Or (Valid From is set and >= now)
				 */
				?? orderedCerts.FirstOrDefault(c => (c.CE_ActiveFromUTC ?? c.CE_ValidFromUTC) >= effectiveUTC)
				/* 
				 * Or last expired cert
				 *  (Valid To is set and < now)
				 */
				?? orderedCerts.LastOrDefault(c => c.CE_ValidToUTC < effectiveUTC)
				/* 
				 * Or most recent
				 */
				?? orderedCerts.LastOrDefault();
		}

		internal static Func<DateTime> GetDateTimeUtcNow = () => DateTime.UtcNow;
		internal static Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
	}
}
