using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderBillValidation : Customs.Business.CusCodeDataValidation
	{
		public DeliveryOrderBillValidation(DeliveryOrderBill bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckCY_Code()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.CY_CodeInfo, "Bill Type");
			ListValidation.WarnIfInvalidCode(Parent.CY_CodeInfo, Parent.Lookups.CY_CodeList, "Bill Type");
		}

		protected override void CheckCY_Data()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.CY_DataInfo);
		}
	}
}
