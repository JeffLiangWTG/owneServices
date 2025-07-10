using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WorkItemConsumerType : JobInvoicingConsumerType
	{
		public WorkItemConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WorkItem; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IWorkItem>(); }
		}

		public override bool ValidateJobForMiscellaneousDepartment
		{
			get { return false; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
