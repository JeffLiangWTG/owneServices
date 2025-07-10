using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.GUI
{
	public class TWCustomsNumberViewStmNumsWrapperCollection : CustomsNumberViewStmNumsCompanyWrapperCollection
	{
		public TWCustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection)
			: base(collection)
		{ }

		public new TWCustomsNumberViewStmNumsWrapper this[int i] => (TWCustomsNumberViewStmNumsWrapper)base[i];

		public new TWCustomsNumberViewStmNumsWrapper AddNew() => (TWCustomsNumberViewStmNumsWrapper)base.AddNew();
	}
}
