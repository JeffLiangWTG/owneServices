using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class WorkRequestConsumerType : JobInvoicingConsumerType
	{
		public WorkRequestConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.CustomerServiceTicket;

		public override Type BizoType => ObjectFactory.GetType<IWorkRequest>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.None;

		public override bool ValidateJobForMiscellaneousDepartment => false;
	}
}
