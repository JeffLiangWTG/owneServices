//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCFXUpliftConfigurationViewLookups
//
//    This class should be used for overriding collections in AutoAccCFXUpliftConfigurationViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.JobConfigurationLookupsExtensions;

namespace Enterprise.MasterFiles.Business
{
	public class AccCFXUpliftConfigurationViewLookups : AutoAccCFXUpliftConfigurationViewLookups
	{
		public AccCFXUpliftConfigurationViewLookups(AutoAccCFXUpliftConfigurationView parent) : base(parent)
		{
		}

		public new IJobConfiguration Parent => (IJobConfiguration)base.Parent;

		public CodeDescriptionPairList JobTypesList
		{
			get
			{
				var jobTypeList = GetJobTypeList();
				jobTypeList.RemoveCode(JobInvoicingConsumerTypes.ForwardingConsol);
				return jobTypeList;
			}
		}

		public CodeDescriptionPairList DirectionsList => Parent.GetDirectionList();

		public CodeDescriptionPairList TransportModesList => Parent.GetTransportModeList();
	}
}
