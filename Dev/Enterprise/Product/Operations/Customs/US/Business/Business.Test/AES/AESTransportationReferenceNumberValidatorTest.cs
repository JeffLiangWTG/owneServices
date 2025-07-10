using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESTransportationReferenceNumberValidatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var validator = new AESTransportationReferenceNumberValidator();
			var checkDigitmessageError = "Invalid check digit. The last digit should be '0'";

			var dummy = Factory.New<DummyBusinessObject>();

			dummy.Z0_VarCharMax = "AMF";
			dummy.Z0_VarCharMax = "AMF123789432";
			validator.ValidateAndAddMessageError(dummy.Z0_VarCharMaxInfo);
			AssertNoMessageError(dummy.Z0_VarCharMaxInfo, checkDigitmessageError);

			dummy.Z0_VarCharMax = "123-23789432";
			validator.ValidateAndAddMessageError(dummy.Z0_VarCharMaxInfo);
			AssertHasMessageError(dummy.Z0_VarCharMaxInfo, checkDigitmessageError);

			dummy.Z0_VarCharMax = "123-23789430";
			validator.ValidateAndAddMessageError(dummy.Z0_VarCharMaxInfo);
			AssertNoMessageError(dummy.Z0_VarCharMaxInfo, checkDigitmessageError);
			if (ErrorReporter.LastKeyReported == "Validation:Z0_VarCharMax")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
