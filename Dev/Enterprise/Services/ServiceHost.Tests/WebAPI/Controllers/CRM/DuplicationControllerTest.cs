using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class DuplicationControllerTest : TestCaseWithFactory
	{
		public void TestDuplicationController_DetectAllResults()
		{
			var org1 = InitOrgValue("ABVZA1");
			var org2 = InitOrgValue("ABVZA2");
			var org3 = InitOrgValue("ABVZA3");

			Factory.Save();

			Regenerate(org1);
			Regenerate(org2);
			Regenerate(org3);

			var tempOrg = InitOrgValue("ABVZA");
			var controller = new DuplicationController();
			var fields = CreateOrgDuplicationDetectionFields(tempOrg);

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = controller.FindOrgDuplicates(fields) as JsonResult<List<DuplicationResponse>>;

				AssertEquals(3, results.Content.Count);
				foreach (var result in results.Content)
				{
					AssertEquals(1d, result.Score);
				}

				AssertContainsExactElementsInAnyOrder(new Guid[] { org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid() }, results.Content.Select(r => r.PK));
			}
		}

		public void TestDuplicationController_NotExsitingUNLOCO_UNLOCODoesNotAffect()
		{
			var org1 = InitOrgValue("ABVZA1");
			var org2 = InitOrgValue("ABVZA2");
			var org3 = InitOrgValue("ABVZA3");

			Factory.Save();

			Regenerate(org1);
			Regenerate(org2);
			Regenerate(org3);

			var tempOrg = InitOrgValue("ABVZA");
			var controller = new DuplicationController();
			var fields = CreateOrgDuplicationDetectionFields(tempOrg);

			fields.UNLOCO = null;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = controller.FindOrgDuplicates(fields) as JsonResult<List<DuplicationResponse>>;

				AssertEquals(3, results.Content.Count);
				foreach (var result in results.Content)
				{
					AssertEquals(1d, result.Score);
				}

				AssertContainsExactElementsInAnyOrder(new Guid[] { org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid() }, results.Content.Select(r => r.PK));
			}
		}

		public void TestDuplicationController_CanProcessAnyfieldIsNull()
		{
			var org1 = InitOrgValue("ABVZA1");
			var org2 = InitOrgValue("ABVZA2");
			var org3 = InitOrgValue("ABVZA3");

			Factory.Save();

			Regenerate(org1);
			Regenerate(org2);
			Regenerate(org3);

			var controller = new DuplicationController();
			var fields = new OrgDuplicationRequest();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = controller.FindOrgDuplicates(fields) as JsonResult<List<DuplicationResponse>>;

				AssertEquals(0, results.Content.Count);
			}
		}

		public void TestMatchingBrandNames()
		{
			var org1 = InitOrgValue("ABVZA1");
			var org2 = InitOrgValue("ABVZA2");
			var org3 = InitOrgValue("ABVZA3");
			var org4 = InitOrgValue("ABVZA4");

			InitBrandValue(org1, "Brand Name Test");
			InitBrandValue(org2, "Another brand name");
			InitBrandValue(org4, "Brand Name Test");
			InitBrandValue(org4, "Brand Name Test");
			InitBrandValue(org4, "Another brand name");

			Factory.Save();

			Regenerate(org1);
			Regenerate(org2);
			Regenerate(org3);
			Regenerate(org4);

			var controller = new DuplicationController();
			var fields = new OrgDuplicationRequest() { Name = "Brand Name Test", WebsiteURL = "www.test.com" };

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = controller.FindOrgDuplicates(fields) as JsonResult<List<DuplicationResponse>>;

				AssertEquals(2, results.Content.Count);

				foreach (var result in results.Content)
				{
					AssertEquals("Brand Name Test", result.MatchingBrandName);
				}

				AssertContainsExactElementsInAnyOrder(new Guid[] { org1.PK.ToGuid(), org4.PK.ToGuid() }, results.Content.Select(r => r.PK));
			}
		}

		public void TestMatchingAddress_EnableDeduplicationDectectionWhenAddressValidationStatusNTC()
		{
			TestMatchingAddress_EnableEngine("NTC", true);
		}

		public void TestMatchingAddress_EnableDeduplicationDectectionWhenAddressValidationStatusVAD()
		{
			TestMatchingAddress_EnableEngine("VAD", true);
		}

		public void TestMatchingAddress_EnableDeduplicationDectectionWhenAddressValidationStatusMAN()
		{
			TestMatchingAddress_EnableEngine("MAN", true);
		}
		public void TestMatchingAddress_DisableDeduplicationDectectionWhenAddressValidationStatusNYV()
		{
			TestMatchingAddress_EnableEngine("NYV", false);
		}

		public void TestMatchingAddress_EnableEngine(string validationStatusCode, bool shouldEnableEngine)
		{
			var org = InitOrgValue("ABVZA1");

			Factory.Save();

			Regenerate(org);

			var controller = new DuplicationController();
			var fields = new OrgDuplicationRequest() {
				Name = "COSTCO PTY",
				Address1 = "72 O'Riordan Street",
				Address2 = "Falt 8 Room 2",
				CountryRegion = "AU",
				City = "SYDNEY",
				Postcode = "2015",
				State = "NSW",
				ValidationStatus = validationStatusCode,
			};

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = controller.FindOrgDuplicates(fields) as JsonResult<List<DuplicationResponse>>;

				if (shouldEnableEngine)
				{
					AssertEquals(string.Format("ValidationStatus {0} will enable the engine", validationStatusCode), 1, results.Content.Count);
					AssertEquals(string.Format("ValidationStatus {0} will enable the engine", validationStatusCode), org.PK, results.Content[0].PK);
					AssertEquals("Deduplication engine finds the matching address1", "72 O'Riordan Street", results.Content[0].MatchingAddress1);
					AssertEquals("Deduplication engine finds the matching address2", "Falt 8 Room 2", results.Content[0].MatchingAddress2);
					AssertEquals("Deduplication engine finds the matching city", "SYDNEY", results.Content[0].MatchingCity);
					AssertEquals("Deduplication engine finds the matching state", "NSW", results.Content[0].MatchingState);
					AssertEquals("Deduplication engine finds the matching country region", "AU - Australia", results.Content[0].MatchingCountryRegion);
				}
				else
				{
					AssertEquals(string.Format("ValidationStatus {0} will not enable the engine", validationStatusCode), 0, results.Content.Count);
				}
			}
		}

		#region Implementation

		OrgDuplicationRequest CreateOrgDuplicationDetectionFields(OrgHeader tempOrg)
		{
			return new OrgDuplicationRequest()
			{
				Name = tempOrg.OH_FullName,
				UNLOCO = tempOrg.UNLOCO?.Code ?? string.Empty,
				Address1 = tempOrg.MainAddress.Address1,
				Address2 = tempOrg.MainAddress.Address2,
				CountryRegion = tempOrg.MainAddress.OA_RN_NKCountryCode,
				City = tempOrg.MainAddress.OA_City,
				Postcode = tempOrg.MainAddress.OA_PostCode,
				State = tempOrg.MainAddress.OA_State,
				Phone = tempOrg.MainAddress.OA_Phone,
				Mobile = tempOrg.MainAddress.OA_Mobile,
				Email = tempOrg.MainAddress.OA_Email,
				Fax = tempOrg.MainAddress.OA_Fax,
				WebsiteURL = tempOrg.MainWebURL.PU_URL,
				ValidationStatus = AddressValidationStatus.Verified,
			};
		}

		OrgHeader InitOrgValue(string code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = "COSTCO PTY";
			orgHeader.OH_RL_NKClosestPort = "USLAX";

			orgHeader.MainAddress.OA_Address1 = "72 O'Riordan Street";
			orgHeader.MainAddress.OA_Address2 = "Falt 8 Room 2";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			orgHeader.MainAddress.OA_City = "SYDNEY";
			orgHeader.MainAddress.OA_PostCode = "2015";
			orgHeader.MainAddress.OA_State = "NSW";
			orgHeader.MainAddress.OA_Phone = "12345";
			orgHeader.MainAddress.OA_Mobile = "67890";
			orgHeader.MainAddress.OA_Email = "US";
			orgHeader.MainAddress.OA_Fax = "2333";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Masden";
			contact.OC_Phone = "+61449743938";

			var website = orgHeader.OrgWebURLs.AddNew();
			website.PU_URL = "www.test.com";
			website.PU_IsPrimary = true;

			return orgHeader;
		}

		OrgBrandOrRelatedName InitBrandValue(OrgHeader org, string name)
		{
			var brand = org.BrandsOrRelatedNames.AddNew(name);

			return brand;
		}

		void Regenerate(OrgHeader org)
		{
			org.PatternMatchingRecalculator.Regenerate(org);
		}

		#endregion
	}
}
