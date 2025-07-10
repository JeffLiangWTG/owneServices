using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveLineItemValidation : Customs.Business.CusInBondMoveLineItemValidation
	{
		public CusInBondMoveLineItemValidation(CusInBondMoveLineItem parent)
			: base(parent)
		{
		}

		public new CusInBondMoveLineItem Parent => (CusInBondMoveLineItem)base.Parent;

		protected override void CheckBI_QuantityUQ()
		{
			base.CheckBI_QuantityUQ();
			var targetInfo = Parent.BI_QuantityUQInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.PackageTypeList);
			if (!Parent.BI_Quantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckBI_Quantity()
		{
			base.CheckBI_Quantity();
			var targetInfo = Parent.BI_QuantityInfo;
			if (!Parent.BI_QuantityUQ.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(targetInfo);
			}
			MandatoryValidation.MessageErrorIfIsNegative(targetInfo);
		}

		protected override void CheckBI_Description()
		{
			base.CheckBI_Description();
			if (Parent.MoveDetail?.MoveHeader?.Header?.IsTransportModeAir ?? ZBool.False)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BI_DescriptionInfo);
			}
		}

		protected override void CheckBI_PackagingDescription()
		{
			base.CheckBI_PackagingDescription();
			if (Parent.TW_IsCoPackaged)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BI_PackagingDescriptionInfo);
			}
		}
	}
}
