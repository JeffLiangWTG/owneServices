using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsTransitNCTSConsumerType : JobInvoicingConsumerType
	{
		public CustomsTransitNCTSConsumerType(string code, MultilingualString description) : base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.Customs.EU.NctsMovementController;

		public override Type BizoType => ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;
	}
}
