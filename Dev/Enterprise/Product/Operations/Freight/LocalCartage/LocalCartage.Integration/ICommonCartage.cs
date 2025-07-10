using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICommonCartage
	{
		ZGuid PK { get; }
		ZBool IsImportOrDestination { get; }
		ZBool IsExportOrOrigin { get; }
		ZBool JJ_IsCancelled { get; set; }
		Enterprise.Integration.Freight.IJobSailing SailingStandalone { get; }
		IJobHeader Job { get; }

		ZGuid JJ_ParentID { get; set; }
		ZString JJ_ParentTableCode { get; set; }
		ZString JJ_ConsignmentID { get; set; }

		IEnumerable<ICommonCartageLeg> Legs { get; }
	}
}
