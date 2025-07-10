using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderProcessTask : ProcessTask
	{
		public CusOutturnHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CusOutturnHeader);
		public new CusOutturnHeader Parent => (CusOutturnHeader)base.Parent;
	}
}
