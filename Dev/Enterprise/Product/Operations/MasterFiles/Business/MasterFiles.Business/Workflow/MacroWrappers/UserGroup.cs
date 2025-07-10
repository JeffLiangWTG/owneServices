using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class UserGroup : IUserGroup
	{
		public UserGroup(GlbGroup group)
		{
			this.group = group;
		}
		readonly GlbGroup group;

		public ZString Code => group?.GG_Code ?? ZString.Empty;

		public ZString Description => group?.GG_Desc ?? ZString.Empty;

		public ZString DomainName => group?.GG_DomainName ?? ZString.Empty;

		public ZString Category => group?.GG_Category ?? ZString.Empty;

		public IUserGroup ParentGroup
		{
			get
			{
				if (group?.ParentGroup is GlbGroup parentGroup)
				{
					return new UserGroup(parentGroup);
				}
				return null;
			}
		}

		public ZBool IsActive => group?.GG_IsActive ?? ZBool.False;

		public ZBool IsNonSecurity => group?.IsNonSecurityGroup ?? ZBool.False;
	}
}
