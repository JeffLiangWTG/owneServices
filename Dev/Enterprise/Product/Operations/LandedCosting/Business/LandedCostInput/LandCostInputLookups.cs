//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandCostInputLookups
//
//    This class should be used for overriding collections in AutoLandCostInputLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LandedCosting.Business
{
	public class LandCostInputLookups : AutoLandCostInputLookups
	{
		public LandCostInputLookups(AutoLandCostInput parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DistributeCostBy
		{
			get { return Factory.GetCachedValue<CostDistributionMechanismList>(); }
		}

		public CodeDescriptionPairList ParentAssociableList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (LCInput.Header != null && LCInput.Header.Parent != null && LCInput.Header.Parent.CandidatesToDistributeCostTo != null)
				{
					foreach (ILandedCostDistributeTo lCDistributee in LCInput.Header.Parent.CandidatesToDistributeCostTo)
					{
						result.AddPair(lCDistributee.UniqueCode, lCDistributee.Description);
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList LandCostGroupList
		{
			get { return BuildLandCostGroupList(LCInput.Header); }
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
		public static CodeDescriptionPairList BuildLandCostGroupList(LandedCostHeader header)
		{
			var result = new UntranslatableCodeDescriptionPairList("Descriptions are defined in the database");
			if (header != null && header.PreferenceCalculator != null)
			{
				var preferences = header.PreferenceCalculator.GetPreferences();
				if (preferences != null)
				{
					foreach (var pref in preferences)
					{
						result.AddPair(pref.LCGroupID.ToString(), pref.LCGroupName);
					}
				}
			}
			return result;
		}

		protected LandCostInput LCInput
		{
			get { return Parent as LandCostInput; }
		}

		#endregion
	}
}
