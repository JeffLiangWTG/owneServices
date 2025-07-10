//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSupportingInfoValidation
//
//    This class should be used for overriding validation in AutoCusSupportingInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusSupportingInfoValidation : AutoCusSupportingInfoValidation
	{
		public CusSupportingInfoValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			MandatoryValidation.CheckNotNegative(Parent.CSI_LineNoInfo);
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			MandatoryValidation.CheckNotNegative(Parent.CSI_QuantityInfo);
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			MandatoryValidation.CheckNotNegative(Parent.CSI_Quantity2Info);
		}

		protected override void CheckCSI_Quantity3()
		{
			base.CheckCSI_Quantity3();
			MandatoryValidation.CheckNotNegative(Parent.CSI_Quantity3Info);
		}

		protected override void CheckCSI_PackQty()
		{
			base.CheckCSI_PackQty();
			MandatoryValidation.CheckNotNegative(Parent.CSI_PackQtyInfo);
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			MandatoryValidation.CheckNotNegative(Parent.CSI_ItemNumberInfo);
		}
	}
}
