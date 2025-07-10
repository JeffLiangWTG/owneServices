using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	public class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckCI_CC()
		{
			var classificationIMP1 = Factory.New<BaseCusClassification>();
			classificationIMP1.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP1.CC_LookupCode = "CL1";
			var classificationIMP2 = Factory.New<BaseCusClassification>();
			classificationIMP2.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP2.CC_LookupCode = "CL2";
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = Factory.New<BaseCusClassPartPivot>();
			pivot1.CI_OP = part.PK;
			pivot1.CI_RN_NKCountry = classificationIMP1.CC_RN_NKCountryCode;
			pivot1.CI_CC = classificationIMP1.PK;
			var pivot2 = Factory.New<BaseCusClassPartPivot>();
			pivot2.CI_OP = part.PK;
			pivot2.CI_RN_NKCountry = classificationIMP2.CC_RN_NKCountryCode;
			pivot2.CI_CC = classificationIMP1.PK;
			AssertNoErrors(pivot2.CI_CCInfo);
			pivot2.CI_CC = classificationIMP2.PK;
			AssertNoErrors(pivot2.CI_CCInfo);
		}
	}
}
