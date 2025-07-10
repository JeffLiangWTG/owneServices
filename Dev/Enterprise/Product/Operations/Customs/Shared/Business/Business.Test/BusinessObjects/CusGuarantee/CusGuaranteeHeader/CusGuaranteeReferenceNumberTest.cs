using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceNumber))]
	sealed class CusGuaranteeReferenceNumberTest : CusCodeDataTest<CusGuaranteeReferenceNumber>
	{
		protected override IEnumerable<CusGuaranteeReferenceNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewCusGuaranteeReferenceNumber(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewCusGuaranteeReferenceNumber(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewCusGuaranteeReferenceNumber();
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GuaranteeCusCodeDataTypeList.Codes.GRN, cusGuaranteeReferenceNumber.CY_Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeReferenceNumber = GetNewCusGuaranteeReferenceNumber();
		}
		CusGuaranteeReferenceNumber cusGuaranteeReferenceNumber;

		CusGuaranteeReferenceNumber GetNewCusGuaranteeReferenceNumber(BusinessObjectFactory factory = null)
		{
			var currentFactory = factory ?? Factory;
			var cusGuarantee = currentFactory.NewWithValidTestData<CusGuaranteeHeaderForTest>();
			var cusGuaranteeRefNumber = cusGuarantee.AdditionalGuaranteeReferences.AddNew();
			return cusGuaranteeRefNumber;
		}
	}
}
