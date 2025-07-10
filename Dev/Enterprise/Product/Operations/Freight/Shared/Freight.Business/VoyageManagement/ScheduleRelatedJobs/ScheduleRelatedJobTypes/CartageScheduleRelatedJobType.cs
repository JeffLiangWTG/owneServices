using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class CartageScheduleRelatedJobType : ScheduleRelatedJobType
	{
		public CartageScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Cartage; }
		}

		public override Type BizOType
		{
			get { return ObjectFactory.GetType<LocalCartage.Integration.ICommonCartage>(); }
		}
	}
}
