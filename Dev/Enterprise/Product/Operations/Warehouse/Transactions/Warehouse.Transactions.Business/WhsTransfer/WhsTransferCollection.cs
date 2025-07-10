using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsTransfer)]
	public class WhsTransferCollection : WhsDocketCollection
	{
		public WhsTransferCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsTransferCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public new WhsTransfer this[int index]
		{
			get { return (WhsTransfer)(base[index]); }
		}

		public new WhsTransfer AddNew()
		{
			return (WhsTransfer)base.AddNew();
		}

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.Transfer }; }
		}
	}
}
