using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CYDAdHocServiceOrderConsumerType : JobInvoicingConsumerType
	{
		public CYDAdHocServiceOrderConsumerType(string code, MultilingualString description)
		: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.CYDAdHocServiceOrder;

		public override Type BizoType => ObjectFactory.GetType<ICYDAdHocServiceOrder>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceContainerYard;
	}
}
