using System;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(PortAuthoritySettings))]
	internal class PortAuthoritySettingsBOTest : RegistryBusinessObjectTemplateTestCase<PortAuthoritySettings>
	{
		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override PortAuthoritySettings GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override PortAuthoritySettings GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PortAuthoritySettings originalBusinessObject, PortAuthoritySettings newBusinessObject, bool isClone)
		{
			AssertEquals(originalBusinessObject.Settings.Count, newBusinessObject.Settings.Count);
			for (int i = 0; i < originalBusinessObject.Settings.Count; i++)
			{
				AssertEquals(originalBusinessObject.Settings[i].Port, newBusinessObject.Settings[i].Port);
				AssertEquals(originalBusinessObject.Settings[i].Status, newBusinessObject.Settings[i].Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			PortAuthorityPortCollection portCollection = new PortAuthorityPortCollection();
			PortAuthorityPort port1 = portCollection.AddNew();
			port1.Port = "AUBNE";
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";
			PortAuthorityPort port2 = portCollection.AddNew();
			port2.Port = "AUMEL";
			port2.ProductionEmail = "prod2@freadnet.org";
			port2.ProductionID = "prod2";
			port2.TestingEmail = "test2@freadnet.org";
			port2.TestingID = "test2";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
		}

		PortAuthoritySettings NewPopulatedBusinessObject()
		{
			PortAuthoritySettings value = new PortAuthoritySettings();
			foreach (PortAuthoritySetting settings in value.Settings)
			{
				settings.Status = PortAuthoritySettingStatus.Codes.Production;
				settings.SenderID = "random id";
			}

			return value;
		}
		#endregion
	}
}
