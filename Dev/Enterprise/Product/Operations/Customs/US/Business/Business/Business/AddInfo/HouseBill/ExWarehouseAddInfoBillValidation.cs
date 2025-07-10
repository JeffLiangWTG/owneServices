namespace Enterprise.Customs.US.Business
{
	public class ExWarehouseAddInfoBillValidation : FormalImportAddInfoBillValidation
	{
		public ExWarehouseAddInfoBillValidation(AddInfoBill addInfoBill)
			: base(addInfoBill)
		{
		}

		protected override bool ShouldValidateUS_UI_NKBillIssuerSCAC => false;
	}
}
