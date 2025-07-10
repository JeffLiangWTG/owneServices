using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FormCustomisationSettingsStorageSerializerTest : TestCase
	{
		public void TestSerialize()
		{
			AssertNull(FormCustomisationSettingsStorageSerializer.Serialize(null));

			FormCustomisationSettingsStorage storage = new FormCustomisationSettingsStorage();
			storage.Tab.Add(new FormCustomisationSettingsStorageTab { Name = "AAA", Description = "AAA Desc", Visible = true });
			storage.Tab.Add(new FormCustomisationSettingsStorageTab { Name = "BBB", Description = "BBB Desc", Visible = false });

			storage.Field.Add(new FormCustomisationSettingsStorageField { Name = "CCC", Description = "CCC Desc", Group = "Group1", Position = 0, Visible = true });
			storage.Field.Add(new FormCustomisationSettingsStorageField { Name = "DDD", Description = "DDD Desc", Group = "Group1", Position = 1, Visible = false });

			byte[] serializedValue = FormCustomisationSettingsStorageSerializer.Serialize(storage);

			AssertXMLEquals("serialized value", Xml, Encoding.ASCII.GetString(serializedValue));
		}

		public void TestDeserializeInvalid()
		{
			AssertExceptionThrown(typeof(CustomisationSettingsDeserializeException), () =>
			{
				FormCustomisationSettingsStorageSerializer.Deserialize(Encoding.ASCII.GetBytes(Xml + "fdkjlgnfdjkgndf"));
			}
			);
		}

		public void TestDeserialize()
		{
			AssertNull(FormCustomisationSettingsStorageSerializer.Deserialize(null));

			byte[] serializedValue = Encoding.ASCII.GetBytes(Xml);

			FormCustomisationSettingsStorage storage = FormCustomisationSettingsStorageSerializer.Deserialize(serializedValue);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|AAA Desc|True", "BBB|BBB Desc|False" },
				storage.Tab.Cast<FormCustomisationSettingsStorageTab>().Select((tab) => string.Concat(tab.Name, "|", tab.Description, "|", tab.Visible)).ToArray());

			AssertContainsExactElementsInAnyOrder(new[] { "CCC|CCC Desc|Group1|0|True", "DDD|DDD Desc|Group1|1|False" },
				storage.Field.Cast<FormCustomisationSettingsStorageField>().Select((tab) => string.Concat(tab.Name, "|", tab.Description, "|", tab.Group, "|", tab.Position, "|", tab.Visible)).ToArray());
		}

#if NETFRAMEWORK
		const string Xml = @"<?xml version=""1.0""?>
<FormCustomisationSettingsStorage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <Tab>
    <Name>AAA</Name>
    <Description>AAA Desc</Description>
    <Visible>true</Visible>
  </Tab>
  <Tab>
    <Name>BBB</Name>
    <Description>BBB Desc</Description>
    <Visible>false</Visible>
  </Tab>
  <Field>
    <Name>CCC</Name>
    <Description>CCC Desc</Description>
    <Group>Group1</Group>
    <Visible>true</Visible>
    <Position>0</Position>
  </Field>
  <Field>
    <Name>DDD</Name>
    <Description>DDD Desc</Description>
    <Group>Group1</Group>
    <Visible>false</Visible>
    <Position>1</Position>
  </Field>
</FormCustomisationSettingsStorage>";
#else
		const string Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<FormCustomisationSettingsStorage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <Tab>
    <Name>AAA</Name>
    <Description>AAA Desc</Description>
    <Visible>true</Visible>
  </Tab>
  <Tab>
    <Name>BBB</Name>
    <Description>BBB Desc</Description>
    <Visible>false</Visible>
  </Tab>
  <Field>
    <Name>CCC</Name>
    <Description>CCC Desc</Description>
    <Group>Group1</Group>
    <Visible>true</Visible>
    <Position>0</Position>
  </Field>
  <Field>
    <Name>DDD</Name>
    <Description>DDD Desc</Description>
    <Group>Group1</Group>
    <Visible>false</Visible>
    <Position>1</Position>
  </Field>
</FormCustomisationSettingsStorage>";
#endif
	}
}
