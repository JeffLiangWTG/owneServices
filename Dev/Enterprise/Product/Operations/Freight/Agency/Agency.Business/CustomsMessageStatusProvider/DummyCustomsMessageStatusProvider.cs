using System;

namespace Enterprise.Freight.Agency.Business
{
	public class DummyCustomsMessageStatusProvider : ICustomsMessageStatusProvider
	{
		[System.Diagnostics.DebuggerStepThrough]
		DummyCustomsMessageStatusProvider()
		{
		}

		public static DummyCustomsMessageStatusProvider Instance
		{
			get
			{
				return instance ?? (instance = new DummyCustomsMessageStatusProvider());
			}
		}

		[ThreadStatic]
		static DummyCustomsMessageStatusProvider instance;
		public static void Reset()
		{
			instance = null;
		}

		public bool ShouldShow { get; set; }

		public string CustomsStatus { get; set; }

		public string MessageStatus { get; set; }

		public string UserFriendlyStatusMessage { get; set; }

		#region ICustomsMessageStatusProvider Members
		bool ICustomsMessageStatusProvider.ShouldShow(BillOfLading billOfLading)
		{
			return ShouldShow;
		}

		string ICustomsMessageStatusProvider.GetCustomsStatus(BillOfLading billOfLading)
		{
			return CustomsStatus;
		}

		string ICustomsMessageStatusProvider.GetMessageStatus(BillOfLading billOfLading)
		{
			return MessageStatus;
		}

		string ICustomsMessageStatusProvider.GetUserFriendlyStatusMessage(BillOfLading billOfLading)
		{
			return UserFriendlyStatusMessage;
		}
		#endregion
	}
}
