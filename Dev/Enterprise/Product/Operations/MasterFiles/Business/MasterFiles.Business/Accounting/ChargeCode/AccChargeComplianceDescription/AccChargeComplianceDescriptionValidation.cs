//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeComplianceDescriptionValidation
//
//    This class should be used for overriding validation in AutoAccChargeComplianceDescriptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeComplianceDescriptionValidation : AutoAccChargeComplianceDescriptionValidation
	{
		public AccChargeComplianceDescriptionValidation(AutoAccChargeComplianceDescription parent) : base(parent)
		{
			Parent = parent as AccChargeComplianceDescription;
		}
		new AccChargeComplianceDescription Parent { get; }

		protected override void CheckADE_JobType()
		{
			base.CheckADE_JobType();
			ObjectFactory.Get<IJobConfigurationHelperFactory>().GetJobTypeHelper(Parent).Validate();
		}

		protected override void CheckADE_TransportMode()
		{
			base.CheckADE_TransportMode();
			ObjectFactory.Get<IJobConfigurationHelperFactory>().GetTransportModeHelper(Parent).Validate();
		}

		protected override void CheckADE_SupplyType()
		{
			base.CheckADE_SupplyType();
			ObjectFactory.Get<IJobConfigurationHelperFactory>().GetSupplyTypeHelper(Parent).Validate();
		}

		protected override void CheckADE_Description()
		{
			base.CheckADE_Description();
			MandatoryValidation.CheckEntered(Parent.ADE_DescriptionInfo);
		}
	}
}
