// -----------------------------------------------------------------------
// <copyright file="FormalEntryStatusListPartial.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business
{
	partial class FormalEntryStatusList : Integration.Customs.NZ.IFormalEntryStatusList
	{
		public static bool IsDeliveryStatusCleared(string code)
		{
			switch (code)
			{
				case FormalEntryStatusList.Codes.DeliveryOnPayment:
				case FormalEntryStatusList.Codes.DeliveryOrderReceived:
				case FormalEntryStatusList.Codes.DOSentToRecipient:
					return true;
				default:
					return false;
			}
		}
	}
}
