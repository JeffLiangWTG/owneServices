using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Shared.Mime;

namespace CargoWise.eHub.Products.ACAS.BR.Helpers
{
	public class AuthenticationHelper
	{
		public const string BRCustomsConfigurationTokenName = "ACAS_BRToken";

		public static XmlDocument UpdateAuthenticationToken(string clientSystemId, string staffCode, string headers)
		{
			if (string.IsNullOrWhiteSpace(clientSystemId))
				throw new ArgumentNullException("clientSystemId");
			if (string.IsNullOrWhiteSpace(staffCode))
				throw new ArgumentNullException("staffCode");
			if (string.IsNullOrEmpty(headers))
				throw new InvalidOperationException(String.Format(@"Http Headers configuration must be provided. Criteria was:
Client System: {0}; Staff Code: {1}", clientSystemId, staffCode));

			XNamespace nsEHubClientSystemRegistrations = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration";
			XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";

			var (xmlHeaders, expiryUnixDate) = SerializeHttpHeadersToXML(headers);
			if (string.IsNullOrWhiteSpace(expiryUnixDate))
			{
				throw new InvalidOperationException(String.Format(@"Http Headers configuration invalid. Provided headers for
Client System: {0}; Staff Code: {1}, was: {2}", clientSystemId, staffCode, headers));
			}

			var reg = GetClientSystemRegistration(clientSystemId, staffCode);

			if (reg == null || reg.CD_PK == null)
			{
				throw new InvalidOperationException(String.Format(@"Client System Registration does not exists. Criteria was:
Client System: {0}; Staff Code: {1}; Headers: {2}", clientSystemId, staffCode, xmlHeaders.OuterXml));
			}

			var expiryUtcDate = UnixTimeStampToDateTime(expiryUnixDate);

			var flag = expiryUtcDate.AddMinutes(-5) > DateTime.UtcNow ? 1 : 0;

			var revisedHeaders = Regex.Replace(xmlHeaders.OuterXml,"Set-Token>", "Authorization>", RegexOptions.IgnoreCase);

			var wcfSqlUpdateClientSystemRegistrations =
			new XElement(nsEHubClientSystemRegistrations + "Update",
				new XElement(nsEHubClientSystemRegistrations + "Rows",
					new XElement(nsEHubClientSystemRegistrations + "RowPair",
						new XElement(nsEHubClientSystemRegistrations + "After",
							new XElement(nsSqlTableOps + "CD_IssuedUTC", DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff")),
							new XElement(nsSqlTableOps + "CD_ExpiryUTC", expiryUtcDate),
							new XElement(nsSqlTableOps + "CD_ConfigXml", revisedHeaders),
							new XElement(nsSqlTableOps + "CD_Flag1", flag)
						),
						new XElement(nsEHubClientSystemRegistrations + "Before",
							new XElement(nsSqlTableOps + "CD_PK", reg.CD_PK)
						)
					)
				)
			);
			return WcfSqlOperationMessages.CombineWcfSqlOps(wcfSqlUpdateClientSystemRegistrations);

		}

		private static (XmlDocument, string) SerializeHttpHeadersToXML(string headers)
		{
			var headerDictionary = MimeUtils.ParseHeaders(headers);

			string expiryUnixDate = null;
			var xmlConfig = new XmlDocument();
			var xmlString = "<Config>";
			foreach (var header in headerDictionary)
			{
				var xmlTag = header.Key;
				if (header.Key.Equals("X-CSRF-Expiration", StringComparison.OrdinalIgnoreCase))
				{
					expiryUnixDate = header.Value;
					xmlTag = "X-CSRF-Expiration";
				}
				else if (header.Key.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase))
				{
					xmlTag = "X-CSRF-Token";
				}

				xmlString += String.Format("<{0}>{1}</{0}>", xmlTag, header.Value);
			}
			xmlString += "</Config>";
			xmlConfig.LoadXml(xmlString);
			return (xmlConfig, expiryUnixDate);
		}

		private static eHubClientSystemRegistration GetClientSystemRegistration(string clientSystemId, string staffCode)
		{
			var reg = new eHubClientSystemRegistration();
			using (var context = GetContext())
			{
				var regType = context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_ID == BRCustomsConfigurationTokenName);
				var clientSystem = context.eHubClientSystems.FirstOrDefault(x => x.EH_ID == clientSystemId);
				reg = context.eHubClientSystemRegistrations.FirstOrDefault(r =>
					r.eHubRegistrationType.RT_PK == regType.RT_PK
					&& r.CD_Code == staffCode
					&& r.eHubClientSystem.EH_ID == clientSystemId);
			}
			return reg;
		}

		public static Hashtable GetValidClientSystemRegistrationConfigXml(string clientSystemId, string staffCode)
		{
			try
			{
				var registration = GetClientSystemRegistration(clientSystemId, staffCode);
				if (registration == null || !registration.CD_Flag1.HasValue || registration.CD_Flag1.Value == 0 || !registration.CD_ExpiryUTC.HasValue || registration.CD_ExpiryUTC.Value.AddMinutes(-5) < DateTime.UtcNow)
				{
					return null;
				}

				var authentication = new XmlDocument();
				authentication.LoadXml(registration.CD_ConfigXml);

				var tokens = new Hashtable();

				tokens.Add("XCSRFToken", authentication.SelectSingleNode("//Config/X-CSRF-Token").InnerText);

				tokens.Add("Authorization", authentication.SelectSingleNode("//Config/Authorization").InnerText);

				return tokens;


			}
			catch
			{
				return null;
			}
		}

		public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(Int64.Parse(unixTimeStamp));
		}

		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();

	}
}
