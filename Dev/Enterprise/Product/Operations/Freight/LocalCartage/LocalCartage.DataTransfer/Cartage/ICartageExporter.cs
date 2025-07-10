using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public interface ICartageExporter
	{
		BusinessObjectFactory ParentFactory { get; }
		Logs ParentLogs { get; }
		Notes ParentNotes { get; }
		OrgHeader SendTo { get; }
		ZString SendToDescription { get; }
		ZString Description { get; }
		ZString ParentJobNumber { get; }
		bool IsParentSaved { get; }
		CommonCartageType CartageJobType { get; }

		CommonCartage GetCartageForExport(NotificationBuffer buffer);
		void CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn);
	}
}
