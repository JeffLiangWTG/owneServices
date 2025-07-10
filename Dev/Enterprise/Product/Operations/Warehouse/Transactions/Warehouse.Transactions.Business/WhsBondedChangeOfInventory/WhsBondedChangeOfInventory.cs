using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	[UniversalDataContext(DataContextType.WarehouseBondedChangeOfInventory)]
	public class WhsBondedChangeOfInventory : NonPersistentBusinessObject, IJobNumber, IStmALogParent
	{
		#region Constructor

		public WhsBondedChangeOfInventory(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Related Entities

		public WhsOrder Order { get; set; }

		// Receive may be in another factory if it is not to be persisted
		public WhsReceive Receive { get; set; }

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => (NoResString)"NON PERSISTENT WAREHOUSE BONDED CHANGE OF INVENTORY"; // Not translatable, constant and used in place of a job number. Required for Universal.

		#endregion

		#region IStmALogParent members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		Logs IStmALogProvider.Logs => new Logs(this);

		BusinessObjectFactory IStmALogProvider.LogsFactory => new ReadOnlyBusinessObjectFactory(); // Should not save any logs added to this bizo. Universal will try to add a DIM event.

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => "";

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow => false;

		#endregion
	}
}
