using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.NumberFountain;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Tools
{
	public class ReleaseManager
	{
		public ReleaseManager(BusinessObjectFactory factory, WhsWarehouse warehouse)
		{
			Argument.NotNull(factory, "Factory");
			Argument.NotNull(warehouse, "Warehouse");

			Factory = factory;
			Warehouse = warehouse;
		}

		readonly BusinessObjectFactory Factory;
		readonly WhsWarehouse Warehouse;

		#region ReleaseJobForPk / UnreleaseJobForPk

		public ActionResult ReleaseJobForPk(ZGuid jobPk) => ReleaseOrUnreleaseJobForPk(jobPk, true);

		public ActionResult UnreleaseJobForPk(ZGuid jobPk) => ReleaseOrUnreleaseJobForPk(jobPk, false);

		ActionResult ReleaseOrUnreleaseJobForPk(ZGuid jobPk, bool release)
		{
			var order = Factory.Load<WhsOrder>(jobPk);
			string error;
			if (order != null)
			{
				error = ReleaseOrUnreleaseOrder(new[] { order }, release);
			}
			else
			{
				error = Res.GetString("D1C27240-2EBF-4CC4-B4CF-926CE8108597", "The Job does not exist.");
			}
			return string.IsNullOrEmpty(error) ? ActionResult.Success() : ActionResult.Failure(error);
		}

		#endregion

		#region ReleaseJobForReference / UnreleaseJobForReference

		public ReleaseManagerResult<IPackageJobInfoForRelease> ReleaseJobForReference(string jobId) => ReleaseOrUnreleaseJobForReference(jobId, true);

		public ReleaseManagerResult<IPackageJobInfoForRelease> UnreleaseJobForReference(string jobId) => ReleaseOrUnreleaseJobForReference(jobId, false);

		ReleaseManagerResult<IPackageJobInfoForRelease> ReleaseOrUnreleaseJobForReference(string jobId, bool release)
		{
			var error = string.Empty;
			var (orders, areJobsFoundByPickNumber) = FindJobForReference(jobId);

			if (areJobsFoundByPickNumber || orders.Length == 1)
			{
				error = ReleaseOrUnreleaseOrder(orders, release);
			}
			else if (orders.Length == 0)
			{
				error = Res.GetString("B937547A-13A2-4CB0-A7D0-5E7FFC135F19", "Reference '{0}' not found!", jobId);
			}

			if (string.IsNullOrEmpty(error))
			{
				var packageJobInfo = areJobsFoundByPickNumber ? GetPackageJobInfos(jobId) : GetPackageJobInfos(orders);
				return ReleaseManagerResult.Success(packageJobInfo);
			}
			else
			{
				return ReleaseManagerResult.Failure<IPackageJobInfoForRelease>(error);
			}
		}

		#region FindJobForReference

		(WhsOrder[], bool) FindJobForReference(string reference)
		{
			var result = FindJob_ByPickNumber(reference);
			var areJobsFoundByPickNumber = result.Length != 0;

			if (result.Length == 0)
			{
				result = FindJob_ByExternalReference(reference);
			}
			if (result.Length == 0)
			{
				result = FindJob_ByCustomerReference(reference);
			}
			if (result.Length == 0)
			{
				result = FindJob_ByDocketID(reference);
			}
			if (result.Length == 0)
			{
				result = FindJob_ByDocketReference(reference);
			}

			return (result, areJobsFoundByPickNumber);
		}

		WhsOrder[] FindJob_ByPickNumber(string pickNumber)
		{
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickNo, pickNumber);

			var parentJobSubQuery = GetOrderSubQuery();
			parentJobSubQuery.AddSubQuery(pickSubQuery, JoinCondition.And);
			return Factory.Load<WhsOrder>(parentJobSubQuery);
		}

		WhsOrder[] FindJob_ByExternalReference(string reference)
		{
			var parentJobSubQuery = GetOrderSubQuery();
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_ExternalReference, reference);
			return Factory.Load<WhsOrder>(parentJobSubQuery);
		}

		WhsOrder[] FindJob_ByCustomerReference(string reference)
		{
			var parentJobSubQuery = GetOrderSubQuery();
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_CustomerReference, reference);
			return Factory.Load<WhsOrder>(parentJobSubQuery);
		}

		WhsOrder[] FindJob_ByDocketID(string reference)
		{
			var parentJobSubQuery = GetOrderSubQuery();
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketID, reference);
			return Factory.Load<WhsOrder>(parentJobSubQuery);
		}

		WhsOrder[] FindJob_ByDocketReference(string reference)
		{
			var referenceSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketReference), WhsDocketReferenceSchema.WX_WD);
			referenceSubQuery.AddToFilter(WhsDocketReferenceSchema.WX_Reference, reference);

			var parentJobSubQuery = GetOrderSubQuery();
			parentJobSubQuery.AddSubQuery(referenceSubQuery, JoinCondition.And);
			return Factory.Load<WhsOrder>(parentJobSubQuery);
		}

		ZDBOnlyQuery GetOrderSubQuery()
		{
			// Parent Job's Warehouse and Docket Type
			var parentJobSubQuery = new ZDBOnlyQuery(typeof(WhsOrder));
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, Warehouse.PK);
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order);

			return parentJobSubQuery;
		}

		#endregion

		#region ReleaseOrUnreleaseOrder

		string ReleaseOrUnreleaseOrder(IEnumerable<WhsOrder> orders, bool isRelease)
		{
			var error = string.Empty;
			AddFetchHintsForOrders(orders);
			AddFetchHintsForPkgPackageAndProcessTaskNotification(orders);

			if (isRelease && !CanReleaseOrders(Warehouse, orders))
			{
				error = Res.GetString("a2b5429f-29b4-4871-9aea-c8e2ec468167", "Job can't be released because some packages on the job are not yet fully picked.");
			}
			else
			{
				foreach (var order in orders)
				{
					ReleaseOrUnreleaseOrderCore(order, isRelease);
				}
				if (orders.Any(order => order.HasChanges))
				{
					Factory.Save();
				}
			}
			return error;
		}

		static bool CanReleaseOrders(WhsWarehouse warehouse, IEnumerable<WhsOrder> orders)
		{
			IEnumerable<WhsOrder> ordersThatNeedChecking;
			if (warehouse.WW_PreventReleaseOfPackageIfNotPicked)
			{
				ordersThatNeedChecking = orders;
			}
			else
			{
				ordersThatNeedChecking = GetListOfOrdersWithOrgSettingOn(orders);
			}
			return AreAllPackagesPicked(ordersThatNeedChecking);
		}

		static IEnumerable<WhsOrder> GetListOfOrdersWithOrgSettingOn(IEnumerable<WhsOrder> orders)
		{
			var clientsAndOrdersHashMap = new Dictionary<ZGuid, ZBool>();
			var listOfOrdersWithOrgPreventReleaseOfPackageSettingOn = new List<WhsOrder>();

			foreach (var order in orders)
			{
				if (!clientsAndOrdersHashMap.TryGetValue(order.WD_OH_Client, out var orgPreventReleaseBeforePackingFlag))
				{
					orgPreventReleaseBeforePackingFlag = order.Client.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked;
					clientsAndOrdersHashMap.Add(order.WD_OH_Client, orgPreventReleaseBeforePackingFlag);
				}
				if (orgPreventReleaseBeforePackingFlag)
				{
					listOfOrdersWithOrgPreventReleaseOfPackageSettingOn.Add(order);
				}
			}
			return listOfOrdersWithOrgPreventReleaseOfPackageSettingOn;
		}

		static bool AreAllPackagesPicked(IEnumerable<WhsOrder> orders)
		{
			var packages = orders.Where(order => order.PackageJob != null)
								.Select(order => order.PackageJob)
								.SelectMany(packageJob => packageJob.Packages)
								.ToArray();

			return packages.All(package => IsPackagePicked(package));
		}

		void ReleaseOrUnreleaseOrderCore(WhsOrder order, bool isRelease)
		{
			var packageJob = order.PackageJob;
			if (packageJob != null && packageJob.Packages.Count > 0)
			{
				packageJob.KJ_ReleasedTimeUtc = isRelease ? ZDateTime.UtcNow : ZDateTime.Empty;
			}
			else if (isRelease)
			{
				((IPackingParent)order).OnPackageJobReleased();
			}
		}

		void AddFetchHintsForOrders(IEnumerable<WhsOrder> orders)
		{
			var picks = new HashSet<ZGuid>();

			foreach (var order in orders)
			{
				if (picks.Add(order.WD_WP))
				{
					Factory.AddFetchHint(StmALogSchema.SL_Parent, order.WD_WP);
					Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.WD_WP);
				}

				Factory.AddFetchHint(PkgPackageJobSchema.KJ_ParentID, order.PK);
				Factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				Factory.AddFetchHint(StmALogSchema.SL_Parent, order.PK);
			}
		}

		void AddFetchHintsForPkgPackageAndProcessTaskNotification(IEnumerable<WhsOrder> orders)
		{
			foreach (var order in orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					Factory.AddFetchHint(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
				}

				var workFlows = order.WorkflowItems;
				foreach (var workFlow in workFlows)
				{
					Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, workFlow.PK);
				}
			}

			foreach (var order in orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					foreach (var package in packageJob.GetAllPackagesOnJob())
					{
						Factory.AddFetchHint(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK);
						Factory.AddFetchHint(WhsPickTrolleySlotSchema.WTS_KP_Package, package.PK);
					}
				}
			}
		}

		#endregion

		#region GetPackageJobs

		IPackageJobInfoForRelease[] GetPackageJobInfos(string reference)
		{
			return new[] { new PackageJobInfoForRelease(reference) };
		}

		IPackageJobInfoForRelease[] GetPackageJobInfos(WhsOrder[] orders)
		{
			var result = new List<IPackageJobInfoForRelease>(orders.Length);
			foreach (var order in orders)
			{
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				result.Add(new PackageJobInfoForRelease(packageJob, order));
			}
			return result.ToArray();
		}

		#endregion

		#endregion

		#region ReleasePackageForPk / UnreleasePackageForPk

		public ActionResult ReleasePackageForPk(ZGuid packagePk) => ReleaseOrUnreleasePackageForPk(packagePk, true);

		public ActionResult UnreleasePackageForPk(ZGuid packagePk) => ReleaseOrUnreleasePackageForPk(packagePk, false);

		ActionResult ReleaseOrUnreleasePackageForPk(ZGuid packagePk, bool release)
		{
			var package = Factory.Load<PkgPackage>(packagePk);
			string error;
			if (package != null)
			{
				if ((!package.IsReleased && release)
					|| (package.IsReleased && !release))
				{
					error = ReleaseOrUnreleasePackage(release, package);
				}
				else
				{
					var messageContent = release ? Res.GetString("7F03DE77-E9FE-4CB7-BACA-85FF209DB3EC", "released") : Res.GetString("B43AE627-2E15-4520-8CFA-07617618C7F7", "canceled");
					error = Res.GetString("608e37e1-c862-4860-ae47-066d7c33e10e", "The package is already {0}.", messageContent);
				}
			}
			else
			{
				error = Res.GetString("4E878A50-40B1-47FF-B1F4-E4A4CC171903", "The package does not exist.");
			}
			return string.IsNullOrEmpty(error) ? ActionResult.Success() : ActionResult.Failure(error);
		}

		string ReleaseOrUnreleasePackage(bool release, PkgPackage package)
		{
			var error = string.Empty;
			if (release && !CanReleasePackage(package, Warehouse))
			{
				error = GetCannotReleasePackageErrorMessage(package);
			}
			else
			{
				ReleaseOrUnreleasePackageCore(release);
				var validationErrors = package.KP_ReleasedTimeUtcInfo.GetErrors();
				if (validationErrors.Any())
				{
					ReleaseOrUnreleasePackageCore(!release);
					error = validationErrors.ToUniqueMessageListString();
				}
				else
				{
					Factory.Save();
				}
			}
			return error;

			void ReleaseOrUnreleasePackageCore(bool isRelease)
			{
				package.KP_ReleasedTimeUtc = isRelease ? ZDateTime.UtcNow : ZDateTime.Empty;
			}
		}

		public static bool CanReleasePackage(PkgPackage package, WhsWarehouse warehouse)
		{
			var order = (WhsOrder)package.PackageJob.ParentJob;
			return (!warehouse.WW_PreventReleaseOfPackageIfNotPicked && !order.Client.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked) || IsPackagePicked(package);
		}

		static bool IsPackagePicked(PkgPackage package)
		{
			var pickLines = new PackagePackingHelper(package).GetPickLinesFromPackage();
			return pickLines.Length > 0 && pickLines.All(pickLine => !PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
		}

		public static ZString GetCannotReleasePackageErrorMessage(PkgPackage package)
		{
			return Res.GetString("0549ff53-fdbe-43fa-af2a-20212d9d6220", "Package '{0}' can't be released because it has not been fully picked.", package.KP_PackageID);
		}

		#endregion

		#region ReleasePackageForReference / UnreleasePackageForReference

		public ReleaseManagerResult<PkgPackage> ReleasePackageForReference(string packageId) => ReleaseOrUnreleasePackageForReference(packageId, true);

		public ReleaseManagerResult<PkgPackage> UnreleasePackageForReference(string packageId) => ReleaseOrUnreleasePackageForReference(packageId, false);

		ReleaseManagerResult<PkgPackage> ReleaseOrUnreleasePackageForReference(string packageId, bool release)
		{
			var error = string.Empty;
			var packages = GetPackageForReference(packageId, release);
			if (packages.Length == 1)
			{
				var result = ReleaseOrUnreleasePackageForPk(packages[0].PK, release);
				if (!result.IsSuccess)
				{
					packages = null;
					error = result.ErrorMessage;
				}
			}
			else if (packages.Length == 0)
			{
				var messageContent = release ? Res.GetString("4ACCC286-16B5-466F-A344-2EFC690D1188", "already released") : Res.GetString("B43AE627-2E15-4520-8CFA-07617618C7F7", "canceled");
				error = Res.GetString("5DF539D7-3764-422E-8A37-4CC4C91B2C7B", "Package '{0}' is not found, not an outer or {1}!", packageId, messageContent);
			}
			return string.IsNullOrEmpty(error) ?
				ReleaseManagerResult.Success(packages) :
				ReleaseManagerResult.Failure<PkgPackage>(error);
		}

		PkgPackage[] GetPackageForReference(ZString packageId, bool release)
		{
			var barcodeToSearchFor = packageId;
			var ssccBarCode = SSCCBarCodeChecker.GetSSCCFromRawBarcode(barcodeToSearchFor);
			if (!string.IsNullOrEmpty(ssccBarCode))
			{
				barcodeToSearchFor = ssccBarCode;
			}

			var query = new ZDBOnlyQuery(typeof(PkgPackage));

			// Package Id
			var packageIDSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIDSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, barcodeToSearchFor);
			query.AddToFilter(JoinCondition.And, PkgPackageSchema.KP_KP_ParentPackage, SQLComparisonOperator.Equal, null);

			// Package Job
			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageJobSubQuery.AddToFilter(JoinCondition.And, PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);

			// Parent Job
			var parentJobSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), PkgPackageJobSchema.KJ_ParentID);
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, Warehouse.PK);
			parentJobSubQuery.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order);
			packageJobSubQuery.AddSubQuery(parentJobSubQuery, JoinCondition.And);

			query.AddSubQuery(packageIDSubQuery, JoinCondition.And);
			query.AddSubQuery(packageJobSubQuery, JoinCondition.And);
			var releaseFilterQuery = new ZQuery();
			releaseFilterQuery.AddToFilter(PkgPackageSchema.KP_ReleasedTimeUtc, release ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			releaseFilterQuery.AddToFilter(JoinCondition.Or, PkgPackageSchema.KP_IsReleased, release ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, 0);
			query.AddToFilter(releaseFilterQuery);
			return Factory.Load<PkgPackage>(query);
		}

		#endregion

		#region PackageJobInfoForRelease

		class PackageJobInfoForRelease : IPackageJobInfoForRelease
		{
			public PackageJobInfoForRelease(PkgPackageJob packageJob, WhsOrder order)
			{
				Argument.NotNull(packageJob, nameof(packageJob));
				Argument.NotNull(order, nameof(order));

				if (packageJob.KJ_ParentID != order.PK)
				{
					throw new ArgumentException("Package job and order mismatch.");
				}

				PK = packageJob.KJ_ParentID.ToGuid();
				JobId = packageJob.KJ_JobID;
				ParentJobNo = order.WD_ExternalReference;
				ClientCode = order.Client.OH_Code;
				RequiredDate = order.WD_RequiredDate.ToDateTime();
				JobStatus = order.WarehouseOrderStatus;
			}

			public PackageJobInfoForRelease(string reference)
			{
				JobId = reference;
			}

			public Guid PK { get; }
			public string JobId { get; }
			public string ParentJobNo { get; }
			public string ClientCode { get; }
			public DateTime RequiredDate { get; }
			public string JobStatus { get; }
		}

		#endregion
	}
}
