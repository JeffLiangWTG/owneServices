using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressValidationForTesting : JobDocAddressValidation
	{
		public JobDocAddressValidationForTesting(AutoJobDocAddress parent) : base(parent)
		{
		}

		public bool ShouldValidateFKToCancelledRecord_Exposed(ZPropertyInfo info) => base.ShouldValidateFKToCancelledRecord(info);
	}
}
