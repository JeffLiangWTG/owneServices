using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class ForwardingConsolScheduleRelatedJobType : ScheduleRelatedJobType
	{
		public ForwardingConsolScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobConsol; }
		}

		public override Type BizOType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(); }
		}
	}
}
