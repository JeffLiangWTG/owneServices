using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupRoleValidation : AutoGlbGroupRoleValidation
	{
		public GlbGroupRoleValidation(AutoGlbGroupRole parent)
			: base(parent)
		{
		}

		protected override void CheckGGR_RoleName()
		{
			base.CheckGGR_RoleName();

			if (Parent.Group != null && Parent.Group.Roles.Any(x => x.PK != Parent.PK && x.GGR_RoleName == Parent.GGR_RoleName))
			{
				Parent.GGR_RoleNameInfo.AddError(Res.GetString("8514a59f-3af4-458b-b9bf-593112ee38dd", "Role Membership must be unique."));
			}
		}

		new GlbGroupRole Parent
		{
			get { return (GlbGroupRole)base.Parent; }
		}
	}
}
