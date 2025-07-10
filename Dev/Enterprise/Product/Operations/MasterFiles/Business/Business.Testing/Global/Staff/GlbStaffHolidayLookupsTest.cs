using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing.Global.Staff
{
	public class GlbStaffHolidayLookupsTest : TestCaseWithFactory
	{
		public void TestHolidayLookupsReal_CodesMatchCargoWiseDefintions_StaffHolidayApproval()
		{
			var pairListCodes = typeof(GlbStaffHolidayLookupsReal).GetAllPublicConstantValues();
			var definitionsCodes = typeof(StaffHolidayApprovalCodes).GetAllPublicConstantValues();
			if (typeof(ZLookups).IsAssignableFrom(typeof(GlbStaffHolidayLookupsReal)))
			{
				pairListCodes.Remove(ZLookups.LookupsBindingMember);
			}

			AssertContainsExactElementsInAnyOrder("All GlbStaffHolidayLookupReal codes in Dev should exist in CargoWise.Definitions", pairListCodes, definitionsCodes);
		}

		public void TestHolidayLookups_CodesMatchCargoWiseDefintions_StaffHolidayRecordType()
		{
			var pairListCodes = typeof(GlbStaffHolidayLookups.RecordTypes).GetAllPublicConstantValues();
			pairListCodes.Add(StaffHolidayRecordTypeCodes.BufferManagementLeave);

			var definitionsCodes = typeof(StaffHolidayRecordTypeCodes).GetAllPublicConstantValues();
			if (typeof(ZLookups).IsAssignableFrom(typeof(GlbStaffHolidayLookups)))
			{
				pairListCodes.Remove(ZLookups.LookupsBindingMember);
			}

			AssertContainsExactElementsInAnyOrder("All GlbStaffHolidayLookup codes in Dev should exist in CargoWise.Definitions", pairListCodes, definitionsCodes);
		}
	}
}
