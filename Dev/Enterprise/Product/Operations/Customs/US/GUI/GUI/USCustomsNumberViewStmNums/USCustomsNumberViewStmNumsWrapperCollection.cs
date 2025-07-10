using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.GUI
{
	public class USCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsCompanyWrapperCollection
	{
		public USCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection)
			: base(collection)
		{ }

		public new USCustomsNumberViewStmNumsWrapper this[int i] => (USCustomsNumberViewStmNumsWrapper)base[i];

		public new USCustomsNumberViewStmNumsWrapper AddNew() => (USCustomsNumberViewStmNumsWrapper)base.AddNew();
	}
}
