using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	static class ProductFilterReValidator
	{
		public static void RevalidateProductFilter(ModuleGuidFilter clientFilter, ModuleGuidFilter productFilter, BusinessObjectFactory factory)
		{
			Argument.NotNull(clientFilter, "Client Filter", "Client Filter should not be null.");
			Argument.NotNull(productFilter, "Product Filter", "Product Filter should not be null.");

			if (productFilter.Property.IsMissing && clientFilter.Property.IsValid)
			{
				var partNum = FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name);
				if (!string.IsNullOrEmpty(partNum))
				{
					var relatedOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
					relatedOrgSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, clientFilter.Property);

					var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
					productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNum);
					productQuery.AddSubQuery(relatedOrgSubQuery, JoinCondition.And);

					var part = factory.LoadTop1<OrgSupplierPart>(productQuery);
					if (part != null)
					{
						FieldInvalidTextMemory.SetInvalidText(productFilter, productFilter.PropertyInfo.Name, ""); // clear old error
						productFilter.Property = part.PK; // reasigning value will revalidate the field.
					}
				}
			}
			else if (!productFilter.Property.IsEmpty)
			{
				productFilter.Validation.ValidateAll();
				productFilter.PropertyInfo.RefreshBinding();
			}
		}
	}
}
