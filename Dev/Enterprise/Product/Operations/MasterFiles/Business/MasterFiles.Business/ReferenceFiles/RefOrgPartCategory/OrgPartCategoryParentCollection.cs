using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartCategoryParentCollection : OrgPartCategoryCollection
	{
		#region Constructors

		public OrgPartCategoryParentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			CategoryHelper = new OrgPartProductCategoryHelper(factory);
		}

		public OrgPartCategoryParentCollection(BusinessObjectFactory factory, OrgPartCategory category)
			: base(factory)
		{
			CategoryHelper = new OrgPartProductCategoryHelper(factory);
			CurrentCategory = category;
			AdditionalFilter = GetParentCategoryAdditionalQuery();
		}

		readonly OrgPartCategory CurrentCategory;
		readonly OrgPartProductCategoryHelper CategoryHelper;

		#endregion

		#region GetParentCategoryAdditionalQuery

		ZQuery GetParentCategoryAdditionalQuery()
		{
			var additionalFilterQuery = new ZDBOnlyQuery(typeof(OrgPartCategory));
			if (CurrentCategory != null)
			{
				var allPKs = CategoryHelper.ProductCategoryAndSubCategories(CurrentCategory.PK);
				foreach (ZGuid guidToExclude in allPKs)
				{
					additionalFilterQuery.AddToFilter(OrgPartCategorySchema.PK, SQLComparisonOperator.NotEqual, guidToExclude);
				}
			}

			return additionalFilterQuery;
		}

		#endregion

		#region Overrides

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var category = (OrgPartCategory)selectedBusinessObject;

			if (CurrentCategory != null && CategoryHelper.IsCategroyASubCategoryOfCurrentCategory(category, CurrentCategory))
			{
				errors.Add(CategoryCannotBeSelectedError);
			}
		}

		public static string CategoryCannotBeSelectedError
		{
			get { return ResString.GetMultilingualString("0E502A5B-FE72-4694-935A-19C37DB75599", "This category is a sub category of current category, cannot be selected."); }
		}

		#endregion
	}
}
