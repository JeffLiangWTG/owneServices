using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(ThreeLetterRefAirline))]
	sealed class ThreeLetterRefAirlineTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestLoadFromAirline3LetterCode()
		{
			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			var airline2 = Factory.NewWithValidTestData<RefAirline>();
			var airline3 = Factory.NewWithValidTestData<RefAirline>();
			var airline4 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_TwoCharacterCode = "~1";
			airline1.RM_ThreeLetterCode = "~1A";
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "A00";
			airline2.RM_TwoCharacterCode = "~2";
			airline2.RM_ThreeLetterCode = "~2A";
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			airline3.RM_TwoCharacterCode = "~3";
			airline3.RM_ThreeLetterCode = "~3A";
			airline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "A01";
			airline4.RM_TwoCharacterCode = "~3";
			airline4.RM_ThreeLetterCode = "~3A";
			airline4.RM_EagleAddedAirlinePrefixOrAccountingCode = "A01";
			airline4.RM_IsActive = false;
			Factory.Save();
			AssertEquals("A00", ThreeLetterRefAirline.LoadFromAirline3LetterCode(new BusinessObjectFactory(), "~1A").RM_EagleAddedAirlinePrefixOrAccountingCode);
			AssertNull(ThreeLetterRefAirline.LoadFromAirline3LetterCode(new BusinessObjectFactory(), "~2A"));
			AssertEquals("A01", ThreeLetterRefAirline.LoadFromAirline3LetterCode(new BusinessObjectFactory(), "~3A").RM_EagleAddedAirlinePrefixOrAccountingCode);
		}
	}
}
