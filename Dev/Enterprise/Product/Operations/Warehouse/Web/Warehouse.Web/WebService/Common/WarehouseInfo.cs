using System;

namespace Enterprise.Warehouse.Web.WebService.Common
{
#if DEBUG
	[Serializable]
#endif
	public class WarehouseInfo : DataObjectInfo
	{
		#region Constructors

		public WarehouseInfo()
		{
			this.branchCode = "";
			this.code = "";
			this.name = "";
			this.countryCode = "";
		}

		#endregion

		#region Properties

		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string BranchCode
		{
			get { return branchCode; }
			set { branchCode = value; }
		}

		public string CountryCode
		{
			get { return countryCode; }
			set { countryCode = value; }
		}

		#endregion

		#region Implementation

		string code;
		string name;
		string branchCode;
		string countryCode;

		#endregion
	}
}
