using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class RemoveHoldAllPackagesApplicator : WhsOperationalActionMethodApplicator
	{
		public RemoveHoldAllPackagesApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("4276C883-3804-4A45-BB6E-D214734D1849", "Remove Hold All Packages"), factory) // text used for logging
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Order 3 - all the packages are without hold now..

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] orders)
		{
			log.SetSectionProgressMax(orders.Length);

			AddFetchHints(orders);
			foreach (WhsOrder order in orders)
			{
				log.BumpSectionProgress();
				RemoveHoldAllPackages(order, log);
			}
		}

		#region AddFetchHints

		void AddFetchHints(BusinessObject[] orders)
		{
			foreach (WhsOrder order in orders)
			{
				order.Factory.AddFetchHint(PkgPackageJobSchema.KJ_ParentID, order.PK);
				order.Factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
				order.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				order.Factory.AddFetchHint(WhsPickSchema.PK, order.WD_WP);
			}

			AddFetchHintForPackages(orders);
		}

		void AddFetchHintForPackages(BusinessObject[] orders)
		{
			foreach (WhsOrder order in orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					order.Factory.AddFetchHint(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
				}
			}
		}

		#endregion

		#region RemoveHoldAllPackages

		void RemoveHoldAllPackages(WhsOrder order, IOperationalActionSectionLog log)
		{
			var orderLink = GetDocketIdLink(order);
			if (order.IsCancelled)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("B21CF0FA-2FA4-4195-B768-DFB75F15D432", "is canceled and cannot remove hold of packages."));
			}
			else if (order.IsFinalised)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("1FBE60DF-E23D-4FB8-9868-F73A8BA86D87", "is already finalized and cannot remove hold of packages."));
			}
			else if (order.WD_WP.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("52690297-BCFB-42D4-AC5F-6C575CAABD77", "cannot remove hold of packages as it has not been Picked."));
			}
			else
			{
				order.PackageJob.RemoveHoldOfAllPackages();
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("393AFAAE-C565-4238-9E60-261B4FD17153", "all the packages are without hold now."));
			}
		}
		#endregion

		#region Validation

		public RemoveHoldAllPackagesApplicatorValidation Validation
		{
			get { return new RemoveHoldAllPackagesApplicatorValidation(this); }
		}

		#endregion
	}

	#region RemoveHoldAllPackagesApplicatorValidation

	/// <summary>
	/// This class is unnecessary and only exists to satisfy the Z-test for IObsoleteValidation.
	/// </summary>
	public class RemoveHoldAllPackagesApplicatorValidation : ZValidation
	{
		public RemoveHoldAllPackagesApplicatorValidation(RemoveHoldAllPackagesApplicator parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(RemoveHoldAllPackagesApplicator); }
		}

		public override void ValidateAll()
		{
		}
	}

	#endregion
}
