using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class FCLStorageConsumerType : JobInvoicingConsumerType
	{
		public FCLStorageConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.PackContainerRegistration; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<CFS.ICFSContainer>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCFS; }
		}
	}
}
