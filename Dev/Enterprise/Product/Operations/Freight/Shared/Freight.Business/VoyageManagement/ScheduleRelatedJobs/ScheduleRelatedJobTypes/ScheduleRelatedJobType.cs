using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ImmutableObject(true)]
	public abstract class ScheduleRelatedJobType : CodeDescriptionPair
	{
		protected ScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public abstract ControllerID ControllerID { get; }

		public abstract Type BizOType { get; }
	}
}
