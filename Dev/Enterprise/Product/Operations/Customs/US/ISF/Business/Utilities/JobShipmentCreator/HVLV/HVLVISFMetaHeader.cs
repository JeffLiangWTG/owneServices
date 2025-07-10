using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class HVLVISFMetaHeader : NonPersistentBusinessObject
	{
		public HVLVISFMetaHeader(BusinessObjectFactory factory, ZGuid shipmentPK)
			: this(GetRelatedJobs(factory, shipmentPK))
		{
		}

		public HVLVISFMetaHeader(RelatedJobCollection relatedJobs)
			: base(relatedJobs.FirstOrDefault().Factory)
		{
			RelatedJobs = relatedJobs;
		}

		public RelatedJobCollection RelatedJobs { get; }

		public CusISFHeader FirstImporterSecurityFilingJob
		{
			get
			{
				if (firstImporterSecurityFilingJob == null)
				{
					SetLeadingJob();
				}

				return firstImporterSecurityFilingJob;
			}
		}

		CusISFHeader firstImporterSecurityFilingJob;

		public CusISFHeaderLookups Lookups => FirstImporterSecurityFilingJob?.Lookups;

		static RelatedJobCollection GetRelatedJobs(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			var result = new RelatedJobCollection(factory);

			var query = new ZQuery(CusISFHeaderSchema.BF_JS_Shipment, shipmentPK);
			query.AddToFilter(CusISFHeaderSchema.BF_IsCancelled, false);

			var cusISFHeaders = factory.Load<CusISFHeader>(query).OrderBy(j => j.BF_JobReference);

			if (cusISFHeaders.Any())
			{
				result.AddRange(cusISFHeaders);
			}

			return result;
		}

		void SetLeadingJob()
		{
			firstImporterSecurityFilingJob = RelatedJobs.FirstOrDefault() as CusISFHeader;
			if (firstImporterSecurityFilingJob != null)
			{
				firstImporterSecurityFilingJob.PropertyValueChanged += FirstImporterSecurityFilingJob_PropertyValueChanged;
				firstImporterSecurityFilingJob.BuyingParty.DocAddressChanged += DocAddressChanged;
				firstImporterSecurityFilingJob.StuffingLocation.DocAddressChanged += DocAddressChanged;
				firstImporterSecurityFilingJob.MainShipToParty.DocAddressChanged += DocAddressChanged;
				firstImporterSecurityFilingJob.SellingParty.DocAddressChanged += DocAddressChanged;
				firstImporterSecurityFilingJob.Consolidator.DocAddressChanged += DocAddressChanged;
			}
		}

		void DocAddressChanged(object sender, EventArgs e)
		{
			SyncDocAddress(sender as ISFDocAddress);
		}

		void SyncDocAddress(ISFDocAddress newDocAddress)
		{
			if (newDocAddress != null)
			{
				foreach (CusISFHeader job in RelatedJobs.Except(FirstImporterSecurityFilingJob))
				{
					var addressRequirement = ((IDocAddresses)job).GetDocAddressRequirement(newDocAddress.DocAddressType);
					var docAddress = job.DocAddresses.FindOrCreateWithRequirement(addressRequirement);
					using (docAddress.GetValidationSuspender())
					{
						docAddress.SetActualFieldValuesFromParent(newDocAddress);
					}
				}
			}
		}

		void FirstImporterSecurityFilingJob_PropertyValueChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			var propertyName = e.Property.Name;
			if (!PropertiesExcludedFromPropagateToAllJobs.Contains(propertyName))
			{
				RelatedJobs.Except(FirstImporterSecurityFilingJob).ForEach(job => job.FindPropertyInfo(propertyName).Value = e.Property.Value);
			}
		}

		static string[] PropertiesExcludedFromPropagateToAllJobs => new[]
		{
			CusISFHeaderSchema.Constants.BF_JobReference,
			CusISFHeaderSchema.Constants.BF_CustomsStatus,
		};

		#region HVLVISFMessageSendWrapperCollection

		public HVLVISFMessageSendWrapperCollection MessagesSendWrapperCollection => importerSecurityFilingJobsToSendMessage ?? (importerSecurityFilingJobsToSendMessage = new HVLVISFMessageSendWrapperCollection(RelatedJobs));

		HVLVISFMessageSendWrapperCollection importerSecurityFilingJobsToSendMessage;

		#endregion
	}
}
