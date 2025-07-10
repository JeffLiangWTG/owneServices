using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsWorkOrder)]
	public class WhsWorkOrderCollection : WhsComponentOrderCollection
	{
		#region Constructors

		public WhsWorkOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		public new WhsWorkOrder this[int index]
		{
			get { return (WhsWorkOrder)base[index]; }
		}

		protected override bool AllowNewDocket
		{
			get { return true; }
		}

		public new WhsWorkOrder AddNew()
		{
			return (WhsWorkOrder)base.AddNew();
		}

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.WorkOrder }; }
		}
	}
}
