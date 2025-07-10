//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSundryChargesLookups
//
//    This class should be used for overriding collections in AutoJobSundryChargesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class JobSundryChargesLookups : AutoJobSundryChargesLookups
	{
		public JobSundryChargesLookups(AutoJobSundryCharges parent)
			: base(parent) { }

		public override OrgHeaderCollection BillToParties
		{
			get { return new DebtorCollection(Factory); }
		}

		public ICodeDescriptionPairListWithDefaultCode Types
		{
			get { return AgencyRegistry.Instance.SundryChargeTypes.GetFactoryCachedValue(Factory); }
		}

		public ICodeDescriptionPairListWithDefaultCode Modes
		{
			get { return AgencyRegistry.Instance.SundryChargeModes.GetFactoryCachedValue(Factory); }
		}

		public ICodeDescriptionPairListWithDefaultCode Activities
		{
			get { return AgencyRegistry.Instance.SundryChargeActivities.GetFactoryCachedValue(Factory); }
		}
	}
}
