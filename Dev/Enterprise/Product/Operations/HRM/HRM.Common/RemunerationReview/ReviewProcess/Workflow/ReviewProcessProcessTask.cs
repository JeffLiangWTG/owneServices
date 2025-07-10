using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessProcessTask : ProcessTasks
	{
		public ReviewProcessProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type ParentType => typeof(ReviewProcess);
	}
}
