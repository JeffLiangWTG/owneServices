using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountWaveProcessTask : ProcessTask
	{
		public WhsCycleCountWaveProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ParentType

		protected override Type ParentType => typeof(WhsCycleCountWave);

		#endregion
	}
}
