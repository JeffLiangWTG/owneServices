using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	class CYDReceiveAdviceConsumerType : JobInvoicingConsumerType
	{
		public CYDReceiveAdviceConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.CYDReceiveAdvice;

		public override Type BizoType => ObjectFactory.GetType<ICYDReceiveAdvice>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceContainerYard;
	}
}
