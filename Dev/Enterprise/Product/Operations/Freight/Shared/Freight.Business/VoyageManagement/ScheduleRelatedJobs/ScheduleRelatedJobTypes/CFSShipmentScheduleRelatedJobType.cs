using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class CFSShipmentScheduleRelatedJobType : ScheduleRelatedJobType
	{
		public CFSShipmentScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}

		public override Type BizOType
		{
			get { return ObjectFactory.GetType<Integration.CFS.ICFSShipment>(); }
		}
	}
}
