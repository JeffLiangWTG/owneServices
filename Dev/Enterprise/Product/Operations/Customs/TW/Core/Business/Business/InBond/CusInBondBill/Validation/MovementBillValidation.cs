namespace Enterprise.Customs.TW.Business
{
	public class MovementBillValidation : CusInBondBillValidation
	{
		public MovementBillValidation(CusInBondBill parent)
			: base(parent)
		{
		}

		protected override void CheckB0_ReferenceID()
		{
			if (IsTransportModeSea)
			{
				base.CheckB0_ReferenceID();
				var referenceID = Parent.B0_ReferenceID;
				var targetInfo = Parent.B0_ReferenceIDInfo;

				if (!referenceID.IsEmpty)
				{
					if (referenceID.Length != 4)
					{
						targetInfo.AddMessageError(ValidationConstants.CusInBondBill.LengthForSoNo);
					}

					if (!referenceID.IsLettersAndNumbersOnlyOrEmpty)
					{
						targetInfo.AddMessageError(ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForSoNo);
					}
				}
			}
		}
	}
}
