using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotRef))]
	sealed class CusClassPartPivotRefBaseOnlyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniqueIndexFailureHandler()
		{
			var pivot = PivotRef;
			var handler = GetUniqueIndexFailureHandler(pivot);
			CombineAssertions(() =>
			{
				AssertType<CusClassPartPivotRefUniqueIndexFailureHandler>("Correct Handler Type", handler);
				AssertSame("Cached", handler, GetUniqueIndexFailureHandler(pivot));
			});
		}

		public void TestITypeDeciderContext()
		{
			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusClassPartPivotRef>() as ITypeDeciderContext).Country);

				var pivotRef = PivotRef;
				pivotRef.CusClassPartPivot.CI_RN_NKCountry = "CN";
				AssertEquals("From CI_RN_NKCountry", "CN", (pivotRef as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => PivotRef;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreatePivotRef(factory);

		CusClassPartPivotRef PivotRef => pivotRef ?? (pivotRef = CreatePivotRef(Factory));
		CusClassPartPivotRef pivotRef;

		static CusClassPartPivotRef CreatePivotRef(BusinessObjectFactory factory)
		{
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "111";
			var pivot = part.PivotsForBinding.AddNew();
			var result = factory.New<CusClassPartPivotRef>();
			result.CIR_CI = pivot.PK;
			result.CIR_ReferenceType = "AGA";
			result.CIR_ReferenceNumber = "REF";
			return result;
		}

		static IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(CusClassPartPivotRef partPivotRef)
		{
			return ((IEnumerable<IUniqueIndexFailureHandler>)typeof(CusClassPartPivotRef).GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(partPivotRef, null)).Single();
		}
	}
}
