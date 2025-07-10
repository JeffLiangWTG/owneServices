using System;

namespace Enterprise.Warehouse.Web.WebService.Common
{
#if DEBUG
	[Serializable]
#endif
	public class BranchInfo : DataObjectInfo
	{
		#region Constructors

		public BranchInfo()
			: base()
		{
			this.code = "";
			this.name = "";
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

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		#endregion

		#region Implementation

		string code;
		string name;
		Guid pk;

		#endregion
	}
}
