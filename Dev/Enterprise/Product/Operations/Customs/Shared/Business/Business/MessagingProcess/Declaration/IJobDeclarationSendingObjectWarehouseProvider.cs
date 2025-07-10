namespace Enterprise.Customs.Business.MessagingProcess.Declaration
{
	public interface IJobDeclarationSendingObjectWarehouseProvider
	{
		WarehouseExtensions.MessageAction GetMessageAction();
		bool ShouldProcessWarehouse { get; }
	}
}
