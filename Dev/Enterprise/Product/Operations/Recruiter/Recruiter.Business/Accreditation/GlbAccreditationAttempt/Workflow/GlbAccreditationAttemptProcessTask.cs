using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptProcessTask : ProcessTasks
	{
		public GlbAccreditationAttemptProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(GlbAccreditationAttempt);

		public new GlbAccreditationAttempt Parent => (GlbAccreditationAttempt)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.GlbAccreditationAttempt;
	}
}
