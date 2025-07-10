using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusUnderbondValidation))]
	public abstract class CusUnderbondValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestC4_OutturnedValidation()
		{
			AssertHasMandatoryValidationWithOutturn(Underbond.C4_OuturnedInfo);
		}

		protected void AssertHasMandatoryValidationWithOutturn(ZPropertyInfo infoToCheck)
		{
			AssertEquals("Pre-Condition - No errors", false, infoToCheck.HasMessageErrors());

			var outturn = Underbond.Outturns.AddNew();
			infoToCheck.Value = infoToCheck.Value;
			AssertEquals("Should have a message error", true, infoToCheck.HasMessageErrors());
		}

		protected abstract CusUnderbond CreateNewUnderbond();

		protected CusUnderbond Underbond => underbond ?? (underbond = CreateNewUnderbond());

		CusUnderbond underbond;
	}
}
