using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class WhsCommonLookups
	{
		public static IBusinessObjectCollection GetPrintersList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CommonLookups|GetPrintersList", () => GetPrinters(factory));
		}

		static IBusinessObjectCollection GetPrinters(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.AddToFilter(StmPrintQueueSchema.SQ_AllowPrinting, true);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueDeleted, null);

			var result = (BusinessObjectCollection)ObjectFactory.Get<IStmPrintQueueCollection>("IStmPrintQueueCollection", factory, query);
			result.Load();
			return result;
		}
	}
}
