using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileSettingCollection))]
	sealed class OrgSecurityProfileSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgSecurityProfileSettingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OrgSecurityProfileSettingCollection GetCollectionToTest()
		{
			return new OrgSecurityProfileSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgSecurityProfileSetting();
		}

		public void TestOnDeserialized()
		{
			var collection = new OrgSecurityProfileSettingCollection();
			collection.PopulateDefaultSettings();

			ZString securityKey = "";

			foreach (var setting in collection.OfType<OrgSecurityProfileSetting>().ToArray())
			{
				if (setting.Granted)
				{
					securityKey = setting.SecurityKey;
					collection.RemoveAndDelete(setting);
					break;
				}
			}

			AssertEquals(0, collection.OfType<OrgSecurityProfileSetting>().Count(x => x.SecurityKey == securityKey));
			collection.OnDeserialized();
			AssertEquals(false, collection.OfType<OrgSecurityProfileSetting>().First(x => x.SecurityKey == securityKey).Granted);
		}

		#endregion Implementation
	}
}
