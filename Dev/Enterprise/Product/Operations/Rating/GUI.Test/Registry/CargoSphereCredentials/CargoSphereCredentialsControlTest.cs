using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Registry.GUI.Testing
{
	[TestedType(typeof(CargoSphereCredentialsControl))]
	public class CargoSphereCredentialsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CargoSphereCredentials();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CargoSphereCredentialsControl)control).ReadOnly;
		}
	}
}
