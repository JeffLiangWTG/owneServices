using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick, WhsValidationHelper.WhsCheckForUnreferencedWhsDockDoorAssignment, WhsDockDoorAssignmentSchema.Constants.PK, typeof(IWhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy))]
	public class WhsDockDoorAssignment : AutoWhsDockDoorAssignment,
		ICriticalChangesVersionID,
		IWhsDockDoorAssignment
	{
		public WhsDockDoorAssignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WDA_WL_AssignedDockDoor

		public override ZGuid WDA_WL_AssignedDockDoor
		{
			get => base.WDA_WL_AssignedDockDoor;
			set
			{
				base.WDA_WL_AssignedDockDoor = value;
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsDockDoorAssignment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			}
		}

		#endregion

		#region WDA_FirstPutawayToDockDoorUtc

		public override ZDateTime WDA_FirstPutawayToDockDoorUtc
		{
			get => base.WDA_FirstPutawayToDockDoorUtc;
			set
			{
				base.WDA_FirstPutawayToDockDoorUtc = value;
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsDockDoorAssignment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WDA_CriticalChangesVersionID), WDA_FirstPutawayToDockDoorUtc.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
			}
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			SetAttachPickDockDoorLocations();
			base.Delete();
		}

		void SetAttachPickDockDoorLocations()
		{
			var query = new ZQuery(WhsPickSchema.WP_WDA_DockDoorAssignment, PK);
			var picks = Factory.Load<WhsPick>(query);
			picks.ForEach((p) =>
			{
				p.WP_WL_DockDoor = WDA_WL_AssignedDockDoor;
				p.WP_WDA_DockDoorAssignment = ZGuid.Empty;
			});
		}

		public void DeleteLooseForPick()
		{
			base.Delete();
		}

		#endregion

		#region Related Entities

		public WhsLocation AssignedDockDoor => Factory.Load<WhsLocation>(WDA_WL_AssignedDockDoor);

		#endregion

		#region ICriticalChangesVersionID members

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WDA_CriticalChangesVersionID = value;
			get => WDA_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => WDA_FirstPutawayToDockDoorUtc.IsValid;

		#endregion
	}
}
