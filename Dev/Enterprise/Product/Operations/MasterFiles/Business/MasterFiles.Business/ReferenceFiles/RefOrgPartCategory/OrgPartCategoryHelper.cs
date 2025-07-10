using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartProductCategoryHelper
	{
		public OrgPartProductCategoryHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region ProductCategoryAndSubCategories

		public List<ZGuid> ProductCategoryAndSubCategories(ZGuid categoryGuid)
		{
			var list = new List<ZGuid>();

			if (!categoryGuid.IsEmpty)
			{
				var collection = new DynamicBusinessObjectCollection(Factory);

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@CategoryGuid", categoryGuid, OrgPartCategorySchema.PK);
				collection.Load(@"SELECT CategoryPK FROM WhsProductCategoryAndChildCategories(@CategoryGuid)", sqlParams);
				if (collection.Count > 0)
				{
					foreach (DynamicBusinessObject obj in collection)
					{
						list.Add((ZGuid)obj["CategoryPK"]);
					}
				}
			}

			return list;
		}

		#endregion

		#region IsCategroyASubCategoryOfCurrentCategory

		public bool IsCategroyASubCategoryOfCurrentCategory(OrgPartCategory category, OrgPartCategory currentCategory)
		{
			return category.PK == currentCategory.PK || (category.Parent != null) && IsCategroyASubCategoryOfCurrentCategory(category.Parent, currentCategory);
		}

		#endregion
	}
}
