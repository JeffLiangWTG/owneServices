namespace Enterprise.Freight.Agency.Business
{
	public interface ICustomsMessageStatusProvider
	{
		bool ShouldShow(BillOfLading billOfLading);
		string GetCustomsStatus(BillOfLading billOfLading);
		string GetMessageStatus(BillOfLading billOfLading);
		string GetUserFriendlyStatusMessage(BillOfLading billOfLading);
	}
}
