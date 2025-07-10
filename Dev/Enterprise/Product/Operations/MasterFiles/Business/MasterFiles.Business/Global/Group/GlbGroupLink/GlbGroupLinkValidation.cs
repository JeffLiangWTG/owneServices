using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLinkValidation : AutoGlbGroupLinkValidation
	{
		public GlbGroupLinkValidation(AutoGlbGroupLink parent)
			: base(parent)
		{
		}

		#region GK_GG
		protected override void CheckGK_GG()
		{
			base.CheckGK_GG();
			ValidateDomainsForStaffAndGroup();
			if (!Parent.IsInDatabase && !Parent.GK_GGInfo.HasErrors() && Env.Security != null)
			{
				var group = Parent.Group;
				try
				{
					if (group.IsAllStaffGroup)
					{
						return;
					}
					else if (group.GG_IsSales)
					{
						if (Env.Security.SalesTeamsEdit.IsAllowed)
						{
							return;
						}
					}
					else if (Env.Security.StaffGroups.IsAllowed)
					{
						return; //if we ran upgrade before login controller, securitycore may be initialized but usercontext is still being set up = NRE
					}
				}
				catch (NullReferenceException)
				{
					return;
				}

				if (!Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group.PK.ToGuid()).IsAllowed && !group.IsCurrentUserGroupOwnerForThisGroup)
				{
					Parent.GK_GGInfo.AddError(Res.GetString("2187b3aa-7593-48d3-9915-d8a1501da99d", "You do not have security rights to add staff members to the '{0}' group.", group.GG_Code));
				}
			}
		}

		public void ValidateDomainsForStaffAndGroup()
		{
			if (ObjectFactory.Get<IADRegistry>().EntitiesToSync == EntitiesToSync.UsersAndGroups)
			{
				var staff = Parent.Staff;
				if (staff != null && staff.IsADIntegrationEnabled && !staff.GS_IsSystemAccount)
				{
					var group = Parent.Group;
					if (group != null && !group.GG_IsSystemDefined)
					{
						var adEntityProvider = ObjectFactory.Get<IADEntityProvider>();
						var staffDomain = adEntityProvider.GetADUser(staff).DomainCredentialDomainName;
						var groupDomain = adEntityProvider.GetADGroup(group).DomainCredentialDomainName;

						if (staffDomain != groupDomain)
						{
							var message = Res.GetString("6BFF576B-C5EB-4AF5-B159-C27A8C96AC52", "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.");
							staff.AddRowWarning(message);
							group.AddRowWarning(message);
						}
					}
				}
			}
		}

		#endregion

		#region GK_SkillLevel

		protected override void CheckGK_SkillLevel()
		{
			base.CheckGK_SkillLevel();
			if (Parent.GK_SkillLevel < 0 || Parent.GK_SkillLevel > 10)
			{
				Parent.GK_SkillLevelInfo.AddError(Res.GetString("79c4cc90-cb34-418a-bdac-b301a25b2338", "Skill level must be between 0 and 10 inclusive."));
			}
		}

		#endregion

		#region GK_MembershipType

		protected override void CheckGK_MembershipType()
		{
			base.CheckGK_MembershipType();
			ListValidation.ErrorIfInvalidCode(Parent.GK_MembershipTypeInfo);
		}

		#endregion

		#region GK_GS

		protected override void CheckGK_GS()
		{
			base.CheckGK_GS();
			ValidateDomainsForStaffAndGroup();
			if (Parent.Staff != null && !Parent.Staff.GS_IsActive)
			{
				if (!Parent.Staff.GS_IsSalesRep || (Parent.Group != null && !Parent.Group.GG_IsSales))
				{
					Parent.GK_GSInfo.AddError(Res.GetString("22ec4f07-9b14-4742-b233-7b7c1431f4f2", "Staff member {0} ({1}) is inactive and cannot be used.", Parent.Staff.GS_FullName, Parent.Staff.GS_Code));
				}
				else
				{
					Parent.GK_GSInfo.AddWarning(Res.GetString("6df8bb30-effc-4981-b4f4-73e585d94ed7", "Staff member {0} ({1}) is inactive.", Parent.Staff.GS_FullName, Parent.Staff.GS_Code));
				}
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info) => false;

		#endregion
	}
}
