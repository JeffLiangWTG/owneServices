using System;

namespace Enterprise.Warehouse.Web.WebService.Common
{
#if DEBUG
	[Serializable]
#endif
	public class DepartmentInfo : DataObjectInfo
	{
		#region Constructors

		public DepartmentInfo()
			: base()
		{
			this.code = "";
			this.description = "";
			this.pk = Guid.Empty;
		}

		#endregion

		#region Properties

		public Guid PK
		{
			get { return pk; }
			set { pk = value; }
		}

		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		#endregion

		#region Implementation

		string code;
		string description;
		Guid pk;

		#endregion
	}
}
