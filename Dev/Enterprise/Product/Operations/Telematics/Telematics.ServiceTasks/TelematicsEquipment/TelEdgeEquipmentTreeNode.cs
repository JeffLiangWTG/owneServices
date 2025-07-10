using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	public class TelEdgeEquipmentTreeNode : DynamicBusinessObject
	{
		public TelEdgeEquipmentTreeNode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZGuid TET_TelEdgePK => (ZGuid)base[nameof(TET_TelEdgePK)];
		public ZGuid TET_ParentPK => (ZGuid)base[nameof(TET_ParentPK)];
		public ZGuid TET_ChildPK => (ZGuid)base[nameof(TET_ChildPK)];
		public string TET_ParentTableCode => base[nameof(TET_ParentTableCode)].ToString();
		public string TET_ChildTableCode => base[nameof(TET_ChildTableCode)].ToString();
		public string TET_ParentConfig => base[nameof(TET_ParentConfig)].ToString();
		public string TET_ChildConfig => base[nameof(TET_ChildConfig)].ToString();
		public string TET_ChildId => base[nameof(TET_ChildId)].ToString();
		public string TET_ParentId => base[nameof(TET_ParentId)].ToString();
		public string TET_ChildType => base[nameof(TET_ChildType)].ToString();
		public string TET_ParentType => base[nameof(TET_ParentType)].ToString();
		public DateTimeOffset? TET_StartTime => ((ZDateTimeOffset)base[nameof(TET_StartTime)]).ToDateTimeOffset();
		public DateTimeOffset? TET_EndTime => ((ZDateTimeOffset)base[nameof(TET_EndTime)]).IsEmpty
			? null
			: ((ZDateTimeOffset)base[nameof(TET_EndTime)]).ToDateTimeOffset();
	}
}
