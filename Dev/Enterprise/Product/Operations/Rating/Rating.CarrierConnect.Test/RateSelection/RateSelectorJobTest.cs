using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WiseRates.Api.Model;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class RateSelectorJobTest : BaseRateSelectorTest
	{
		#region Consol

		public void TestForwardingConsol_WhenAutorated_ShowsAllPossibleResults()
		{
			var consol = CreateForwardingConsol();

			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry1.TI_ContractNumber = "Test123";
			var frtLine1 = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry2.TI_ContractNumber = "Test456";
			var frtLine2 = costEntry2.AddFlatRateLine("FRT", 40m, "AUD");

			var tp2Costing = Helper.NewCosting(TransportProvider2);

			var costEntry3 = tp2Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry3.TI_ContractNumber = "Test789";
			var frtLine3 = costEntry3.AddFlatRateLine("FRT", 60m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				// Card 1 - TP1 Test123 (the Consol contract)
				new ()
				{
					RateEntries = [costEntry1],
					RateLines = [frtLine1,],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
				// Card 2 - TP1 Test456 (not the Consol contract but still a valid selection)
				new ()
				{
					RateEntries = [costEntry2],
					RateLines = [frtLine2],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
				},
				// Card 3 - TP2 Test789 (different contract and provider)
				new ()
				{
					RateEntries = [costEntry3],
					RateLines = [frtLine3],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 60m, "20GP", "GEN")
				},
			});
		}

		public void TestForwardingConsol_WhenAutoratedWithOverriddenOrigin_ShowsRelevantMatches()
		{
			var consol = CreateForwardingConsol();
			var tp1Costing = Helper.NewCosting(TransportProvider1);
			var melEntry = tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "FRT", 20m, container: "20GP", currency: "AUD");
			var sydEntry = tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FRT", 40m, container: "20GP", currency: "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [melEntry],
					RateLines = [melEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
			});

			query = CreateRateQuery("SEA", "FCL", "AUSYD", "USLAX");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [sydEntry],
					RateLines = [sydEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
				},
			});
		}

		public void TestForwardingConsol_WhenAutoratedWithOverriddenDestination_ShowsRelevantMatches()
		{
			var consol = CreateForwardingConsol();
			var tp1Costing = Helper.NewCosting(TransportProvider1);
			var melEntry = tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUMEL", "FRT", 20m, container: "20GP", currency: "AUD");
			var sydEntry = tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUSYD", "FRT", 40m, container: "20GP", currency: "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "USLAX", "AUMEL");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [melEntry],
					RateLines = [melEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
			});

			query = CreateRateQuery("SEA", "FCL", "USLAX", "AUSYD");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [sydEntry],
					RateLines = [sydEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
				},
			});
		}

		public void TestForwardingConsol_WithCustomizedDateConfiguration_UseCorrectRate()
		{
			var consol = CreateForwardingConsol();

			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2024, 03, 01);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2024, 03, 04);

			var carrierCosting = Helper.NewCosting(TransportProvider1);
			// Only valid for ETD.
			var etdEntry = carrierCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUMEL", "FRT", 20m, container: "20GP", currency: "AUD");
			etdEntry.TI_RateStartDate = new ZDate(2024, 02, 01);
			etdEntry.TI_RateEndDate = new ZDate(2024, 03, 03);

			// Only valid for ETA.
			var etaEntry = carrierCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "AUMEL", "FRT", 40m, container: "20GP", currency: "AUD");
			etaEntry.TI_RateStartDate = new ZDate(2024, 03, 04);
			etaEntry.TI_RateEndDate = new ZDate(2024, 04, 01);

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = Core.Constants.RatingDateFilterTypes.Codes.Custom;
			var config = TestHelper.CreateOrganizationRatingDateConfig(TransportProvider1,
				ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode,
				Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.RateMode.SEA, JobRateTypes.Codes.Cost,
				Core.Constants.ContainerModes.FCL, "", JobDateTypes.Codes.DepartureDate);
			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "USLAX", "AUMEL");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [etdEntry],
					RateLines = [etdEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
			});

			config.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;
			Factory.Save();

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [etaEntry],
					RateLines = [etaEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
				},
			});
		}

		public void TestForwardingConsol_SEA_GRPandBCN_Query_ShowsRelevantMatches()
		{
			var (costing, consol) = CreateBCNForwardingConsol();
			var melEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "FRT", 20m, container: "20GP", currency: "AUD");
			var sydEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FRT", 40m, container: "20GP", currency: "AUD");

			Factory.Save();

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			var query = CreateRateQuery(consol, "AUMEL", "USLAX");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [melEntry],
					RateLines = [melEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
			});

			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			query = CreateRateQuery(consol, "AUSYD", "USLAX");

			PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [sydEntry],
					RateLines = [sydEntry.RateLines[0]],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
				},
			});
		}

		public void TestForwardingConsol_SEA_BCNRegistrySet_Query_ShowsOnlyBCNMatch()
		{
			var (costing, consol) = CreateBCNForwardingConsol();
			_ = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "FRT", 40m, container: "20GP", currency: "AUD"); // Given the Registry is enabled, this should not be found as a search for BCN
			var sydBCNEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.BCN, "AUSYD", "USLAX", "BAF", 0m, container: "20GP", currency: "USD", description: "FCL-BCN");

			Factory.Save();

			var query = CreateRateQuery(consol, "AUSYD", "USLAX");

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [sydBCNEntry],
						RateLines = [sydBCNEntry.RateLines[0]],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("BAF", 0m, "20GP", "GEN")
					},
				});
			}
		}

		public void TestForwardingConsol_ContainerQualityShouldNotBeCurated()
		{
			var consol = CreateForwardingConsol();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TEST456";
			container2.JC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "TEST789";
			container3.JC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR")).PK;
			container3.JC_ContainerQuality = "NOR";

			Factory.Save();

			var (entry, line) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
			var (entry2, line2) = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "BAF", commodityCode: "GEN", containerCode: "40GP");
			var (entry3, line3) = UrsHelper.CreateUrsEntryAndLine(id: "3", "FCL", "DST", commodityCode: "GEN", containerCode: "20FR",
				updateEntry: (e) => e.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "NOR" }]);
			List<IRateEntry> entries = [entry, entry2, entry3];

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX");

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PerformSearchAndAssert(query, consol, new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [entry, entry2, entry3],
						RateLines = [line, line2, line3],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("DST", 5m, "20FR", "GEN")
							.AddCharge("BAF", 5m, "40GP", "GEN")
							.AddCharge("FRT", 5m, "20GP", "GEN"),
					},
				}, mockProviderFactory.Object);
			}
		}

		#endregion

		ForwardingConsol CreateForwardingConsol(string origin = "AUMEL", string destination = "USLAX")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.JK_CarrierContractNumber = "Test123";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1234";
			container.JC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ActualVolume = 0.5M;
			shipment.JS_ActualWeight = 55M;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			Factory.Save();
			return consol;
		}

		(Costing, ForwardingConsol) CreateBCNForwardingConsol()
		{
			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", creditor, "SEA", Core.Constants.PaymentType.Collect);

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;

			return (costing, consol);
		}
	}
}
