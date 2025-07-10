using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public static class WhsPickByLabelHelper
	{
		#region AddPackageToListOfPickByLabelForUser

		public static WhsPickByLabelJob AddPackageToListOfPickByLabelForUser(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid ddlPK, ZString userNK, ZGuid packagePK)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrEmpty(userNK, nameof(userNK));
			ArgumentZGuidIsNotEmpty(warehousePK, nameof(warehousePK));
			ArgumentZGuidIsNotEmpty(ddlPK, nameof(ddlPK));

			var pickByLabelJob = GetOrCreatePickByLabelJob(factory, warehousePK, userNK, ddlPK);
			GetOrCreatePickByLabelLabel(pickByLabelJob, packagePK);
			return pickByLabelJob;
		}

		#endregion

		#region PickByLabelPackageFinder

		public static PackageFinder PickByLabelPackageFinder(BusinessObjectFactory factory, string packageID, ZGuid warehousePK, GlbStaff staff) => new PackageFinder(factory, packageID, warehousePK, staff);

		#endregion

		#region GetOrCreatePickByLabelJob

		public static WhsPickByLabelJob GetOrCreatePickByLabelJob(BusinessObjectFactory factory, ZGuid warehousePK, ZString rfUser, ZGuid ddlPK)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrEmpty(rfUser, nameof(rfUser));
			ArgumentZGuidIsNotEmpty(warehousePK, nameof(warehousePK));
			ArgumentZGuidIsNotEmpty(ddlPK, nameof(ddlPK));

			return GetWhsPickByLabelJob(factory, warehousePK, rfUser) ?? CreateWhsPickByLabelJob(factory, warehousePK, ddlPK, rfUser);
		}

		#endregion

		#region GetWhsPickByLabelJob

		public static WhsPickByLabelJob GetWhsPickByLabelJob(BusinessObjectFactory factory, ZGuid warehousePK, ZString rfUser)
		{
			return factory.LoadTop1<WhsPickByLabelJob>(GetActiveWhsPickByLabelJobQuery(warehousePK, rfUser));
		}

		#endregion

		#region GetOrCreatePickByLabelLabel

		public static WhsPickByLabelLabel GetOrCreatePickByLabelLabel(WhsPickByLabelJob pickByLabelJob, ZGuid packagePK)
		{
			Argument.NotNull(pickByLabelJob, nameof(pickByLabelJob));
			ArgumentZGuidIsNotEmpty(packagePK, nameof(packagePK));

			var label = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().FirstOrDefault(l => l.WTL_KP_Package.Equals(packagePK));
			if (label == null)
			{
				label = pickByLabelJob.Labels.AddNew();
				label.WTL_KP_Package = packagePK;
			}

			return label;
		}

		#endregion

		#region PutawayPickedLabelsAndSplitJobIfNecessary

		public static void PutawayPickedLabelsAndSplitJobIfNecessary(WhsPickByLabelJob pickByLabelJob)
		{
			CloseAndSplitJobIfNecessary(pickByLabelJob, l => l.IsPickedFromPutawayLocation);
		}

		#endregion

		#region CloseAndSplitNotPutawayLabelsIfNecessary

		public static WhsPickByLabelJob CloseAndSplitNotPutawayLabelsIfNecessary(WhsPickByLabelJob pickByLabelJob)
		{
			return CloseAndSplitJobIfNecessary(pickByLabelJob, l => l.IsPutawayIntoOutboundDDL);
		}

		static WhsPickByLabelJob CloseAndSplitJobIfNecessary(WhsPickByLabelJob pickByLabelJob, Func<WhsPickByLabelLabel, bool> isLabelComplete)
		{
			Argument.NotNull(pickByLabelJob, nameof(pickByLabelJob));
			WhsPickByLabelJob result = null;

			var completedLabels = new List<WhsPickByLabelLabel>();
			var incompleteLabels = new List<WhsPickByLabelLabel>();

			foreach (var label in pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>())
			{
				if (isLabelComplete(label))
				{
					completedLabels.Add(label);
				}
				else
				{
					incompleteLabels.Add(label);
				}
			}

			if (completedLabels.Count > 0)
			{
				pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;

				if (incompleteLabels.Count > 0)
				{
					result = GetOrCreatePickByLabelJob(pickByLabelJob.Factory, pickByLabelJob.WTK_WW_Warehouse, pickByLabelJob.WTK_GS_NKAssignedTo, pickByLabelJob.WTK_WL_DockDoor);
					foreach (var incompleteLabel in incompleteLabels)
					{
						pickByLabelJob.Labels.Remove(incompleteLabel);
						result.Labels.Add(incompleteLabel);
					}
				}
			}

			return result;
		}

		#endregion

		#region CancelPickByLabel

		public static void CancelPickByLabel(WhsPickByLabelLabel label)
		{
			Argument.NotNull(label, nameof(label));

			var pickByLabelJob = label.PickByLabelJob;
			if (pickByLabelJob != null)
			{
				pickByLabelJob.Labels.Remove(label);
				if (pickByLabelJob.Labels.Count == 0)
				{
					pickByLabelJob.Delete();
				}
			}

			label.Delete();
		}

		#endregion

		#region Implementation

		#region CreateWhsPickByLabelJob

		static WhsPickByLabelJob CreateWhsPickByLabelJob(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid ddlPK, ZString rfUser)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rfUser, nameof(rfUser));
			ArgumentZGuidIsNotEmpty(warehousePK, nameof(warehousePK));
			ArgumentZGuidIsNotEmpty(ddlPK, nameof(ddlPK));

			var pickByLabelJob = factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = warehousePK;
			pickByLabelJob.WTK_WL_DockDoor = ddlPK;
			pickByLabelJob.WTK_GS_NKAssignedTo = rfUser;
			return pickByLabelJob;
		}

		#endregion

		#region PackageHasActivePickByLabel

		public static WhsPickByLabelLabel GetActivePickByLabelByPackage(PkgPackage package)
		{
			var queryJob = new ZDBOnlySubQuery(typeof(WhsPickByLabelJob), WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob);
			queryJob.AddToFilter(WhsPickByLabelJobSchema.WTK_FinalisedDate, null);
			var query = new ZDBOnlyQuery(typeof(WhsPickByLabelLabel));
			query.AddToFilter(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK);
			query.AddSubQuery(queryJob, JoinCondition.And);
			return package.Factory.LoadTop1<WhsPickByLabelLabel>(query);
		}

		#endregion

		#region HasAnyPackageAssignedToAPickByLabelJob

		public static bool HasAnyPackageAssignedToAPickByLabelJob(this WhsOrder order)
		{
			return GetPickByLabelJobForPackage(order, null, false) != null;
		}

		public static bool IsPackageAssignedToPickByLabelJob(this WhsOrder order, PkgPackage package)
		{
			return GetPickByLabelJobForPackage(order, package, false) != null;
		}

		static IWhsPickByLabelJob GetPickByLabelJobForPackage(WhsOrder order, PkgPackage package, bool activeJobsOnly = true)
		{
			var packagesPKs = package == null ? order.PackageJob?.Packages.Select(p => p.PK) : new ZGuid[] { package.PK };
			IWhsPickByLabelJob result = null;
			if (packagesPKs != null)
			{
				var packageIsInDatabase = package == null || package.IsInDatabase;

				var labelJobLabelQuery = new ZQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, packagesPKs) { FetchOnlyFromLocalCache = !packageIsInDatabase };
				var labelJobLabels = order.Factory.Load<IWhsPickByLabelLabel>(labelJobLabelQuery);

				var queryPickByLabelJob = new ZQuery(WhsPickByLabelJobSchema.PK, labelJobLabels.Select(s => s.WTL_WTK_PickByLabelJob).Distinct()) { FetchOnlyFromLocalCache = !packageIsInDatabase };
				if (activeJobsOnly)
				{
					queryPickByLabelJob.AddToFilter(WhsPickByLabelJobSchema.WTK_FinalisedDate, SQLComparisonOperator.Equal, null);
				}

				result = order.Factory.LoadTop1<IWhsPickByLabelJob>(queryPickByLabelJob);
			}
			return result;
		}

		#endregion

		#region GetActiveWhsPickByLabelJobQuery

		static ZQuery GetActiveWhsPickByLabelJobQuery(ZGuid warehousePK, ZString userNK)
		{
			var query = new ZQuery(WhsPickByLabelJobSchema.WTK_WW_Warehouse, warehousePK);
			query.AddToFilter(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, userNK);
			query.AddToFilter(WhsPickByLabelJobSchema.WTK_FinalisedDate, null);
			return query;
		}

		#endregion

		static void ArgumentZGuidIsNotEmpty(ZGuid argument, string name)
		{
			Argument.NotNullOrEmpty((argument == ZGuid.Empty ? "" : argument.ToString()), name);
		}

		#endregion
	}
}
