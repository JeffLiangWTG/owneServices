using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PortBasedOrgAddressRetrieverTestCase : TestCaseWithFactory
	{
		public void TestPreferPort()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			CreateCorrectType(org);
			CreateCorrectTypeAndDefault(org);
			CreateCorrectCountryAndType(org);
			CreateCorrectCountryAndTypeAndDefault(org);
			OrgAddress correctPortAndType = CreateCorrectPortAndType(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(correctPortAndType.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		ZGuid CreateHAWB(SchemaGuidColumn orgCol, ZGuid orgPK, ZString port)
		{
			var hawb = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			hawb.FillWithValidTestData();
			hawb[orgCol] = orgPK;
			hawb[CusHAWBSchema.CS_RL_NKDestination] = port;
			return hawb.PK;
		}

		public void TestPreferCountryAndTypeDefault()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			CreateCorrectType(org);
			CreateCorrectTypeAndDefault(org);
			OrgAddress correctCountryAndType = CreateCorrectCountryAndType(org);
			OrgAddress correctCountryAndTypeAndDefault = CreateCorrectCountryAndTypeAndDefault(org);

			correctCountryAndType.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery.Code);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			AssertEquals("precondition", true, correctCountryAndTypeAndDefault.AddressCapability.GetIsMainAddress(OrgAddressType.Delivery.Code));
			AssertEquals("precondition", false, correctCountryAndType.AddressCapability.GetIsMainAddress(OrgAddressType.Delivery.Code));

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(correctCountryAndTypeAndDefault.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestPreferCountryAndType()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			CreateCorrectType(org);
			CreateCorrectTypeAndDefault(org);
			OrgAddress correctCountryAndType = CreateCorrectCountryAndType(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(correctCountryAndType.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestPreferTypeAndDefault()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			CreateCorrectType(org);
			OrgAddress correctTypeAndDefault = CreateCorrectTypeAndDefault(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(correctTypeAndDefault.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestPreferType()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			OrgAddress correctType = CreateCorrectType(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(correctType.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestMainIfNothingElse()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(org.MainAddress.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestBestAddressType()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			org.MainAddress.OA_Address1 = "main";

			OrgAddress pickup = org.Addresses.AddNew();
			pickup.OA_RL_NKRelatedPortCode = "AUSYD";
			pickup.OA_Address1 = "pickup";
			pickup.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			pickup.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);

			OrgAddress delivery = org.Addresses.AddNew();
			delivery.OA_RL_NKRelatedPortCode = "AUSYD";
			delivery.OA_Address1 = "delivery";
			delivery.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			delivery.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);

			OrgAddress pickupAndDelivery = org.Addresses.AddNew();
			pickupAndDelivery.OA_RL_NKRelatedPortCode = "AUSYD";
			pickupAndDelivery.OA_Address1 = "pickupAndDelivery";
			pickupAndDelivery.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			pickupAndDelivery.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			AssertEquals("delivery", new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery, OrgAddressType.PickupAndDelivery, OrgAddressType.Pickup).BestAddress.OA_Address1);
			AssertEquals("pickup", new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Pickup, OrgAddressType.PickupAndDelivery, OrgAddressType.Delivery).BestAddress.OA_Address1);
			AssertEquals("pickupAndDelivery", new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.PickupAndDelivery, OrgAddressType.Delivery, OrgAddressType.Pickup).BestAddress.OA_Address1);

			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			pickup.OA_RL_NKRelatedPortCode = "AUMEL";

			Factory.Save();

			AssertEquals("pickupAndDelivery", new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Pickup, OrgAddressType.PickupAndDelivery, OrgAddressType.Office).BestAddress.OA_Address1);
		}

		public void TestNonGlobalOrgDoesNotIgnoreCountryAndPortMatching()
		{
			OrgHeader org = CreateOrg();

			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			org.MainAddress.OA_Address1 = "main";

			CreateCorrectType(org);
			OrgAddress correctTypeAndDefault = CreateCorrectTypeAndDefault(org);

			var consigneeHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignee, org.PK, "AUSYD");
			var consignorHawbPK = CreateHAWB(CusHAWBSchema.CS_OH_Consignor, org.PK, "AUSYD");

			Factory.Save();

			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery.Code);
			AssertEquals(org.MainAddress.OA_Address1, retriever.BestAddress.OA_Address1);
		}

		public void TestBestAddress_GetConsistentAddressWhenHaveMultipleMatching()
		{
			OrgHeader org = CreateGlobalOrg();
			SetMainAddressToForeignPort(org);
			org.MainAddress.OA_Address1 = "main";

			OrgAddress delivery1 = org.Addresses.AddNew();
			delivery1.OA_RL_NKRelatedPortCode = "AUSYD";
			delivery1.OA_Address1 = "delivery1";
			delivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			delivery1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);
			delivery1.OA_SystemCreateTimeUtc = new ZDateTime(2022, 11, 23);

			OrgAddress delivery2 = org.Addresses.AddNew();
			delivery2.OA_RL_NKRelatedPortCode = "AUSYD";
			delivery2.OA_Address1 = "delivery2";
			delivery2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			delivery2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);
			delivery2.OA_SystemCreateTimeUtc = new ZDateTime(2022, 11, 24);

			var addressRetriever = new PortBasedOrgAddressRetriever(org, "AUSYD", OrgAddressType.Delivery);
			AssertEquals(delivery1.PK, addressRetriever.BestAddress.PK);

			delivery1.OA_SystemCreateTimeUtc = new ZDateTime(2022, 11, 24);
			delivery2.OA_SystemCreateTimeUtc = new ZDateTime(2022, 11, 23);
			AssertEquals(delivery2.PK, addressRetriever.BestAddress.PK);
		}

		#region Implementation

		OrgHeader CreateGlobalOrg()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();

			result.OH_IsGlobalAccount = true;

			return result;
		}

		OrgHeader CreateOrg()
		{
			return Factory.NewWithValidTestData<OrgHeader>();
		}

		static OrgAddress CreateCorrectPortAndType(OrgHeader org)
		{
			OrgAddress correctPortAndType = org.Addresses.AddNew();
			correctPortAndType.OA_RL_NKRelatedPortCode = "AUSYD";
			correctPortAndType.AddAddressType(OrgAddressType.Delivery);
			correctPortAndType.OA_Address1 = "correctPortAndType";
			return correctPortAndType;
		}

		static OrgAddress CreateCorrectCountryAndTypeAndDefault(OrgHeader org)
		{
			OrgAddress correctCountryAndTypeAndDefault = org.Addresses.AddNew();
			correctCountryAndTypeAndDefault.OA_RL_NKRelatedPortCode = "AUMEL";
			correctCountryAndTypeAndDefault.AddAddressType(OrgAddressType.Delivery);
			correctCountryAndTypeAndDefault.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);
			correctCountryAndTypeAndDefault.OA_Address1 = "correctCountryAndTypeAndDefault";
			return correctCountryAndTypeAndDefault;
		}

		static OrgAddress CreateCorrectCountryAndType(OrgHeader org)
		{
			OrgAddress correctCountryAndType = org.Addresses.AddNew();
			correctCountryAndType.OA_RL_NKRelatedPortCode = "AUMEL";
			correctCountryAndType.AddAddressType(OrgAddressType.Delivery);
			correctCountryAndType.OA_Address1 = "correctCountryAndType";
			return correctCountryAndType;
		}

		static OrgAddress CreateCorrectTypeAndDefault(OrgHeader org)
		{
			OrgAddress correctTypeAndDefault = org.Addresses.AddNew();
			correctTypeAndDefault.OA_RL_NKRelatedPortCode = "SGSIN";
			correctTypeAndDefault.AddAddressType(OrgAddressType.Delivery);
			correctTypeAndDefault.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery.Code);
			correctTypeAndDefault.OA_Address1 = "correctTypeAndDefault";
			return correctTypeAndDefault;
		}

		static OrgAddress CreateCorrectType(OrgHeader org)
		{
			OrgAddress correctType = org.Addresses.AddNew();
			correctType.OA_RL_NKRelatedPortCode = "SGSIN";
			correctType.AddAddressType(OrgAddressType.Delivery);
			correctType.AddressCapability.SetIsNotMainAddress(OrgAddressType.Delivery.Code);
			correctType.OA_Address1 = "correctType";
			return correctType;
		}

		static void SetMainAddressToForeignPort(OrgHeader org)
		{
			OrgAddress main = org.MainAddress;
			main.OA_RL_NKRelatedPortCode = "SGSIN";
			main.OA_Address1 = "main";
		}

		#endregion
	}
}
