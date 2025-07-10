namespace Enterprise.Customs.TW.Business
{
	public class NCATKMessageSendingObject : ControllingMessageSendingObject
	{
		public NCATKMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ControllingMessageSendingObjectValidation GetNewValidation()
		{
			return new NCATKMessageSendingObjectValidation(this);
		}

		public new NCATKMessageSendingObjectValidation Validation => base.Validation as NCATKMessageSendingObjectValidation;
	}
}
