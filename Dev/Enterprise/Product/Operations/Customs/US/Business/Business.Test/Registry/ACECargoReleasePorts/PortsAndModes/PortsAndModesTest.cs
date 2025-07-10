using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(PortsAndModes))]
	sealed class PortsAndModesTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new ACECargoReleaseTypePortMapping();
			return data.PortsAndModes.AddNew();
		}
	}
}
