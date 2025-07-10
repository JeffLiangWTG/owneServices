using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class CargoIMPPhase2RouteMapCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CargoIMPPhase2RouteMapCollection()
		{
		}

		public new CargoIMPPhase2RouteMap this[int i]
		{
			get { return (CargoIMPPhase2RouteMap)Elements[i]; }
		}

		public new CargoIMPPhase2RouteMap AddNew()
		{
			return (CargoIMPPhase2RouteMap)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoIMPPhase2RouteMapCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CargoIMPPhase2RouteMap();
		}
	}
}
