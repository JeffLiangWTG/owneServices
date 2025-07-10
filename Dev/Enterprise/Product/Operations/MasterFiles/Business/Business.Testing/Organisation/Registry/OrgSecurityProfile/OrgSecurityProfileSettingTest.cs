using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileSetting))]
	sealed class OrgSecurityProfileSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OrgSecurityProfileSetting();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion Implementation
	}
}
