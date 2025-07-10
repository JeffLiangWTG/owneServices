using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotRefForTest))]
	sealed class CusClassPartPivotRefTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var cusClassPartPivotRef = Factory.New<CusClassPartPivotRefForTest>();
			NUnit.Framework.Assert.That(cusClassPartPivotRef.CIR_ReferenceType, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "Default CIR_ReferenceType");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return PivotRefs;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return PivotRefs;
		}

		CusClassPartPivotRefForTest PivotRefs
		{
			get
			{
				if (pivotRefs == null)
				{
					pivotRefs = Factory.New<CusClassPartPivotRefForTest>();
					pivotRefs.CIR_CI = Pivot.PK;
					pivotRefs.CIR_ReferenceNumber = "123";
				}

				return pivotRefs;
			}
		}

		CusClassPartPivotRefForTest pivotRefs;
		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var part = Factory.New<OrgSupplierPart>();
					part.OP_PartNum = "NEWPROD1";
					pivot = part.PivotsForBinding.AddNew();
				}

				return pivot;
			}
		}

		CusClassPartPivot pivot;
	}
}
