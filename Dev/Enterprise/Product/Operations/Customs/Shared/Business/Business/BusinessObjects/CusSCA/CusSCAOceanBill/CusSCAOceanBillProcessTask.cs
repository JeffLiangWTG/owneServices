using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillProcessTask : ProcessTask
	{
		public CusSCAOceanBillProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(BaseCusSCAOceanBill);
	}
}
