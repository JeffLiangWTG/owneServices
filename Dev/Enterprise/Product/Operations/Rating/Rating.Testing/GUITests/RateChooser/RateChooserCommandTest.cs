using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Test
{
	public class RateChooserCommandTest : TestCaseWithFactory
	{
		public void TestSelectRate()
		{
			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var shipment = BaseRatingIntegrationTest.CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.FCL, consignorPK: ZGuid.NewZGuid(), consigneePK: ZGuid.NewZGuid(), "USLAX", "AUBNE", weight: 10m);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var criteria = new RatingCriteria(shipment.RatingAdapter, Factory);
				try
				{
					new RateSelectorCommand().SelectRate(new RatingContext(), criteria, null);
				}
				catch (AutoRater.RatingCancelledException) { }

				AssertEquals
				(
					"GIVEN Shipment SEA-FCL with no container THEN should show validation error",
					"Container Type is mandatory for running Autorating Costs.",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		#region CanRate

		public void TestCanRate_Consol()
		{
			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AssertCanRateConsol(true, TransportModes.Sea, Directions.Export, ContainerModes.FCL, hasContainer: true);
				AssertCanRateConsol(true, TransportModes.Sea, Directions.Import, ContainerModes.FCL, hasContainer: true);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Import, ContainerModes.Loose);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Export, ContainerModes.BuyersConsol, hasContainer: true);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Import, ContainerModes.BuyersConsol, hasContainer: true);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Export, ContainerModes.Groupage, hasContainer: true);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Import, ContainerModes.Groupage, hasContainer: true);
				AssertCanRateConsol(true, TransportModes.Air, Directions.Export, ContainerModes.Loose);
				AssertCanRateConsol(true, TransportModes.Air, Directions.Import, ContainerModes.Loose);
				AssertCanRateConsol(false, TransportModes.Air, Directions.Import);

				AssertCanRateConsol(false, TransportModes.Sea, Directions.Export, ContainerModes.LCL, hasContainer: false);
				AssertCanRateConsol(false, TransportModes.Sea, Directions.Export, ContainerModes.LCL, hasContainer: true);
				AssertCanRateConsol(true, TransportModes.Sea, Directions.Export, ContainerModes.FCL, hasContainer: false);
				AssertCanRateConsol(true, TransportModes.Sea, Directions.Export, ContainerModes.FCL, hasContainer: true);
			}
		}

		public void TestHasFCLContainerOrEmpty()
		{
			var notLcl = ZGuid.BrettsGuid;
			var lcl = MeasureInfo.ContainerInfo.LCL;

			AssertEquals(false, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { lcl }));
			AssertEquals(false, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { lcl, lcl, lcl }));
			AssertEquals(true, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { lcl, lcl, notLcl }));
			AssertEquals(true, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { notLcl, notLcl, notLcl }));
			AssertEquals(true, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { ZGuid.Empty }));
			AssertEquals(true, RateSelectorCommand.HasFCLContainerOrEmpty(new[] { ZGuid.Empty, ZGuid.Empty, ZGuid.Empty }));
			AssertEquals(true, RateSelectorCommand.HasFCLContainerOrEmpty(Array.Empty<ZGuid>()));
		}

		#region CanRate Shipment

		public void TestCanRate_Shipment()
		{
			using (DataRegistryRating.Instance.DiagnosticSettingsEnableOnODPL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				CombineAssertions("GIVEN BCN shipment THEN should not support RateChooser", () =>
				{
					AssertEquals("Precondition: Home port", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
					AssertNotEquals("Precondition: Non home port", "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
					AssertCanRateShipment(TransportModes.Sea, ContainerModes.BuyersConsol, "USLAX", "AUBNE", hasContainer: false, expectedCanRate: false);
					AssertCanRateShipment(TransportModes.Sea, ContainerModes.BuyersConsol, "AUBNE", "USLAX", hasContainer: false, expectedCanRate: false);
				});

				AssertCanRateShipment(TransportModes.Sea, ContainerModes.LCL, "USLAX", "AUBNE", hasContainer: false, expectedCanRate: false);
				AssertCanRateShipment(TransportModes.Sea, ContainerModes.FCL, "USLAX", "AUBNE", hasContainer: false, expectedCanRate: true);
				AssertCanRateShipment(TransportModes.Sea, ContainerModes.FCL, "USLAX", "AUBNE", hasContainer: true, expectedCanRate: true);
			}
		}

		void AssertCanRateShipment(string transportMode, string containerMode, string origin, string destination, bool hasContainer, bool expectedCanRate)
		{
			var shipment = BaseRatingIntegrationTest.CreateForwardingShipment(Factory, transportMode, containerMode, consignorPK: ZGuid.NewZGuid(), consigneePK: ZGuid.NewZGuid(), origin, destination, weight: 10m);
			var consol = BaseRatingIntegrationTest.CreateForwardingConsol(TransportModes.Sea, origin, destination, transportProvider: Helper.NewOrgHeader(), shipment);

			if (hasContainer)
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerCount = 1;
				container.JC_RC = Helper.Containers["20GP"].PK;
				container.JC_ContainerMode = ContainerModes.FCL;
				container.PackLines.Add(shipment.OuterPackLines.AddNew());
			}

			var criteria = new RatingCriteria(shipment.RatingAdapter, Factory);
			AssertEquals($"shipment: {transportMode}, {containerMode}, {origin}-{destination}", expectedCanRate, RateSelectorCommand.CanRate(criteria));
		}

		protected TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion

		#region CanRate Quoted Booking

		public void TestCanRate_QuotedBooking()
		{
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AssertCanRateQuotedBooking(TransportModes.Sea, RateMode.SEA, "AUBNE", "USLAX", QuotedBookingState.QuoteOnly, hasContainer: true, expectedCanRate: true, "GIVEN OneOffQuote with SEA(LCL and FCL) mode with container THEN should be treated as FCL");
				AssertCanRateQuotedBooking(TransportModes.Sea, RateMode.SEA, "AUBNE", "USLAX", QuotedBookingState.QuoteOnly, hasContainer: false, expectedCanRate: false, "GIVEN OneOffQuote with SEA(LCL and FCL) mode with no container THEN should be treated as LCL");
			}
		}

		void AssertCanRateQuotedBooking(string transportMode, string containerMode, string origin, string destination, QuotedBookingState quotedBookingState, bool hasContainer, bool expectedCanRate, string message)
		{
			var quotedBooking = BaseRatingIntegrationTest.CreateQuotedBooking
			(
				Factory,
				transportMode,
				containerMode,
				paymentTerms: ZString.Empty,
				client: null,
				consignor: null,
				consignee: null,
				carrier: null,
				origin: origin,
				destination: destination,
				weight: 1m,
				volume: 1m,
				quotedBookingState
			);

			if (hasContainer)
			{
				var container = quotedBooking.Quote.CurrentOneOffQuote.Containers.AddNew();
				container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.TC_ContainerCount = 1;
			}

			var adapter = ((IRatingSupporter)quotedBooking)
				.AdaptersProvider
				.GetAdapters(null, AutoRateOptions.AutorateCosts)
				.Single();

			var criteria = new RatingCriteria(adapter, Factory);
			AssertEquals
			(
				$"{message} => transport mode: {transportMode}, container mode: {containerMode}, origin: {origin}, destination: {destination}, state: {quotedBookingState}, hasContainer: {hasContainer}",
				expectedCanRate,
				RateSelectorCommand.CanRate(criteria)
			);
		}

		#endregion

		#endregion

		public void TestValidate_ShouldReturnTrue_WhenWiseRateIsNotActive()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = "AUSYD";
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActiveAndLoadPortIsEmpty()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Sea;
				consolidation.JK_RL_NKLoadPort = ZString.Empty;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnTrue_WhenThereIsNoServiceLevel()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_PrepaidCollect = PaymentType.Prepaid;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = "AUSYD";
				consolidation.JK_AWBServiceLevel = ZString.Empty;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnTrue_WhenThereIsNoCarrierOrCreditor()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_PrepaidCollect = PaymentType.Prepaid;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = "AUSYD";
				consolidation.JK_OA_ShippingLineAddress = ZGuid.Empty;
				consolidation.JK_OA_CreditorAddress = ZGuid.Empty;

				AssertEquals(1, consolidation.Transports.Count);

				consolidation.Transports[0].JW_OA_CarrierAddress = ZGuid.Empty;
				consolidation.Transports[0].JW_OA_CreditorAddress = ZGuid.Empty;

				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActiveAndDischargePortIsEmpty()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = ZString.Empty;
				consolidation.JK_TransportMode = TransportModes.Sea;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnTrue_WhenWiseRateIsActive_TransportModeIsSea_AndDischargePort_LoadPort_ETA_AreValid_AndContainersAreProvided()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Sea;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				var c = consolidation.Containers.AddNew();
				c.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				c.JC_ContainerCount = 1;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActive_TransportModeIsSea_ContainersAreNotProvided()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Sea;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_WhenTransportModeIsAir_ContainerModeIsULD_ContainerTypeIsMandatory()
		{
			using (SetRatesServiceSubscriptionAirUldEnabled())
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_ConsolMode = ContainerModes.ULD;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;

				var validationResult = RateSelectorCommand.Validate(new RatingCriteria(consolidation.RatingAdapter, Factory));
				AssertEquals(false, validationResult.isValid);
				AssertEquals(false, validationResult.continueAutorating);
				AssertEquals(
					"GIVEN Consol AIR-ULD with no container THEN should show validation error",
					"Container Type is mandatory for running Autorating Costs.",
					UnitTestUserNotification.Instance.LastMessage.Text
				);

				var c = consolidation.Containers.AddNew();
				c.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-1").PK;
				c.JC_ContainerMode = ContainerModes.ULD;
				c.JC_ContainerCount = 1;

				validationResult = RateSelectorCommand.Validate(new RatingCriteria(consolidation.RatingAdapter, Factory));
				AssertEquals(true, validationResult.isValid);
				AssertEquals(true, validationResult.continueAutorating);
			}
		}

		IDisposable SetRatesServiceSubscriptionAirUldEnabled()
		{
			var newValue = new RatesServiceRegistrySettingsCollection();
			newValue.Add(new RatesServiceRegistrySettings(TransportModes.Air, ContainerModes.ULD, true));

			return DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}

		public void TestValidate_ShouldReturnTrue_WhenWiseRateIsActive_TransportModeIsAir_DischargePort_LoadPort_ETA_AreValid_And_CargoGuideIsNotActive()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActive_TransportModeIsAir_DischargePort_LoadPort_ETA_AreValid_CargoGuideIsActive_AndServiceLevelIsEmpty()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				consolidation.JK_AWBServiceLevel = ZString.Empty;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActive_TransportModeIsAir_DischargePort_LoadPort_ETA_AreValid_CargoGuideIsActive_AndPrepaidCollectIsEmpty()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				consolidation.JK_PrepaidCollect = ZString.Empty;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnFalse_WhenWiseRateIsActive_TransportModeIsAir_DischargePort_LoadPort_ETA_AreValid_CargoGuideIsActive_AndShippingAddressIsEmpty()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				consolidation.JK_OA_ShippingLineAddress = ZGuid.Empty;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(false, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldReturnTrue_WhenWiseRateIsActive_TransportModeIsAir_DischargePort_LoadPort_ETA_AreValid_CargoGuideIsActive_And_ServiceLevel_PrepaidCollect_ShippingAddress_AreValid()
		{
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_Address1 = "SLMA";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Air;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				consolidation.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
				consolidation.JK_PrepaidCollect = PaymentType.Prepaid;
				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		public void TestValidate_ShouldNotCheckDatesAndReturnTrue()
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
				consolidation.JK_TransportMode = TransportModes.Sea;
				consolidation.JK_RL_NKLoadPort = "USLAX";
				consolidation.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consolidation.Transports.MostInterestingTransport.JW_ETA = DateTime.Now;
				var container = consolidation.Containers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerCount = 1;

				consolidation.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Empty;
				consolidation.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;

				var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
				AssertEquals(true, RateSelectorCommand.Validate(criteria).isValid);
			}
		}

		void AssertCanRateConsol(bool expected, string transportMode, Directions jobDirection, string containerMode = "FCL", bool hasContainer = false)
		{
			var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
			consolidation.JK_TransportMode = transportMode;
			string origin;
			string destination;
			switch (jobDirection)
			{
				case Directions.Import:
					origin = "USLAX";
					destination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					break;
				case Directions.Export:
					origin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					destination = "USLAX";
					break;
				default:
					throw new ArgumentException("JobDirection should be Import or Export.", nameof(jobDirection));
			}
			consolidation.JK_RL_NKLoadPort = origin;
			consolidation.JK_RL_NKDischargePort = destination;
			consolidation.JK_ConsolMode = containerMode;
			AssertEquals("PRE:", jobDirection, consolidation.JobDirection);

			if (hasContainer)
			{
				var container = consolidation.Containers.AddNew();
				container.JC_ContainerCount = 1;
				container.JC_RC = Helper.Containers["20GP"].PK;
				container.JC_ContainerMode = ContainerModes.FCL;
			}

			var criteria = new RatingCriteria(consolidation.RatingAdapter, Factory);
			AssertEquals(consolidation.JK_TransportMode + "-" + consolidation.JobDirection, expected, RateSelectorCommand.CanRate(criteria));
		}
	}
}
