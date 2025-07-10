using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class JobDocAddressDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestEmptyJobDocAddressReturnsNullAddress()
		{
			var jobDocAddressBO = Factory.New<JobDocAddress>();
			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO)));
			var addressData = writer.GetDataObject(jobDocAddressBO);

			AssertNull("Empty JobDocAddress returns null.", addressData);
		}

		public void TestOverrideContactName()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "NZAKL";

			var addressBO = organizationBO.MainAddress;

			Factory.Save();

			var jobDocAddressBO = Factory.New<JobDocAddress>();
			jobDocAddressBO.E2_OA_Address = addressBO.PK;
			jobDocAddressBO.E2_Contact = "Test";
			jobDocAddressBO.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO)));
			var addressData = writer.GetDataObject(jobDocAddressBO);

			AssertEquals("Test", addressData.Contact);
		}

		public void TestJobDocAddressWithRealAddressWritesOutAddressShortCodeAndClosestPort()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "NZAKL";

			var addressBO = organizationBO.MainAddress;
			addressBO.OA_Address1 = "1 Sandwich Way";
			addressBO.PrimaryOrgAddressAdditionalInfoDetail = "No. 5-1, Lane 17";
			addressBO.OA_City = "CONDIMENTIA";
			addressBO.OA_Code = "SAUCEPLEASE";
			addressBO.OA_Email = "maker@sandwich.gov.gs";
			addressBO.OA_Fax = "0011 7 8978 9789";
			addressBO.OA_Mobile = "12";
			addressBO.OA_Phone = "0011 10 0000 0000";
			addressBO.OA_PostCode = "3289";
			addressBO.OA_RL_NKRelatedPortCode = "NZAKL";
			addressBO.OA_State = "YUM";

			organizationBO.OH_Code = "MAGICSAND";

			Factory.Save();

			var jobDocAddressBO = Factory.New<JobDocAddress>();
			jobDocAddressBO.E2_OA_Address = addressBO.PK;
			jobDocAddressBO.E2_AddressType = DocAddressTypes.Codes.BuyingParty;

			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO)));
			var addressData = writer.GetDataObject(jobDocAddressBO);

			CombineAssertions(delegate
			{
				AssertEquals("addressData.AddressType", "BuyingParty", addressData.AddressType);
				AssertEquals("addressData.AddressOverride", ZBool.False, addressData.AddressOverride);
				AssertEquals("addressData.OrganizationCode", "MAGICSAND", addressData.OrganizationCode);
				AssertEquals("addressData.CompanyName", "THE MAGIC SAND FOOD COMPANY", addressData.CompanyName);

				AssertEquals("addressData.Port", "NZAKL - Auckland", addressData.Port.Contents());
				AssertEquals("addressData.Country", "NZ - New Zealand", addressData.Country.Contents());

				AssertEquals("addressData.AddressShortCode", "SAUCEPLEASE", addressData.AddressShortCode);
				AssertEquals("addressData.Address1", "1 Sandwich Way", addressData.Address1);
				AssertEquals("addressData.Address2", "", addressData.Address2);
				AssertEquals("addressData.AdditionalAddressInformation", "No. 5-1, Lane 17", addressData.AdditionalAddressInformation);
				AssertEquals("addressData.City", "CONDIMENTIA", addressData.City);
				AssertEquals("addressData.Postcode", "3289", addressData.Postcode);
				AssertEquals("addressData.State", "YUM", (string)addressData.State);

				AssertEquals("addressData.Email", "maker@sandwich.gov.gs", addressData.Email);
				AssertEquals("addressData.Fax", "0011 7 8978 9789", addressData.Fax);
				AssertEquals("addressData.Phone", "0011 10 0000 0000", addressData.Phone);
			});
		}

		public void TestWrite()
		{
			var addressBO = Factory.New<JobDocAddress>();

			addressBO.E2_AddressType = DocAddressTypes.Codes.BuyingParty;
			addressBO.E2_AddressOverride = true;
			addressBO.E2_CompanyName = "THE MAGIC SAND FOOD COMPANY";
			addressBO.E2_RN_NKCountryCode = "GS";
			addressBO.E2_ScreeningStatus = "UNK";

			addressBO.E2_Address1 = "1 Sandwich Way";
			addressBO.E2_AdditionalAddressInformation = "No. 5-1, Lane 17";
			addressBO.E2_City = "CONDIMENTIA";
			addressBO.E2_Postcode = "3289";
			addressBO.E2_State = "YUM";

			addressBO.E2_Contact = "THE MAKER";
			addressBO.E2_Email = "maker@sandwich.gov.gs";
			addressBO.E2_Fax = "0011 7 8978 9789";
			addressBO.E2_Mobile = "12";
			addressBO.E2_Phone = "0011 10 0000 0000";

			addressBO.E2_GovRegNum = "NO!!!";
			addressBO.E2_GovRegNumType = "GST";

			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, addressBO)));
			var addressData = writer.GetDataObject(addressBO);

			CombineAssertions(delegate
			{
				AssertEquals("addressData.AddressType", "BuyingParty", addressData.AddressType);
				AssertEquals("addressData.AddressOverride", ZBool.True, addressData.AddressOverride);
				AssertEquals("addressData.OrganizationCode", null, addressData.OrganizationCode);
				AssertEquals("addressData.CompanyName", "THE MAGIC SAND FOOD COMPANY", addressData.CompanyName);

				AssertEquals("addressData.Country", "GS - South Georgia and the South Sandwic", addressData.Country.Contents());
				AssertEquals("addressData.ScreeningStatus", "UNK - Unknown", addressData.ScreeningStatus.Contents());

				AssertEquals("addressData.Address1", "1 Sandwich Way", addressData.Address1);
				AssertEquals("addressData.Address2", "", addressData.Address2);
				AssertEquals("addressData.AdditionalAddressInformation", "No. 5-1, Lane 17", addressData.AdditionalAddressInformation);
				AssertEquals("addressData.City", "CONDIMENTIA", addressData.City);
				AssertEquals("addressData.Postcode", "3289", addressData.Postcode);
				AssertEquals("addressData.State", "YUM", (string)addressData.State);

				AssertEquals("addressData.Contact", "THE MAKER", addressData.Contact);
				AssertEquals("addressData.Email", "maker@sandwich.gov.gs", addressData.Email);
				AssertEquals("addressData.Fax", "0011 7 8978 9789", addressData.Fax);
				AssertEquals("addressData.Mobile", "12", addressData.Mobile);
				AssertEquals("addressData.Phone", "0011 10 0000 0000", addressData.Phone);

				AssertEquals("addressData.GovRegNum", "NO!!!", addressData.GovRegNum);
				AssertEquals("addressData.GovRegNumType", "GST - Government GST Code", addressData.GovRegNumType.Contents());
			});
		}

		public void TestWriteE2_IsResidential()
		{
			var addressBO = Factory.New<JobDocAddress>();
			addressBO.E2_AddressOverride = true;
			addressBO.E2_IsResidential = false;

			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, addressBO)));

			var addressData = writer.GetDataObject(addressBO);
			AssertNull("IsResidential not populated", addressData.IsResidential);

			writer.PopulateIsResidential = true;
			addressData = writer.GetDataObject(addressBO);
			AssertEquals("IsResidential is populated (false)", false, addressData.IsResidential);

			addressBO.E2_IsResidential = true;
			addressData = writer.GetDataObject(addressBO);
			AssertEquals("IsResidential is populated (true)", true, addressData.IsResidential);

			writer.PopulateIsResidential = false;
			addressData = writer.GetDataObject(addressBO);
			AssertEquals("IsResidential is populated (true)", true, addressData.IsResidential);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();
		}

		public void TestPopulateOrgAddressState()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "CNSNZ";
			var addressBO = organizationBO.MainAddress;
			addressBO.City = "Shenzhen";
			addressBO.StateCode = "44"; // Guangdong
			Factory.Save();

			var jobDocAddressBO = Factory.New<JobDocAddress>();
			jobDocAddressBO.E2_OA_Address = addressBO.PK;
			jobDocAddressBO.E2_Contact = "Test";
			jobDocAddressBO.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO));
			var writer = new JobDocAddressDataObjectWriter(writeManager);
			var addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals("44", addressData.State.Code);
			AssertEquals("Guangdong", addressData.State.Description);

			jobDocAddressBO.E2_AddressOverride = true;
			jobDocAddressBO.E2_CompanyName = "China Corp";
			jobDocAddressBO.E2_Address1 = "Dropoff Address";
			jobDocAddressBO.E2_City = "Yangzhen";
			jobDocAddressBO.E2_RN_NKCountryCode = "CN"; // China
			jobDocAddressBO.E2_State = "11"; // Beijing

			addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals("11", addressData.State.Code);
			AssertEquals("Beijing", addressData.State.Description);
		}

		public void TestPopulateOrgAddress_PopulateGeoLocation()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "CNSNZ";
			var addressBO = organizationBO.MainAddress;
			addressBO.GeoLocation = ZGeography.CreatePoint(1.23, 4.56);
			Factory.Save();

			var jobDocAddressBO = Factory.New<JobDocAddress>();
			jobDocAddressBO.E2_OA_Address = addressBO.PK;
			jobDocAddressBO.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO));
			var writer = new JobDocAddressDataObjectWriter(writeManager);

			writer.PopulateGeoLocation = true;

			var addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals(new ZDecimal(1.23), addressData.GeoLocation.Longitude);
			AssertEquals(new ZDecimal(4.56), addressData.GeoLocation.Latitude);

			jobDocAddressBO.E2_AddressOverride = true;
			jobDocAddressBO.E2_GeoLocation = ZGeography.CreatePoint(7.89, 9.99);

			addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals(new ZDecimal(7.89), addressData.GeoLocation.Longitude);
			AssertEquals(new ZDecimal(9.99), addressData.GeoLocation.Latitude);

			writer.PopulateGeoLocation = false;

			addressData = writer.GetDataObject(jobDocAddressBO);
			AssertNull("addressData.GeoLocation", addressData.GeoLocation);
		}

		public void TestPopulateOrgAddress_PopulateValidationStatus()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "CNSNZ";
			var addressBO = organizationBO.MainAddress;
			Factory.Save();

			var jobDocAddressBO = Factory.New<JobDocAddress>();
			jobDocAddressBO.E2_OA_Address = addressBO.PK;
			jobDocAddressBO.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			jobDocAddressBO.E2_ValidationStatus = "VST";

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, jobDocAddressBO));
			var writer = new JobDocAddressDataObjectWriter(writeManager);

			writer.PopulateValidationStatus = true;

			var addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals("addressData.ValidationStatus", "VST - Verified to street address", addressData.ValidationStatus.Contents());

			jobDocAddressBO.E2_AddressOverride = true;
			jobDocAddressBO.E2_ValidationStatus = "VAD";

			addressData = writer.GetDataObject(jobDocAddressBO);
			AssertEquals("addressData.ValidationStatus", "VAD - Verified", addressData.ValidationStatus.Contents());

			writer.PopulateValidationStatus = false;

			addressData = writer.GetDataObject(jobDocAddressBO);
			AssertNull("addressData.ValidationStatus", addressData.ValidationStatus);
		}

		public void TestWriteJobAddressNumbers()
		{
			var addressBO = Factory.New<JobDocAddressForTest>();
			addressBO.E2_AddressOverride = true;
			addressBO.E2_IsResidential = false;

			var regNum = addressBO.DocAddressNumbers.AddNew();
			regNum.E2N_NumberType = "ABN";
			regNum.E2N_Number = "12345675";
			var cbfNum = addressBO.DocAddressNumbers.AddNew();
			cbfNum.E2N_NumberType = "CBF";
			cbfNum.E2N_Number = "12345";

			var writer = new JobDocAddressDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, addressBO)));
			var addressData = writer.GetDataObject(addressBO);
			AssertEquals("GovRegNum", "12345675", addressData.GovRegNum);
			AssertEquals("GovRegNumType", "ABN", addressData.GovRegNumType.Code.Value);

			var regNums = addressData.RegistrationNumberCollection;
			AssertEquals("RegistrationNumber", 1, regNums.Count);
			AssertEquals("RegistrationNumber", "CBF", regNums[0].Type.Code);
			AssertEquals("RegistrationNumber", "12345", regNums[0].Value);
		}

		class JobDocAddressForTest : JobDocAddress
		{
			public JobDocAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override bool SupportsDocAddressNumbers => true;
		}
	}

	public static class ICodeDataObjectExtensions
	{
		public static string Contents(this UNLOCO data)
		{
			return data.Code + " - " + data.Name;
		}

		public static string Contents(this ICodeNameDataObject data)
		{
			return data.Code + " - " + data.Name;
		}

		public static string Contents(this ICodeDescriptionDataObject data)
		{
			return data.Code + " - " + data.Description;
		}
	}
}
