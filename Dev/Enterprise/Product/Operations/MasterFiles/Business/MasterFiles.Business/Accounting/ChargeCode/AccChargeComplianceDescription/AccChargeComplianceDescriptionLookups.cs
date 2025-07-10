//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeComplianceDescriptionLookups
//
//    This class should be used for overriding collections in AutoAccChargeComplianceDescriptionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeComplianceDescriptionLookups : AutoAccChargeComplianceDescriptionLookups
	{
		public AccChargeComplianceDescriptionLookups(AutoAccChargeComplianceDescription parent) : base(parent)
		{
			Parent = parent as AccChargeComplianceDescription;
		}
		new AccChargeComplianceDescription Parent { get; }

		public CodeDescriptionPairList JobTypeList => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetJobTypeHelper(Parent).GetLookupList();

		public CodeDescriptionPairList TransportModeList => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetTransportModeHelper(Parent).GetLookupList();

		public CodeDescriptionPairList SupplyTypeList => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetSupplyTypeHelper(Parent).GetLookupList();
	}
}
