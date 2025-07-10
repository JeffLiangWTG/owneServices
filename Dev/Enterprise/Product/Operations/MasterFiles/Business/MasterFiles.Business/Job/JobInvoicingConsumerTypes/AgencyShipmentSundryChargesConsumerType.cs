using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgencyShipmentSundryChargesConsumerType : AgencyShipmentConsumerType
	{
		public AgencyShipmentSundryChargesConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencySundryCharges; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Agency.ISundryCharges>(); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}
	}
}
