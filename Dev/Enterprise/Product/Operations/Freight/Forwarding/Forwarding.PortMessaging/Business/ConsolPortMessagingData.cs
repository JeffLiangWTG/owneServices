using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ConsolPortMessagingData : PortMessagingData
	{
		public ConsolPortMessagingData(ForwardingConsol consol)
			: base(consol)
		{
		}

		#region Log Parent

		protected override IStmALogParent LogParent
		{
			get { return Consol; }
		}

		#endregion

		protected override bool CheckPortMessaging(Func<IPortMessaging, bool> predicate)
		{
			return PortMessagingHelper.CheckPortMessagingForConsol(Consol, predicate);
		}
	}
}
