using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestProcessTask : ProcessTasks
	{
		public HRHiringRequestProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(HRHiringRequest);

		public override ControllerID ParentControllerID => ControllerIDs.HRHiringRequest;
	}
}
