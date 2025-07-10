using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class ProjectConsumerType : JobInvoicingConsumerType
	{
		public ProjectConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Project; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IProject>(); }
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
