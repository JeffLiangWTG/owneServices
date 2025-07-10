using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReactivateBranchOrAddressItem))]
	public class ReactivateBranchOrAddressItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "DUM";
			branch.GB_BranchName = "WOW";
			branch.Address1 = "DUMMY1";
			branch.Postcode = "PO";
			branch.GB_IsActive = true;
			var parent = new ReactivateBranchOrAddressModel(company.Branches);
			var item = new ReactivateBranchOrAddressItem(branch, parent);

			CombineAssertions(() =>
			{
				AssertEquals("DUM", item.Code);
				AssertEquals("WOW", item.Name);
				AssertEquals(branch.GetFullAddressString(), item.Address);
				AssertEquals(true, item.Selected);
			});

			branch.GB_IsActive = false;
			item = new ReactivateBranchOrAddressItem(branch, parent);
			AssertEquals(false, item.Selected);

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_IsActive = false;
			address.Address1 = "HAHA";
			address.Postcode = "WOWO";
			address.AddressCode = "DUMMA";
			parent = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			item = new ReactivateBranchOrAddressItem(address, parent);

			CombineAssertions(() =>
			{
				AssertEquals("DUMMA", item.Code);
				AssertEquals(ZString.Empty, item.Name);
				AssertEquals(address.GetFullAddressString(), item.Address);
				AssertEquals(false, item.Selected);
			});

			address.OA_IsActive = true;
			item = new ReactivateBranchOrAddressItem(address, parent);
			AssertEquals(true, item.Selected);
		}

		public void TestApplyActiveStatus()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_IsActive = true;
			var parent = new ReactivateBranchOrAddressModel(company.Branches);
			var item = new ReactivateBranchOrAddressItem(branch, parent);
			item.Selected = false;
			item.ApplyActiveStatus();
			AssertEquals(false, branch.GB_IsActive);

			item.Selected = true;
			AssertEquals(false, branch.GB_IsActive);

			item.ApplyActiveStatus();
			AssertEquals(true, branch.GB_IsActive);

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_IsActive = false;
			parent = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			item = new ReactivateBranchOrAddressItem(address, parent);
			item.Selected = true;
			item.ApplyActiveStatus();
			AssertEquals(true, address.OA_IsActive);

			item.Selected = false;
			AssertEquals(true, address.OA_IsActive);

			item.ApplyActiveStatus();
			AssertEquals(false, address.OA_IsActive);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			var parent = new ReactivateBranchOrAddressModel(company.Branches);
			return new ReactivateBranchOrAddressItem(branch, parent);
		}
	}
}
