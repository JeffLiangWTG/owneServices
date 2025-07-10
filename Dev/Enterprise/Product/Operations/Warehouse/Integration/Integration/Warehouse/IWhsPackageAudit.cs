
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPackageAudit
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }
		ZDateTimeOffset WPA_AuditCompleteTime { get; set; }
		ZString WPA_GS_NKAuditor { get; set; }
		ZString WPA_PackageID { get; set; }
		ZGuid WPA_WD_Order { get; set; }
	}
}
