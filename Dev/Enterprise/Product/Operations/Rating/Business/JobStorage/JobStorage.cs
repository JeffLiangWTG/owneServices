using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Rating.Business
{
	public abstract class JobStorage : AutoJobStorage, IJobInvoicingPlugIn
	{
		protected JobStorage(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		public static readonly JobStorageTypeDecider TypeDecider = new JobStorageTypeDecider();

		#endregion

		#region Job Number Population

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PopulateET_StorageJobNumberIfRequired();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!IsInDatabase && !saveSucceeded)
			{
				ET_StorageJobNumber = ZString.Empty;
			}
		}

		void PopulateET_StorageJobNumberIfRequired()
		{
			if (ET_StorageJobNumber.IsEmpty)
			{
				ET_StorageJobNumber = NewStorageJobNumber();
			}
		}

		protected abstract ZString NewStorageJobNumber();

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return ET_StorageJobNumber; }
		}

		#endregion

		#region IJobHeaderParent

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateET_StorageJobNumberIfRequired();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		#region IJobHeaderParent_AllowInvoiceDeletion

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return AllowInvoiceDeletionCore; }
		}

		protected virtual bool AllowInvoiceDeletionCore
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region IJobInvoicingPlugIn Members

		JobStorageInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = GetNewInvoicingSupporter()); }
		}

		protected abstract JobStorageInvoicingSupporter GetNewInvoicingSupporter();

		#endregion

		#region Invoicing Supporter

		public abstract class JobStorageInvoicingSupporter : JobInvoicingSupporter
		{
			protected JobStorageInvoicingSupporter(JobStorage parent)
				: base(parent)
			{
				Parent = parent;
			}

			protected readonly JobStorage Parent;

			public override OrgHeader Consignor
			{
				get { return Parent.Client; }
			}

			public override ZString ConsolType
			{
				get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return JobInvoicingConsumerTypes.WarehouseStorage; }
			}

			public override ZString EditSecurityMessage
			{
				get { return EditSecurityMessageCore; }
			}

			protected virtual ZString EditSecurityMessageCore
			{
				get { return ZString.Empty; }
			}

			public override bool EditSecurityLock
			{
				get { return EditSecurityLockCore; }
			}

			protected virtual bool EditSecurityLockCore
			{
				get { return false; }
			}
		}

		#endregion

		#region Test Helpers
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateFK(ZPropertyInfo fKProperty, TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
			{
				if (fKProperty.Name.IndexOf("_GB_") == -1)
				{
					base.PopulateFK(fKProperty, kind, propertyPath);
				}
				else
				{
					fKProperty.Value = GlbBranch.CurrentBranch.PK;
				}
			}
		}

#endif
		#endregion
	}
}

