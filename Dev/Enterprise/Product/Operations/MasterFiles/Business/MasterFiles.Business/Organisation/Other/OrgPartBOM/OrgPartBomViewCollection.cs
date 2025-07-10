using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartBomViewCollection : NonPersistentBusinessObjectCollection<OrgPartBomView>
	{
		public OrgPartBomViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		OrgPartBOM root;
		uint rootLastChangedNumber;
		const int treeDepth = 4;

		public void Reload(OrgPartBOM root)
		{
			if (this.root != root || (root != null && root.LastChangeNumber != rootLastChangedNumber))
			{
				this.root = root;
				if (root != null)
				{
					rootLastChangedNumber = root.LastChangeNumber;
				}
				RemoveAll();
				Load();
			}
		}

		public override void Load()
		{
			if (root != null)
			{
				AddSubTree(root, 1);
			}
		}

		void AddSubTree(OrgPartBOM topLevel, int level)
		{
			if (topLevel.Component != null)
			{
				if (level <= treeDepth)
				{
					foreach (OrgPartBOM bom in topLevel.Component.BillOfMaterials)
					{
						Add(new OrgPartBomView(bom, level));
						AddSubTree(bom, level + 1);
					}
				}
			}
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgPartBomView(root, 0);
		}
	}
}
