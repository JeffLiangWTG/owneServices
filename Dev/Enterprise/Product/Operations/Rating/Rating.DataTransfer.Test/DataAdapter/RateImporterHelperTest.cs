using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	public class RateImporterHelperTest : RatingTestCase
	{
		public void TestFindChargeCode()
		{
			var freightChargeCode = RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			var context = RateTestHelper.GetContext(Factory);

			AssertNotNull("AccChargeCode is not found", RateImportHelper.Instance.FindChargeCode(freightChargeCode, context));
			AssertNotNull("YYY is not found", RateImportHelper.Instance.FindChargeCode("YYY", context));

			AssertNull("AccChargecode 'XXX' must not be found", RateImportHelper.Instance.FindChargeCode("XXX", context));
		}

		public void TestGetOrgAddress()
		{
			var addReference = SetUpAddressReference();
			var orgAddr = RateImportHelper.Instance.GetOrgAddress(addReference, Consignee);

			AssertNotNull("Orgaddress is found", orgAddr);
			AssertEquals("Address Code", ConsigneeAddress2.OA_Code, orgAddr.OA_Code);

			orgAddr = RateImportHelper.Instance.GetOrgAddress(addReference, Consignee2);

			AssertNull("OrgAddress Not Found", orgAddr);
		}

		Xsd.AddressReference SetUpAddressReference()
		{
			var reference = new Xsd.AddressReference();
			reference.AddressSequenceRef = 2;
			reference.Organisation.EDICode = Consignee.OH_Code;
			reference.Organisation.OrganisationDetails.Name = Consignee.OH_FullName;
			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var addr1 = addressCollection.AddNew();
			addr1.AddressLine1 = ConsigneeAddress1.OA_Address1;
			addr1.AddressCode = ConsigneeAddress1.OA_Code;
			addr1.Sequence = 1;
			addr1.AddressType = Xsd.OrgAddressAddressType.PAD;

			var addr2 = addressCollection.AddNew();
			addr2.AddressLine1 = ConsigneeAddress2.OA_Address1;
			addr2.AddressCode = ConsigneeAddress2.OA_Code;
			addr2.AddressType = Xsd.OrgAddressAddressType.OFC;
			addr2.Sequence = 2;

			return reference;
		}

		OrgAddress consigneeAddress1;
		OrgAddress ConsigneeAddress1
		{
			get
			{
				if (consigneeAddress1 == null)
				{
					consigneeAddress1 = Consignee.Addresses.AddNew();
					consigneeAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
					consigneeAddress1.OA_Address1 = "1123 Bourke Street";
					consigneeAddress1.OA_Code = "ConsigneeAddress1";
				}
				return consigneeAddress1;
			}
		}

		OrgAddress consigneeAddress2;
		OrgAddress ConsigneeAddress2
		{
			get
			{
				if (consigneeAddress2 == null)
				{
					consigneeAddress2 = Consignee.Addresses.AddNew();
					consigneeAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
					consigneeAddress2.OA_Address1 = "1456 Bourke Street";
					consigneeAddress2.OA_Code = "ConsigneeAddress2";
				}
				return consigneeAddress2;
			}
		}

		OrgHeader fConsignee2;
		OrgHeader Consignee2
		{
			get
			{
				if (fConsignee2 == null)
				{
					fConsignee2 = Factory.NewWithValidTestData<OrgHeader>();
					fConsignee2.OH_FullName = "Consignee2";
					fConsignee2.OH_RL_NKClosestPort = "AUSYD";
					fConsignee2.OH_Code = "CONSIGNEE2";
					fConsignee2.OH_IsConsignee = true;
				}
				return fConsignee2;
			}
		}
	}
}
