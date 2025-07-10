using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoiceRatingAdaptersProvider : RatingAdaptersProvider<WhsInvoice>
	{
		public WhsInvoiceRatingAdaptersProvider(WhsInvoice parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(WhsInvoice parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var iParent = new WhsInvoiceRatingAdapter(parent);
			var result = new List<IAutoRating> { iParent };

			WhsInvoice.StorageDates[] storageDates = parent.GetStorageDates();
			for (int i = 1; i < storageDates.Length; i++)
			{
				var storagePeriod = new StoragePeriod(iParent, new StoragePeriodJobDatesProvider(parent), storageDates[i].From, storageDates[i].To);
				result.Add(storagePeriod);
			}

			return result;
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(WhsInvoice parent)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(parent));

			var dockets = parent.GetAdditionalDockets();
			// tested in WhsInvoiceJobStorageTest/InvoicingFormBasherTest
			AddFetchHintsForDockets(parent.Factory, dockets);

			result.AddRange(dockets);

			var adHocJobs = parent.AdHocServiceJobs();
			// tested in WhsInvoiceJobStorageTest/InvoicingFormBasherTest
			AddFetchHintsForAdHocJobs(parent.Factory, adHocJobs);

			result.AddRange(adHocJobs);
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}

		static void AddFetchHintsForDockets(BusinessObjectFactory factory, WhsDocket[] dockets)
		{
			var orders = new List<WhsOrder>();

			foreach (var docket in dockets)
			{
				if (docket.WD_DocketType == DocketType.Codes.Receive || docket.WD_DocketType == DocketType.Codes.Order)
				{
					factory.AddFetchHint(WhsDocketContainerSchema.WC_WD, docket.PK);
				}

				if (docket.WD_DocketType == DocketType.Codes.Receive)
				{
					factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, docket.PK);
				}

				if (docket.WD_DocketType == DocketType.Codes.Order)
				{
					factory.AddFetchHint(PkgPackageJobSchema.KJ_ParentID, docket.PK);
					factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, docket.PK);
					orders.Add((WhsOrder)docket);
				}

				if (docket.WD_DocketType == DocketType.Codes.Adjustment)
				{
					var query = new ZQuery();
					query.AddToFilter(JobHeaderSchema.JH_ParentID, docket.PK);
					query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					factory.AddFetchHint(JobHeaderSchema.Instance, query);
				}

				factory.AddFetchHint(JobServiceSchema.ES_ParentID, docket.PK);
				factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, docket.PK);
			}

			AddFetchHintsForPackages(factory, orders);
		}

		static void AddFetchHintsForPackages(BusinessObjectFactory factory, List<WhsOrder> orders)
		{
			var packageJobs = new List<PkgPackageJob>();
			foreach (var order in orders)
			{
				var packageJob = factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));
				if (packageJob != null)
				{
					packageJobs.Add(packageJob);
					factory.AddFetchHint(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
				}
			}

			foreach (var packageJob in packageJobs)
			{
				var packages = packageJob.GetAllPackagesOnJob();
				foreach (var p in packages)
				{
					factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, p.PK);
				}
			}
		}

		static void AddFetchHintsForAdHocJobs(BusinessObjectFactory factory, WhsAdHocServiceJob[] adHocJobs)
		{
			var pks = new List<ZGuid>(adHocJobs.Length);

			foreach (var adHocJob in adHocJobs)
			{
				pks.Add(adHocJob.PK);
				factory.AddFetchHint(JobServiceSchema.ES_ParentID, adHocJob.PK);
			}

			var query = new ZQuery();
			query.AddToFilter(JobHeaderSchema.JH_ParentID, pks);
			query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}

		class StoragePeriod : AutoRatingProxy
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
			public StoragePeriod(WhsInvoiceRatingAdapter iInvoice, StoragePeriodJobDatesProvider jobDatesProvider, ZDateTime from, ZDateTime to)
				: base(iInvoice)
			{
				ValuesCanBeSet = true;
				jobDatesProvider.SetDate(JobDateTypes.Codes.DepartureDate, from);
				jobDatesProvider.SetDate(JobDateTypes.Codes.ArrivalDate, to);
				JobDatesProvider = jobDatesProvider;
				var rateableMeasures = iInvoice.GetMeasures(jobDatesProvider);
				rateableMeasures.Time = new TimeInfo(JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate),
					JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
				RateableMeasures = rateableMeasures;
			}
		}
	}
}
