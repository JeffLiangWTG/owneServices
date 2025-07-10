using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ChargeGroupSetupTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

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
			get { return true; }
		}

		#endregion

	}
}
