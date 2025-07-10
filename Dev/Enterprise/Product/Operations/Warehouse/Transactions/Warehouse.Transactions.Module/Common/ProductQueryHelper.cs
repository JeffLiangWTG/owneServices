using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public static class ProductQueryHelper
	{
		public static void AddProductToQuery(BusinessObjectFactory factory, ZDBOnlyQuery query, SchemaColumn fKColumn, ZGuid productPk)
		{
			var product = factory.Load<OrgSupplierPart>(productPk);
			var partNum = product?.OP_PartNum ?? ZString.Empty;

			var subQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), fKColumn);
			subQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNum);
			query.AddSubQuery(subQuery, JoinCondition.And);
		}
	}
}
