namespace Enterprise.Customs.US.Business
{
	public class DeclarationLevelPackingGroupCollection : Customs.Business.BaseDeclarationLevelPackingGroupCollection
	{
		public DeclarationLevelPackingGroupCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

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
