using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface IHoldWriterProvider
	{
		ITopLevelDataObjectWriter GetLastHoldWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType);
	}
}
