using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(CMMFlagsControl))]
	internal class CMMFlagsControlBOTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CMMFlags();
		}
	}
}
