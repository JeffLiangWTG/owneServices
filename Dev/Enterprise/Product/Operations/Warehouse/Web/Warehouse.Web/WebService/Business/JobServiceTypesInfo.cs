using System;
using CargoWise.Integration;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class JobServiceTypesInfo : DataObjectInfo
	{
		#region Constructors

		public JobServiceTypesInfo()
			: this("", "")
		{
		}

		public JobServiceTypesInfo(ICodeDescription codeDescription)
			: this(codeDescription.Code, codeDescription.Description)
		{
		}

		JobServiceTypesInfo(string code, string description)
		{
			Code = code;
			Description = description;
		}

		#endregion

		#region Properties

		public string Code { get; set; }
		public string Description { get; set; }
		public WhsJobServiceInfo ExistingJobService { get; set; }

		#endregion
	}
}
