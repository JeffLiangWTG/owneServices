using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusDispositionCollection : Customs.Business.CusDispositionCollection
	{
		public CusDispositionCollection(ICusDispositionParent cusDispositionParent) : base(cusDispositionParent)
		{
		}

		public new CusDisposition AddNew() => (CusDisposition)base.AddNew();

		public new CusDisposition this[int index] => (CusDisposition)base[index];
	}
}
