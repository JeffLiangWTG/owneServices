using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgBrandOrRelatedNameCollection : DependentBusinessObjectCollection<OrgBrandOrRelatedName, OrgHeader>
	{
		public OrgBrandOrRelatedNameCollection(OrgHeader parentHeader) : base(parentHeader)
		{
		}

		public OrgBrandOrRelatedName this[string brandName]
		{
			get { return FindBrandByName(brandName); }
		}

		#region Add New

		public OrgBrandOrRelatedName AddNew(string brandName)
		{
			var newBrand = AddNew();
			newBrand.P1_RelatedName = brandName;
			return newBrand;
		}

		#endregion

		#region Contains/Finding

		public bool Contains(string brandName)
		{
			return FindBrandByName(brandName) != null;
		}

		OrgBrandOrRelatedName FindBrandByName(string brandName)
		{
			OrgBrandOrRelatedName result = null;

			foreach (OrgBrandOrRelatedName brand in Elements)
			{
				if (brand.P1_RelatedName == brandName)
				{
					result = brand;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			if (!IsDeletingForDataRefresh)
			{
				MarkParentAsNeedingPatternMatchesRegen();
			}
			base.Remove(elementToRemove);
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			MarkParentAsNeedingPatternMatchesRegen();
			Master.FindDuplicates();
			base.RemoveAndDelete(elementToDelete);
		}

		void MarkParentAsNeedingPatternMatchesRegen()
		{
			OrgHeader parent = Master;
			if (parent != null)
			{
				parent.PatternMatchRequiresRegen = true;
			}
		}

		#endregion
	}
}
