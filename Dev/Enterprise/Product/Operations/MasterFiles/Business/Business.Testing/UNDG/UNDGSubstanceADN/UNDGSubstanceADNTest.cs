using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGSubstanceADN))]
	sealed class UNDGSubstanceADNTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLabels()
		{
			var substance = Factory.New<UNDGSubstanceADN>();

			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.ADN_Label1);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.ADN_Label2);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.ADN_Label3);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.ADN_Label4);

			substance.ADN_Labels = "1+23";

			AssertEquals("Label 1 is populated", "1", substance.ADN_Label1);
			AssertEquals("Label 2 is populated", "23", substance.ADN_Label2);
			AssertEquals("Label 3 is empty", ZString.Empty, substance.ADN_Label3);
			AssertEquals("Label 4 is empty", ZString.Empty, substance.ADN_Label4);

			substance.ADN_Labels = "1+23+45+6";

			AssertEquals("All Labels are populated", "1", substance.ADN_Label1);
			AssertEquals("All Labels are populated", "23", substance.ADN_Label2);
			AssertEquals("All Labels are populated", "45", substance.ADN_Label3);
			AssertEquals("All Labels are populated", "6", substance.ADN_Label4);
		}

		public void TestClassificationCodes()
		{
			var substance = Factory.New<UNDGSubstanceADN>();

			AssertEquals(string.Empty, substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.ADN_ClassificationCode = "ABC";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.ADN_ClassificationCode = "ABC or DEF";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals("DEF", substance.ClassificationCode2);

			substance.ADN_ClassificationCode = "ABC OR DEF";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals("DEF", substance.ClassificationCode2);
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
			return factory.NewWithValidTestData<UNDGSubstanceADN>();
		}
	}
}
