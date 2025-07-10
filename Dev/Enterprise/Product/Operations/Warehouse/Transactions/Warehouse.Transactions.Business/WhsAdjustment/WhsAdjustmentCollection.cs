using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsAdjustment)]
	public class WhsAdjustmentCollection : WhsDocketCollection
	{
		public WhsAdjustmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsAdjustmentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsAdjustmentCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public new WhsAdjustment this[int index]
		{
			get { return (WhsAdjustment)(base[index]); }
		}

		public new WhsAdjustment AddNew()
		{
			return (WhsAdjustment)base.AddNew();
		}

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.Adjustment }; }
		}
	}
}
