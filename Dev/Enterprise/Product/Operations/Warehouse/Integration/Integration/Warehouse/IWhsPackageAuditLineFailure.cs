
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPackageAuditLineFailure
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }
		ZDecimal WPF_AuditedQty { get; set; }
		ZDecimal WPF_ExpectedQty { get; set; }
		ZGuid WPF_OP { get; set; }
		ZGuid WPF_WPA_WhsPackageAudit { get; set; }
	}
}
