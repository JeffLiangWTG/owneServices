using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardingShipmentDefaultingTest : TestCaseWithFactory
	{
		#region Rate Commodity Defaulting Logic

		public void Test_RateCommodityDefaultingRule_Origin()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, origin: "AUSYD");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_Destination()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, destination: "AUMEL");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_TransportMode()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, transportMode: "SEA");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_ContainerMode()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA", packingMode: "FCL");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, containerMode: "FCL");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_Direction()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, direction: "DOM");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_ServiceLevel()
		{
			var localClient = CreateOrgHeader("ORG1", "Org 1");
			var shipment = CreateShipment("AUSYD", "AUMEL", "SEA");
			SetupJob(shipment, localClient);

			var commodityRule = CreateCommodityRule("ALUM", localClient, serviceLevel: "DIR");

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_LocalClientEmpty_Export()
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "ORG1";
			localClient.OH_FullName = "Org 1";
			var localAddress = localClient.Addresses.AddNew();
			localAddress.OA_RN_NKCountryCode = "AU";
			localAddress.OA_Address1 = "Test Street";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "ORG2";
			consignor.OH_FullName = "Org 2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_TransportMode = "SEA";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var commodityRule = Factory.New<OrgRateCommodityDefaultingRule>();
			commodityRule.ORC_RH_NKCommodityCode = "ALUM";
			commodityRule.ORC_OH = consignor.PK;
			commodityRule.ORC_Origin = "AUSYD";

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_LocalClientEmpty_Import()
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "ORG1";
			localClient.OH_FullName = "Org 1";
			var localAddress = localClient.Addresses.AddNew();
			localAddress.OA_RN_NKCountryCode = "AU";
			localAddress.OA_Address1 = "Test Street";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ORG2";
			consignee.OH_FullName = "Org 2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = "SEA";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var commodityRule = Factory.New<OrgRateCommodityDefaultingRule>();
			commodityRule.ORC_RH_NKCommodityCode = "ALUM";
			commodityRule.ORC_OH = consignee.PK;
			commodityRule.ORC_Destination = "AUSYD";

			AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

			Factory.Save();

			AssertEquals("prepoulated after save", "ALUM", shipment.JS_RH_NKRateCommodity);
		}

		public void Test_RateCommodityDefaultingRule_LocalClientEmpty_Registry()
		{
			var ruleTable = DataRegistryRating.Instance.DefaultRateCommodities.Value;
			ruleTable.Add(CreateCommodityDefaultingRule("XX1", "AUSYD", "AUMEL", "SEA", "BCN", "DOM", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX2", "", "AUMEL", "SEA", "BCN", "DOM", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX3", "", "", "SEA", "BCN", "DOM", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX4", "", "", "AIR", "BCN", "DOM", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX5", "", "", "", "", "DOM", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX6", "", "", "", "", "", "DIR"));
			ruleTable.Add(CreateCommodityDefaultingRule("XX7", "AUSYD", "AUMEL", "SEA", "BCN", "DOM", ""));
			ruleTable.Add(CreateCommodityDefaultingRule("XX8", "AUSYD", "AUMEL", "SEA", "BCN", "", ""));
			ruleTable.Add(CreateCommodityDefaultingRule("XX9", "AUSYD", "AUMEL", "SEA", "", "", ""));
			ruleTable.Add(CreateCommodityDefaultingRule("XX10", "AUSYD", "AUMEL", "", "", "", ""));
			ruleTable.Add(CreateCommodityDefaultingRule("XX11", "AUSYD", "", "", "", "", ""));

			using (DataRegistryRating.Instance.DefaultRateCommodities.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ruleTable))
			{
				var localClient = Factory.New<OrgHeader>();
				localClient.OH_Code = "ORG1";
				localClient.OH_FullName = "Org 1";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "AUMEL";
				shipment.JS_TransportMode = "SEA";

				var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				AssertEquals(ZString.Empty, shipment.JS_RH_NKRateCommodity);

				Factory.Save();
				AssertEquals("prepoulated after save", "XX9", shipment.JS_RH_NKRateCommodity);
			}
		}

		public void Test_RateCommodityDefaultingRule_Hierarchy()
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "ORG1";
			localClient.OH_FullName = "Org 1";

			CreateRateCommodityDefaultingRule("XX1", localClient.PK, "AUSYD", "AUMEL", "SEA", "BCN", "DOM", "DIR");
			CreateRateCommodityDefaultingRule("XX2", localClient.PK, "", "AUMEL", "SEA", "BCN", "DOM", "DIR");
			CreateRateCommodityDefaultingRule("XX3", localClient.PK, "", "", "SEA", "BCN", "DOM", "DIR");
			CreateRateCommodityDefaultingRule("XX4", localClient.PK, "", "", "AIR", "BCN", "DOM", "DIR");
			CreateRateCommodityDefaultingRule("XX5", localClient.PK, "", "", "", "", "DOM", "DIR");
			CreateRateCommodityDefaultingRule("XX6", localClient.PK, "", "", "", "", "", "DIR");
			CreateRateCommodityDefaultingRule("XX7", localClient.PK, "AUSYD", "AUMEL", "SEA", "BCN", "DOM", "");
			CreateRateCommodityDefaultingRule("XX8", localClient.PK, "AUSYD", "AUMEL", "SEA", "BCN", "", "");
			CreateRateCommodityDefaultingRule("XX9", localClient.PK, "AUSYD", "AUMEL", "SEA", "", "", "");
			CreateRateCommodityDefaultingRule("XX10", localClient.PK, "AUSYD", "AUMEL", "", "", "", "");
			CreateRateCommodityDefaultingRule("XX11", localClient.PK, "AUSYD", "", "", "", "", "");

			Factory.Save();

			AssertCommodityCodeByShipment(localClient, "XX1", "AUSYD", "AUMEL", "SEA", "BCN", "DIR");
			AssertCommodityCodeByShipment(localClient, "XX2", "AUPER", "AUMEL", "SEA", "BCN", "DIR");
			AssertCommodityCodeByShipment(localClient, "XX3", "AUPER", "AUSYD", "SEA", "BCN", "DIR");
			AssertCommodityCodeByShipment(localClient, "XX4", "AUPER", "AUSYD", "AIR", "BCN", "DIR");
			AssertCommodityCodeByShipment(localClient, "XX5", "AUPER", "AUSYD", "AIR", "SCN", "DIR");
			AssertCommodityCodeByShipment(localClient, "XX7", "AUSYD", "AUMEL", "SEA", "BCN", "D2D");
			AssertCommodityCodeByShipment(localClient, "XX9", "AUSYD", "AUMEL", "SEA", "SCN", "D2D");
			AssertCommodityCodeByShipment(localClient, "XX10", "AUSYD", "AUMEL", "AIR", "SCN", "D2D");
			AssertCommodityCodeByShipment(localClient, "XX11", "AUSYD", "AUPER", "AIR", "SCN", "D2D");
		}

		void AssertCommodityCodeByShipment(OrgHeader localClient, string expectedCommodityCode, string origin, string destination, string transportMode, string containerMode, string serviceLevel)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RS_NKServiceLevel = serviceLevel;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			shipment.Factory.Save();

			AssertEquals("prepoulated after save", expectedCommodityCode, shipment.JS_RH_NKRateCommodity);
		}

		RateCommodityDefaultingRule CreateCommodityDefaultingRule(string commodityCode, string origin = "", string destination = "", string transportMode = "", string containerMode = "", string direction = "", string serviceLevel = "")
		{
			var commodityRule = new RateCommodityDefaultingRule();

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = commodityCode;
			commodity.Factory.Save();
			commodityRule.RateCommodityCode = commodityCode;
			commodityRule.Origin = origin;
			commodityRule.Destination = destination;
			commodityRule.TransportMode = transportMode;
			commodityRule.ContainerMode = containerMode;
			commodityRule.Direction = direction;
			commodityRule.ServiceLevel = serviceLevel;
			return commodityRule;
		}

		void CreateRateCommodityDefaultingRule(string commodityCode, ZGuid orgHeader, string origin = "", string destination = "", string transportMode = "", string containerMode = "", string direction = "", string serviceLevel = "")
		{
			var commodityRule = Factory.New<OrgRateCommodityDefaultingRule>();
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = commodityCode;
			commodityRule.ORC_RH_NKCommodityCode = commodityCode;
			commodityRule.ORC_OH = orgHeader;
			commodityRule.ORC_Origin = origin;
			commodityRule.ORC_Destination = destination;
			commodityRule.ORC_TransportMode = transportMode;
			commodityRule.ORC_ContainerMode = containerMode;
			commodityRule.ORC_Direction = direction;
			commodityRule.ORC_RS_NKServiceLevel = serviceLevel;
		}

		OrgHeader CreateOrgHeader(string code, string fullName)
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = code;
			localClient.OH_FullName = fullName;
			return localClient;
		}

		ForwardingShipment CreateShipment(string origin, string destination, string transportMode, string packingMode = null)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = packingMode;
			return shipment;
		}

		void SetupJob(ForwardingShipment shipment, OrgHeader localClient)
		{
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
		}

		OrgRateCommodityDefaultingRule CreateCommodityRule(string commodityCode, OrgHeader localClient, string origin = null, string destination = null, string transportMode = null, string containerMode = null, string direction = null, string serviceLevel = null)
		{
			var commodityRule = Factory.New<OrgRateCommodityDefaultingRule>();
			commodityRule.ORC_RH_NKCommodityCode = commodityCode;
			commodityRule.ORC_OH = localClient.PK;
			commodityRule.ORC_Origin = origin;
			commodityRule.ORC_Destination = destination;
			commodityRule.ORC_TransportMode = transportMode;
			commodityRule.ORC_ContainerMode = containerMode;
			commodityRule.ORC_Direction = direction;
			commodityRule.ORC_RS_NKServiceLevel = serviceLevel;
			return commodityRule;
		}

		#endregion

	}
}
