using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	public static class WhsPickByLabelHelperForTesting
	{
		#region CreateWhsPickByLabelJob

		public static WhsPickByLabelJob CreateWhsPickByLabelJob(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid ddlPK, ZString rfUser)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rfUser, nameof(rfUser));

			var pickByLabelJob = factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = warehousePK;
			pickByLabelJob.WTK_WL_DockDoor = ddlPK;
			pickByLabelJob.WTK_GS_NKAssignedTo = rfUser;
			return pickByLabelJob;
		}

		#endregion

		#region AddNewPackageForJob

		public static WhsPickByLabelLabel AddNewPackageForJob(WhsPickByLabelJob pickByLabelJob, ZGuid packagePK)
		{
			Argument.NotNull(pickByLabelJob, nameof(pickByLabelJob));

			var label = pickByLabelJob.Labels.AddNew();
			label.WTL_KP_Package = packagePK;
			return label;
		}

		#endregion

		#region PackageHasActivePickByLabel

		public static WhsPickByLabelLabel GetPackageHasActivePickByLabel(PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));

			var queryJob = new ZDBOnlySubQuery(typeof(WhsPickByLabelJob), WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob);
			queryJob.AddToFilter(WhsPickByLabelJobSchema.WTK_FinalisedDate, null);
			var query = new ZDBOnlyQuery(typeof(WhsPickByLabelLabel));
			query.AddToFilter(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK);
			query.AddSubQuery(queryJob, JoinCondition.And);
			return package.Factory.LoadTop1<WhsPickByLabelLabel>(query);
		}

		#endregion

		#region GetOrCreatePickByLabelJob

		public static WhsPickByLabelJob GetOrCreatePickByLabelJob(BusinessObjectFactory factory, ZString rfUser, ZGuid warehousePK, ZGuid ddlPK)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rfUser, nameof(rfUser));

			return factory.LoadTop1<WhsPickByLabelJob>(GetActiveWhsPickByLabelJobQuery(warehousePK, rfUser)) ?? CreateWhsPickByLabelJob(factory, warehousePK, ddlPK, rfUser);
		}

		#endregion

		#region Implementation

		#region GetActiveWhsPickByLabelJobQuery

		static ZQuery GetActiveWhsPickByLabelJobQuery(ZGuid warehousePK, ZString userNK)
		{
			var query = new ZQuery(WhsPickByLabelJobSchema.WTK_WW_Warehouse, warehousePK);
			query.AddToFilter(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, userNK);
			query.AddToFilter(WhsPickByLabelJobSchema.WTK_FinalisedDate, null);
			return query;
		}

		#endregion

		#endregion
	}
}
