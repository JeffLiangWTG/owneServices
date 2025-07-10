using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class ShipmentDeclarationLookups : ZLookups
	{
		public ShipmentDeclarationLookups(IShipmentDeclaration parent)
			: base((BusinessObject)parent)
		{
		}

		public RefUNLOCOCollection Ports
		{
			get { return ports ?? (ports = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection ports;

		public RefCurrencyCollection Currencies
		{
			get { return currencies ?? (currencies = new RefCurrencyCollection(Factory)); }
		}
		RefCurrencyCollection currencies;

		public RefServiceLevelCollection ServiceLevels
		{
			get { return serviceLevels ?? (serviceLevels = new WebServiceLevelCollection(Factory)); }
		}
		RefServiceLevelCollection serviceLevels;
	}
}
