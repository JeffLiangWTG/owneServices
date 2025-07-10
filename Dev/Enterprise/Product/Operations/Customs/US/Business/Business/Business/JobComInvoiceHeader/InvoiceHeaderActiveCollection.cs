using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection, IShouldUpdateScreeningStatus, IDisposable
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public override void Delete(BaseJobComInvoiceHeader businessObject)
		{
			base.Delete(businessObject);

			var jobComInvoice = businessObject as JobComInvoiceHeader;

			if (jobComInvoice != null)
			{
				jobComInvoice.ShouldUpdateScreeningStatusChangedToTrue -= new EventHandler(ResetMyScreeningStatus);
			}

			((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
		}

		protected override void OnLoadedIntoCollectionCore(BaseJobComInvoiceHeader loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);

			var jobComInvoice = loadedObject as JobComInvoiceHeader;

			if (jobComInvoice != null)
			{
				jobComInvoice.ShouldUpdateScreeningStatusChangedToTrue += new EventHandler(ResetMyScreeningStatus);
			}
		}

		protected override void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
			base.OnAddIntoRelationshipCore(businessObject);
			if (declaration.IsExport && !businessObject.IsInDatabase && businessObject is JobComInvoiceHeader invoiceHeader)
			{
				foreach (var docAddress in invoiceHeader.DocAddresses.Cast<USOrganisationDocAddress>().Where(x => !x.HasChanges && !x.IsEmpty))
				{
					docAddress.HasChanges = true;
				}
			}
		}

		public new JobComInvoiceHeader AddNew()
		{
			var baseInvoice = base.AddNew();

			var jobComInvoice = baseInvoice as JobComInvoiceHeader;

			if (jobComInvoice != null)
			{
				jobComInvoice.ShouldUpdateScreeningStatusChangedToTrue += new EventHandler(ResetMyScreeningStatus);
			}

			((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
			return jobComInvoice;
		}

		#region IShouldUpdateScreeningStatus Members

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus
		{
			get
			{
				return fShouldUpdateScreeningStatus;
			}

			set
			{
				fShouldUpdateScreeningStatus = value;
				if (fShouldUpdateScreeningStatus)
				{
					var parentProvider = JobDeclaration as IShouldUpdateScreeningStatus;
					if (parentProvider != null)
					{
						parentProvider.ShouldUpdateScreeningStatus = true;
					}
				}
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ZBool fShouldUpdateScreeningStatus;

		#endregion

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		#region Public interfaces

		public ZDecimal TotalInvoiceQty
		{
			get
			{
				if (totalInvoiceQtyCached == null)
				{
					totalInvoiceQtyCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0m;

						foreach (JobComInvoiceHeader invoice in this)
						{
							result += invoice.JobComInvoiceLines.TotalPackQty;
						}

						return result;
					}
					);
				}

				return totalInvoiceQtyCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalInvoiceQtyCached;

		public bool HasNonContainerisedInvoiceLines
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in this)
				{
					if (invoice.JobComInvoiceLines.HasNonContainerisedInvoiceLines)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool RequiresPriorNoticeReporting
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in this)
				{
					if (invoice.JobComInvoiceLines.RequiresPriorNoticeReporting)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasFDATariffsToBeDeclared
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in this)
				{
					if (invoice.JobComInvoiceLines.HasFDATariffsToBeDeclared)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasOGATariffsToBeDeclared
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in this)
				{
					if (invoice.JobComInvoiceLines.HasOGATariffsToBeDeclared)
					{
						return true;
					}
				}

				return false;
			}
		}

		internal IEnumerable<ReconIssues> ReconIssues
		{
			get
			{
				foreach (var invoice in this)
				{
					var link = invoice.SupplierBuyerLink;
					if (link != null)
					{
						yield return link.GetReconIssueCalculated();
					}
				}
			}
		}

		internal IEnumerable<ZBool> ReconNAFTAs
		{
			get
			{
				foreach (var invoice in this)
				{
					var link = invoice.SupplierBuyerLink;
					if (link != null)
					{
						var addInfo = link.GetAddInfo();
						if (addInfo != null)
						{
							yield return addInfo.ZO_NAFTAReconIndicator;
						}
					}
				}
			}
		}

		#endregion

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader child)
		{
			base.SetDefaultsForNewElementCore(child);
			new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)child, JobDeclaration).DefaultForNewElement();
		}

		protected override void OnAdded(BaseJobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);
			JobComInvoiceHeader invoice = (JobComInvoiceHeader)businessObject;
			if (invoice.IsAttachedToPersistentDeclaration && invoice.JobDeclaration.IsENSFormalImport)
			{
				MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, (IMessageAttacheeInDeclaration)businessObject, MessageAttacheeActionType.Added);
			}
		}

		#endregion

		void ResetMyScreeningStatus(object sender, EventArgs e)
		{
			((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
		}

		#region IDisposable Members

		public void Dispose()
		{
			foreach (BaseJobComInvoiceHeader invoice in this)
			{
				var jobComInvoice = invoice as JobComInvoiceHeader;
				if (jobComInvoice != null)
				{
					jobComInvoice.ShouldUpdateScreeningStatusChangedToTrue -= new EventHandler(ResetMyScreeningStatus);
				}
			}
		}

		#endregion
	}
}
