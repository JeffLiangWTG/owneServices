using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.Business.Testing
{
	public class TestNotificationBuffer : NotificationBuffer
	{
		public bool Response;

		public INotification LastEvent
		{
			get { return Events.LastOrDefault(); }
		}

		public QueryUserMsgBoxEventArgs LastQuery
		{
			get;
			private set;
		}

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			base.QueryUser(e);

			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Response;

			LastQuery = args;
		}
	}
}
