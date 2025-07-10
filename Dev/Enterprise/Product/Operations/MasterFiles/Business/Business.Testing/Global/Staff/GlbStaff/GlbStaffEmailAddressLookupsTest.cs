using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffEmailAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAllEmailTypeList()
		{
			var lookups = GetNewLookups();

			AssertNotNull(lookups.AllEmailTypeList);

			AssertEquals("All email type list should contain main type", Core.Constants.EmailFromAddressTypes.Descriptions.Main, lookups.AllEmailTypeList.GetDescriptionFromCode(Core.Constants.EmailFromAddressTypes.Codes.Main));
		}

		public void TestSelectableEmailTypeList()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "main@test.com";

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList))
			{
				var emailAddresses = staff.EmailAddresses;
				var emailAddress = emailAddresses.AddNew();
				emailAddress.GSE_GC_Company = company.PK;
				emailAddress.GSE_Type = "IMP";

				var list = new GlbStaffEmailAddressLookups(emailAddress).SelectableEmailTypeList;

				AssertNotNull(list);
				AssertEquals(3, list.Count);
				AssertEquals("EXP", list[0].Code);
				AssertEquals("Export", list[0].Description);
				AssertEquals("IMP", list[1].Code);
				AssertEquals("Import", list[1].Description);
				AssertEquals("IMP", list[2].Code);
				AssertEquals("Import (WTG)", list[2].Description);
			}
		}

		GlbStaffEmailAddressLookups GetNewLookups()
		{
			var staff = Factory.New<GlbStaff>();
			var emailAddress = staff.EmailAddresses.AddNew();

			return new GlbStaffEmailAddressLookups(emailAddress);
		}
	}
}
