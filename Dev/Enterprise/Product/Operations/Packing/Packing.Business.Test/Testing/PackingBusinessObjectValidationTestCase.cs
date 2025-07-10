#if DEBUG

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class PackingBusinessObjectValidationTestCase : BusinessObjectValidationTestCase
	{
		protected void AssertNonNegativeValidation(ZPropertyInfo info, ZString description)
		{
			info.Value = (ZDecimal)0;
			AssertNoErrors(info);

			info.Value = (ZDecimal)1;
			AssertNoErrors(info);

			info.Value = (ZDecimal)(-1);
			AssertHasError(info, description + " cannot be less than 0.");
		}

		protected void AssertUnitValidation(ZPropertyInfo valueInfo, ZDecimal validValue, ZPropertyInfo unitInfo, ZString validUnit, ZString invalidUnit)
		{
			valueInfo.ClearValue();
			unitInfo.ClearValue();
			AssertEquals(false, unitInfo.HasErrors());

			valueInfo.Value = validValue;
			unitInfo.Value = validUnit;
			unitInfo.ClearValue();
			AssertHasErrors("Unit field is mandatory if the value field is not empty.", unitInfo);

			unitInfo.Value = validUnit;
			AssertNoErrors(unitInfo);
		}

		protected void AssertListValidation(ZPropertyInfo info, ZString valid, ZString invalid)
		{
			info.Value = valid;
			AssertNoErrors(info);

			info.Value = invalid;
			AssertHasErrors(info);

			info.Value = valid;
			AssertNoErrors(info);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected TestDataForPacking Data
		{
			get { return data ?? (data = new TestDataForPacking(Factory)); }
		}

		TestDataForPacking data;
	}
}

#endif
