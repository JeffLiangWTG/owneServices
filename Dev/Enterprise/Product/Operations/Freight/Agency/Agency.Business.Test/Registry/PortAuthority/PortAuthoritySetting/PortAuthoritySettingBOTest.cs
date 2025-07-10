using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthoritySetting))]
	internal class PortAuthoritySettingBOTest : RegistryBusinessObjectTemplateTestCase<PortAuthoritySetting>
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

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortAuthoritySetting GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortAuthoritySetting GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PortAuthoritySetting originalBusinessObject, PortAuthoritySetting newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("Port", originalBusinessObject.Port, newBusinessObject.Port);
			AssertEquals("Version", originalBusinessObject.Version, newBusinessObject.Version);
			AssertEquals("Email", originalBusinessObject.Email, newBusinessObject.Email);
		}

		PortAuthoritySetting GetNewPopulatedBusinessObject()
		{
			PortAuthoritySettingCollection collection = new PortAuthoritySettingCollection();
			PortAuthoritySetting setting = collection.AddNew();
			setting.Port = "AUSYD";
			setting.Status = PortAuthoritySettingStatus.Codes.Production;
			return setting;
		}

		#endregion
	}
}
