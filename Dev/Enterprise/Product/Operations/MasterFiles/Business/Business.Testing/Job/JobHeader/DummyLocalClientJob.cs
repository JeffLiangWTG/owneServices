using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyLocalClientJob : DummyJobHeaderParent, ILocalClientJobHandler, ITemplateRecordProvider, ICancellable
	{
		public DummyLocalClientJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobHeader JobHeader
		{
			get
			{
				if (jobHeader == null || jobHeader.IsDeleted)
				{
					jobHeader = new JobHeader.Loader(this).Load(true, false);
					if (jobHeader != null)
					{
						RegisterEditableChildObject(jobHeader);
					}
				}
				return jobHeader;
			}
		}
		JobHeader jobHeader;

		public void CreateJobHeaderWithMutex()
		{
			if (jobHeader == null || jobHeader.IsDeleted)
			{
				jobHeader = new JobHeader.Loader(this).TryLoadOrCreateWithMutex();
				if (jobHeader != null)
				{
					RegisterEditableChildObject(jobHeader);
				}
			}
		}

		public void SaveToTemplateRecord() => throw new NotImplementedException();

		public void LoadFromTemplateRecord(ITemplateRecord templateRecord) => throw new NotImplementedException();

		public bool IsTemplateRecord { get; set; }

		public ITemplateRecord TemplateRecord { get; set; }

		BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord) => throw new NotImplementedException();

		#region ICancellable Members

		public string CanCancel()
		{
			return null;
		}

		public string CanReactivate()
		{
			return null;
		}

		public bool IsCancelled
		{
			get;
			set;
		}

		public bool IsCancelledHasChanged
		{
			get { return false; }
		}

		#endregion
	}
}
