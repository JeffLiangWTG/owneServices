using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeProcessTask : ProcessTask
	{
		public WhsStocktakeProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ParentType

		protected override Type ParentType
		{
			get { return typeof(WhsStocktake); }
		}

		#endregion
	}
}
