using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eServices.Encryption.Server.Decryptor;

namespace CargoWise.eHub.Portal.Helpers
{
    public class ConfigXmlHelper
	{
		public static string UpdateRegistrationConfigXml(string xml, eHubRegistrationType regTypeInstance, string customValue1 = null, string customValue2 = null)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(customValue1))
			{
				var semantics = eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue1");
				result = UpdateConfigXmlField(customValue1, xml, semantics);
			}
			if (!string.IsNullOrEmpty(customValue2))
			{
				var semantics = eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue2");
				result = UpdateConfigXmlField(customValue2, result, semantics);
			}

			return result;
		}

		public static string UpdateConfigXmlField(string newValue, string regoXml, XPathCustomValue semantics)
		{

			if (string.IsNullOrEmpty(regoXml)) return null;
			var configXml = XDocument.Parse(regoXml as string);
			var ele = configXml.XPathSelectElement(semantics.XPath);
			ele?.SetValue(semantics.IsPasswordValue ?
				CargoWise.eServices.Encryption.Client.Encryptor.EhubClientEncryptor.Encrypt(newValue)
				: newValue);

			return configXml.ToString();
		}

		public static List<ConfigXmlCustomValueView> GetClientRegistrationCustomsValueList(eHubRegistrationType regTypeInstance, IQueryable<eHubClientRegistration> regos)
		{
			var customsValueList = new List<ConfigXmlCustomValueView>();
			var customValue1 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue1");
			var customValue2 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue2");
			foreach (var rego in regos)
			{
				AddConfigValuesToList(rego.CX_PK, rego.CX_ConfigXml, customValue1, customValue2, customsValueList);
			}
			return customsValueList;
		}

		public static List<ConfigXmlCustomValueView> GetClientSystemRegistrationCustomsValueList(eHubRegistrationType regTypeInstance, IQueryable<eHubClientSystemRegistration> regos)
		{
			var customsValueList = new List<ConfigXmlCustomValueView>();
			var customValue1 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue1");
			var customValue2 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue2");
			foreach (var rego in regos)
			{
				AddConfigValuesToList(rego.CD_PK, rego.CD_ConfigXml, customValue1, customValue2, customsValueList);
			}
			return customsValueList;
		}

		public static List<ConfigXmlCustomValueView> GetAsyncPollingRegistrationsCustomsValueList(eHubRegistrationType regTypeInstance, IQueryable<eHubAsyncPollingRegistration> regos)
		{
			var customsValueList = new List<ConfigXmlCustomValueView>();
			var customValue1 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue1");
			var customValue2 =
				eHubPortalSemanticsFactory.GetSemantics<XPathCustomValue>(regTypeInstance.RT_ID, "CustomValue2");
			foreach (var rego in regos)
			{
				AddConfigValuesToList(rego.PR_PK, rego.PR_XML, customValue1, customValue2, customsValueList);
			}
			return customsValueList;
		}

		public static string GetSingleCustomValueFromConfig(string regoXml, XPathCustomValue customValue)
		{
			var value = string.Empty;

			if (string.IsNullOrEmpty(regoXml) || customValue.XPath == null) return value;
			var xnav = new XPathDocument(new StringReader(regoXml)).CreateNavigator();
			value = xnav.SelectSingleNode(customValue.XPath)?.InnerXml;
			if (string.IsNullOrEmpty(value)) return value;
			if (!Setting.ShowHidden() && customValue.IsHidden) return value;
			try
			{
				value = customValue.IsPasswordValue ? EhubServerDecryptor.Decrypt(value) : value;
			}
			catch (Exception)
			{
				value = "Invalid";
			}
			return value;
		}

		private static void AddConfigValuesToList(Guid pk, string xml, XPathCustomValue customValue1, XPathCustomValue customValue2, List<ConfigXmlCustomValueView> customsValueList)
		{
			var configCustomsValues = new ConfigXmlCustomValueView()
			{
				PK = pk,
				CustomValue1 = GetSingleCustomValueFromConfig(xml, customValue1),
				CustomValue2 = GetSingleCustomValueFromConfig(xml, customValue2)
			};
			customsValueList.Add(configCustomsValues);
		}

		public static void UploadConfiguration(IeHubTransactionsContext context, string registrationpk, string regTypepk, Stream xml)
		{
			var isTest = Setting.ShowHidden();
			var result = string.Empty;
			if (!isTest) throw  new Exception("Not allowed to upload configuration");

			var registrationGuid = new Guid(registrationpk);
			var regTypeGuid = new Guid(regTypepk);
			if (registrationGuid == Guid.Empty || regTypeGuid == Guid.Empty)
				throw new Exception("Empty Guid");

			result = GetConfigFromFile(xml);

			var regType = context.eHubRegistrationTypes.FirstOrDefault(x => x.RT_PK == regTypeGuid);
			switch (regType.RT_RegistrantType)
			{
				case "Client":
					var clientRego = context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == registrationGuid);
					if (clientRego != null)
					{
						clientRego.CX_ConfigXml = result;
					}
					break;
				case "ClientSystem":
					var systemClientRego = context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == registrationGuid);
					if (systemClientRego != null)
					{
						systemClientRego.CX_ConfigXml = result;
					}
					break;
				case "AsyncPolling":
					var asyncRego = context.eHubAsyncPollingRegistrations.FirstOrDefault(r => r.PR_PK == registrationGuid);
					if (asyncRego != null)
					{
						asyncRego.PR_XML = result;
					}
					break;
			}
			context.SaveChanges();

		}

		public static string GetConfigFromFile(Stream xml)
		{
			string result = null;
			if (xml !=null)
			{
				using (var rdr = new StreamReader(xml))
				{
					result = rdr.ReadToEnd();
				}
			}
			return result;
		}
	}
}