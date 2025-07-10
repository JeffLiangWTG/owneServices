using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class LastDataEntryNumberWriterProvider : ILastDataEntryNumberWriterProvider
	{
		#region LastDataEntryNumberWriterProvider Members

		public ITopLevelDataObjectWriter GetLastDataWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			return new LastDataEntryNumberWriter(manager, recipientRoleType);
		}

		#endregion
	}
}
