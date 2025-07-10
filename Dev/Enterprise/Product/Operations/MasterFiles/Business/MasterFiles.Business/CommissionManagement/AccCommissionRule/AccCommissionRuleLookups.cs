//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleLookups
//
//    This class should be used for overriding collections in AutoAccCommissionRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleLookups : AutoAccCommissionRuleLookups
	{
		public AccCommissionRuleLookups(AutoAccCommissionRule parent)
			: base(parent)
		{
		}

		protected new AccCommissionRule Parent
		{
			get { return (AccCommissionRule)base.Parent; }
		}

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
