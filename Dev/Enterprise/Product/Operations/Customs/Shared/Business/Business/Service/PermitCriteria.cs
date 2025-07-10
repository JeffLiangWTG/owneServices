using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class PermitCriteria
	{
		public PermitCriteria(bool isDetailedTracking, OrgAddress owner, OrgAddress warehouse, OrgAddress manufacturer, ZString uq, ZString countryOfOrigin, ZString productCode, ZString tariff, ZString zoneStatus)
		{
			IsDetailedTracking = isDetailedTracking;
			Owner = owner;
			Warehouse = warehouse;
			Manufacturer = manufacturer;
			UQ = uq;
			CountryOfOrigin = countryOfOrigin;
			ProductCode = productCode;
			Tariff = tariff;
			ZoneStatus = zoneStatus;
		}

		public bool IsDetailedTracking { get; private set; }
		public OrgAddress Owner { get; private set; }
		public OrgAddress Warehouse { get; private set; }
		public OrgAddress Manufacturer { get; private set; }
		public ZString UQ { get; private set; }
		public ZString CountryOfOrigin { get; private set; }
		public ZString ProductCode { get; private set; }
		public ZString Tariff { get; private set; }
		public ZString ZoneStatus { get; private set; }
	}
}
