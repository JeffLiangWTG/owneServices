using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface IAmendmentWriterProvider
	{
		ITopLevelDataObjectWriter GetPreAmendmentWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType);
		ITopLevelDataObjectWriter GetPreviousClearedWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType);
	}
}
