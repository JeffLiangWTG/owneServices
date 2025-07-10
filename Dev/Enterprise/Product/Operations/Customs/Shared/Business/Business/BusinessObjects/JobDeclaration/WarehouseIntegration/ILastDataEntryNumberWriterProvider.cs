using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface ILastDataEntryNumberWriterProvider
	{
		ITopLevelDataObjectWriter GetLastDataWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType);
	}
}
