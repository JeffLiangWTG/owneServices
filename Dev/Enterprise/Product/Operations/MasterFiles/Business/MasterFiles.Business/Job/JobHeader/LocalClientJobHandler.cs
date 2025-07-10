using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class LocalClientJobHandler : IDisposable
	{
		#region Constructor

		public LocalClientJobHandler(ILocalClientJobHandler localClientJob)
		{
			this.localClientJob = localClientJob;
			JobLoader = new JobHeader.Loader(localClientJob);
		}

		readonly ILocalClientJobHandler localClientJob;

		public bool HasJobCreationTriggered { get; private set; }

		public void Initialize()
		{
			var templateRecordProvider = localClientJob as ITemplateRecordProvider;
			if (templateRecordProvider != null && templateRecordProvider.IsTemplateRecord && localClientJob.JobHeader == null)
			{
				InitializationMessage = Res.GetString("83f111d0-ed2d-4385-a56b-fa7eec85b1c7", "The Billing tab will be available when a Record is created from this Template.");
				return;
			}

			if (localClientJob is ICancellable cancellable && cancellable.IsCancelled)
			{
				return;
			}

			HasJobCreationTriggered = false;
			var accounting = ObjectFactory.Get<IAccounting>();
			if (!accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(localClientJob as BusinessObject))
			{
				if (localClientJob.JobHeader == null)
				{
					var hasInactiveJobHeader = JobLoader.HasInactiveJobHeader(GlbCompany.CurrentCompany);
					if (hasInactiveJobHeader)
					{
						InitializationMessage = Res.GetString("7952BAD2-E282-4585-A7C0-5CD975F73205", @"The Job Invoicing Record has been created but is currently not active. 
Please click on 'Billing' tab or 'Job Invoicing' menu to activate the job.");
					}
					else
					{
						InitializationMessage = Res.GetString("3467cfb1-286a-4916-83ed-a2ac857b85cc", "The registry item [{0}] has been set so that billing jobs will only be created upon entry to the Billing tab.",
						accounting.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation);
					}
				}
			}
			else
			{
				if (IsClientJobHandlerEligibleToCreateJob)
				{
					localClientJob.CreateJobHeaderWithMutex();
					HasJobCreationTriggered = true;
				}

				if (localClientJob.JobHeader == null)
				{
					InitializationMessage = Res.GetString("14966a37-cf77-4dbc-96fa-c912a631f775", "Automated creation of Job Header.");
				}
				else
				{
					job = localClientJob.JobHeader;

					if (job != null && (ILocalClientJobHandler)(job.Parent) != localClientJob)
					{
						job.Parent = localClientJob;
					}
				}
			}
		}

		public bool IsClientJobHandlerEligibleToCreateJob
		{
			get
			{
				var jobInvoicingPlugIn = localClientJob as IJobInvoicingPlugIn;
				return jobInvoicingPlugIn != null
					&& jobInvoicingPlugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob
					&& jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity.IsAllowed;
			}
		}

		#endregion

		#region InitializationMessage

		public string InitializationMessage { get; private set; }

		#endregion

		#region Job Loader

		public JobHeader.Loader JobLoader { get; private set; }

		#endregion

		#region Dispose

		public void Dispose()
		{
			if (job != null)
			{
				job.Dispose();
			}
		}
		JobHeader job;

		#endregion
	}
}
