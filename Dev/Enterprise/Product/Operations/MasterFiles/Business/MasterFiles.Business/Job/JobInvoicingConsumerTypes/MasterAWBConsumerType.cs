using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class MasterAWBConsumerType : JobInvoicingConsumerType
	{
		public MasterAWBConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobMawb; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IJobMAWB>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
