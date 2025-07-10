//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVItemValidation
//
//    This class should be used for overriding validation in AutoCusUSLVItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemValidation : AutoCusUSLVItemValidation
	{
		public CusUSLVItemValidation(AutoCusUSLVItem parent) : base(parent)
		{
		}

		public new CusUSLVItem Parent => (CusUSLVItem)base.Parent;

		#region ULI_RX_NKCurrency

		protected override void CheckULI_RX_NKCurrency()
		{
			base.CheckULI_RX_NKCurrency();
			CusUSLVItemValidationHelper.CheckCurrency(Parent, Parent.ULI_RX_NKCurrencyInfo);
		}

		#endregion

		#region ULI_Tariff

		protected override void CheckULI_Tariff()
		{
			base.CheckULI_Tariff();
			CusUSLVItemValidationHelper.CheckTariff(Parent, Parent.ULI_TariffInfo);
		}

		#endregion

		#region ULI_RN_NKCountryOfOrigin

		protected override void CheckULI_RN_NKCountryOfOrigin()
		{
			base.CheckULI_RN_NKCountryOfOrigin();
			CusUSLVItemValidationHelper.CheckCountryOfOrigin(Parent, Parent.ULI_RN_NKCountryOfOriginInfo);
		}

		#endregion

		#region ULI_GoodsValue

		protected override void CheckULI_GoodsValue()
		{
			base.CheckULI_GoodsValue();
			CusUSLVItemValidationHelper.CheckLineValue(Parent, Parent.ULI_GoodsValueInfo);
		}

		#endregion

		#region ULI_GoodsDescription
		protected override void CheckULI_GoodsDescription()
		{
			base.CheckULI_GoodsDescription();
			CusUSLVItemValidationHelper.CheckGoodsDescription(Parent, Parent.ULI_GoodsDescriptionInfo);
		}

		#endregion

		protected override void CheckULI_AntiDumping()
		{
			base.CheckULI_AntiDumping();
			CusUSLVItemValidationHelper.CheckAntiDumping(Parent, Parent.ULI_AntiDumpingInfo, Parent.ULI_AntiDumping);
		}

		protected override void CheckULI_Countervailing()
		{
			base.CheckULI_Countervailing();
			CusUSLVItemValidationHelper.CheckCountervailing(Parent, Parent.ULI_CountervailingInfo, Parent.ULI_Countervailing);
		}

		protected override void CheckULI_PartNo()
		{
			base.CheckULI_PartNo();
			CusUSLVItemValidationHelper.CheckProductCode(Parent, Parent.ULI_PartNoInfo);
		}
	}
}
