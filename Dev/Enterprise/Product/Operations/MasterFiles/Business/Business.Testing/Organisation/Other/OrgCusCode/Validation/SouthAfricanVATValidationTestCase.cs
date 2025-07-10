using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SouthAfricanVATValidationTestCase : NUnit.Framework.TestCase
	{
		public void TestIsValidCheckDigit()
		{
			Assert("Valid", new SouthAfricanVATValidation().IsValidCheckDigit("4770181941"));
			foreach (ZString validVAT in ValidVATs)
			{
				Assert(validVAT + " is valid", new SouthAfricanVATValidation().IsValidCheckDigit(validVAT));
			}
		}

		public void TestIsValidCheckDigitFalse()
		{
			Assert("Invalid", !new SouthAfricanVATValidation().IsValidCheckDigit("4770181942"));
		}

		#region Implementation

		readonly ZString[] ValidVATs = new ZString[] {  "4150161869",
															"4170120382",
															"4190108938",
															"4210102051",
															"4210102770",
															"4240114696",
															"4250103175",
															"4250173897",
															"4270120779",
															"4270180047",
															"4280109507",
															"4290109042",
															"4330109044" };

		#endregion
	}
}
