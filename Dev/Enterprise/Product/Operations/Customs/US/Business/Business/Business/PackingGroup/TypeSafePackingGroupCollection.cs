namespace Enterprise.Customs.US.Business
{
	partial class PackingGroupCollection : Customs.Business.BasePackingGroupCollection
	{
		public new PackingGroup this[int index]
		{
			get { return (PackingGroup)base[index]; }
		}

		public new PackingGroup AddNew()
		{
			return (PackingGroup)base.AddNew();
		}
	}
}
