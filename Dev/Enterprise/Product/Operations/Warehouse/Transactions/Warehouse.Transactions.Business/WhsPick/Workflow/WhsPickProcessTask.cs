using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickProcessTask : ProcessTask
	{
		public WhsPickProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(WhsPick); }
		}

		public new WhsPick Parent
		{
			get { return (WhsPick)base.Parent; }
		}
	}
}
