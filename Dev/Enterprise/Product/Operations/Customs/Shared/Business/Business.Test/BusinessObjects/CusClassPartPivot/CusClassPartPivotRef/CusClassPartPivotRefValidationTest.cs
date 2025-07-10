using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusClassPartPivotRefValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCIR_ReferenceType()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(pivotRef.CIR_ReferenceTypeInfo);
		}

		public void TestCheckCIR_ReferenceNumber_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(pivotRef.CIR_ReferenceNumberInfo);
		}

		public void TestCheckCIR_ReferenceNumber_CheckDuplication()
		{
			CombineAssertions(() =>
			{
				var pivotRef2 = CreatePivotRef(Factory, pivot, "AGA", "REF123");
				AssertHasErrorContaining("Duplicate", pivotRef2.CIR_ReferenceNumberInfo, CannotBeDuplicated);

				pivotRef2.CIR_ReferenceNumber = "DDD";
				AssertNoErrorContaining("Unique", pivotRef2.CIR_ReferenceNumberInfo, CannotBeDuplicated);
			});
		}

		public void TestCheckCIR_ReferenceNumber_CheckDuplication_ExistingInDB()
		{
			Factory.Save();
			CombineAssertions(() =>
			{
				var pivotRef2 = CreatePivotRef(new BusinessObjectFactory(), pivot, "AGA", "REF123");
				AssertHasErrorContaining("Duplicate", pivotRef2.CIR_ReferenceNumberInfo, CannotBeDuplicated);

				pivotRef2.CIR_ReferenceNumber = "DDD";
				AssertNoErrorContaining("Unique", pivotRef2.CIR_ReferenceNumberInfo, CannotBeDuplicated);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "111";
			pivot = part.PivotsForBinding.AddNew();
			pivotRef = CreatePivotRef(Factory, pivot, "AGA", "REF123");
		}

		BaseCusClassPartPivot pivot;
		CusClassPartPivotRef pivotRef;

		const string CannotBeDuplicated = "cannot be duplicated.";

		static CusClassPartPivotRef CreatePivotRef(BusinessObjectFactory factory, BaseCusClassPartPivot partPivot, string referenceType, string number)
		{
			var result = factory.New<CusClassPartPivotRef>();
			result.CIR_CI = partPivot.PK;
			result.CIR_ReferenceType = referenceType;
			result.CIR_ReferenceNumber = number;
			return result;
		}
	}
}
