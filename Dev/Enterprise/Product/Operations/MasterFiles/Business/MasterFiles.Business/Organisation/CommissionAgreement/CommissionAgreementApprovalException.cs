using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class CommissionAgreementApprovalException : Exception
	{
		public CommissionAgreementApprovalException(string message, string userFriendlyMessage) : base(message)
		{
			UserFriendlyMessage = userFriendlyMessage;
		}

#if NETFRAMEWORK
		protected CommissionAgreementApprovalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			UserFriendlyMessage = info.GetString(nameof(UserFriendlyMessage));
		}

		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(UserFriendlyMessage), UserFriendlyMessage);
		}
#endif

		/// <summary>
		/// The message for the user interface.
		/// </summary>
		public ZString UserFriendlyMessage { get; }
	}
}
