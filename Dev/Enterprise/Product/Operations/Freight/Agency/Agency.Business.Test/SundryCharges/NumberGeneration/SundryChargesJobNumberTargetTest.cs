using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class SundryChargesJobNumberTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(AgencyRegistry.Instance.SundryJobNumberCustomisation, "SCN");
			NumberGeneratorTarget target = new SundryChargesJobNumberTarget();
			target.Context = new NumberGeneratorContext();
			AssertCustomisation("Should find the Sundry Job Number Customisation", "SCN", target.NumberCustomisation);
			AssertLocation(AgencyRegistry.Instance.SundryJobNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobSundryChargesSchema.D4_JobNumber.MaxLength, target.MaxLength);
			AssertEquals("job number", target.Name);
		}
	}
}
