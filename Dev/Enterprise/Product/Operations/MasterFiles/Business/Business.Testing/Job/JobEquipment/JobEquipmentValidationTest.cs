using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJEQ_Code()
		{
			var jobEqu1 = Factory.New<JobEquipment>();
			jobEqu1.JEQ_Code = "JE1";
			var jobEqu2 = Factory.New<JobEquipment>();
			jobEqu2.JEQ_Code = "JE2";
			AssertNoErrorContaining(jobEqu2.JEQ_CodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoError(jobEqu2.JEQ_CodeInfo, JobEquipmentValidation.DuplicateJobEquipmentCode);
			jobEqu2.JEQ_Code = "JE1";
			AssertHasError(jobEqu2.JEQ_CodeInfo, JobEquipmentValidation.DuplicateJobEquipmentCode);
			AssertNoErrorContaining(jobEqu2.JEQ_CodeInfo, MandatoryValidation.MustBeEntered);
			jobEqu2.JEQ_Code = ZString.Empty;
			AssertHasErrorContaining(jobEqu2.JEQ_CodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckJEQ_Description()
		{
			var jobEquipment = Factory.New<JobEquipment>();
			jobEquipment.JEQ_Description = ZString.Empty;
			AssertHasErrorContaining(jobEquipment.JEQ_DescriptionInfo, MandatoryValidation.MustBeEntered);
			jobEquipment.JEQ_Description = "Something";
			AssertNoErrorContaining(jobEquipment.JEQ_DescriptionInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
