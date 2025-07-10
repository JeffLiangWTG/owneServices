using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLinkLookups : AutoGlbGroupLinkLookups
	{
		public GlbGroupLinkLookups(AutoGlbGroupLink parent) : base(parent)
		{
		}

		#region Staff Membership Types List

		public CodeDescriptionPairList StaffMembershipTypesList
		{
			get
			{
				return Factory.GetCachedValue("GlbGroupLinkLookups.StaffMembershipTypesList",
					() =>
					{
						return new CodeDescriptionPairList(OLookUpEditType.StaffMembershipType);
					});
			}
		}

		#endregion
	}
}
