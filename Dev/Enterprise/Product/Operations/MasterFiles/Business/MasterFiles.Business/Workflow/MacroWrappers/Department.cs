using CargoWise.Types;
using IDepartment = Enterprise.DocumentVisualizer.DocDataObjects.IDepartment;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Department : IDepartment
	{
		public Department(GlbDepartment department)
		{
			this.department = department;
		}

		readonly GlbDepartment department;

		public ZString Code => department?.GE_Code ?? ZString.Empty;

		public ZGuid PK => department?.PK ?? ZGuid.Empty;

		public ZString Description => department?.GE_Desc ?? ZString.Empty;
	}
}

