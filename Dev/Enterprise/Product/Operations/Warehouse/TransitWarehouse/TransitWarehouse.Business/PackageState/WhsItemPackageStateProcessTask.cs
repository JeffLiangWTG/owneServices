using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateProcessTask : ProcessTask
	{
		public WhsItemPackageStateProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ParentType

		protected override Type ParentType
		{
			get { return typeof(WhsItemPackageState); }
		}

		#endregion
	}
}
