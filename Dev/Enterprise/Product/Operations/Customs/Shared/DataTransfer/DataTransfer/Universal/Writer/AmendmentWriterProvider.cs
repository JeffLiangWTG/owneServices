using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AmendmentWriterProvider : IAmendmentWriterProvider
	{
		#region IAmendmentWriterProvider Members

		public ITopLevelDataObjectWriter GetPreAmendmentWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			return new PreAmendmentWriter(manager, recipientRoleType);
		}

		public ITopLevelDataObjectWriter GetPreviousClearedWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			return new PreviousClearedWriter(recipientRoleType);
		}

		#endregion
	}
}
