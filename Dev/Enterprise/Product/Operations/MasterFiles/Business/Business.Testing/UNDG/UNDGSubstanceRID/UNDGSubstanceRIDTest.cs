using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(UNDGSubstanceRID))]
	sealed class UNDGSubstanceRIDTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLabels()
		{
			var substance = Factory.New<UNDGSubstanceRID>();

			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.RID_Label1);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.RID_Label2);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.RID_Label3);
			AssertEquals("Precondition: Labels are empty", ZString.Empty, substance.RID_Label4);

			substance.RID_Labels = "1+23";

			AssertEquals("Label 1 is populated", "1", substance.RID_Label1);
			AssertEquals("Label 2 is populated", "23", substance.RID_Label2);
			AssertEquals("Label 3 is empty", ZString.Empty, substance.RID_Label3);
			AssertEquals("Label 4 is empty", ZString.Empty, substance.RID_Label4);

			substance.RID_Labels = "1+23+45+6";

			AssertEquals("All Labels are populated", "1", substance.RID_Label1);
			AssertEquals("All Labels are populated", "23", substance.RID_Label2);
			AssertEquals("All Labels are populated", "45", substance.RID_Label3);
			AssertEquals("All Labels are populated", "6", substance.RID_Label4);
		}

		public void TestClassificationCodes()
		{
			var substance = Factory.New<UNDGSubstanceRID>();

			AssertEquals(string.Empty, substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.RID_ClassificationCode = "ABC";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals(string.Empty, substance.ClassificationCode2);

			substance.RID_ClassificationCode = "ABC or DEF";

			AssertEquals("ABC", substance.ClassificationCode1);
			AssertEquals("DEF", substance.ClassificationCode2);

			substance.RID_ClassificationCode = "ABC OR DEF";

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
			return factory.NewWithValidTestData<UNDGSubstanceRID>();
		}
	}
}
