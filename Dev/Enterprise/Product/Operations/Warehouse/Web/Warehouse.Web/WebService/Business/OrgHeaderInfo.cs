using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class OrgHeaderInfo : DataObjectInfo
	{
		public OrgHeaderInfo()
		{
		}

		public OrgHeaderInfo(OrgHeader organisation)
		{
			Code = organisation.OH_Code;
			FullName = organisation.OH_FullNameTruncated;
		}

		#region Properties

		public string Code { get; set; }

		public string FullName { get; set; }

		#endregion
	}
}
