namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsCompanyWrapperCollection : CustomsNumberViewStmNumsWrapperCollection
	{
		public CustomsNumberViewStmNumsCompanyWrapperCollection(CustomsNumberViewStmNumsCollection collection) : base(collection)
		{
		}

		public new CustomsNumberViewStmNumsCompanyWrapper this[int i] => (CustomsNumberViewStmNumsCompanyWrapper)base[i];

		public new CustomsNumberViewStmNumsCompanyWrapper AddNew() => (CustomsNumberViewStmNumsCompanyWrapper)base.AddNew();
	}
}
