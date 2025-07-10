using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionSourceMonitorStrategy : IBusinessObjectStrategy
	{
		protected IHaveJobHeader parent;

		public CommissionSourceMonitorStrategy(ZPropertyInfo[] infos, IHaveJobHeader parent)
		{
			if (infos == null)
			{
				throw new ArgumentNullException(nameof(infos));
			}

			if (infos.Length == 0)
			{
				throw new ArgumentException("No property infos provided");
			}

			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (infos.Select(i => i.BizObj).Distinct().Count() > 1)
			{
				throw new ArgumentException("Multiple objects were provided");
			}

			this.infos = infos;
			this.parent = parent;
		}

		readonly ZPropertyInfo[] infos;

		public void OnSaving(BusinessObject businessObject)
		{
			bool propertiesChanged = CheckPropertiesChanged();

			if (propertiesChanged && businessObject.IsInDatabase)
			{
				ReverseIfRequired();
			}
		}

		protected virtual bool CheckPropertiesChanged()
		{
			return infos.Any(i => !i.Value.Equals(i.OriginalValue));
		}

		void ReverseIfRequired()
		{
			var jobHeader = parent.JobHeader;

			if (jobHeader != null)
			{
				jobHeader.ReverseTransactionCommissions();
			}
		}

		#region not used

		public void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
		}

		public DeleteDetails DeleteDetails(BusinessObject businessObject)
		{
			return new DeleteDetails.Allow();
		}

		public void FetchForLoad(BusinessObject businessObject)
		{
		}

		public void OnDelete(BusinessObject businessObject)
		{
		}

		public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		public void OnFactorySaving(BusinessObject businessObject)
		{
		}

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
		{
		}

		public void OnSaveRollback(BusinessObject businessObject)
		{
		}

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject)
		{
		}

		#endregion
	}
}
