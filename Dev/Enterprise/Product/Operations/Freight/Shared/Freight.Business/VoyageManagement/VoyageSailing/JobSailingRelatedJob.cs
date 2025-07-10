using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class JobSailingRelatedJob
	{
		public JobSailingRelatedJob(BusinessObject job, JobSailing sailing, ZString jobNumber, ControllerID controllerID)
		{
			this.Job = job;
			this.Sailing = sailing;
			this.JobNumber = jobNumber;
			this.ControllerID = controllerID;
		}

		public readonly BusinessObject Job;
		public readonly JobSailing Sailing;
		public readonly ZString JobNumber;
		public readonly ControllerID ControllerID;

		public ZString JobUrl
		{
			get { return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerID, Job.PK.ToGuid()); }
		}
	}
}
