using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsInventoryCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public TrackingWhsInventoryCollectionFetchStrategy(TrackingWhsInventoryCollection collection) : base(collection) { }

		public void AddFetchHints(TrackingWhsInventoryCollection collection)
		{
			var factory = collection.Factory;
			var inventories = collection.Cast<TrackingWhsInventory>();
			var supplierPartPKs = inventories.Select(x => x.WI_OP).Distinct();
			factory.AddFetchHint(OrgSupplierPartSchema.Instance, new ZQuery(OrgSupplierPartSchema.PK, supplierPartPKs));
			factory.AddFetchHint(OrgPartRelationSchema.Instance, new ZQuery(OrgPartRelationSchema.OU_OP, supplierPartPKs));

			var locationPKs = inventories.Select(x => x.WI_WL).Distinct();
			factory.AddFetchHint(WhsLocationViewSchema.Instance, new ZQuery(WhsLocationViewSchema.PK, locationPKs));

			var docketLinePKs = inventories.Select(x => x.WI_WE_InDocketLine).Distinct();
			var ratingDocketLinePKs = inventories.Select(x => x.WI_WE_OriginalInDocketLineForRating).Distinct();
			var docketLineQuery = GetTVPQuery(WhsDocketLineSchema.PK, docketLinePKs.Union(ratingDocketLinePKs));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, docketLineQuery); // Fetch Hint may not be necessary

			var docketPKs = inventories.Select(x => x.WI_WD).Distinct();
			var docketQuery = GetTVPQuery(WhsDocketSchema.PK, docketPKs);
			factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);

			var ratingDocketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			var ratingDocketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD) { AllowTableValuedParameters = true };
			ratingDocketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, ratingDocketLinePKs);
			ratingDocketQuery.AddSubQuery(WhsDocketSchema.PK, ratingDocketLineSubQuery, JoinCondition.And);
			factory.AddFetchHint(WhsDocketSchema.Instance, ratingDocketQuery);

			var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgPartRelationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OH);
			orgPartRelationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OP, supplierPartPKs);
			orgQuery.AddSubQuery(OrgHeaderSchema.PK, orgPartRelationSubQuery, JoinCondition.And);
			factory.AddFetchHint(OrgHeaderSchema.Instance, orgQuery);

			var orgMiscServQuery = new ZDBOnlyQuery(typeof(OrgMiscServ));
			orgMiscServQuery.AddSubQuery(OrgMiscServSchema.OM_OH, OrgPartRelationSchema.OU_OH, orgPartRelationSubQuery, JoinCondition.And);
			factory.AddFetchHint(OrgMiscServSchema.Instance, orgMiscServQuery);

			var stmNoteQuery = GetTVPQuery(StmNoteSchema.ST_ParentID, docketLinePKs);
			factory.AddFetchHint(StmNoteSchema.Instance, stmNoteQuery);

			var pickLineQuery = GetTVPQuery(WhsPickLineSchema.WZ_WE_InventoryLine, docketLinePKs);
			factory.AddFetchHint(WhsPickLineSchema.Instance, pickLineQuery);
		}

		static ZQuery GetTVPQuery<T>(SchemaColumn column, IEnumerable<T> values)
		{
			var query = new ZQuery { AllowTableValuedParameters = true };
			query.AddToFilter(column, values);
			return query;
		}
	}
}
