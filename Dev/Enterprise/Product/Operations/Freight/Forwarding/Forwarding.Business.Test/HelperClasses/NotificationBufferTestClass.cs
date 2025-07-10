using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class NotificationBufferTestClass : NotificationBuffer
	{
		public bool AllowToSaveWithErrors
		{
			set
			{
				allowToSaveWithErrors = value;
			}
		}
		bool allowToSaveWithErrors = true;

		public string LastQueryUserMessage { get; set; }

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			base.QueryUser(e);
			if (allowToSaveWithErrors && e is QueryUserYesNoEventArgs)
			{
				((QueryUserYesNoEventArgs)e).Response = true;
			}

			QueryUserMsgBoxEventArgs queryArgs = e as QueryUserMsgBoxEventArgs;
			if (queryArgs != null)
			{
				LastQueryUserMessage = queryArgs.Message;
			}
		}
	}
}
