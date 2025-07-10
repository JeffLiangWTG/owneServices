using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class HoldWriterProvider : IHoldWriterProvider
	{
		#region IHoldWriterProvider Members

		public ITopLevelDataObjectWriter GetLastHoldWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			return new LastHoldWriter(manager, recipientRoleType);
		}

		#endregion
	}
}
