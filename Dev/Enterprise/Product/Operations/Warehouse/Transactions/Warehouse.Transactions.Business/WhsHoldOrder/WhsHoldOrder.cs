using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	[UniversalDataContext(DataContextType.WarehouseHoldOrder)]
	public class WhsHoldOrder : NonPersistentBusinessObject, IJobNumber, IStmALogParent
	{
		#region Constructor

		public WhsHoldOrder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Related Entities

		#region Client

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(ClientPK); }
		}

		#endregion

		#region Lines

		#region Lines

		[ChildEditable(true)]
		public WhsHoldOrderLineCollection Lines
		{
			get { return lines ?? (lines = GetLinesCollection()); }
		}

		WhsHoldOrderLineCollection GetLinesCollection()
		{
			var collection = new WhsHoldOrderLineCollection(Factory);
			RegisterEditableChildObject(collection);
			return collection;
		}

		WhsHoldOrderLineCollection lines;

		#endregion

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WarehousePK); }
		}

		#endregion

		#endregion

		#region Properties

		#region ClientPK

		[RelatedBusinessObject("Client")]
		public ZGuid ClientPK
		{
			get { return clientPK; }
			set { SetNonPersistentPropertyValue(ClientPKInfo, ref clientPK, value); }
		}

		public ZPropertyInfo ClientPKInfo
		{
			get { return GetZPropertyInfo(nameof(ClientPK)); }
		}

		ZGuid clientPK;

		#endregion

		#region WarehousePK

		[RelatedBusinessObject("Warehouse")]
		public ZGuid WarehousePK
		{
			get { return warehousePK; }
			set { SetNonPersistentPropertyValue(WarehousePKInfo, ref warehousePK, value); }
		}

		public ZPropertyInfo WarehousePKInfo
		{
			get { return GetZPropertyInfo(nameof(WarehousePK)); }
		}

		ZGuid warehousePK;

		#endregion

		#endregion

		#region Finalise

		public bool Finalise()
		{
			var warehouse = Warehouse;
			var client = Client;

			return Lines.Cast<WhsHoldOrderLine>().Aggregate(true, (result, line) => result &= line.Finalise(warehouse, client));
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return (NoResString)"NON PERSISTENT HOLD ORDER"; } // Not translatable, constant and used in place of a job number. Required for Universal.
		}

		#endregion

		#region IStmALogParent members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		Logs IStmALogProvider.Logs
		{
			get { return new Logs(this); }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get
			{
				// Should not save any logs added to this bizo. Universal will try to add a DIM event.
				return new ReadOnlyBusinessObjectFactory();
			}
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return ""; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion
	}
}
