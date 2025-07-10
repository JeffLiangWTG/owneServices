using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsDocketProductHoldCodeInfo : DataObjectInfo
	{
		#region Constructors

		public WhsDocketProductHoldCodeInfo()
			: this(string.Empty)
		{
		}

		public WhsDocketProductHoldCodeInfo(string productcode, params string[] holdCodes)
		{
			ProductCode = productcode;
			holdCodes.ForEach(hcc => HoldCodes.Add(hcc));
		}

		#endregion

		#region Properties

		public string ProductCode { get; set; }

		public HashSet<string> HoldCodes
		{
			get { return holdCodes ?? (holdCodes = new HashSet<string>()); }
			set { holdCodes = value; }
		}
		HashSet<string> holdCodes;

		#endregion
	}
}
