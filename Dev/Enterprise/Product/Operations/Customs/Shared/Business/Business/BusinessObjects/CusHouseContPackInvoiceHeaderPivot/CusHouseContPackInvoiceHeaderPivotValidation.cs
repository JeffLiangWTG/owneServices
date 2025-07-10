//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusHouseContPackInvoiceHeaderPivotValidation
//
//    This class should be used for overriding validation in AutoCusHouseContPackInvoiceHeaderPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusHouseContPackInvoiceHeaderPivotValidation : AutoCusHouseContPackInvoiceHeaderPivotValidation
	{
		public CusHouseContPackInvoiceHeaderPivotValidation(AutoCusHouseContPackInvoiceHeaderPivot parent) : base(parent)
		{
		}

		protected override void CheckCHZ_NumberOfPacks()
		{
			base.CheckCHZ_NumberOfPacks();

			var pivot = Parent as ICusPackagePivot;
			var supporter = pivot?.PivotSupporter;

			if (supporter == null || !supporter.IsSupportEmptyPackType(pivot.Package))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CHZ_NumberOfPacksInfo);
			}
		}
	}
}
