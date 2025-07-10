using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class ModificationWriterProvider : IModificationWriterProvider
	{
		public ITopLevelDataObjectWriter GetLastHoldWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			return new LastModificationWriter(manager, recipientRoleType);
		}
	}
}
