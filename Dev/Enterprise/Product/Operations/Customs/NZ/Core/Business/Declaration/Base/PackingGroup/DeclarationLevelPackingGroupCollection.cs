using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class DeclarationLevelPackingGroupCollection : BaseDeclarationLevelPackingGroupCollection
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
