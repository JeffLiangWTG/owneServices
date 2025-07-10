using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public delegate void QueryUserEventHandler(object sender, QueryUserEventArgs e);
	public class TestNotificationBuffer : NotificationBufferWithDefaultResponse
	{
		public TestNotificationBuffer()
			: this(true)
		{
		}

		public TestNotificationBuffer(bool defaultResponse)
			: base(defaultResponse)
		{
		}

		public QueryUserEventArgs LastQueryUserEventArgs;

		public event QueryUserEventHandler PreQueryUser
		{
			add { fPreQueryUser += value; }
			remove { fPreQueryUser -= value; }
		}

		event QueryUserEventHandler fPreQueryUser;

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			if (fPreQueryUser != null)
			{
				fPreQueryUser(this, (QueryUserEventArgs)e);
				if (e is QueryUserMsgBoxEventArgs)
				{
					DefaultResponse = ((QueryUserMsgBoxEventArgs)e).Response;
				}
			}

			LastQueryUserEventArgs = (QueryUserEventArgs)e;
			base.QueryUser(e);
		}

		public INotification LastEvent
		{
			get { return (Events.Length > 0) ? Events[Events.Length - 1] : null; }
		}
	}
}
