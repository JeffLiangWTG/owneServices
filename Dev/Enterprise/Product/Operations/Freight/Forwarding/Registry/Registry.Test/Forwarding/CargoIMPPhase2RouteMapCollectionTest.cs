using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2RouteMapCollection))]
	public class CargoIMPPhase2RouteMapCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CargoIMPPhase2RouteMapCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CargoIMPPhase2RouteMapCollection GetCollectionToTest()
		{
			return new CargoIMPPhase2RouteMapCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CargoIMPPhase2RouteMap();
		}

		#endregion
	}
}
