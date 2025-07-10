using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDA74Container))]
	sealed class DocDA74ContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerNumbers()
		{
			CreateAndTestContainer("", "", ",,,,,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("A", "", "A,,,,,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("AB", "", "A,B,,,,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABC", "", "A,B,C,,,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD", "", "A,B,C,D,,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD1", "", "A,B,C,D,1,,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD12", "", "A,B,C,D,1,2,,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD123", "", "A,B,C,D,1,2,3,,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD1234", "", "A,B,C,D,1,2,3,4,,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD12345", "", "A,B,C,D,1,2,3,4,5,,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD123456", "", "A,B,C,D,1,2,3,4,5,6,", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "", "A,B,C,D,1,2,3,4,5,6,7", ",,,,,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "L", "A,B,C,D,1,2,3,4,5,6,7", "L,,,,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "LE", "A,B,C,D,1,2,3,4,5,6,7", "L,E,,,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "LEW", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,,,,,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI3", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,,,,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI31", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,,,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI310", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,0,,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI3102", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,0,2,,,");
			CreateAndTestContainer("ABCD1234567", "LEWI31023", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,0,2,3,,");
			CreateAndTestContainer("ABCD1234567", "LEWI310235", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,0,2,3,5,");
			CreateAndTestContainer("ABCD1234567", "LEWI3102357", "A,B,C,D,1,2,3,4,5,6,7", "L,E,W,I,3,1,0,2,3,5,7");
		}

		void CreateAndTestContainer(ZString containerNum1, ZString containerNum2, ZString expectedValues1, ZString expectedValues2)
		{
			container = new DocDA74Container(containerNum1, containerNum2, Factory);

			AssertEquals("FirstContainer ContainerNumber", containerNum1, container.FirstContainerNumber);
			AssertEquals("SecondContainer ContainerNumber", containerNum2, container.SecondContainerNumber);

			ZString[] firstContainerValues = expectedValues1.Split(',');
			ZString[] secondContainerValues = expectedValues2.Split(',');

			AssertEquals("FirstContainer Prefix1", firstContainerValues[0], container.FirstContainerPrefix1);
			AssertEquals("FirstContainer Prefix2", firstContainerValues[1], container.FirstContainerPrefix2);
			AssertEquals("FirstContainer Prefix3", firstContainerValues[2], container.FirstContainerPrefix3);
			AssertEquals("FirstContainer Prefix4", firstContainerValues[3], container.FirstContainerPrefix4);
			AssertEquals("FirstContainer Number1", firstContainerValues[4], container.FirstContainerNumber1);
			AssertEquals("FirstContainer Number2", firstContainerValues[5], container.FirstContainerNumber2);
			AssertEquals("FirstContainer Number3", firstContainerValues[6], container.FirstContainerNumber3);
			AssertEquals("FirstContainer Number4", firstContainerValues[7], container.FirstContainerNumber4);
			AssertEquals("FirstContainer Number5", firstContainerValues[8], container.FirstContainerNumber5);
			AssertEquals("FirstContainer Number6", firstContainerValues[9], container.FirstContainerNumber6);
			AssertEquals("FirstContainer Number7", firstContainerValues[10], container.FirstContainerNumber7);

			AssertEquals("SecondContainer Prefix1", secondContainerValues[0], container.SecondContainerPrefix1);
			AssertEquals("SecondContainer Prefix2", secondContainerValues[1], container.SecondContainerPrefix2);
			AssertEquals("SecondContainer Prefix3", secondContainerValues[2], container.SecondContainerPrefix3);
			AssertEquals("SecondContainer Prefix4", secondContainerValues[3], container.SecondContainerPrefix4);
			AssertEquals("SecondContainer Number1", secondContainerValues[4], container.SecondContainerNumber1);
			AssertEquals("SecondContainer Number2", secondContainerValues[5], container.SecondContainerNumber2);
			AssertEquals("SecondContainer Number3", secondContainerValues[6], container.SecondContainerNumber3);
			AssertEquals("SecondContainer Number4", secondContainerValues[7], container.SecondContainerNumber4);
			AssertEquals("SecondContainer Number5", secondContainerValues[8], container.SecondContainerNumber5);
			AssertEquals("SecondContainer Number6", secondContainerValues[9], container.SecondContainerNumber6);
			AssertEquals("SecondContainer Number7", secondContainerValues[10], container.SecondContainerNumber7);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDA74Container("", "", Factory);
		}

		DocDA74Container container;
	}
}
