using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsJobServiceInfo : DataObjectInfo
	{
		#region Constructors

		public WhsJobServiceInfo()
			: this("", "", 0)
		{
		}

		public WhsJobServiceInfo(JobService jobService)
			: this(jobService.ES_ServiceCode, jobService.ES_ServiceNote, jobService.ES_ServiceCount)
		{
		}

		WhsJobServiceInfo(string code, string serviceNote, decimal count)
		{
			Code = code;
			ServiceNote = serviceNote;
			Count = count;
		}

		#endregion

		#region Properties

		public string Code { get; set; }
		public decimal Count { get; set; }
		public string ServiceNote { get; set; }

		#endregion
	}
}
