using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ContractManagement.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Business.Testing.RatingAdapterTestHelper;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentRateSelectorEnabledRatingAdapterTest : TestCaseWithFactory
	{
		#region IJobDataUpdater

		public void TestUpdateCarrierConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			Assert("Confirmation is needed to update carrier on consol", ratingAdapter.UpdateCarrierConfirmationIsNeeded("Carrier", out var confirmationMessage));
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateOriginConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			Assert("Confirmation is needed to update origin on consol", ratingAdapter.UpdateOriginConfirmationIsNeeded("Origin", out var confirmationMessage));
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateDestinationConfirmationIsNeeded()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			Assert("Confirmation is needed to update destination on consol", ratingAdapter.UpdateDestinationConfirmationIsNeeded("Destination", out var confirmationMessage));
			AssertNotNullOrEmpty(confirmationMessage);
		}

		public void TestUpdateServiceLevel()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));
			AssertEquals("STD", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			ratingAdapter.UpdateServiceLevel("EXP");
			AssertEquals("EXP", consol.JK_AWBServiceLevel);
			AssertEquals("EXP", ratingAdapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));
		}

		public void TestUpdateCarrier()
		{
			var oldCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var newCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = oldCarrier.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));
			AssertEquals(oldCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);

			ratingAdapter.UpdateCarrier(newCarrier);
			AssertEquals(newCarrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals(newCarrier.OH_Code, ratingAdapter.Carrier.OH_Code);
		}

		public void TestUpdateLocation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";

			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);

			ratingAdapter.UpdateOrigin("SGSIN");
			ratingAdapter.UpdateDestination("USLAX");
			AssertEquals("SGSIN", consol.JK_RL_NKLoadPort);
			AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
			AssertEquals("UAIEV", ratingAdapter.Origin.Code);
			AssertEquals("AUSYD", ratingAdapter.Destination.Code);
		}

		public void TestUpdatePaymentTerms()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PrepaidCollect = "PPD";

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("PPD", t.Value));

			ratingAdapter.UpdatePaymentTerms("CCL");
			AssertEquals("CCL", consol.JK_PrepaidCollect);
			ratingAdapter.PaymentTerm.PaymentTermInfoCollection.ForEach(t => AssertEquals("CCL", t.Value));
		}

		public void TestUpdateNamedAccount()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, "NAC");

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));
			AssertEquals("NAC", ratingAdapter.NamedAccount);

			ratingAdapter.UpdateNamedAccount("NewNAC");
			AssertEquals("NewNAC", consol.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount).CE_EntryNum);
			AssertEquals("NewNAC", ratingAdapter.NamedAccount);
		}

		public void TestUpdateCarrierQuoteNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			ratingAdapter.UpdateCarrierQuoteNumber("PI0001");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0003");
			ratingAdapter.UpdateCarrierQuoteNumber("PI0002");

			var values = consol.Numbers.GetAllReferenceNumbersByType("CQN");
			AssertEquals(3, values.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PI0001", "PI0002", "PI0003" }, values);
		}

		public void TestUpdateContainerPenalties()
		{
			var carrier = CreateCarrierOrg("MAERSK LINES PTY LTD", "MAE");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "USLGB";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			var containerPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, 10),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				}
			};

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol20GPContainer = consol.Containers.AddNew();
			consol20GPContainer.JC_RC = containerType20GP.PK;
			consol20GPContainer.JC_ContainerCount = 1;
			var consolContainerPenalty = consol20GPContainer.ImportPenalties.AddNew();
			consolContainerPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			consolContainerPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			consolContainerPenalty.CPY_PerUnitCost = 45;
			consolContainerPenalty.CPY_RX_NKCurrency = "EUR";
			consolContainerPenalty.CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, 17);
			consolContainerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			ratingAdapter.UpdateContainerPenalties(containerPenalties, true);

			var expectedPenalties = new[]
			{
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO,
					CPY_FreeTime = TimeSpan.FromDays(16),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 45,
					CPY_RX_NKCurrency = "EUR"
				},
				new DummyContainerPenalty("20GP")
				{
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_FreeTime = TimeSpan.FromDays(9),
					CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention,
					CPY_ProcessType = ContainerPenaltyProcessType.Import,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_PerUnitCost = 32,
					CPY_RX_NKCurrency = "USD"
				},
			};

			consol20GPContainer.ImportPenalties.Should().BeEquivalentTo(expectedPenalties);
			Assert(true);
		}

		public void TestUpdateSpotBookingTerms_ShouldOverride()
		{
			var carrier = CreateCarrierOrg("MAERSK LINES PTY LTD", "MAE");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));
			var existingNote = consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description, "Existing Note");

			ratingAdapter.UpdateSpotBookingTerms("Terms");

			consol.Notes.GetAllNotes().Should().BeEquivalentTo(new[]
			{
				new
				{
					ST_Description = new ZString(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees.Description),
					ST_NoteText = new ZString("Terms")
				},
			});

			Assert(true);
		}

		public void TestUpdateTransportDetails_ShouldDeleteExistingAndCreateTransportsWithCorrectInformationBasedOnNewTransports()
		{
			var carrier = CreateCarrierOrg("MAERSK LINES PTY LTD", "MAE");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new AutoRatingProxy(new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment));

			var currentConsolTransportLegPKs = new[]
				{
					consol.Transports[0].PK,
					consol.Transports.AddNew().PK,
					consol.Transports.AddNew().PK,
					consol.Transports.AddNew().PK,
					consol.Transports.AddNew().PK
				};

			var transports = new[]
			{
				new DummyTransport()
				{
					JW_Status = TransportStatus.Planned,
					JW_IsLinked = true,
					JW_LegOrder = 1,
					JW_TransportMode = TransportModes.Sea,
					JW_VoyageFlight = "V9849384",
					JW_Vessel = "ADRIANA D",
					JW_RL_NKLoadPort = "SEGVX",
					JW_RL_NKDiscPort = "DEBRV",
					JW_ETA = new ZDateTime(2021, 01, 15, 14, 0, 0),
					JW_ETD = new ZDateTime(2021, 01, 10, 07, 0, 0),
					JW_LegNotes = "Some Notes",
					JW_DocumentaryCutOff = new ZDateTime(2021, 01, 08, 10, 0, 0),
					JW_TerminalCutOff = new ZDateTime(2021, 01, 08, 19, 0, 0),
					JW_VGMCutOff = new ZDateTime(2021, 01, 08, 17, 0, 0),
				},
				new DummyTransport()
				{
					JW_LegOrder = 2,
					JW_RL_NKLoadPort = "DEBRV",
					JW_RL_NKDiscPort = "ESALG",
				}
			};

			AssertEquals(5, consol.Transports.Count);

			ratingAdapter.UpdateTransports(transports);

			AssertEquals(2, consol.Transports.Count);

			Assert(!currentConsolTransportLegPKs.Contains(consol.Transports[0].PK));
			Assert(!currentConsolTransportLegPKs.Contains(consol.Transports[1].PK));

			consol
				.Transports
				.Select(t => (ITransport)t)
				.Should()
				.BeEquivalentTo
					(
						transports
						, options => options.ComparingByMembers<ITransport>().Excluding(i => i.ParentType).Excluding(i => i.JW_ParentGUID).Excluding(i => i.JW_VesselScreeningStatus)
					);
		}

		#endregion

		#region RatingAdapter

		public void TestSkipFreightCharge_IsPopulated_WithCorrectValue()
		{
			var consol = Factory.New<ForwardingConsol>();

			var contractAllocationLine = Factory.New<RatingContractAllocationLine>();
			consol.JK_RCA_AllocationLine = contractAllocationLine.PK;

			var shipment = consol.Shipments.AddNew();
			var ratingAdapter = new ForwardingShipmentRateSelectorEnabledRatingAdapter(shipment);

			AssertEquals(false, ratingAdapter.SkipFreightCharge);

			contractAllocationLine.RCA_AllowFreightSpotRate = true;

			AssertEquals(true, ratingAdapter.SkipFreightCharge);
		}

		#endregion

		#region Test Helper Method

		public OrgHeader CreateCarrierOrg(string fullName, string scac = "SCAC")
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = scac + "CARRIER";
			carrier.OH_FullName = fullName;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsCreditor = true;
			carrier.CompanyData.SetAPTaxApplicable(false);

			if (!string.IsNullOrWhiteSpace(scac))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = scac;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
			}

			return carrier;
		}

		#endregion
	}
}
