using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.AddressExtensions;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AddressExtensionsTest : TestCaseWithFactory
	{
		public void TestIsToOrder()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			void AssertIsToOrder(string companyName, bool expectedIsToOrder)
			{
				address.CompanyName = companyName;
				AssertEquals($"CompanyName: {companyName}", expectedIsToOrder, address.IsToOrder());
			}

			AssertIsToOrder("to order", true);
			AssertIsToOrder("To Order", true);
			AssertIsToOrder("TO ORDER", true);
			AssertIsToOrder("to order of", true);
			AssertIsToOrder("To Order Of", true);
			AssertIsToOrder("TO ORDER OF", true);
			AssertIsToOrder("to the order", true);
			AssertIsToOrder("To The Order", true);
			AssertIsToOrder("TO THE ORDER", true);
			AssertIsToOrder("to the order of", true);
			AssertIsToOrder("To The Order Of", true);
			AssertIsToOrder("TO THE ORDER OF", true);
			AssertIsToOrder("ABC to order", false);
			AssertIsToOrder("ABC To Order", false);
			AssertIsToOrder("ABC TO ORDER", false);
			AssertIsToOrder("to order of ABC", true);
			AssertIsToOrder("To Order Of ABC", true);
			AssertIsToOrder("TO ORDER OF ABC", true);
			AssertIsToOrder("ABC to the order", false);
			AssertIsToOrder("ABC To The Order", false);
			AssertIsToOrder("ABC TO THE ORDER", false);
			AssertIsToOrder("to the order of ABC", true);
			AssertIsToOrder("To The Order Of ABC", true);
			AssertIsToOrder("TO THE ORDER OF ABC", true);
			AssertIsToOrder("ABC", false);
		}

		public void TestIsSameAsConsignee()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			void AssertIsSameAsConsignee(string companyName, bool expectedIsSameAsConsignee)
			{
				address.CompanyName = companyName;
				AssertEquals($"CompanyName: {companyName}", expectedIsSameAsConsignee, address.IsSameAsConsignee());
			}

			AssertIsSameAsConsignee("same as consignee", true);
			AssertIsSameAsConsignee("Same As Consignee", true);
			AssertIsSameAsConsignee("SAME AS CONSIGNEE", true);
			AssertIsSameAsConsignee("ABC", false);
		}

		public void TestAddAsAgentInfoToCompanyName()
		{
			var context = new CommonContext(Factory);
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Fudge burners";

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_OA_Address = org.MainAddress.PK;

			var jobDocAddress2 = Factory.New<JobDocAddress>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			AssertAddAsAgentInfoResult(org.MainAddress, true);
			AssertAddAsAgentInfoResult(jobDocAddress1, true);
			AssertAddAsAgentInfoResult(jobDocAddress2, false);
			AssertAddAsAgentInfoResult(consol.SendingForwarderWithContact, true);

			void AssertAddAsAgentInfoResult(object addressObject, bool infoAdded)
			{
				var address = AddressBuilder.Create(context, org.MainAddress);
				address.AddAsAgentInfoToCompanyName(addressObject);
				AssertEquals("Fudge burners", address.CompanyName);

				address = address.AddAsAgentInfoToCompanyName(null);
				AssertEquals("Fudge burners", address.CompanyName);

				org.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgent;
				address.CompanyName = "Fudge burners";
				AssertEquals("Same instance", address, address.AddAsAgentInfoToCompanyName(addressObject));
				AssertEquals(infoAdded ? $"Fudge burners {org.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgent)}" : "Fudge burners", address.CompanyName);

				org.MiscServ.OM_FWAsAgentName = "ABC Lines";
				address.CompanyName = "Fudge burners";
				address = address.AddAsAgentInfoToCompanyName(addressObject);
				AssertEquals(infoAdded ? $"Fudge burners {org.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgent)} ABC Lines" : "Fudge burners", address.CompanyName);

				org.MiscServ.OM_FWAsAgentOption = string.Empty;
				org.MiscServ.OM_FWAsAgentName = string.Empty;
			}
		}

		public void TestAddToOrderSupport()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);

			AssertAddSameAsSupport(address, "To Order", () => address.AddToOrderSupport());
		}

		public void TestAddSameAsConsingeeSupport()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);

			AssertAddSameAsSupport(address, "Same As Consignee", () => address.AddSameAsConsigneeSupport());
		}

		void AssertAddSameAsSupport(Address address, string identifier, Action addSupportAction)
		{
			address.CompanyName = "Sun Quan";
			address.AddressLine1 = "Address1";
			address.AddressLine2 = "Address2";
			address.City = "Nanjing";
			address.State = "Jiangsu";
			address.Country.Code = "CN";
			address.Country.Name = "China";
			address.Postcode = "000";
			address.Contact = "Zhongmou Sun";
			address.Phone = "12345678";
			address.Fax = "2222";
			address.Email = "zhongmou.sun@wu.com";

			addSupportAction();
			address.CompanyName = identifier;

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", identifier, address.CompanyName);
				AssertEquals("AddressLine1", string.Empty, address.AddressLine1);
				AssertEquals("AddressLine2", string.Empty, address.AddressLine2);
				AssertEquals("City", string.Empty, address.City);
				AssertEquals("State", string.Empty, address.State);
				AssertEquals("Country.Code", string.Empty, address.Country.Code);
				AssertEquals("Country.Name", string.Empty, address.Country.Name);
				AssertEquals("Postcode", string.Empty, address.Postcode);
				AssertEquals("Contact", string.Empty, address.Contact);
				AssertEquals("Phone", string.Empty, address.Phone);
				AssertEquals("Fax", string.Empty, address.Fax);
				AssertEquals("Email", string.Empty, address.Email);
			});

			address.CompanyName = "reset";
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "reset", address.CompanyName);
				AssertEquals("AddressLine1", "Address1", address.AddressLine1);
				AssertEquals("AddressLine2", "Address2", address.AddressLine2);
				AssertEquals("City", "Nanjing", address.City);
				AssertEquals("State", "Jiangsu", address.State);
				AssertEquals("Country.Code", "CN", address.Country.Code);
				AssertEquals("Country.Name", "China", address.Country.Name);
				AssertEquals("Postcode", "000", address.Postcode);
				AssertEquals("Contact", "Zhongmou Sun", address.Contact);
				AssertEquals("Phone", "12345678", address.Phone);
				AssertEquals("Fax", "2222", address.Fax);
				AssertEquals("Email", "zhongmou.sun@wu.com", address.Email);
			});
		}
	}
}
