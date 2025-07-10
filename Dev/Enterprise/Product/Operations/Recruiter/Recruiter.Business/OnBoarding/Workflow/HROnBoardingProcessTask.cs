using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingProcessTask : ProcessTasks
	{
		public HROnBoardingProcessTask(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(HROnBoarding);

		public override ControllerID ParentControllerID => ControllerIDs.HROnBoarding;
	}
}
