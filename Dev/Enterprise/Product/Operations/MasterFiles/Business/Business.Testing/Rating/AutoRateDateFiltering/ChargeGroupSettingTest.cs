using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ChargeGroupSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
