using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Customs
{
	public class ProcessTaskCollection<T, U> : ProcessTaskCollection where T : ProcessTask where U : BusinessObject
	{
		public ProcessTaskCollection(U parent)
			: base(parent) { }

		public new T this[int index] => (T)Elements[index];

		public new T AddNew() => (T)base.AddNew();

		public new U Parent => (U)base.Parent;
	}
}
