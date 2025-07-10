using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GatewayChargeDefaultDebtorConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsolDirectionList()
		{
			AssertEquals(5, Lookups.ConsolDirectionList.Count);
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("EXP"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("IMP"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("DOM"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("OTH"));
		}

		public void TestConsolTransportModeList()
		{
			AssertEquals(5, Lookups.ConsolTransportModeList.Count);
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("AIR"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("SEA"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("ROA"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("RAI"));
		}

		public void TestChargeGroupList()
		{
			AssertEquals(12, Lookups.ChargeGroupList.Count);
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("ORG"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("LOD"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("FRT"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("INS"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("UNL"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("DST"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("OBR"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("BRK"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("CDS"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("OBO"));
			AssertEquals(true, Lookups.ChargeGroupList.ContainsCode("BON"));
		}

		public void TestConsolPaymentTermList()
		{
			AssertEquals(3, Lookups.ConsolPaymentTermList.Count);
			AssertEquals(true, Lookups.ConsolPaymentTermList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ConsolPaymentTermList.ContainsCode("PPD"));
			AssertEquals(true, Lookups.ConsolPaymentTermList.ContainsCode("CCX"));
		}

		public void TestPreviousSendingAgentList()
		{
			Func<IEnumerable<(string, string)>> actual = () => Lookups.PreviousSendingAgentList.Cast<ICodeDescription>().Select(x => (x.Code, x.Description));

			var expected = new[]
			{
				(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All.ToString())
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			configuration.RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment;

			expected = new[]
			{
				(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All.ToString()),
				(GatewayPreviousSendingAgent.Codes.SendingAgent, GatewayPreviousSendingAgent.Descriptions.SendingAgent.ToString()),
				(GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent, GatewayPreviousSendingAgent.Descriptions.NoPrevSendingAgent.ToString()),
				(GatewayPreviousSendingAgent.Codes.GatewayAgent, GatewayPreviousSendingAgent.Descriptions.GatewayAgent.ToString()),
				(GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT, GatewayPreviousSendingAgent.Descriptions.GatewayAgentWithFT.ToString())
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			configuration.RelatedJob = GatewayRelatedJob.Codes.All;

			expected = new[]
			{
				(GatewayPreviousSendingAgent.Codes.All, GatewayPreviousSendingAgent.Descriptions.All.ToString())
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			configuration.RelatedJob = GatewayRelatedJob.Codes.NotRelatedToJob;
			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		public void TestDebtorList()
		{
			Func<IEnumerable<(string, string)>> actual = () => Lookups.DebtorList.Cast<ICodeDescription>().Select(x => (x.Code, x.Description));

			var expected = new[]
			{
				(GatewayDebtor.Codes.SendingAgent, GatewayDebtor.Descriptions.SendingAgent.ToString()),
				(GatewayDebtor.Codes.ReceivingAgent, GatewayDebtor.Descriptions.ReceivingAgent.ToString()),
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			configuration.RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment;
			expected = new[]
			{
				(GatewayDebtor.Codes.SendingAgent, GatewayDebtor.Descriptions.SendingAgent.ToString()),
				(GatewayDebtor.Codes.ReceivingAgent, GatewayDebtor.Descriptions.ReceivingAgent.ToString()),
				(GatewayDebtor.Codes.ShipmentPickupAgent, GatewayDebtor.Descriptions.ShipmentPickupAgent.ToString()),
				(GatewayDebtor.Codes.ShipmentDeliveryAgent, GatewayDebtor.Descriptions.ShipmentDeliveryAgent.ToString()),
				(GatewayDebtor.Codes.PreviousSendingAgent, GatewayDebtor.Descriptions.PreviousSendingAgent.ToString())
			};

			AssertContainsExactElementsInAnyOrder(expected, actual());

			expected = new[]
			{
				(GatewayDebtor.Codes.SendingAgent, GatewayDebtor.Descriptions.SendingAgent.ToString()),
				(GatewayDebtor.Codes.ReceivingAgent, GatewayDebtor.Descriptions.ReceivingAgent.ToString()),
			};

			configuration.RelatedJob = GatewayRelatedJob.Codes.All;
			AssertContainsExactElementsInAnyOrder(expected, actual());

			configuration.RelatedJob = GatewayRelatedJob.Codes.NotRelatedToJob;
			AssertContainsExactElementsInAnyOrder(expected, actual());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new GatewayChargeDefaultDebtorConfiguration();
			Lookups = new GatewayChargeDefaultDebtorConfigurationLookups(configuration);
		}
		GatewayChargeDefaultDebtorConfigurationLookups Lookups;
		GatewayChargeDefaultDebtorConfiguration configuration;

		#endregion
	}
}
