using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class DeveloperExceptionsToBeSentAfterSavingService : IAfterSaveInTransactionService
	{
		#region static methods

		static DeveloperExceptionsToBeSentAfterSavingService GetService(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetAfterSaveInTransactionService<DeveloperExceptionsToBeSentAfterSavingService>();
		}

		static DeveloperExceptionsToBeSentAfterSavingService GetOrCreateService(BusinessObjectFactory factory)
		{
			var service = GetService(factory);
			if (service == null)
			{
				service = new DeveloperExceptionsToBeSentAfterSavingService();
				factory.ServiceContainer.AddAfterSaveInTransactionService(service);
			}
			return service;
		}

		public static void QueueReport(BusinessObject bizObj, ZString key, ZString message, Exception ex)
		{
			var service = GetOrCreateService(bizObj.Factory);
			var report = new ReportToBeSent(bizObj.PK, key, message, ex);
			service.reportsToBeSent.Add(report);
		}

		public static void RegisterObjectIsSaving(BusinessObjectFactory factory, ZGuid objectPK)
		{
			var service = GetOrCreateService(factory);
			service.objectsSaving.Add(objectPK);
		}

		#endregion

		public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var report in reportsToBeSent.ToArray())
			{
				if (objectsSaving.Contains(report.objectPK))
				{
					reportsToBeSent.Remove(report);
					ErrorReporter.ReportOnce(report.key, report.message, report.ex);
				}
			}
			objectsSaving.Clear();
		}

		readonly HashSet<ZGuid> objectsSaving = new HashSet<ZGuid>();

		readonly List<ReportToBeSent> reportsToBeSent = new List<ReportToBeSent>();

		struct ReportToBeSent
		{
			public ReportToBeSent(ZGuid objectPK, ZString key, ZString message, Exception ex)
			{
				this.objectPK = objectPK;
				this.key = key;
				this.message = message;
				this.ex = ex;
			}

			public ZGuid objectPK;
			public ZString key, message;
			public Exception ex;
		}
	}
}
