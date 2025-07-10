using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsDocketJobPivot
	{
		ZGuid WV_WD_Docket { get; set; }
		ZString WV_DocketType { get; set; }
		ZGuid WV_ParentId { get; set; }
		ZString WV_ParentTableCode { get; set; }
	}
}
