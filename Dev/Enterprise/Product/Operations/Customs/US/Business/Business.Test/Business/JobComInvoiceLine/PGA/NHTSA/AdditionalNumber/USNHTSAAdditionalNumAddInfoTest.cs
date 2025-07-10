using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNHTSAAdditionalNumAddInfo))]
	public class USNHTSAAdditionalNumAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAdditionalNum()
		{
			var additionalNum = Factory.New<NHTSAAdditionalNum>();
			var addInfo = new USNHTSAAdditionalNumAddInfo(additionalNum.B7_AddInfoDataInfo);
			AssertEquals(additionalNum.PK, addInfo.AdditionalNum.PK);
		}

		public void TestHumanReadableNames()
		{
			var additionalNum = Factory.New<NHTSAAdditionalNum>();
			AssertEquals("Number Type", additionalNum.US_NHTAdditionalIdentityNumQualifierInfo.HumanReadableName);
			AssertEquals("Number", additionalNum.US_NHTAdditionalIdentityNumberInfo.HumanReadableName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var additionalNum = Factory.New<NHTSAAdditionalNum>();
			var addInfo = new USNHTSAAdditionalNumAddInfo(additionalNum.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
