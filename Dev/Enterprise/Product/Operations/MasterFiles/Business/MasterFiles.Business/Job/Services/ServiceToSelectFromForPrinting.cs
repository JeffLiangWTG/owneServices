using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ServiceToSelectFromForPrinting : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ServiceToSelectFromForPrinting(JobService service) : base(service.Factory)
		{
			fService = service;
			fES_Calc_PrintDocumentForService = ZBool.True;
		}

		#region Service

		public JobService Service
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fService; }
		}
		readonly JobService fService;

		#endregion

		#region ES_Calc_PrintDocumentForService

		public ZBool ES_Calc_PrintDocumentForService
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fES_Calc_PrintDocumentForService; }
			set { SetNonPersistentPropertyValue(ES_Calc_PrintDocumentForServiceInfo, ref fES_Calc_PrintDocumentForService, value); }
		}
		protected ZBool fES_Calc_PrintDocumentForService;

		public ZPropertyInfo ES_Calc_PrintDocumentForServiceInfo
		{
			get { return GetZPropertyInfo(nameof(ES_Calc_PrintDocumentForService)); }
		}

		#endregion
	}
}
