namespace Enterprise.Warehouse.Transactions.Business
{
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Packing.Business;
	using ZArchitecture.Schema;

	public static class WhsPackageAuditManager
	{
		#region AuditPackage

		public static WhsPackageAudit AuditPackage(PkgPackage package)
		{
			return AuditPackage(package, ZDateTimeOffset.Now, GlbStaff.CurrentUser);
		}

		public static WhsPackageAudit AuditPackage(PkgPackage package, ZDateTimeOffset auditCompleteTime, GlbStaff auditor)
		{
			Argument.NotNull(package, nameof(package));
			Argument.NotNullOrEmpty(package.KP_PackageID, nameof(package.KP_PackageID));
			Argument.NotNull(auditor, nameof(auditor));

			if (package.PackageJob.KJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				var order = (WhsOrder)package.PackageJob.ParentJob;
				if (order != null)
				{
					var packageAudit = order.Factory.New<WhsPackageAudit>();
					packageAudit.WPA_WD_Order = order.PK;
					packageAudit.WPA_PackageID = package.KP_PackageID;
					packageAudit.WPA_AuditCompleteTime = auditCompleteTime;
					packageAudit.WPA_GS_NKAuditor = auditor.GS_Code;
					return packageAudit;
				}
			}
			return null;
		}

		#endregion

		#region GetLastWhsPackageAudit

		public static WhsPackageAudit GetLastWhsPackageAudit(PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));
			WhsPackageAudit result = null;

			if (!package.KP_PackageID.IsEmpty && package.PackageJob != null && package.PackageJob.KJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				var orderPk = package.PackageJob.KJ_ParentID;
				var query = new ZQuery(WhsPackageAuditSchema.WPA_WD_Order, orderPk);
				query.AddToFilter(WhsPackageAuditSchema.WPA_PackageID, package.KP_PackageID);
				query.OrderBy = WhsPackageAuditSchema.WPA_AuditCompleteTime.Name + OrderByClause.Descending;
				result = package.Factory.LoadTop1<WhsPackageAudit>(query);
			}

			return result;
		}

		#endregion

		#region HasPassedAudit

		public static bool HasPassedAudit(PkgPackage package)
		{
			var result = false;
			var lastAudit = GetLastWhsPackageAudit(package);
			if (lastAudit != null)
			{
				result = !(lastAudit.PackageAuditFailureLines.Count > 0);
			}
			return result;
		}

		#endregion

		#region HasBeenAudited

		public static bool HasBeenAudited(PkgPackage package)
		{
			return GetLastWhsPackageAudit(package) != null;
		}

		#endregion

		#region HasAuditFailures

		public static bool HasAuditFailures(PkgPackage package)
		{
			var result = false;
			var lastAudit = GetLastWhsPackageAudit(package);
			if (lastAudit != null)
			{
				result = (lastAudit.PackageAuditFailureLines.Count > 0);
			}
			return result;
		}

		#endregion
	}
}
