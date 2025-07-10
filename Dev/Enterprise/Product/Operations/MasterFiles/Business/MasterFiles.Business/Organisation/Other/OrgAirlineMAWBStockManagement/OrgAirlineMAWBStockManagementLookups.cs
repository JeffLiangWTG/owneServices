//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgAirlineMAWBStockManagementLookups
//
//    This class should be used for overriding collections in AutoOrgAirlineMAWBStockManagementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("See WI00292226")]
	public class OrgAirlineMAWBStockManagementLookups : AutoOrgAirlineMAWBStockManagementLookups
	{
		public OrgAirlineMAWBStockManagementLookups(AutoOrgAirlineMAWBStockManagement parent) : base(parent)
		{
		}
		public CodeDescriptionPairList AllowUseOtherBranchStocks
		{
			get
			{
				var types = new CodeDescriptionPairList();
				types.AddPair("Y", ResString.GetMultilingualString("4f1d1f9d-3911-4c13-84e3-c0c3d11e90b7", "Yes"));
				types.AddPair("N", ResString.GetMultilingualString("0a7a2f35-e134-41c3-88cb-cc69c6f7096c", "No"));
				types.AddPair("R", ResString.GetMultilingualString("39ae10c7-0bc8-4cf8-b19b-621d2c73e826", "Registry"));
				return types;
			}
		}

		GlbBranchCollection branches;

		public override GlbBranchCollection Branches
		{
			get
			{
				branches ??= new GlbBranchCollection(Factory);
				branches.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault((ZString)(NoResString)"Company", "Property", ((AutoOrgAirlineMAWBStockManagement)Parent).OHM_GC_Company));
				return branches;
			}
		}
	}
}
