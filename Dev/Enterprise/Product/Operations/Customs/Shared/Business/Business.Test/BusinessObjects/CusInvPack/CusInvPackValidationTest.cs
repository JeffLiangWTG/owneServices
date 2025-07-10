using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInvPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_TypeOfDifference_ValidOrInvalid()
		{
			CombineAssertions(() =>
			{
				parent.B5_TypeOfDifference = "N/A";
				AssertHasErrorContaining("Invalid Code according to DB Constraints", parent.B5_TypeOfDifferenceInfo, ListValidation.InvalidCodeError);

				parent.B5_TypeOfDifference = CusUnloadedStateList.Codes.DEC;
				AssertNoErrors("no error", parent.B5_TypeOfDifferenceInfo);

				parent.B5_TypeOfDifference = CusUnloadedStateList.Codes.DIF;
				AssertNoErrors("no error", parent.B5_TypeOfDifferenceInfo);

				parent.B5_TypeOfDifference = CusUnloadedStateList.Codes.MIS;
				AssertNoErrors("no error", parent.B5_TypeOfDifferenceInfo);

				parent.B5_TypeOfDifference = CusUnloadedStateList.Codes.NEW;
				AssertNoErrors("no error", parent.B5_TypeOfDifferenceInfo);
			});
		}

		public void TestCheckB5_UnitCount()
		{
			CombineAssertions(() =>
			{
				parent.B5_UnitCount = -1;
				AssertHasErrorContaining("ValueCannotBeNegative", parent.B5_UnitCountInfo, MandatoryValidation.ValueCannotBeNegative);

				parent.B5_UnitCount = 0;
				AssertNoErrors("no error", parent.B5_UnitCountInfo);
			});
		}

		public void TestCheckB5_UnitsReleased()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(parent.B5_UnitsReleasedInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<CusInvPackForTest>();
		}
		CusInvPack parent;

		class CusInvPackForTest : CusInvPack<DummyBusinessObject, CusInvPackLookups, CusInvPackValidation>
		{
			public CusInvPackForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
