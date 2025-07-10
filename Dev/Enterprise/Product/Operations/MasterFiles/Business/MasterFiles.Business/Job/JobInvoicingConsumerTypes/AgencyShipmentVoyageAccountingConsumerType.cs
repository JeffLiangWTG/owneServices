using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgencyShipmentVoyageAccountingConsumerType : AgencyShipmentConsumerType
	{
		public AgencyShipmentVoyageAccountingConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyVoyageAccounting; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Agency.IVoyageAccount>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}
	}
}
