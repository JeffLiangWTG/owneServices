using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ArrivalBillValidation : CusInBondBillValidation
	{
		public ArrivalBillValidation(CusInBondBill parent)
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
						targetInfo.AddMessageError(ValidationConstants.CusInBondBill.LengthForMenifestNo);
					}

					if (!referenceID.IsLettersAndNumbersOnlyOrEmpty)
					{
						targetInfo.AddMessageError(ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForMenifestNo);
					}
				}
			}
		}

		protected override void CheckB0_Weight()
		{
			base.CheckB0_Weight();
			var targetInfo = Parent.B0_WeightInfo;
			if (IsTransportModeAir)
			{
				if (!Parent.B0_WeightUQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfIsZero(targetInfo);
				}
				MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
			}
		}

		protected override void CheckB0_WeightUQ()
		{
			base.CheckB0_WeightUQ();
			var targetInfo = Parent.B0_WeightUQInfo;
			if (IsTransportModeAir && !Parent.B0_Weight.IsEmpty && Parent.B0_WeightUQ.IsEmpty)
			{
				targetInfo.AddMessageError(ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);
			}
		}

		protected override void CheckB0_ManifestQty()
		{
			base.CheckB0_ManifestQty();
			var targetInfo = Parent.B0_ManifestQtyInfo;
			MandatoryValidation.MessageErrorIfIsZero(targetInfo);
			MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
		}

		protected override void CheckB0_ManifestUQ()
		{
			base.CheckB0_ManifestUQ();
			var targetInfo = Parent.B0_ManifestUQInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.ManifestUnits);
			if (!Parent.B0_ManifestQty.IsEmpty && Parent.B0_ManifestUQ.IsEmpty)
			{
				targetInfo.AddMessageError(ValidationConstants.CusInBondBill.ManifestQuantityUnitIsNotEmpty);
			}
		}

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_MasterBillNumberInfo);
		}
	}
}
