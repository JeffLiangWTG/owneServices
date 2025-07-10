namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ABLEntryNumCollection : ASYCUDA.Business.ABLEntryNumCollection
	{
		public ABLEntryNumCollection(AsycudaBill master)
			: base(master)
		{
		}

		public new ABLEntryNum this[int index] => (ABLEntryNum)base[index];

		public new ABLEntryNum AddNew()
		{
			return (ABLEntryNum)base.AddNew();
		}
	}
}
