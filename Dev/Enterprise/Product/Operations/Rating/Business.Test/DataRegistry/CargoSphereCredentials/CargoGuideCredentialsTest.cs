using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CargoGuideCredentials))]
	public class CargoGuideCredentialsTest : RegistryBusinessObjectTemplateTestCase<CargoGuideCredentials>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CargoGuideCredentials GetBusinessObjectToClone()
		{
			return new CargoGuideCredentials { Login = "testlogin1", Password = "testpassword1" };
		}

		protected override CargoGuideCredentials GetBusinessObjectToSerialise()
		{
			return new CargoGuideCredentials { Login = "testlogin2", Password = "testpassword2" };
		}
	}
}
