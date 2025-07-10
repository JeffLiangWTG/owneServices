namespace Enterprise.MasterFiles.Integration
{
	public interface IEDocDeliveryAuthorization
	{
		bool IsDocumentViewEnabled(IeDocBase doc);
		bool IsDocumentDeliveryDisclaimerRequired(IeDocBase doc);
		string DeliveryDisclaimerMessage { get; }
	}
}
