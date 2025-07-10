using System;
using CargoWise.Application;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CYDTransportationUnitConsumerType : JobInvoicingConsumerType
	{
		public CYDTransportationUnitConsumerType(string code, MultilingualString description)
		: base(code, description)
		{
		}

		public override ControllerID ControllerID => ControllerIDs.CYDTransportationUnit;

		public override Type BizoType => ObjectFactory.GetType<ICYDTransportationUnit>();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceContainerYard;
	}
}
