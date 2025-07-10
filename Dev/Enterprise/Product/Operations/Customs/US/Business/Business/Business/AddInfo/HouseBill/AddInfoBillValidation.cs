namespace Enterprise.Customs.US.Business
{
	public class AddInfoBillValidation : USAddInfoValidation
	{
		public AddInfoBillValidation(AddInfoBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		protected new AddInfoBill Parent => (AddInfoBill)base.Parent;
	}
}
