using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsReceive)]
	public class WhsReceiveCollection : WhsDocketCollection
	{
		public WhsReceiveCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsReceiveCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsReceiveCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public new WhsReceive this[int index]
		{
			get { return (WhsReceive)(base[index]); }
		}

		public virtual new WhsReceive AddNew()
		{
			return (WhsReceive)base.AddNew();
		}

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.Receive }; }
		}
	}
}
