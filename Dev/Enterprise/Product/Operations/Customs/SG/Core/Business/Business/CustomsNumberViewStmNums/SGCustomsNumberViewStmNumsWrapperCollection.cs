using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsCompanyWrapperCollection
	{
		public SGCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection)
			: base(collection)
		{ }

		public new SGCustomsNumberViewStmNumsWrapper this[int i] => (SGCustomsNumberViewStmNumsWrapper)base[i];

		public new SGCustomsNumberViewStmNumsWrapper AddNew() => (SGCustomsNumberViewStmNumsWrapper)base.AddNew();
	}
}
