using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class AddressModelTest : TestCaseWithFactory
	{
		CreditReportExtractAddress addressInfo;
		OrgAddress matchAddress;
		List<OrgAddress> addressList;

		public void TestConstructor()
		{
			var model1 = new AddressModel(addressInfo, matchAddress, addressList);
			var model2 = new AddressModel(addressInfo, null, addressList);

			CombineAssertions(() =>
			{
				AssertEquals(addressInfo.Address, model1.Address1);
				AssertEquals(addressInfo.City, model1.City);
				AssertEquals(matchAddress, model1.MatchAddress);
				AssertContainsExactElementsInAnyOrder(addressList, model1.AddressCollection);

				AssertEquals(addressInfo.Address, model2.Address1);
				AssertEquals(addressInfo.City, model2.City);
				AssertEquals(null, model2.MatchAddress);
				AssertContainsExactElementsInAnyOrder(addressList, model2.AddressCollection);
			});
		}

		public void TestDefaultMergeAction()
		{
			var model1 = new AddressModel(addressInfo, matchAddress, addressList);
			var model2 = new AddressModel(addressInfo, null, addressList);

			CombineAssertions(() =>
			{
				AssertEquals(MergeAction.Codes.Update, model1.SelectedMergeAction);
				AssertEquals(MergeAction.Codes.Add, model2.SelectedMergeAction);
			});
		}

		public void TestMergeActions()
		{
			var expectedActions = new List<string>() { "Add", "Update", "Ignore" };

			var model1 = new AddressModel(addressInfo, null, addressList);
			var model2 = new AddressModel(addressInfo, matchAddress, addressList);

			CombineAssertions(() =>
			{
				AssertSequencesEqual(expectedActions, model1.MergeActions.Select(m => m.Value));
				AssertSequencesEqual(expectedActions, model2.MergeActions.Select(m => m.Value));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			addressInfo = new CreditReportExtractAddress
			{
				Address = "40 Sunset Drive",
				City = "Oombabeer",
				State = "QLD",
				Postcode = "4718",
				Country = "AU",
				Capabilities = new List<string> { "OFC" }
			};

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.Address1 = "578 Springvale Rd";

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.Address1 = "22 Banksia St";

			addressList = new List<OrgAddress>() { address1, address2 };

			matchAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchAddress.Address1 = "578 Springvale Rd";
		}
	}
}
