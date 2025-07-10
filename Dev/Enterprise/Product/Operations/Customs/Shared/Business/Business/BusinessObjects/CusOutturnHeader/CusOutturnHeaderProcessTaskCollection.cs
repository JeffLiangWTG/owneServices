using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public CusOutturnHeaderProcessTaskCollection(CusOutturnHeader header)
			: base(header)
		{
		}

		public new CusOutturnHeader Parent => (CusOutturnHeader)base.Parent;
		public new CusOutturnHeaderProcessTask this[int index] => (CusOutturnHeaderProcessTask)Elements[index];
		public new CusOutturnHeaderProcessTask AddNew() => (CusOutturnHeaderProcessTask)base.AddNew();
	}
}
