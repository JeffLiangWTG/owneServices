using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccCommissionHeader
	{
		ZGuid PK { get; }
		ZGuid CH0_AH_Source { get; }
		ZGuid CH0_CA0 { get; }
		ZDate CH0_CommissionDate { get; }
		ZGuid CH0_GC { get; }
		ZGuid CH0_GroupingSourceID { get; }
		ZString CH0_GroupingSourceTableCode { get; }
		ZString CH0_Mode { get; }
		ZGuid CH0_OH_Customer { get; }
		ZGuid CH0_OH_Debtor { get; }
		ZString CH0_NKDestination { get; }
		ZString CH0_NKOrigin { get; }
		ZString CH0_Product { get; }
		ZString CH0_Service { get; }
		ZString CH0_SubModule { get; }
		ZString CH0_JobNumber { get; }
		ZDateTime CH0_SystemCreateTimeUtc { get; }
		IEnumerable<IAccCommissionLineGroup> LineGroups { get; }
		IEnumerable<IAccCommissionLine> Lines { get; }
	}
}
