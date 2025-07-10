using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingProfitShareRedistribution))]
	sealed class ForwardingProfitShareRedistributionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadingOfShipmentsAfterSaving()
		{
			var consol = CreateNewConsol("C001");
			var shipment1 = CreateNewShipment(consol, "S001");
			var shipment2 = CreateNewShipment(consol, "S002");

			var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();

			var consolProfitShare = profitShareRedistribution.ConsolProfitShares.AddNew();
			consolProfitShare.CPS_JK = consol.PK;
			consolProfitShare.CPS_RX_NKCurrency = "AUD";

			var shipment1ProfitShare = consolProfitShare.ShipmentProfitShares.AddNew();
			shipment1ProfitShare.PSS_JS = shipment1.PK;

			var shipment2ProfitShare = consolProfitShare.ShipmentProfitShares.AddNew();
			shipment2ProfitShare.PSS_JS = shipment2.PK;

			Factory.Save();

			var shipment3 = CreateNewShipment(consol, "S003"); // new Shipment added to consol after profit share redistribution
			consol.Shipments.Remove(shipment2); // some shipment removed from consol after profit share redistribution

			Factory.Save();

			var forwardingProfitShareRedistribution = Factory.Load<ForwardingProfitShareRedistribution>(profitShareRedistribution.PK);
			forwardingProfitShareRedistribution.Shipments.Select(s => s.PK).Should().BeEquivalentTo(new[] { shipment1.PK, shipment2.PK });

			Assert("This test uses FluentAssertions", true);
		}

		public void TestAddRemoveConsols()
		{
			var consol1 = CreateNewConsol("C1");
			CreateNewShipment(consol1, "S11");
			CreateNewShipment(consol1, "S12");

			var consol2 = CreateNewConsol("C2");
			CreateNewShipment(consol2, "S21");
			var shipment22 = CreateNewShipment(consol2, "S22");
			CreateNewShipment(consol2, "S23");

			var consol3 = CreateNewConsol("C3");
			CreateNewShipment(consol3, "S31");

			var consol4 = CreateNewConsol("C4");
			var shipment41 = CreateNewShipment(consol4, "S41");
			CreateNewShipment(consol4, "S42");

			// specialties: shared shipments
			consol3.Shipments.Add(shipment22);
			consol3.Shipments.Add(shipment41);

			var gatewayProfitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();
			gatewayProfitShareRedistribution.Consols.Should().BeEmpty("Precondition: no consols attached");

			// attach consols 1+2
			gatewayProfitShareRedistribution.AddConsols(new[] { consol1, consol2 });
			ConsolsAssertionByUniqueConsignRefs("Should have consols 1+2", gatewayProfitShareRedistribution, "C1", "C2");
			ShipmentsAssertionByUniqueConsignRefs("Should have unique shipments from consols 1+2", gatewayProfitShareRedistribution, "S11", "S12", "S21", "S22", "S23");

			// detach consol 1
			gatewayProfitShareRedistribution
				.RemoveConsols(gatewayProfitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>()
					.Where(x => x.JK_UniqueConsignRef == "C1").ToArray());

			ConsolsAssertionByUniqueConsignRefs("Should have consol 2", gatewayProfitShareRedistribution, "C2");
			ShipmentsAssertionByUniqueConsignRefs("Should have only shipments from consol 2", gatewayProfitShareRedistribution, "S21", "S22", "S23");

			// attach consol 3
			gatewayProfitShareRedistribution.AddConsols(new[] { consol3 });
			ConsolsAssertionByUniqueConsignRefs("Should have consols 2+3", gatewayProfitShareRedistribution, "C2", "C3");
			ShipmentsAssertionByUniqueConsignRefs("Should have unique shipments from consols 2+3", gatewayProfitShareRedistribution, "S21", "S22", "S23", "S31", "S41");

			// detach consol 2
			gatewayProfitShareRedistribution
				.RemoveConsols(gatewayProfitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>()
					.Where(x => x.JK_UniqueConsignRef == "C2").ToArray());

			ConsolsAssertionByUniqueConsignRefs("Should have consol 3", gatewayProfitShareRedistribution, "C3");
			ShipmentsAssertionByUniqueConsignRefs("Should have shipments from consol 3", gatewayProfitShareRedistribution, "S22", "S31", "S41");

			// attach consol 4
			gatewayProfitShareRedistribution.AddConsols(new[] { consol4 });

			ConsolsAssertionByUniqueConsignRefs("Should have consols 3+4", gatewayProfitShareRedistribution, "C3", "C4");
			ShipmentsAssertionByUniqueConsignRefs("Should have unique shipments from consols 3+4", gatewayProfitShareRedistribution, "S22", "S31", "S41", "S42");

			// detach consols 3+4
			gatewayProfitShareRedistribution
				.RemoveConsols(gatewayProfitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>()
					.Where(x => x.JK_UniqueConsignRef == "C3" || x.JK_UniqueConsignRef == "C4").ToArray());

			gatewayProfitShareRedistribution.Consols.Should().BeEmpty();
			gatewayProfitShareRedistribution.Shipments.Should().BeEmpty();

			Assert("This test uses FluentAssertions", true);
		}

		public void TestConsolsAndShipmentsInitializedFromConsolProfitShares()
		{
			// expected consols and shipments
			var consol1 = CreateNewConsol("C1");
			CreateNewShipment(consol1, "S11");
			CreateNewShipment(consol1, "S12");

			var consol2 = CreateNewConsol("C2");
			CreateNewShipment(consol2, "S21");

			var consol3 = CreateNewConsol("C3");
			CreateNewShipment(consol3, "S31");
			CreateNewShipment(consol3, "S32");
			CreateNewShipment(consol3, "S33");

			// unexpected consols and shipments
			var consol4 = CreateNewConsol("C4");
			CreateNewShipment(consol4, "S41");
			var consol5 = CreateNewConsol("C5");
			CreateNewShipment(consol5, "S51");

			var gatewayProfitShareRedistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			CreateNewConsolidationProfitShare(gatewayProfitShareRedistribution, consol1.PK);
			CreateNewConsolidationProfitShare(gatewayProfitShareRedistribution, consol2.PK);
			CreateNewConsolidationProfitShare(gatewayProfitShareRedistribution, consol3.PK);

			ConsolsAssertionByUniqueConsignRefs("Should have consols 1+2+3", gatewayProfitShareRedistribution, "C1", "C2", "C3");
			ShipmentsAssertionByUniqueConsignRefs("Should have unique shipments from consols 1+2+3", gatewayProfitShareRedistribution, "S11", "S12", "S21", "S31", "S32", "S33");

			Assert("This test uses FluentAssertions", true);
		}

		public void TestAddRemoveProfitShareRules()
		{
			var gatewayProfitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();

			var agreement = Factory.NewWithValidTestData<OrgAgentRelationship>();
			agreement.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;

			// Rules expected to be unaccepted
			var details11 = agreement.ProfitShareDetails.AddNew();
			details11.O4_JobType = JobTypesList.Codes.Blank; // unaccepted job type
			var details12 = agreement.ProfitShareDetails.AddNew();
			details12.O4_JobType = JobTypesList.Codes.SHP; // unaccepted job type
			var details13 = agreement.ProfitShareDetails.AddNew();
			details13.O4_JobType = JobTypesList.Codes.GCN;
			details13.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.Blank; // unaccepted method

			// Rules expected to be accepted
			var details21 = agreement.ProfitShareDetails.AddNew();
			details21.O4_JobType = JobTypesList.Codes.GCN;
			details21.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.CHG;

			var details22 = agreement.ProfitShareDetails.AddNew();
			details22.O4_JobType = JobTypesList.Codes.GCN;
			details22.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GVT;

			var details23 = agreement.ProfitShareDetails.AddNew();
			details23.O4_JobType = JobTypesList.Codes.GCN;
			details23.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GWT;

			var details24 = agreement.ProfitShareDetails.AddNew();
			details24.O4_JobType = JobTypesList.Codes.GCN;
			details24.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.SHP;

			agreement.SelectedProfitShareDetailsList.AddRange(new[] { details11, details12, details13, details21, details22, details23, details24 });

			gatewayProfitShareRedistribution.AddProfitShareRules(new[] { agreement });
			ProfitShareRulesAssertion(gatewayProfitShareRedistribution, details21, details22, details23, details24);

			gatewayProfitShareRedistribution.RemoveProfitShareRules(new[] { details22, details23 });
			ProfitShareRulesAssertion(gatewayProfitShareRedistribution, details21, details24);

			gatewayProfitShareRedistribution.RemoveProfitShareRules(new[] { details21, details24 });
			ProfitShareRulesAssertion(gatewayProfitShareRedistribution); // empty list
			gatewayProfitShareRedistribution.ProfitShareRules.Should().BeEmpty();

			Assert("This test uses FluentAssertions", true);
		}

		public void TestAddSelectedProfitShareRules()
		{
			var gatewayProfitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();

			var agreement = Factory.NewWithValidTestData<OrgAgentRelationship>();
			agreement.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;

			// Rules expected to be accepted
			var details21 = agreement.ProfitShareDetails.AddNew();
			details21.O4_JobType = JobTypesList.Codes.GCN;
			details21.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.CHG;

			var details22 = agreement.ProfitShareDetails.AddNew();
			details22.O4_JobType = JobTypesList.Codes.GCN;
			details22.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GVT;

			var details23 = agreement.ProfitShareDetails.AddNew();
			details23.O4_JobType = JobTypesList.Codes.GCN;
			details23.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.GWT;

			var details24 = agreement.ProfitShareDetails.AddNew();
			details24.O4_JobType = JobTypesList.Codes.GCN;
			details24.O4_GatewayProfitApportionmentMethod = GatewayProfitApportionmentMethodList.Codes.SHP;

			agreement.SelectedProfitShareDetailsList.Should().BeEmpty();
			gatewayProfitShareRedistribution.AddProfitShareRules(new[] { agreement });
			ProfitShareRulesAssertion(gatewayProfitShareRedistribution, Array.Empty<OrgProfitShareDetails>());

			agreement.SelectedProfitShareDetailsList.AddRange(new[] { details21, details23, details24 });
			gatewayProfitShareRedistribution.AddProfitShareRules(new[] { agreement });
			// details22 is not selected hence should not be added
			ProfitShareRulesAssertion(gatewayProfitShareRedistribution, details21, details23, details24);

			Assert("This test uses FluentAssertions", true);
		}

		#region Helpers

		static void ConsolsAssertionByUniqueConsignRefs(string message, ForwardingProfitShareRedistribution profitShareRedistribution, params ZString[] consolsUniqueConsignRefs)
		{
			profitShareRedistribution.Consols.Select(x => x.JK_UniqueConsignRef)
				.Should()
				.BeEquivalentTo(consolsUniqueConsignRefs, because: message);
		}

		static void ShipmentsAssertionByUniqueConsignRefs(string message, ForwardingProfitShareRedistribution profitShareRedistribution, params ZString[] shipmentsUniqueConsignRefs)
		{
			profitShareRedistribution.Shipments.Select(x => x.JS_UniqueConsignRef)
				.Should()
				.BeEquivalentTo(shipmentsUniqueConsignRefs, because: message);
		}

		static void ProfitShareRulesAssertion(ForwardingProfitShareRedistribution profitShareRedistribution, params OrgProfitShareDetails[] rules)
		{
			profitShareRedistribution.ProfitShareRules
				.Select(r => r.ProfitShareDetails)
				.Should()
				.BeEquivalentTo(rules);
		}

		ForwardingConsol CreateNewConsol(string consolNumber)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			return consol;
		}

		static ForwardingShipment CreateNewShipment(ForwardingConsol consol, string shipmentNumber)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			return shipment;
		}

		static void CreateNewConsolidationProfitShare(ProfitShareRedistribution profitShareRedistribution, ZGuid consolPK)
		{
			var consolidationProfitShare = profitShareRedistribution.ConsolProfitShares.AddNew();
			consolidationProfitShare.CPS_RX_NKCurrency = "AUD";
			consolidationProfitShare.CPS_JK = consolPK;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();

		#endregion
	}
}
