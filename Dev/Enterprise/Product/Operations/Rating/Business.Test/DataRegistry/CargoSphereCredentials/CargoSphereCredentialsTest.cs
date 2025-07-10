using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CargoSphereCredentials))]
	public class CargoSphereCredentialsTest : RegistryBusinessObjectTemplateTestCase<CargoSphereCredentials>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CargoSphereCredentials GetBusinessObjectToClone()
		{
			return new CargoSphereCredentials { Login = "testlogin1", Password = "testpassword1", SystemCode = "testsystemcode1" };
		}

		protected override CargoSphereCredentials GetBusinessObjectToSerialise()
		{
			return new CargoSphereCredentials { Login = "testlogin2", Password = "testpassword2", SystemCode = "testsystemcode2" };
		}
	}
}
