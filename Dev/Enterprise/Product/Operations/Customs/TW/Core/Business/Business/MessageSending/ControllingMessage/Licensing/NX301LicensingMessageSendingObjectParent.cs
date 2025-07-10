namespace Enterprise.Customs.TW.Business
{
	public class NX301LicensingMessageSendingObjectParent : LicensingMessageSendingObjectParent
	{
		public NX301LicensingMessageSendingObjectParent(JobDeclaration declaration)
			: base(declaration, ControllingMessageTypeList.Codes.NX301)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader)
		{
			return new NX301MessageSendingObject(messageHeader);
		}
	}
}
