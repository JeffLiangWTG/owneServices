using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public sealed class BillValidatorTest : Customs.Business.Testing.BillValidatorTestClass
	{
		[ExpectNoExceptions]
		public void TestGetWarningMessage()
		{
			var billValidator = new BillValidatorForTest();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = "AIR";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(billValidator.GetWarningMessageString("", declaration), NUnit.Framework.Is.EqualTo(""));
				NUnit.Framework.Assert.That(billValidator.GetWarningMessageString("1", declaration), NUnit.Framework.Is.EqualTo("The MAWB should contain 11 digits."));
			});

			declaration.JE_TransportMode = "SEA";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(billValidator.GetWarningMessageString("", declaration), NUnit.Framework.Is.EqualTo("The MAWB should contain 11 digits."));
				NUnit.Framework.Assert.That(billValidator.GetWarningMessageString("1", declaration), NUnit.Framework.Is.EqualTo("The MAWB should contain 11 digits."));
			});
		}
	}

	class BillValidatorForTest : BillValidator
	{
		public string GetWarningMessageString(ZString billValue, BaseJobDeclaration declaration) => GetWarningMessage(billValue, declaration);
	}
}
