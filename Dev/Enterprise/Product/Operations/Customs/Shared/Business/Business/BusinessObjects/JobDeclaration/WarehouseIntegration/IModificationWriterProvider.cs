using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface IModificationWriterProvider
	{
		ITopLevelDataObjectWriter GetLastHoldWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType);
	}
}
