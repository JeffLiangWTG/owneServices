using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReactivateBranchOrAddressModel))]
	public class ReactivateBranchOrAddressModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_IsActive = true;
			var model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			AssertEquals(1, model.ReactivateBranchOrAddressCollection.Count);
			AssertEquals(true, model.ForAddress);
			AssertEquals(true, model.SelectedAll);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_IsActive = false;
			model = new ReactivateBranchOrAddressModel(company.Branches);
			AssertEquals(1, model.ReactivateBranchOrAddressCollection.Count);
			AssertEquals(false, model.ForAddress);
			AssertEquals(false, model.SelectedAll);
		}

		public void TestApplyActiveStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_IsActive = false;
			var model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
			model.ApplyActiveStatus();
			AssertEquals(true, orgHeader.MainAddress.OA_IsActive);
		}

		public void TestSelectedAll()
		{
			var company = Factory.New<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_IsActive = false;
			var branch2 = company.Branches.AddNew();
			branch2.GB_IsActive = false;
			var branch3 = company.Branches.AddNew();
			branch3.GB_IsActive = false;
			var model = new ReactivateBranchOrAddressModel(company.Branches);
			model.SelectedAll = true;
			AssertEquals(true, model.SelectedAll);
			AssertEquals(true, model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().All(u => u.Selected));

			model.SelectedAll = false;
			AssertEquals(false, model.SelectedAll);
			AssertEquals(true, model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().All(u => !u.Selected));
		}

		public void TestHasSelectedItems()
		{
			var company = Factory.New<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_IsActive = false;
			var branch2 = company.Branches.AddNew();
			branch2.GB_IsActive = false;
			var branch3 = company.Branches.AddNew();
			branch3.GB_IsActive = false;
			var model = new ReactivateBranchOrAddressModel(company.Branches);
			AssertEquals(false, model.HasSelectedItems);

			model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
			AssertEquals(true, model.HasSelectedItems);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.New<OrgHeader>();
			return new ReactivateBranchOrAddressModel(orgHeader.Addresses);
		}
	}
}
