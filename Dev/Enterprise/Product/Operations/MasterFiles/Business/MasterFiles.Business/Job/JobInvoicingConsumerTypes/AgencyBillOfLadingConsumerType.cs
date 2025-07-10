using System;
using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgencyBillOfLadingConsumerType : AgencyConsumerType
	{
		public AgencyBillOfLadingConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyBillOfLading; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Agency.IBillOfLading>(); }
		}
	}
}
