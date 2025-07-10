using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusAUNexdocECMCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetSupplementaryCachedList()
		{
			var list = RefCusAUNexdocECMCodeHelper.GetSupplementaryCachedList(Factory, 'D', "AMF", new ZDateTime(2019, 3, 6));
			AssertEquals("DM, EK", list.CodesAsString);
		}

		public void TestGetPackTypeCachedList()
		{
			var list = RefCusAUNexdocECMCodeHelper.GetPackTypeCachedList(Factory, 'D', "AMF", new ZDateTime(2019, 3, 6));
			AssertEquals("CS, PO", list.CodesAsString);
		}

		public void TestGetPreservationCachedList()
		{
			var list = RefCusAUNexdocECMCodeHelper.GetPreservationCachedList(Factory, 'D', "AMF", new ZDateTime(2019, 3, 6));
			AssertEquals("H, UR", list.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NPCKT", "NEXDOCS Pack Type");
			helper.CreateNewOrGetExistingCusCodeType("NPRST", "NEXDOCS Preserve Type");
			helper.CreateNewOrGetExistingCusCodeType("NSUPP", "NEXDOCS Supplementary Code");
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var date3 = new ZDateTime(2019, 3, 3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "PO", "Pods", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPCKT", "PU", "TRAY PACK", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "PT", "PALLET", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", "CS", "BONE-IN CARCASE QUARTERS AND SIDES", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "H", "cooked", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NPRST", "M", "comminuted", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "D", "dried", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", "UR", "unrefrigerated", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "EK", "MORE EU BALAI PRODUCT", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("US", "NSUPP", "E", "ELECTRICAL STIMULATION", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "X", "BARROW PORK - SINGAPORE ONLY", date1, date3);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", "DM", "MANUFACTURING GRADE", date1, date2);
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "H", "AMF", "PO", "EK");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "M", "AMF", "PU", "E");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "D", "AMF", "PT", "X");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "UR", "AMF", "CS", "DM");
			Factory.Save();
		}
	}
}
