using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Agency.Business
{
	internal class PortAuthoritySettingsTest : TestCaseWithFactory
	{
		public void TestSerialise_Populated()
		{
			SetupPorts("AUFRE", "AUMEL");
			PortAuthoritySettings value = new PortAuthoritySettings();
			foreach (PortAuthoritySetting setting in value.Settings)
			{
				if (setting.Port == "AUMEL")
				{
					setting.Status = PortAuthoritySettingStatus.Codes.Testing;
				}
				else
				{
					setting.Status = PortAuthoritySettingStatus.Codes.Disabled;
				}
			}

			PortAuthoritySetting newSetting = value.Settings.AddNew();
			newSetting.Port = "AUBNE";
			newSetting.Status = PortAuthoritySettingStatus.Codes.Testing;
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(PortAuthoritySettings));
			StringBuilder builder = new StringBuilder();
			serializer.Serialize(new StringWriter(builder), value);
			// only serialise non-disabled setting elements with corrisponding port elements.
			const string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<PortAuthoritySettings>\n  <PortAuthoritySetting>\n    <Port>AUFRE</Port>\n    <Status>DIS</Status>\n    <PrincipalPK>00000000-0000-0000-0000-000000000000</PrincipalPK>\n    <SenderID />\n  </PortAuthoritySetting>\n  <PortAuthoritySetting>\n    <Port>AUMEL</Port>\n    <Status>TST</Status>\n    <PrincipalPK>00000000-0000-0000-0000-000000000000</PrincipalPK>\n    <SenderID />\n  </PortAuthoritySetting>\n  <PortAuthoritySetting>\n    <Port>AUBNE</Port>\n    <Status>TST</Status>\n    <PrincipalPK>00000000-0000-0000-0000-000000000000</PrincipalPK>\n    <SenderID />\n  </PortAuthoritySetting>\n</PortAuthoritySettings>\n";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestSerialise_Empty()
		{
			SetupPorts("AUFRE", "AUMEL");
			PortAuthoritySettings value = new PortAuthoritySettings();
			foreach (PortAuthoritySetting setting in value.Settings)
			{
				setting.Status = PortAuthoritySettingStatus.Codes.Disabled;
			}

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(PortAuthoritySettings));
			StringBuilder builder = new StringBuilder();
			serializer.Serialize(new StringWriter(builder), value);
			const string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<PortAuthoritySettings>\n  <PortAuthoritySetting>\n    <Port>AUFRE</Port>\n    <Status>DIS</Status>\n    <PrincipalPK>00000000-0000-0000-0000-000000000000</PrincipalPK>\n    <SenderID />\n  </PortAuthoritySetting>\n  <PortAuthoritySetting>\n    <Port>AUMEL</Port>\n    <Status>DIS</Status>\n    <PrincipalPK>00000000-0000-0000-0000-000000000000</PrincipalPK>\n    <SenderID />\n  </PortAuthoritySetting>\n</PortAuthoritySettings>\n";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestDeserialise_Populated()
		{
			SetupPorts("AUFRE", "AUMEL");
			const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" + "<PortAuthoritySettings>\n" + "  <PortAuthoritySetting>\n" + "    <Port>AUMEL</Port>\n" + "    <Status>TST</Status>\n" + "    <SenderID />\n" + "  </PortAuthoritySetting>\n" + "  <PortAuthoritySetting>\n" + "    <Port>AUBNE</Port>\n" + "    <Status>TST</Status>\n" + "    <PrincipalPK>00000000-3333-2222-1111-000000000000</PrincipalPK>\n" + "    <SenderID />\n" + "  </PortAuthoritySetting>\n" + "</PortAuthoritySettings>\n";
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(PortAuthoritySettings));
			PortAuthoritySettings value = (PortAuthoritySettings)serializer.Deserialize(new StringReader(xml));
			// Abandoned (should have 1 setting element for every port element (preferably in the same order to))
			// Abandoned (if the xml does not define a setting element for a given port element then add a new disabled setting element.)
			// Abandoned (if the xml defines a setting element without a corrisponding port element then ignore it.)
			AssertEquals("should have 2 elements", 2, value.Settings.Count);
			AssertEquals("First Element Port", "AUMEL", value.Settings[0].Port);
			AssertEquals("First Element Status", PortAuthoritySettingStatus.Codes.Testing, value.Settings[0].Status);
			AssertEquals("First Element PrincipalPK", "00000000-0000-0000-0000-000000000000", value.Settings[0].PrincipalPK.ToString());
			AssertEquals("Second Element Port", "AUBNE", value.Settings[1].Port);
			AssertEquals("Second Element Status", PortAuthoritySettingStatus.Codes.Testing, value.Settings[1].Status);
			AssertEquals("Second Element PrincipalPK", "00000000-3333-2222-1111-000000000000", value.Settings[1].PrincipalPK.ToString());
		}

		public void TestDeserialise_Empty()
		{
			SetupPorts("AUFRE", "AUMEL");
			const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" + "<PortAuthoritySettings>\n" + "  <PortAuthoritySetting>\n" + "    <Port>AUMEL</Port>\n" + "    <Status>DIS</Status>\n" + "    <SenderID />\n" + "  </PortAuthoritySetting>\n" + "  <PortAuthoritySetting>\n" + "    <Port>AUFRE</Port>\n" + "    <Status>DIS</Status>\n" + "    <SenderID />\n" + "  </PortAuthoritySetting>\n" + "</PortAuthoritySettings>\n";
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(PortAuthoritySettings));
			PortAuthoritySettings value = (PortAuthoritySettings)serializer.Deserialize(new StringReader(xml));
			// Abandoned (should have 1 setting element for every port element (preferably in the same order to))
			// Abandoned (if the xml does not define a setting element for a given port element then add a new disabled setting element.)
			// Abandoned (if the xml defines a setting element without a corrisponding port element then ignore it.)
			AssertEquals("should have 2 elements", 2, value.Settings.Count);
			AssertEquals("First Element Port", "AUMEL", value.Settings[0].Port);
			AssertEquals("First Element Status", PortAuthoritySettingStatus.Codes.Disabled, value.Settings[0].Status);
			AssertEquals("First Element PrincipalPK", "00000000-0000-0000-0000-000000000000", value.Settings[0].PrincipalPK.ToString());
			AssertEquals("Second Element Port", "AUFRE", value.Settings[1].Port);
			AssertEquals("Second Element Status", PortAuthoritySettingStatus.Codes.Disabled, value.Settings[1].Status);
			AssertEquals("First Element PrincipalPK", "00000000-0000-0000-0000-000000000000", value.Settings[1].PrincipalPK.ToString());
		}

		public void TestRefresh()
		{
			SetupPorts("AUFRE", "AUMEL");
			PortAuthoritySettings settings = new PortAuthoritySettings();
			AssertEquals(2, settings.Settings.Count);

			AssertEquals("AUFRE", settings.Settings[0].Port);
			AssertEquals("AUMEL", settings.Settings[1].Port);
			settings.Settings[0].Status = PortAuthoritySettingStatus.Codes.Production;
			settings.Settings[1].Status = PortAuthoritySettingStatus.Codes.Testing;
			SetupPorts("AUFRE", "AUMEL", "AUCNS");
			AssertEquals(2, settings.Settings.Count);
			AssertEquals("AUFRE", settings.Settings[0].Port);
			AssertEquals("AUMEL", settings.Settings[1].Port);
			AssertEquals(PortAuthoritySettingStatus.Codes.Production, settings.Settings[0].Status);
			AssertEquals(PortAuthoritySettingStatus.Codes.Testing, settings.Settings[1].Status);
		}

		public void TestDefault()
		{
			SetupPorts("AUBNE", "AUMEL");
			PortAuthoritySettings value = new PortAuthoritySettings();
			AssertEquals("should have 2 setting's", 2, value.Settings.Count);
			AssertEquals("first setting should be for AUBNE", "AUBNE", value.Settings[0].Port);
			AssertEquals("AUBNE should be disabled", PortAuthoritySettingStatus.Codes.Disabled, value.Settings[0].Status);
			AssertEquals("second setting should be for AUMEL", "AUMEL", value.Settings[1].Port);
			AssertEquals("AUMEL should be disabled", PortAuthoritySettingStatus.Codes.Disabled, value.Settings[1].Status);
		}

		#region Implementation
		void SetupPorts(params ZString[] ports)
		{
			PortAuthorityPortCollection portCollection = new PortAuthorityPortCollection();
			for (int i = 0; i < ports.Length; i++)
			{
				PortAuthorityPort port = portCollection.AddNew();
				port.Port = string.Format(ports[i]);
				port.ProductionEmail = string.Format("prod{0}@freadnet.org", i + 1);
				port.ProductionID = string.Format("prod{0}", i + 1);
				port.TestingEmail = string.Format("test{0}@freadnet.org", i + 1);
				port.TestingID = string.Format("test{0}", i + 1);
			}

			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
		}
		#endregion
	}
}
