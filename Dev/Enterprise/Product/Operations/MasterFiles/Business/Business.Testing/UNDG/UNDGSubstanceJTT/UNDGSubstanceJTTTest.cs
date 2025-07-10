using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(UNDGSubstanceJTT))]
	sealed class UNDGSubstanceJTTTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClassificationCodes()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();

			AssertEquals(string.Empty, substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.JTT_ClassificationCode = "ABC";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.JTT_ClassificationCode = "ABC or DEF";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals("DEF", substance.ClassificationCode2);

			substance.JTT_ClassificationCode = "ABC OR DEF";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals("DEF", substance.ClassificationCode2);
		}

		public void TestJTT_FormattedLabels()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_Labels = "3+1+4";

			AssertEquals("3, 1, 4", substance.JTT_FormattedLabels);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<UNDGSubstanceJTT>();
		}
	}
}
