using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OverallStaffCommissionRuleLookups : ZLookups
	{
		public OverallStaffCommissionRuleLookups(OverallStaffCommissionRule parent)
			: base(parent)
		{
		}

		new OverallStaffCommissionRule Parent
		{
			get { return (OverallStaffCommissionRule)base.Parent; }
		}

		#region Status

		public ReadOnlyCodeDescriptionPairList Statuses
		{
			get
			{
				if (Parent.Source == OverallStaffCommissionRuleSource.Staff)
				{
					return new StaffCommissionRuleStatusTypes();
				}
				else if (Parent.Source == OverallStaffCommissionRuleSource.Group)
				{
					return new GroupCommissionRuleStatusTypes();
				}
				else
				{
					return new ReadOnlyCodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region Groups

		public SalesTeamCollection SalesTeams
		{
			get
			{
				var staff = Parent.Staff;
				if (staff == null)
				{
					return new SalesTeamCollection(Factory);
				}

				return new SalesTeamCollection(staff);
			}
		}

		#endregion

		#region Companies

		public GlbCompanyCollection Companies
		{
			get { return new GlbCompanyCollection(Factory); }
		}

		#endregion

		#region Products

		public ReadOnlyCodeDescriptionPairList Products
		{
			get { return CommissionRuleLookups.New(Parent).Products; }
		}

		#endregion

		#region Services

		public ReadOnlyCodeDescriptionPairList Services
		{
			get { return CommissionRuleLookups.New(Parent).Services; }
		}

		#endregion

		#region SubModules

		public ReadOnlyCodeDescriptionPairList SubModules
		{
			get { return CommissionRuleLookups.New(Parent).SubModules; }
		}

		#endregion

		#region Modes

		public ReadOnlyCodeDescriptionPairList Modes
		{
			get { return CommissionRuleLookups.New(Parent).Modes; }
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Commission Basis

		public ReadOnlyCodeDescriptionPairList CommissionBasisType
		{
			get { return CommissionLookups.New(Factory).CommissionBasisType; }
		}

		#endregion

		#region Trigger Types

		public ReadOnlyCodeDescriptionPairList TriggerTypes
		{
			get { return CommissionLookups.New(Factory).TriggerTypes; }
		}

		#endregion
	}
}
