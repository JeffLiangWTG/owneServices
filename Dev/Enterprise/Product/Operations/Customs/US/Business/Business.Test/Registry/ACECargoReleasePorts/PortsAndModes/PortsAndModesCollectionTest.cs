using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(PortsAndModesCollection))]
	sealed class PortsAndModesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PortsAndModesCollection>
	{
		protected override PortsAndModesCollection GetCollectionToTest() => new PortsAndModesCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new PortsAndModes(null);
	}
}
