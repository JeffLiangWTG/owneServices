using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfile))]
	sealed class OrgSecurityProfileTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidations()
		{
			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			profile.Name = "";
			profile.Default = false;

			profiles.RunPreSaveValidation();
			AssertHasError(profile.DefaultInfo, "There should be one and only one Default Profile.");
			AssertHasError(profile.NameInfo, "Please enter a value.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OrgSecurityProfile();
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
