using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing
{
	sealed class CredentialSenderTest : TestCaseWithFactory
	{
		public void TestCreateFTP()
		{
			var ftp = CredentialSender.CreateFTP("BOB", 21, "BOB1", ZString.Empty);
			AssertEquals("BOB", ftp.Server);
			AssertEquals(21, ftp.Port);
			AssertEquals("BOB1", ftp.UserName);
			AssertNull(ftp.Password);
		}

		public void TestAddItems(IEnumerable<object> items)
		{
			var sender = new CredentialSender("TEST");
			var configuration = sender.ToConfiguration();
			var systemGroup = configuration.Group[0];
			AssertNull(systemGroup.Items);

			sender.AddItems(null);
			configuration = sender.ToConfiguration();
			systemGroup = configuration.Group[0];
			AssertNull(systemGroup.Items);

			sender.AddItems(new Item() { Name = "HELLO2", Value = "VALUE1" }, new Item() { Name = "HELLO1", Value = "VALUE2" });
			configuration = sender.ToConfiguration();
			systemGroup = configuration.Group[0];
			AssertEquals(2, systemGroup.Items.Length);
			AssertItem((Item)systemGroup.Items[0], "HELLO2", "VALUE1");
			AssertItem((Item)systemGroup.Items[1], "HELLO1", "VALUE2");

			sender.AddItems(new Item() { Name = "HELLO3", Value = "VALUE3" });
			configuration = sender.ToConfiguration();
			systemGroup = configuration.Group[0];
			AssertEquals(3, systemGroup.Items.Length);
			AssertItem((Item)systemGroup.Items[0], "HELLO2", "VALUE1");
			AssertItem((Item)systemGroup.Items[1], "HELLO1", "VALUE2");
			AssertItem((Item)systemGroup.Items[2], "HELLO3", "VALUE3");
		}

		public void TestToConfiguration()
		{
			var sender = new CredentialSender("TEST");
			sender.AddItems(new Item() { Name = "HELLO", Value = "VALUE" });
			var configuration = sender.ToConfiguration();

			AssertEquals("The configuration name should be set", "TEST", configuration.Name);
			AssertEquals("The configuration should contain one group", 1, configuration.Group.Count);
			var systemGroup = configuration.Group[0];
			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("Group reference should be set", "EDIDAT", systemGroup.Reference);
			AssertEquals("The System Group should contain one group", 1, systemGroup.Items.Length);
			AssertItem((Item)systemGroup.Items[0], "HELLO", "VALUE");
		}

		static void AssertItem(Item item, ZString name, ZString value)
		{
			AssertEquals("item.Name", name, item.Name);
			AssertEquals("item.Value", value, item.Value);
		}

		public void TestCreateCredential()
		{
			var credential = CredentialSender.CreateCredential("BOB", "BOB1", ZString.Empty);
			AssertEquals("credential.Name", "BOB", credential.Name);
			AssertEquals("credential.UserName", "BOB1", credential.UserName);
			AssertEquals("credential.Password", Array.Empty<byte>(), credential.Password);

			credential = CredentialSender.CreateCredential("BOB", "BOB1", "HI");
			AssertEquals("credential.Name", "BOB", credential.Name);
			AssertEquals("credential.UserName", "BOB1", credential.UserName);
			AssertNotEquals("credential.Password", Array.Empty<byte>(), credential.Password);
		}

		public void TestEncryptPasswordAsString()
		{
			var encryptedPassword = CredentialSender.EncryptPasswordAsString("Password");
			AssertEquals("Password", DecodePassword(encryptedPassword));
		}

		string DecodePassword(string password)
		{
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				var keyXml = "<RSAKeyValue><Modulus>1XmSsYj8fup2QDmo2Yp0nKVzNsonnECcN2OcZXjF+eIEgQyLydnwngBcdW7UOwqQ0E+ffFcdoW44IaTVZ/A7yNC/pRJQy42BBcXteSo7SqgHt7yvPUph+oFpEWLIe0WnPJDmZCRFNoZnMUoOzK89oA/v0I3neveYHjM0bRgtVBE=</Modulus><Exponent>AQAB</Exponent><P>5sXEF2kBcvgWJ1FSo1rahov1KiqogxqBOJio3FmsLsjafwV9O1xKGmjuL1gdQbgd7rPI7x8b42PTfGctlAIPdQ==</P><Q>7M+6YC9HcPeNvr6/S+jX7tP+OV6Xljqe/U9/j7fisEG/9cfFz9lh+H+oItB1MpOD/dKB3RNW5/ODA5TSsHsarQ==</Q><DP>PQtja63jLD5j3dKtQXjvBVhQae8O1F9Wf1oikOdHnLiU07ToA6POFl5bYzqzwoappFL6fAaGogfuEaJZdCV3YQ==</DP><DQ>5iFYtXA8tQNdtCgaLuKwNV++hnHuTgfZycEf7cJ9gVvj+C2ThlFya9NiybJasjO46UlQ+k54/iAfCbPuq6J2YQ==</DQ><InverseQ>k3hB5HGpUKFMYBO+Dh7dkJJ6z7TGKevKWHw3Rhv6aYFOqVvDBKQyW52pbh4aHv5eX15QrBJNPnXYmovSYj47oQ==</InverseQ><D>A/CjvA+bcMCP5f+6cFNtc2OxWe+cELaiO3p6QpGFU+dE7oMmWazM/lmNhfmsRJpdZwmFLR9oKQMsBDZIMwwnH5/WD+gso3VmPRsg5BIdoOs/F0ZkR1qeUlwbjmaD+fPV4b8Tg/3JVeSWT7K8xtKGMLWDcdLMA3vaRo/1/g+KfD0=</D></RSAKeyValue>";
				var text = Convert.FromBase64String(password);
				rsa.FromXmlString(keyXml);
				return new string(System.Text.Encoding.UTF8.GetChars(rsa.Decrypt(text, true)));
			}
		}

		public void TestCreateCertificate()
		{
			var file = ZBlob.FromAscii("HELLO");
			var certificate = CredentialSender.CreateCertificate(file, "");
			AssertEquals("certificate.Name", Constants.CertificateDetails.Name, certificate.Name);
			AssertEquals("certificate.File.IsSpecified", true, certificate.File.IsSpecified);
			AssertEquals("certificate.File.Value", file, certificate.File.Value);
			AssertEquals("certificate.Passphrase", Array.Empty<byte>(), certificate.Passphrase);

			certificate = CredentialSender.CreateCertificate(file, "HI");
			AssertEquals("certificate.Name", Constants.CertificateDetails.Name, certificate.Name);
			AssertEquals("certificate.File.IsSpecified", true, certificate.File.IsSpecified);
			AssertEquals("certificate.File.Value", file, certificate.File.Value);
			AssertNotEquals("certificate.Passphrase", Array.Empty<byte>(), certificate.Passphrase);
		}

		public void TestCreateGroup()
		{
			var group = CredentialSender.CreateGroup("BOB", ZString.Empty, ZString.Empty);
			AssertEquals("group.Type", "BOB", group.Type);
			AssertEquals("group.Reference", false, group.ReferenceSpecified);
			AssertEquals("group.StatusSpecified", false, group.StatusSpecified);

			group = CredentialSender.CreateGroup("BOB", "BOB1", ZString.Empty);
			AssertEquals("group.Type", "BOB", group.Type);
			AssertEquals("group.Reference", "BOB1", group.Reference);
			AssertEquals("group.StatusSpecified", false, group.StatusSpecified);

			group = CredentialSender.CreateGroup("BOB", "BOB1", "VAL");
			AssertEquals("group.Type", "BOB", group.Type);
			AssertEquals("group.Reference", "BOB1", group.Reference);
			AssertEquals("group.Status", "VAL", group.Status);
		}

		public void TestCreateItem()
		{
			var item = CredentialSender.CreateItem("BOB", "BOB1");
			AssertEquals("item.Name", "BOB", item.Name);
			AssertEquals("item.Value", "BOB1", item.Value);
		}

		public void TestSendCredentialToEHub()
		{
			var sender = new CredentialSender("TestConfigurationName");
			sender.AddItems(new Item() { Name = "HELLO", Value = "VALUE" });

			var factory = new BusinessObjectFactory();
			var interchange = sender.SendCredential(factory);
			var xml = string.Empty;

			CombineAssertions(() =>
			{
				AssertEquals(false, ((BusinessObject)interchange).IsInDatabase);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.eHub, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.Configuration, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("interchange.EI_From", ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("interchange.EI_To", Constants.Configuration.EHubRecipient, interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_SessionGUID", interchange.PK, interchange.EI_SessionGUID);

				xml = sender.ToConfiguration().ToXml();
				AssertEquals("interchange.EI_BodyText", xml, interchange.EI_BodyText);
			});

			sender.SendCredential();

			var interchange2 = Factory.GetLatestEHubConfigurationInterchange();
			AssertNotEquals(interchange.PK, interchange2.PK);
			AssertEquals("interchange2.EI_BodyText", xml, interchange2.EI_BodyText);

			sender = new CredentialSender("TestConfigurationName", EDIInterchangeTypeList.Codes.CustomsWare);

			var interchange3 = sender.SendCredential(factory);
			AssertEquals("interchange3.EI_InterchangeType", EDIInterchangeTypeList.Codes.CustomsWare, interchange3.EI_InterchangeType);
		}

		public void TestSendCredentialToDirectxT()
		{
			var sender = new CredentialSender("TestConfigurationName", EDIInterchangeTypeList.Codes.Configuration, CredentialRecipient.DirectxT);
			sender.AddItems(new Item() { Name = "HELLO", Value = "VALUE" });

			var factory = new BusinessObjectFactory();
			var interchange = sender.SendCredential(factory);

			CombineAssertions(() =>
			{
				AssertEquals(false, ((BusinessObject)interchange).IsInDatabase);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.XHCredentialConfig, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.Configuration, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("interchange.EI_From", ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("interchange.EI_To", Constants.Configuration.XHRecipient, interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
				AssertEquals("interchange.EI_SessionGUID", interchange.PK, interchange.EI_SessionGUID);
				AssertEquals("interchange.EI_BodyText", sender.ToConfiguration().ToXml(), interchange.EI_BodyText);
			});
		}
	}
}
