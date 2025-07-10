using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetableSettings))]
	public class DefaultOrgTimetableSettingsTest : RegistryBusinessObjectTemplateTestCase<DefaultOrgTimetableSettings>
	{
		protected override DefaultOrgTimetableSettings GetBusinessObjectToClone()
		{
			return new DefaultOrgTimetableSettings(Factory);
		}

		protected override DefaultOrgTimetableSettings GetBusinessObjectToSerialise()
		{
			return new DefaultOrgTimetableSettings(Factory);
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}
	}
}
