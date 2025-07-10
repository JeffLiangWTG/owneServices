using System;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public interface IDataSetView
	{
		Guid RVC_ParentPK { get; }
		DateTime? RVC_LastUpdatedUTC { get; }
		short? RVC_DataSetId { get; }
	}
}
