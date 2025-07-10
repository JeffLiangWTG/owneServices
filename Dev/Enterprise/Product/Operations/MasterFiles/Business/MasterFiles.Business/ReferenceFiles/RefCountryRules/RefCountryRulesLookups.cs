//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCountryRulesLookups
//
//    This class should be used for overriding collections in AutoRefCountryRulesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRulesLookups : AutoRefCountryRulesLookups
	{
		public RefCountryRulesLookups(AutoRefCountryRules parent) : base(parent)
		{
		}

		#region RefCountry_List

		RefCountryCollection fRefCountry_List;
		public RefCountryCollection RefCountry_List
		{
			get
			{
				if (fRefCountry_List == null)
				{
					fRefCountry_List = new RefCountryCollection(Factory);
				}
				return fRefCountry_List;
			}
		}

		#endregion

		#region RefServiceLevel_List

		ActiveServiceLevelCollection fRefServiceLevel_List;
		public ActiveServiceLevelCollection RefServiceLevel_List
		{
			get
			{
				if (fRefServiceLevel_List == null)
				{
					fRefServiceLevel_List = new ActiveServiceLevelCollection(Factory);
				}
				return fRefServiceLevel_List;
			}
		}

		#endregion

		#region TransportMode_List

		public CodeDescriptionPairList TransportMode_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(Constants.TransportModes.Air, Res.GetString("9eb6995f-0641-468b-a03e-67cc2741e616", "Air Freight"));
				result.AddPair(Constants.TransportModes.Sea, Res.GetString("c503cbde-e4cd-42f8-87c4-62b2a30df772", "Sea Freight"));
				result.AddPair(Constants.TransportModes.Road, Res.GetString("648b096d-7e03-40c3-9c8a-19d12beca054", "Road Freight"));
				result.AddPair(Constants.TransportModes.Rail, Res.GetString("a778242d-a41f-4295-ae81-a243bf7fb240", "Rail Freight"));
				result.AddPair(Constants.TransportModes.Courier, Res.GetString("0809f156-bac0-4f92-82be-005d3f0a9667", "Courier"));
				result.AddPair(Constants.TransportModes.Other, Res.GetString("26666aef-3034-41a4-818d-af27533d0ded", "Other"));
				return result;
			}
		}

		#endregion

		#region UltimateConsigneeRule_List

		public CodeDescriptionPairList UltimateConsigneeRule_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Constants.UltimateConsigneeRuleTypes.Codes.NotRequired, Constants.UltimateConsigneeRuleTypes.Descriptions.NotRequired);
				result.AddPair(Constants.UltimateConsigneeRuleTypes.Codes.Mandatory, Constants.UltimateConsigneeRuleTypes.Descriptions.Mandatory);
				result.AddPair(Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities, Constants.UltimateConsigneeRuleTypes.Descriptions.VerifyWithCarrierOrLocalAuthorities);
				return result;
			}
		}

		#endregion
	}
}
