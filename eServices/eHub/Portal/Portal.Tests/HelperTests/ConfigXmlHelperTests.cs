using CargoWise.eHub.Portal.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.DataModel.Business.Semantics;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Tests.HelperTests
{
	[TestClass]
	public class ConfigXmlHelperTests
	{
		[TestMethod]
		public void TestRegistrationsGetAndUpdateCustomsValueFromConfigXml()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			//var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cx1 = new eHubClientRegistration
			{
				CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"),
				//eHubClient = cc1,
				CX_RT = rt.RT_PK,
				CX_Code = "REG001",
				CX_ConfigXml =
"<Configuration xmlns='http://www.wisetechglobal.com/Schemas/Configuration' Name='TWCustomsSubscribers' Version='1.0'>" +
"<Group Type='System' Reference='HYETST'> " +
"	<Group Type='Company' Reference='TST'> " +
"	  <Group Type='Staff' Reference='BRK'> " +
"		<Group Type='MailBoxID' Reference='TSBKN00282-C' Status='VAL'> " +
"		  <Item Name='Platform'>TVA</Item> " +
"		  <Item Name='ReceiveAutomatically'>1</Item> " +
"		  <Credential> " +
"			<UserName>UVCBSBTWTXG00282-SWT</UserName> " +
"			<Password>EoMP8+NDTq2hkDUOxlvH4SUPJm6VZlaJVMzii/vZXNDRUpw/IUkGd+Q2ZSVNaibCfR1NA8aBTerh9bseMq+1hOZYbnF8ak/ZYrx5WCteMeXaXchd5Ly+xg5PHSGk8eYeK4rWjp5kvUBnI3x3Bn2f/qUt7KpvApry6fBHcjXR1Us=</Password> " +
"		  </Credential>" +
"          <Certificate Name='Certificate'>" +
"            <File>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</File>" +
"            <Passphrase>qVNU5HvScr4wY+zMTKVoWNezY1M/oe3J3gNBTbYoSRm+r4V5FZMiR4N6ZNqALaxxMQSzDQWbb71QCUrdamBxU5bgnOqtsHqZ0LdFbQREqfQzu13/cvqazgPNSBNqkHGS3rIsjohuGftHkR59wnDhzjcF42A1q3ZDuvNKPp4nLWQ=</Passphrase>" +
"          </Certificate>" +
"        </Group>" +
"      </Group>" +
"    </Group>" +
"  </Group>" +
"</Configuration>"
			};
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientRegistrations.AddObject(cx1);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			XNamespace nameSpace = "http://www.wisetechglobal.com/Schemas/Configuration";
			namespaceManager.AddNamespace("ns0", nameSpace.ToString());

			var regTypeInstance = context.eHubRegistrationTypes.First(r => r.RT_PK == rt.RT_PK);
			var rego = context.eHubClientRegistrations.Where(r => r.CX_RT == regTypeInstance.RT_PK).FirstOrDefault();
			var passphraseSematics = new XPathCustomValue
			{
				XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Certificate']/*[local-name()='Passphrase']",
				IsPasswordValue = true,
				IsHidden = true
			};

			var passswordSematics = new XPathCustomValue
			{
				XPath = "//*[local-name()='Group'][@Type='MailBoxID']/*[local-name()='Credential']/*[local-name()='Password']",
				IsPasswordValue = true,
				IsHidden = true
			};
			var passphraseResult = ConfigXmlHelper.GetSingleCustomValueFromConfig(rego.CX_ConfigXml, passphraseSematics);
			var passswordResult = ConfigXmlHelper.GetSingleCustomValueFromConfig(rego.CX_ConfigXml, passswordSematics);

			Assert.AreEqual("111111", passphraseResult, "Passphrase");
			Assert.AreEqual("TEST", passswordResult, "Password");

			var configXml = ConfigXmlHelper.UpdateConfigXmlField("newPassphraseValue", rego.CX_ConfigXml, passphraseSematics);
			passphraseResult = ConfigXmlHelper.GetSingleCustomValueFromConfig(configXml, passphraseSematics);
			Assert.AreEqual("newPassphraseValue", passphraseResult, "Passphrase");

			configXml = ConfigXmlHelper.UpdateConfigXmlField("newPasswordValue", rego.CX_ConfigXml, passswordSematics);
			passswordResult = ConfigXmlHelper.GetSingleCustomValueFromConfig(configXml, passswordSematics);
			Assert.AreEqual("newPasswordValue", passswordResult, "Password");

		}

		[TestMethod]
		public void TestUploadConfigFile()
		{

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1", RT_RegistrantType = "Client" };
			var cx = new eHubClientRegistration
			{
				CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"),
				CX_RT = rt.RT_PK,
				CX_Code = "REG001",
				CX_ConfigXml = "<OldTESTConfiguration></OldTESTConfiguration>"
			};

			var eh = new eHubClientSystem() { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLS001" };
			var cd = new eHubClientSystemRegistration
			{
				CD_PK = new Guid("{00000000-FFFF-2222-2222-000000000000}"),
				eHubClientSystem = eh,
				CD_RT = rt.RT_PK,
				CD_ConfigXml = "<OldTESTConfiguration></OldTESTConfiguration>"
			};

			var pr = new eHubAsyncPollingRegistration
			{
				PR_PK = new Guid("{00000000-FFFF-3333-3333-000000000000}"),
				eHubClientSystem = eh,
				PR_RT = rt.RT_PK, 
				PR_XML = "<OldTESTConfiguration></OldTESTConfiguration>"
			};
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientRegistrations.AddObject(cx);
			context.eHubClientSystemRegistrations.AddObject(cd);
			context.eHubAsyncPollingRegistrations.AddObject(pr);

			var uploadFile = new MemoryStream(Encoding.UTF8.GetBytes("<NewTESTConfiguration></NewTESTConfiguration>"));

			ConfigXmlHelper.UploadConfiguration(context, cx.CX_PK.ToString(), rt.RT_PK.ToString(), uploadFile);
			var result = context.eHubClientRegistrations.FirstOrDefault(x => x.CX_PK == cx.CX_PK).CX_ConfigXml;

			Assert.AreEqual("<NewTESTConfiguration></NewTESTConfiguration>", result);
		}
	}
}
