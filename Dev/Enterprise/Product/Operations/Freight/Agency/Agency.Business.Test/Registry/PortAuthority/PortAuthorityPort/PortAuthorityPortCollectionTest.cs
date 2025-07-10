using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthorityPortCollection))]
	internal class PortAuthorityPortCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PortAuthorityPortCollection>
	{
		public void TestFindByPort()
		{
			PortAuthorityPortCollection collection = new PortAuthorityPortCollection();
			PortAuthorityPort port1 = collection.AddNew();
			port1.Port = "AUBNE";
			PortAuthorityPort port2 = collection.AddNew();
			port2.Port = "AUMEL";
			PortAuthorityPort port3 = collection.AddNew();
			port3.Port = "AUCNS";
			AssertSame(port1, collection.FindByPort("AUBNE"));
			AssertSame(port2, collection.FindByPort("AUMEL"));
			AssertSame(port3, collection.FindByPort("AUCNS"));
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override PortAuthorityPortCollection GetCollectionToTest()
		{
			return new PortAuthorityPortCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortAuthorityPort();
		}
		#endregion
	}
}
