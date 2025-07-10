using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	/// <summary>
	/// Summary description for OrgSupplierFilterBusinessObject.
	/// </summary>
	public class OrgSupplierFilterBusinessObject : OrganisationFilterBusinessObject
	{
		public OrgSupplierFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(StartsWithPanelFilter);
				query.AddToFilter(DetailsFilterControlFilter);

				if (OH_RelatedConsign.IsValid)
				{
					ZDBOnlyQuery supplierQuery = GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OH_RelatedConsign, SQLComparisonOperator.Equal);
					query.AddToFilter(supplierQuery, JoinCondition.And);
					query.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsActive, ZBool.True);
				}

				return query;
			}
		}
	}
}
