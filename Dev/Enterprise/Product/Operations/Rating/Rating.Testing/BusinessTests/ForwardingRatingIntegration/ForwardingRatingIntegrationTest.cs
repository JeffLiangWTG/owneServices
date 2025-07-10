using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CustomsReferenceNumberCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class ForwardingRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Default Properties

		protected static ZString OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
				{
					return "SGSIN";
				}

				return "USLAX";
			}
		}

		protected static ZString OverseasPort2
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "HKHKG")
				{
					return "HKHKG";
				}

				return "THBKK";
			}
		}

		#endregion

		#region Shipment Duration/ Penalty

		#region Revenue

		public void TestAutoRateShipmentRevenue_DeliveryTruckWaitTimeDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = TimeSpan.FromHours(3);
				shipment.DocsAndCartage.JP_DeliveryTruckWaitCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryTruckWaitTimePenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"DDTN", "Destination Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
			expectedChargeFromRate: new AssertionCharge
			{
				ChargeCode = testChargeCode.AC_Code,
				JR_OSSellAmt = 300
			},
			expectedChargeFromShipmentDuration: new AssertionCharge
			{
				ChargeCode = testChargeCode.AC_Code,
				JR_OSSellAmt = 450
			},
			expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryTruckWaitTime_BothPenaltyAndDuration()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("USLAX", "AUSYD");
			shipment.JS_PackingMode = ContainerModes.FCL;

			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = TimeSpan.FromHours(3);
			shipment.DocsAndCartage.JP_DeliveryTruckWaitCharge = 250;

			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
			consol.JK_TransportMode = "SEA";

			AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
				ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
				ContainerPenaltyTimeUnit.Codes.Hours, 0, 4, 100);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750 // = 3 * 250 . Which is the charge related to Delivery Truck Wait Time on shipment
				},
				new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 400 // = 4 * 100 . Which is the charge related to Penalty on Delivery for shipment.
				},
			};

			using (RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testChargeCode.PK.ToGuid()))
			{
				AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false);
			}
		}

		public void TestAutoRateShipmentRevenue_DeliveryStorageDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 3;
				shipment.DocsAndCartage.JP_LCLAirStorageCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCTOStorageDays = 3;
				container.ArrivalCTOStorageCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationStorageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryStoragePenalty_CTO()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"DDTN", "Destination Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, //STG
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCTOStorageDays = 3;
				container.ArrivalCTOStorageCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationStorageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryStoragePenalty_Carrier()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"DDTN", "Destination Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, //STC
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.DurationAsDays = 3;
				penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
				penalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationCarrierStorageChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryDetentionDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 3;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryDetentionPenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, //DTN
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_DeliveryMergedDemurrageAndDetention()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"MDDTN", "Destination Merged Demurrage and Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier,
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.DurationAsDays = 3;
				penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
				penalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationMergedDemurrageDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PickupTruckWaitTimeDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_PickupTruckWaitTime = TimeSpan.FromHours(3);
				shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_DeparturePackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originTruckWaitPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
				originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
				originTruckWaitPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PickupTruckWaitTimePenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"ODTN", "Origin Detention Charge Code",
			ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
			RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_DeparturePackCFSTransportAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originTruckWaitPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
				originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
				originTruckWaitPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PickupTruckWaitTime_BothPenaltyAndDuration()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, "AUSYD", "USLAX", QuantityUnit.HR, 100);

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX");
			shipment.JS_PackingMode = ContainerModes.FCL;

			shipment.DocsAndCartage.JP_PickupTruckWaitTime = TimeSpan.FromHours(3);
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 250;

			AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
				ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
				ContainerPenaltyTimeUnit.Codes.Hours, 0, 4, 100);

			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
			consol.JK_TransportMode = "SEA";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750 // = 3 * 250 . Which is the charge related to Pickup Truck Wait Time on shipment
				},
				new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 400 // = 4 * 100 . Which is the charge related to Penalty on Pickup for shipment.
				},
			};

			using (RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testChargeCode.PK.ToGuid()))
			{
				AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false);
			}
		}

		public void TestAutoRateShipmentRevenue_PickupDetentionDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_FCLPickupDetentionDays = 3;
				shipment.DocsAndCartage.JP_FCLPickupDetentionCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originDetentionPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PickupDetentionPenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, //DTN
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originDetentionPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PickupMergedDemurrageAndDetention()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"MODTN", "Origin Merged Demurrage and Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier,
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originDetentionPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PenaltyWithMultipleCreditor()
		{
			AssertAutoRateShipmentRevenue_PenaltyWithMultipleCreditor();
		}

		public void TestAutoRateShipmentRevenue_PenaltyWhereCreditorsHaveSubsidiaryCompany()
		{
			var localClient1 = Helper.NewOrgHeader();
			var localClient2 = Helper.NewOrgHeader();
			TransportProvider1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient1);
			TransportProvider2.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient2);

			AssertEquals("Precondition: TransportProvider1 should have 1 subsidiary company", 1, TransportProvider1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: TransportProvider2 should have 1 subsidiary company", 1, TransportProvider2.RelatedManagementSubsidiaryRelations.Organisations.Count());

			AssertAutoRateShipmentRevenue_PenaltyWithMultipleCreditor();
		}

		void AssertAutoRateShipmentRevenue_PenaltyWithMultipleCreditor()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
			"DDTN", "Destination Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150, TransportProvider1.PK);

				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 2, 170, TransportProvider2.PK, CurrencyCodes.India);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargesFromRate: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 300,
							CostAccountCode = TransportProvider1.OH_Code,
							SellAccountCode = "NEWTESSYD",
							JR_RX_NKSellCurrency = CurrencyCodes.Australia
						},
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 200,
							CostAccountCode = TransportProvider2.OH_Code,
							SellAccountCode = "NEWTESSYD",
							JR_RX_NKSellCurrency = CurrencyCodes.Australia
						}
				},
				expectedChargesFromRateWhenShipmentDurationNotSet: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 300,
							CostAccountCode = TransportProvider1.OH_Code,
							SellAccountCode = "NEWTESSYD",
							JR_RX_NKSellCurrency = CurrencyCodes.Australia
						}
				},
				expectedChargesFromShipmentDuration: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 450,
							CostAccountCode = TransportProvider1.OH_Code,
							SellAccountCode = "NEWTESSYD",
							JR_RX_NKSellCurrency = CurrencyCodes.Australia
						},
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 340,
							CostAccountCode = TransportProvider2.OH_Code,
							SellAccountCode = "NEWTESSYD",
							JR_RX_NKSellCurrency = CurrencyCodes.India
						}
				},
				expectedChargesFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PenaltyWhenTotalCostIsOverridden_UnitIsHour()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				var penalty1 = AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150);

				AssertEquals("Pre-condition: 150 x 3 = 450", 450m, penalty1.CPY_TotalCost);
				penalty1.CPY_TotalCost = 0; // complete waive off

				var penalty2 = AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 2, 170);

				AssertEquals("Pre-condition: 170 x 2 = 340", 340m, penalty2.CPY_TotalCost);
				penalty2.CPY_TotalCost = 310; // discount
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargesFromRate: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 500,
							RevenueCalculationDescription = "DDTN: 5 Hour(s) @ AUD 100.00/Hour"
						}
				},
				expectedChargesFromRateWhenShipmentDurationNotSet: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 300,
							RevenueCalculationDescription = "DDTN: 3 Hour(s) @ AUD 100.00/Hour"
						}
				},
				expectedChargesFromShipmentDuration: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 0,
							RevenueCalculationDescription = "DDTN: Base Rate AUD 0.00"
						},
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 310,
							RevenueCalculationDescription = "DDTN: Base Rate AUD 310.00"
						}
				},
				expectedChargesFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentRevenue_PenaltyWhenTotalCostIsOverridden_UnitIsDay()
		{
			var testChargeCode = CreateTestChargeCodeAndClientRate(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				var penalty1 = AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);

				AssertEquals("Pre-condition: 150 x 3 = 450", 450m, penalty1.CPY_TotalCost);
				penalty1.CPY_TotalCost = 0; // complete waive off

				var penalty2 = AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Days, 0, 2, 170);

				AssertEquals("Pre-condition: 170 x 2 = 340", 340m, penalty2.CPY_TotalCost);
				penalty2.CPY_TotalCost = 310; // discount
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromDays(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargesFromRate: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 500,
							RevenueCalculationDescription = "DDTN: 5 Day(s) @ AUD 100.00/Day"
						}
				},
				expectedChargesFromRateWhenShipmentDurationNotSet: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 300,
							RevenueCalculationDescription = "DDTN: 3 Day(s) @ AUD 100.00/Day"
						}
				},
				expectedChargesFromShipmentDuration: new[] {
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 0,
							RevenueCalculationDescription = "DDTN: Base Rate AUD 0.00"
						},
						new AssertionCharge
						{
							ChargeCode = testChargeCode.AC_Code,
							JR_OSSellAmt = 310,
							RevenueCalculationDescription = "DDTN: Base Rate AUD 310.00"
						}
				},
				expectedChargesFromContainerDuration: null);

			AutoRateShipmentPenaltyRevenueSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		#endregion

		#region Cost

		public void TestAutoRateShipmentCost_DeliveryTruckWaitTimeDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = TimeSpan.FromHours(3);
				shipment.DocsAndCartage.JP_DeliveryTruckWaitCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryTruckWaitTimePenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
					"DDTN", "Destination Detention Charge Code",
					ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
					RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalTruckWaitTime = TimeSpan.FromHours(3);
				container.ArrivalTruckWaitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryStorageDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 3;
				shipment.DocsAndCartage.JP_LCLAirStorageCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCTOStorageDays = 3;
				container.ArrivalCTOStorageCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationStorageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryStoragePenalty_CTO()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
			"DDTN", "Destination Detention Charge Code",
			ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
			RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, //STG
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCTOStorageDays = 3;
				container.ArrivalCTOStorageCost = 250;
			}

			var expectedResults = new AssertionCharges(
			expectedChargeFromRate: new AssertionCharge
			{
				ChargeCode = testChargeCode.AC_Code,
				JR_OSSellAmt = 300
			},
			expectedChargeFromShipmentDuration: null,
			expectedChargeFromContainerDuration: new AssertionCharge
			{
				ChargeCode = testChargeCode.AC_Code,
				JR_OSSellAmt = 750
			});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationStorageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryStoragePenalty_Carrier()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, //STC
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.DurationAsDays = 3;
				penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
				penalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationCarrierStorageChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryDetentionDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 3;
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryDetentionPenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, //DTN
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				container.ArrivalCarrierDetentionDays = 3;
				container.ArrivalCarrierDetentionCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_DeliveryMergedDemurrageAndDetention()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"MDDTN", "Destination Merged Demurrage And Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.DST, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.DeliveryPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier,
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_ArrivalCTOAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.DurationAsDays = 3;
				penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
				penalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.DestinationMergedDemurrageDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_PickupTruckWaitTimeDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_PickupTruckWaitTime = TimeSpan.FromHours(3);
				shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_DeparturePackCFSTransportAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originTruckWaitPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
				originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
				originTruckWaitPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_PickupTruckWaitTimePenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.HR, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, //DME
					ContainerPenaltyTimeUnit.Codes.Hours, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_DeparturePackCFSTransportAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originTruckWaitPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Hours);
				originTruckWaitPenalty.CPY_Duration = TimeSpan.FromHours(3);
				originTruckWaitPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_PickupDetentionDurationAndCharge()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				shipment.DocsAndCartage.JP_FCLPickupDetentionDays = 3;
				shipment.DocsAndCartage.JP_FCLPickupDetentionCharge = 150;
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originDetentionPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 450
				},
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentDurationCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_PickupDetentionPenalty()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, //DTN
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originDetentionPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originDetentionPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originDetentionPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginDetentionChargeCode,
				testChargeCode);
		}

		public void TestAutoRateShipmentCost_PickupMergedDemurrageAndDetention()
		{
			var testChargeCode = CreateTestChargeCodeAndCosting(
				"MDDTN", "Destination Merged Demurrage And Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, "USLAX", "AUSYD", QuantityUnit.DY, 100);

			void setupShipmentDuration(ForwardingShipment shipment)
			{
				AttachPenaltyToShipment(shipment.PickupPenalties, shipment, shipment.Containers.FirstOrDefault(),
					ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier,
					ContainerPenaltyTimeUnit.Codes.Days, 0, 3, 150);
			}

			void setupContainerDuration(ForwardingShipment shipment)
			{
				var consol = shipment.Consols.OfType<ForwardingConsol>().Single();
				consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

				var container = consol.Containers.OfType<ForwardingContainer>().Single();
				var originPenalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				originPenalty.CPY_Duration = TimeSpan.FromDays(3);
				originPenalty.CPY_PerUnitCost = 250;
			}

			var expectedResults = new AssertionCharges(
				expectedChargeFromRate: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 300
				},
				expectedChargeFromShipmentDuration: null,
				expectedChargeFromContainerDuration: new AssertionCharge
				{
					ChargeCode = testChargeCode.AC_Code,
					JR_OSSellAmt = 750
				});

			AutoRateShipmentPenaltyCostSuite(
				expectedResults,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem: () => RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode,
				testChargeCode);
		}

		#endregion

		#region Helpers

		struct AssertionCharges
		{
			public AssertionCharges(
				AssertionCharge expectedChargeFromRate,
				AssertionCharge expectedChargeFromShipmentDuration,
				AssertionCharge expectedChargeFromContainerDuration)
			{
				ExpectedChargesFromRate = new[] { expectedChargeFromRate };
				ExpectedChargesFromRateWhenShipmentDurationNotSet = new[] { expectedChargeFromRate };
				ExpectedChargesFromShipmentDuration = new[] { expectedChargeFromShipmentDuration };
				ExpectedChargesFromContainerDuration = new[] { expectedChargeFromContainerDuration };
			}

			public AssertionCharges(
				AssertionCharge[] expectedChargesFromRate,
				AssertionCharge[] expectedChargesFromRateWhenShipmentDurationNotSet,
				AssertionCharge[] expectedChargesFromShipmentDuration,
				AssertionCharge[] expectedChargesFromContainerDuration)
			{
				ExpectedChargesFromRate = expectedChargesFromRate;
				ExpectedChargesFromRateWhenShipmentDurationNotSet = expectedChargesFromRateWhenShipmentDurationNotSet;
				ExpectedChargesFromShipmentDuration = expectedChargesFromShipmentDuration;
				ExpectedChargesFromContainerDuration = expectedChargesFromContainerDuration;
			}

			public readonly AssertionCharge[] ExpectedChargesFromRate;
			public readonly AssertionCharge[] ExpectedChargesFromRateWhenShipmentDurationNotSet;
			public readonly AssertionCharge[] ExpectedChargesFromShipmentDuration;
			public readonly AssertionCharge[] ExpectedChargesFromContainerDuration;
		}

		AccChargeCode CreateTestChargeCodeAndClientRate(
			ZString chargeCodeCode, ZString chargeCodeDescription, string chargeCodeGroup, string chargeCodeSubGroup,
			ZString rateCategory, string origin, string destination, string rateQuantityUnit, ZDecimal ratePerUnitPrice)
		{
			var testChargeCode = Helper.ChargeCodes.New(chargeCodeCode, chargeCodeDescription, UnitCalculator.Code, chargeCodeGroup, chargeCodeSubGroup);

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(rateCategory, RateMode.SEA, origin, destination);
			var rateLine = rateEntry.AddRateLine(testChargeCode, UnitCalculator.Code, rateQuantityUnit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = ratePerUnitPrice;

			Factory.Save();

			return testChargeCode;
		}

		AccChargeCode CreateTestChargeCodeAndCosting(
			ZString chargeCodeCode, ZString chargeCodeDescription, string chargeCodeGroup, string chargeCodeSubGroup,
			ZString rateCategory, string origin, string destination, string rateQuantityUnit, ZDecimal ratePerUnitPrice)
		{
			return CreateTestChargeCodeAndCosting(TransportProvider1,
			chargeCodeCode, chargeCodeDescription, chargeCodeGroup, chargeCodeSubGroup,
			rateCategory, origin, destination, rateQuantityUnit, ratePerUnitPrice);
		}

		AccChargeCode CreateTestChargeCodeAndCosting(OrgHeader orgHeader, ZString chargeCodeCode, ZString chargeCodeDescription,
			string chargeCodeGroup, string chargeCodeSubGroup, ZString rateCategory, string origin, string destination,
			string rateQuantityUnit, ZDecimal ratePerUnitPrice)
		{
			var testChargeCode = Helper.ChargeCodes.New(chargeCodeCode, chargeCodeDescription, UnitCalculator.Code, chargeCodeGroup, chargeCodeSubGroup);

			var costing = Helper.NewCosting(orgHeader);
			var rateEntry = costing.AddRateEntry(rateCategory, RateMode.SEA, origin, destination);
			var rateLine = rateEntry.AddRateLine(testChargeCode, UnitCalculator.Code, rateQuantityUnit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = ratePerUnitPrice;

			orgHeader.OH_IsDebtor = true;

			Factory.Save();

			return testChargeCode;
		}

		ForwardingShipment CreateTestShipmentWithAttachedConsolAndContainer(string origin, string destination, string consolPrepaidCollect = "")
		{
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, origin, destination, 100, 1);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, TransportProvider1, shipment, consolPrepaidCollect);
			TransportProvider1.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = Helper.Containers["20GP"].PK;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(shipment.OuterPackLines.AddNew());

			return shipment;
		}

		void AutoRateShipmentPenaltyRevenueSuite(
			AssertionCharges expected,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			AccChargeCode testChargeCode)
		{
			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testChargeCode.PK.ToString());

			void setupShipmentForValidShipmentPenalty(ForwardingShipment forwardingShipment)
			{
				TransportProvider2.OH_IsCreditor = true;
				forwardingShipment.JS_PackingMode = ContainerModes.FCL;
				setupShipmentDuration?.Invoke(forwardingShipment);
			}

			AutoRateShipmentDurationRevenueSuite(expected, setupShipmentForValidShipmentPenalty, setupContainerDuration, getChargeCodeRegistryItem, testChargeCode);
		}

		void AutoRateShipmentDurationRevenueSuite(
			AssertionCharges expected,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			AccChargeCode testChargeCode)
		{
			// Given:
			// - shipment duration set
			// - container duration set
			// Then:
			// - shipment duration takes priority
			// - If registry has no charge code: expect client rate
			// - If registry has charge code: expect spot rate calculated from shipment duration and its charge
			AutoRateRevenueAndAssert(
				expected.ExpectedChargesFromRate,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem, Guid.Empty);

			AutoRateRevenueAndAssert(
				expected.ExpectedChargesFromShipmentDuration,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem, testChargeCode.PK.ToGuid());

			// Given:
			// - shipment duration not set
			// - container duration set
			// Then:
			// - fall back to use container duration
			// - If registry has no charge code: expect client rate
			// - If registry has charge code: expect client rate since spot rate on container only rates as a cost
			AutoRateRevenueAndAssert(
				expected.ExpectedChargesFromRateWhenShipmentDurationNotSet,
				null,
				setupContainerDuration,
				getChargeCodeRegistryItem, Guid.Empty);

			AutoRateRevenueAndAssert(
				expected.ExpectedChargesFromRateWhenShipmentDurationNotSet,
				null,
				setupContainerDuration,
				getChargeCodeRegistryItem, testChargeCode.PK.ToGuid());
		}

		void AutoRateRevenueAndAssert(
			IEnumerable<AssertionCharge> expectedCharges,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			Guid chargeCodePKToSetToRegistry)
		{
			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("USLAX", "AUSYD");
			setupShipmentDuration?.Invoke(shipment);
			setupContainerDuration?.Invoke(shipment);

			using (getChargeCodeRegistryItem().SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodePKToSetToRegistry))
			{
				AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false);
			}
		}

		void AutoRateShipmentPenaltyCostSuite(
			AssertionCharges expected,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			AccChargeCode testChargeCode)
		{
			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testChargeCode.PK.ToString());

			void setupShipmentForValidShipmentPenalty(ForwardingShipment forwardingShipment)
			{
				forwardingShipment.JS_PackingMode = ContainerModes.FCL;
				setupShipmentDuration?.Invoke(forwardingShipment);
			}

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutoRateShipmentDurationCostSuite(expected, setupShipmentForValidShipmentPenalty, setupContainerDuration, getChargeCodeRegistryItem, testChargeCode);
			}
		}

		void AutoRateShipmentDurationCostSuite(
			AssertionCharges expected,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			AccChargeCode testChargeCode)
		{
			// Given:
			// - container duration set
			// - shipment duration set
			// Then:
			// - container duration takes priority
			// - If registry has no charge code: expect costing for creditor
			// - If registry has charge code: expect spot rate calculated from container duration and its charge
			AutoRateCostAndAssert(
				expected.ExpectedChargesFromRate,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem, Guid.Empty);

			AutoRateCostAndAssert(
				expected.ExpectedChargesFromContainerDuration,
				setupShipmentDuration,
				setupContainerDuration,
				getChargeCodeRegistryItem, testChargeCode.PK.ToGuid());

			// Given:
			// - container duration not set
			// - shipment duration set
			// Then:
			// - fall back to use shipment duration
			// - If registry has no charge code: expect costing for creditor
			// - If registry has charge code: expect rate calculated from client rate since spot rate on shipment only rates as revenue
			AutoRateCostAndAssert(
				expected.ExpectedChargesFromRate,
				setupShipmentDuration,
				null,
				getChargeCodeRegistryItem, Guid.Empty);

			AutoRateCostAndAssert(
				expected.ExpectedChargesFromRate,
				setupShipmentDuration,
				null,
				getChargeCodeRegistryItem, testChargeCode.PK.ToGuid());
		}

		void AutoRateCostAndAssert(
			IEnumerable<AssertionCharge> expectedCharges,
			Action<ForwardingShipment> setupShipmentDuration,
			Action<ForwardingShipment> setupContainerDuration,
			Func<RegistryItemWrapper> getChargeCodeRegistryItem,
			Guid chargeCodePKToSetToRegistry)
		{
			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("USLAX", "AUSYD");
			setupShipmentDuration?.Invoke(shipment);
			setupContainerDuration?.Invoke(shipment);

			using (getChargeCodeRegistryItem().SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodePKToSetToRegistry))
			{
				AutorateAndAssert(expectedCharges, shipment, TransportProvider1, autorateRevenue: false);
			}
		}

		ShipmentContainerPenalty AttachPenaltyToShipment(
			ShipmentContainerPenaltyCollection penaltyCollection,
			ForwardingShipment forwardingShipment,
			CommonContainer container,
			ZString penaltyType,
			ZString creditorType,
			ZString timeUnit,
			ZByte freeTimeAsDays,
			ZByte duration,
			ZDecimal perUnitCost,
			ZGuid creditorPK = default,
			string currency = "")
		{
			var penalty = penaltyCollection.AddNew();
			penalty.CPY_JS_Shipment = forwardingShipment?.PK ?? ZGuid.Empty;

			if (container != default)
			{
				penalty.CPY_JC_Container = container.PK;
			}

			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_CreditorType = creditorType;

			penalty.CPY_TimeUnit = timeUnit;
			penalty.FreeTimeAsDays = freeTimeAsDays;

			if (timeUnit == ContainerPenaltyTimeUnit.Codes.Hours)
			{
				penalty.CPY_Duration = TimeSpan.FromHours(duration);
			}
			else
			{
				penalty.DurationAsDays = duration;
			}

			penalty.CPY_PerUnitCost = perUnitCost;

			if (creditorPK != default)
			{
				penalty.CPY_OH_Creditor = creditorPK;
			}

			if (!string.IsNullOrEmpty(currency))
			{
				penalty.CPY_RX_NKCurrency = currency;
			}

			return penalty;
		}

		#endregion

		#endregion

		#region Consol Penalty

		void AddAgencytoOrganisation(OrgHeader organisation, string portOrCountry)
		{
			var agencyOrg = Factory.NewWithValidTestData<OrgHeader>();
			agencyOrg.OH_IsCreditor = true;

			var port = organisation.CarrierAppointedAgentPorts_Agency.AddNew();
			port.O5_PortOrCountry = portOrCountry;
			port.O5_OA_AgentOfficeAddress = agencyOrg.MainAddress.PK;
		}

		public void TestAutoRateConsolCost_ExportConsol_ChargeSetInRegistry_PenaltyCreditorTypeIsCAR()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var registryOrgDetentionCharge = Helper.ChargeCodes.NewConsolChargeCode("ORGDTNGRP", "ORG Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention);
			registryOrgDetentionCharge.AC_IsGroupageCharge = true;
			registryOrgDetentionCharge.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			using (RatingDataRegistry.Instance.OriginDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryOrgDetentionCharge.PK.ToGuid()))
			{
				var creditorOrg = Factory.NewWithValidTestData<OrgHeader>();
				creditorOrg.OH_IsCreditor = true;

				Factory.Save();

				var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Prepaid);

				var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.CPY_Duration = TimeSpan.FromDays(3);
				penalty.CPY_PerUnitCost = 5;

				Factory.Save();

				AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

				var expectedCosts = new[]
				{
						new AssertionCost
						{
							ChargeCode = registryOrgDetentionCharge.AC_Code,
							E6_OSCostAmount = 15m,
							E6_OH_Creditor = carrierOrg.PK
						}
					};

				penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
				AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when creditor on penalty is empty", null, expectedCosts, consol, autorateRevenue: false);

				penalty.CPY_OH_Creditor = penaltyCreditor.PK;

				expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = registryOrgDetentionCharge.AC_Code,
						E6_OSCostAmount = 15m,
						E6_OH_Creditor = penaltyCreditor.PK
					}
				};

				AutoCostAndAssert("Charge creditor should be penalty's creditor for registry origin detention charge", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		public void TestAutoRateConsolCost_ExportConsol_PenaltyCreditorTypeIsCAR()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var carrierChargeCode = CreateTestChargeCodeAndCosting(carrierOrg,
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 100);

			carrierChargeCode.AC_IsGroupageCharge = true;
			carrierChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var creditorOrg = Factory.NewWithValidTestData<OrgHeader>();
			creditorOrg.OH_IsCreditor = true;

			var creditorChargeCode = CreateTestChargeCodeAndCosting(creditorOrg,
				"ODTL", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 150);

			creditorChargeCode.AC_IsGroupageCharge = true;
			creditorChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var orgDetention = Helper.ChargeCodes.NewConsolChargeCode("ORGDTNGRP", "ORG Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention);
			orgDetention.AC_IsGroupageCharge = true;
			orgDetention.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;
			RatingDataRegistry.Instance.OriginDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgDetention.PK.ToGuid());

			Factory.Save();

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Prepaid);

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carrierChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when creditor on penalty is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditorOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditorOrg.PK
				}
			};

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor when creditor on penalty is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.CreditorPK = creditorOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditorOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > creditor if charge comes from creditor (even if carrier has value)", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carrierChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.CreditorPK = ZGuid.Empty;

			AutoCostAndAssert("Charge creditor should be penalty's creditor even if charge service provider is carrier", null, expectedCosts, consol, autorateRevenue: false);

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditorOrg.PK;

			AutoCostAndAssert("Charge creditor should be the penalty's creditor even if charge service provider is creditor", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"ODTM", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ExportConsol_PenaltyCreditorTypeIsCTO()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(ctoOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var ctoChargeCode = CreateTestChargeCodeAndCosting(ctoOrg,
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 100);

			ctoChargeCode.AC_IsGroupageCharge = true;
			ctoChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, ctoOrg, PaymentType.Prepaid);
			consol.JK_OA_DepartureCTOAddress = ctoOrg.MainAddress.PK;

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = ctoChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = ctoOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Departure > Details > CTO Address when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = ctoChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even charge comes from CTO Address", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"ODTM", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ExportConsol_PenaltyCreditorTypeIsTRS()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var trsOrg = Factory.NewWithValidTestData<OrgHeader>();
			trsOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(trsOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var trsChargeCode = CreateTestChargeCodeAndCosting(trsOrg,
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 100);

			trsChargeCode.AC_IsGroupageCharge = true;
			trsChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, trsOrg, PaymentType.Prepaid);
			consol.JK_OA_DeparturePackCFSTransportAddress = trsOrg.MainAddress.PK;

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = trsChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = trsOrg.PK
				}
			};

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
			AutoCostAndAssert("Charge creditor should be Consol > Details > Departure > Details > Port Transport when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;
			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = trsChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from CTO Address", null, expectedCosts, consol, autorateRevenue: false);

			var enaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"ODTM", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 200);

			enaltyChargeCode.AC_IsGroupageCharge = true;
			enaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = enaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ImportConsol_ChargeSetInRegistry_PenaltyCreditorTypeIsCAR()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var registryDstDetentionCharge = Helper.ChargeCodes.NewConsolChargeCode("DSTDTNGRP", "DST Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention);
			registryDstDetentionCharge.AC_IsGroupageCharge = true;
			registryDstDetentionCharge.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			using (RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryDstDetentionCharge.PK.ToGuid()))
			{
				var creditorOrg = Factory.NewWithValidTestData<OrgHeader>();
				creditorOrg.OH_IsCreditor = true;

				Factory.Save();

				var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Collect);

				var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
				penalty.CPY_Duration = TimeSpan.FromDays(3);
				penalty.CPY_PerUnitCost = 5;

				Factory.Save();

				AssertEquals("Preconditions: Import consol expected.", true, consol.IsImport());

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = registryDstDetentionCharge.AC_Code,
						E6_OSCostAmount = 15m,
						E6_OH_Creditor = carrierOrg.PK
					}
				};

				penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
				AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

				penalty.CPY_OH_Creditor = penaltyCreditor.PK;

				expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = registryDstDetentionCharge.AC_Code,
						E6_OSCostAmount = 15m,
						E6_OH_Creditor = penaltyCreditor.PK
					}
				};

				AutoCostAndAssert("Charge creditor should be penalty's creditor for registry destination detention charge", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		public void TestAutoRateConsolCost_ImportConsol_PenaltyCreditorTypeIsCAR()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var carOrg = Factory.NewWithValidTestData<OrgHeader>();
			carOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carOrg, destination);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var carChargeCode = CreateTestChargeCodeAndCosting(carOrg,
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 100);

			carChargeCode.AC_IsGroupageCharge = true;
			carChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carOrg, PaymentType.Collect);

			var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = carOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = carOrg.MainAddress.PK;
			consol.CreditorPK = creditor.PK;

			var creditorChargeCode = CreateTestChargeCodeAndCosting(creditor,
				"DDTL", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 150);

			creditorChargeCode.AC_IsGroupageCharge = true;
			creditorChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor if charge comes from Consol's creditor (even if Consol's carrier has value)", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = carOrg.MainAddress.PK;
			consol.CreditorPK = ZGuid.Empty;

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from Consol's carrier", null, expectedCosts, consol, autorateRevenue: false);

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditor.PK;

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from Consol's creditor", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"DDTM", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be equal to penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ImportConsol_PenaltyCreditorTypeIsCTO()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(ctoOrg, destination);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var ctoChargeCode = CreateTestChargeCodeAndCosting(ctoOrg,
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 100);

			ctoChargeCode.AC_IsGroupageCharge = true;
			ctoChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, ctoOrg, PaymentType.Collect);
			consol.JK_OA_ArrivalCTOAddress = ctoOrg.MainAddress.PK;

			var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			consol.JK_OA_ArrivalCTOAddress = ctoOrg.MainAddress.PK;
			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = ctoChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = ctoOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Arrival > Details > CTO Address when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = ctoChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from CTO Address", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"DDTM", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ImportConsol_PenaltyCreditorTypeIsTRS()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var trsOrg = Factory.NewWithValidTestData<OrgHeader>();
			trsOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(trsOrg, destination);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var trsChargeCode = CreateTestChargeCodeAndCosting(trsOrg,
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 100);

			trsChargeCode.AC_IsGroupageCharge = true;
			trsChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, trsOrg, PaymentType.Collect);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = trsOrg.MainAddress.PK;

			var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = trsChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = trsOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Arrival > Details > Port Transport when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = trsChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from CTO Address", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"DDTM", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		#region Merged Demurrage And Detention

		public void TestAutoRateConsolCost_ExportConsol_MergedDemurrageAndDetention_CreditorLogic()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var carrierChargeCode = CreateTestChargeCodeAndCosting(carrierOrg,
				"ODTN", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 100);

			carrierChargeCode.AC_IsGroupageCharge = true;
			carrierChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var creditorOrg = Factory.NewWithValidTestData<OrgHeader>();
			creditorOrg.OH_IsCreditor = true;

			var creditorChargeCode = CreateTestChargeCodeAndCosting(creditorOrg,
				"ODTL", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 150);

			creditorChargeCode.AC_IsGroupageCharge = true;
			creditorChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var orgDetention = Helper.ChargeCodes.NewConsolChargeCode("ORGDTNGRP", "ORG Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention);
			orgDetention.AC_IsGroupageCharge = true;
			orgDetention.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;
			RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgDetention.PK.ToGuid());

			Factory.Save();

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Prepaid);

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carrierChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when creditor on penalty is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditorOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditorOrg.PK
				}
			};

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor when creditor on penalty is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.CreditorPK = creditorOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditorOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > creditor if charge comes from creditor (even if carrier has value)", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carrierChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.CreditorPK = ZGuid.Empty;

			AutoCostAndAssert("Charge creditor should be penalty's creditor even if charge service provider is carrier", null, expectedCosts, consol, autorateRevenue: false);

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditorOrg.PK;

			AutoCostAndAssert("Charge creditor should be the penalty's creditor even if charge service provider is creditor", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"ODTM", "Origin Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ExportConsol_MergedDemurrageAndDetention_Calculation()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var testChargeCode = CreateTestChargeCodeAndCosting(carrierOrg,
				"OMDD", "Origin Merged Dumerrage and Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, origin, destination, QuantityUnit.DY, 100);

			testChargeCode.AC_IsGroupageCharge = true;
			testChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var orgChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGMDD", "ORG Merged Dummerage And Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention);
			orgChargeCode.AC_IsGroupageCharge = true;
			orgChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			Factory.Save();

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Prepaid);

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			AutoCostAndAssert("Pre-defined Costing", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_PerUnitCost = 150;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				},
				new AssertionCost
				{
					ChargeCode = orgChargeCode.AC_Code,
					E6_OSCostAmount = 450,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			using (RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgChargeCode.PK.ToGuid()))
			{
				AutoCostAndAssert("Both Costing and Penalty", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		public void TestAutoRateConsolCost_ImportConsol_MergedDemurrageAndDetention_CreditorLogic()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var carOrg = Factory.NewWithValidTestData<OrgHeader>();
			carOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carOrg, destination);

			var penaltyCreditor = Factory.NewWithValidTestData<OrgHeader>();
			penaltyCreditor.OH_IsCreditor = true;

			var carChargeCode = CreateTestChargeCodeAndCosting(carOrg,
				"DDTN", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 100);

			carChargeCode.AC_IsGroupageCharge = true;
			carChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carOrg, PaymentType.Collect);

			var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Carrier when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = carOrg.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			penalty.CPY_OH_Creditor = ZGuid.Empty; //Penalty's creditor is empty
			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor when penalty's creditor is empty", null, expectedCosts, consol, autorateRevenue: false);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = carOrg.MainAddress.PK;
			consol.CreditorPK = creditor.PK;

			var creditorChargeCode = CreateTestChargeCodeAndCosting(creditor,
				"DDTL", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 150);

			creditorChargeCode.AC_IsGroupageCharge = true;
			creditorChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = creditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be Consol > Details > Organisations > Creditor if charge comes from Consol's creditor (even if Consol's carrier has value)", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = carChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = carOrg.MainAddress.PK;
			consol.CreditorPK = ZGuid.Empty;

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from Consol's carrier", null, expectedCosts, consol, autorateRevenue: false);

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = creditorChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.CreditorPK = creditor.PK;

			AutoCostAndAssert("Charge creditor should be penalty's creditor when it is not empty, even if charge comes from Consol's creditor", null, expectedCosts, consol, autorateRevenue: false);

			var penaltyChargeCode = CreateTestChargeCodeAndCosting(penaltyCreditor,
				"DDTM", "Destination Detention Charge Code",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 200);

			penaltyChargeCode.AC_IsGroupageCharge = true;
			penaltyChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			penalty.CPY_OH_Creditor = penaltyCreditor.PK;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = penaltyChargeCode.AC_Code,
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = penaltyCreditor.PK
				}
			};

			AutoCostAndAssert("Charge creditor should be equal to penalty's creditor when it is not empty and charge comes from penalty's creditor", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutoRateConsolCost_ImportConsol_MergedDemurrageAndDetention_Calculation()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var carOrg = Factory.NewWithValidTestData<OrgHeader>();
			carOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carOrg, destination);

			var testChargeCode = CreateTestChargeCodeAndCosting(carOrg,
				"DMDD", "Destination Merged Dummerage and Detention",
				ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.DST, origin, destination, QuantityUnit.DY, 100);

			testChargeCode.AC_IsGroupageCharge = true;
			testChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carOrg, PaymentType.Collect);

			var penalty = container.ImportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			AutoCostAndAssert("Costing Service Calculation", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_PerUnitCost = 150;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 450m,
					E6_OH_Creditor = carOrg.PK
				}
			};

			using (RatingDataRegistry.Instance.DestinationMergedDemurrageDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testChargeCode.PK.ToGuid()))
			{
				AutoCostAndAssert("Costing Calculation Replaced By Job Service Calculation", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		public void TestAutoRateConsolCost_MergedDemurrageAndDetention_DifferentChargesLessSpecificLocation()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsCreditor = true;

			AddAgencytoOrganisation(carrierOrg, origin);

			var testChargeCode = CreateTestChargeCodeAndCosting(carrierOrg,
				"OMDD", "Origin Merged Dumerrage and Detention Charge Code",
				ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention,
				RatingConstants.RateCategory.ORG, "AU", "US", QuantityUnit.DY, 100); // Less Specific (in terms of location) Costing

			testChargeCode.AC_IsGroupageCharge = true;
			testChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var orgChargeCode = Helper.ChargeCodes.NewConsolChargeCode("ORGMDD", "ORG Merged Dummerage And Detention", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.MergedDemurrageDetention);
			orgChargeCode.AC_IsGroupageCharge = true;
			orgChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			Factory.Save();

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, carrierOrg, PaymentType.Prepaid);

			var penalty = container.ExportPenalties.CreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyTimeUnit.Codes.Days);
			penalty.CPY_Duration = TimeSpan.FromDays(3);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			AutoCostAndAssert("Pre-defined Costing", null, expectedCosts, consol, autorateRevenue: false);

			penalty.CPY_PerUnitCost = 150;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCode.AC_Code,
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = carrierOrg.PK
				},
				new AssertionCost
				{
					ChargeCode = orgChargeCode.AC_Code,
					E6_OSCostAmount = 450,
					E6_OH_Creditor = carrierOrg.PK
				}
			};

			using (RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, orgChargeCode.PK.ToGuid()))
			{
				AutoCostAndAssert("Both Costing and Penalty", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		#endregion

		#endregion

		#region Shipment Contract Numbers

		public void TestMultiClientContractNumbersFromRates_CancelPopup_ShouldNotCauseApplicationException()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entryAAA = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			var entryBBB = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";

			var entry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "FRT", 300);
			entry.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX", PaymentType.Prepaid);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var jobHeader = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "", "AAA", "BBB" };
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns((string)null); // value null returned means selection was cancelled

			AssertNoExceptionThrown(() => AutoCostAndAssert("", null, null, consol, dialogService: mockedDialogService.Object));
		}

		public void TestFilterAndPopulateShipmentContractNumber_MultiNumbersFromRates_SameChargeCode()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entryAAA = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			var entryBBB = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";

			var entry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 300);
			entry.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 100);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			jobHeader.JH_ClientContractNumber = "";

			var expectedCharges = new[] {
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200
				}
			};

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "", "AAA", "BBB" };
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("BBB");
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			var message = "Should filter rates by selected contract number.";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false, ratingContext: testRatingContext);

			message = "Multiple rates should have been found and a number should be selected";
			AssertEquals("BBB", jobHeader.JH_ClientContractNumber);
			mockedDialogService.Verify(x => x.SelectSingleClientContractNumber(expectedArgumentSetup), Times.Once);
		}

		public void TestFilterByClientContractNumber_MultiNumbersFromRates_SameChargeCode_ClientRateShouldNotOverrideOnCompanyTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GlobalRateLevel = 1;
			var entryBBB = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";
			companyTariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(consignee);
			var entryAAA = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, consignee.PK, "AUSYD", "USLAX", 100);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			jobHeader.JH_ClientContractNumber = "BBB";

			var expectedCharges = new[] {
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200
				}
			};

			var loggerDecorator = new LoggerDecorator(new TestInteractor());
			var context = new RatingContext(loggerDecorator, Factory, null);

			var message = "Should filter rates by Job's Client Contract Number.";
			AutorateAndAssert(message, expectedCharges, shipment, consignee, autorateCosts: false, ratingContext: context);
		}

		public void TestFilterByClientContractNumber_MultiNumbersFromRates_SameChargeCode_BetweenClientRateAndGroupClientRate()
		{
			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var groupClient1 = Helper.NewOrgHeader();
			var groupClient2 = Helper.NewOrgHeader();
			Factory.Save();

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;
			defaultLevel = groupClient1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			defaultLevel.P7_ApplyGroupRate = true;

			groupClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			groupClient2.RelatedManagementSubsidiaryRelations.AddOrganisation(groupClient1);
			Factory.Save();

			AssertEquals("Precondition: groupClient1 should have 1 subsidiary company", 1, groupClient1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: groupClient2 should have 1 subsidiary company", 1, groupClient2.RelatedManagementSubsidiaryRelations.Organisations.Count());

			var clientRate = Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 10m);
			var groupClientRate1 = Helper.NewClientRateWithSingleRateLine(groupClient1, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 20m);
			var groupClientRate2 = Helper.NewClientRateWithSingleRateLine(groupClient2, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 30m);

			var clientRateEntry = clientRate.GetRateEntryCollectionForCategory("AIR")[0];
			clientRateEntry.TI_ContractNumber = "AAA";
			var groupRateEntry1 = groupClientRate1.GetRateEntryCollectionForCategory("AIR")[0];
			groupRateEntry1.TI_ContractNumber = "BBB";
			var groupRateEntry2 = groupClientRate2.GetRateEntryCollectionForCategory("AIR")[0];
			groupRateEntry2.TI_ContractNumber = "CCC";

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 1000);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			var ratingAdapter = shipment.RatingAdapter as ForwardingShipmentRatingAdapter;
			Factory.Save();

			#region expected rate

			var expectedClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
				},
			};

			var expectedGroupClientRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 20m,
				},
			};

			var expectedGroupClientRate2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
				},
			};

			#endregion

			jobHeader.JH_ClientContractNumber = "AAA";
			var message = "Should filter rates by Job's Client Contract Number (AAA => Client Rate).";
			AutorateAndAssert(message, expectedClientRate, shipment, localClient, autorateCosts: false);

			jobHeader.JH_ClientContractNumber = "BBB";
			message = "Should filter rates by Job's Client Contract Number (BBB => Group Client Rate 1).";
			AutorateAndAssert(message, expectedGroupClientRate1, shipment, localClient, autorateCosts: false);

			jobHeader.JH_ClientContractNumber = "CCC";
			message = "Should filter rates by Job's Client Contract Number (CCC => Group Client Rate 2).";
			AutorateAndAssert(message, expectedGroupClientRate2, shipment, localClient, autorateCosts: false);
		}

		public void TestFilterAndPopulateShipmentContractNumber_MultiNumbersFromRates_SameChargeCode_ClientRateShouldNotOverrideOnCompanyTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GlobalRateLevel = 1;
			var entryBBB = companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";
			companyTariff.Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(consignee);
			var entryAAA = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, consignee.PK, "AUSYD", "USLAX", 100);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			jobHeader.JH_ClientContractNumber = "";

			var expectedCharges = new[] {
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200
				}
			};

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "AAA", "BBB" };
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("BBB"); // the contract number from company tariff
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			var message = "Should filter rates by selected contract number.";
			AutorateAndAssert(message, expectedCharges, shipment, consignee, autorateRevenue: true, autorateCosts: false, ratingContext: testRatingContext);

			message = "Multiple rates should have been found and a number should be selected";
			AssertEquals("BBB", jobHeader.JH_ClientContractNumber);
			mockedDialogService.Verify(x => x.SelectSingleClientContractNumber(expectedArgumentSetup), Times.Once);
		}

		public void TestFilterAndPopulateShipmentContractNumber_MultiNumbersFromRates_SameChargeCode_BetweenClientRateAndGroupClientRate()
		{
			var consignee = Helper.NewOrgHeader();
			var localClient = Helper.NewOrgHeader();
			var groupClient1 = Helper.NewOrgHeader();
			var groupClient2 = Helper.NewOrgHeader();
			Factory.Save();

			var defaultLevel = localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevel.P7_ApplyGroupRate = true;
			defaultLevel = groupClient1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			defaultLevel.P7_ApplyGroupRate = true;

			groupClient1.RelatedManagementSubsidiaryRelations.AddOrganisation(localClient);
			groupClient2.RelatedManagementSubsidiaryRelations.AddOrganisation(groupClient1);
			Factory.Save();

			AssertEquals("Precondition: groupClient1 should have 1 subsidiary company", 1, groupClient1.RelatedManagementSubsidiaryRelations.Organisations.Count());
			AssertEquals("Precondition: groupClient2 should have 1 subsidiary company", 1, groupClient2.RelatedManagementSubsidiaryRelations.Organisations.Count());

			var clientRate = Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 10m);
			var groupClientRate1 = Helper.NewClientRateWithSingleRateLine(groupClient1, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 20m);
			var groupClientRate2 = Helper.NewClientRateWithSingleRateLine(groupClient2, "AIR", "LSE", "AUBNE", "USLAX", "FRT", 30m);

			var clientRateEntry = clientRate.GetRateEntryCollectionForCategory("AIR")[0];
			clientRateEntry.TI_ContractNumber = "AAA";
			var groupRateEntry1 = groupClientRate1.GetRateEntryCollectionForCategory("AIR")[0];
			groupRateEntry1.TI_ContractNumber = "BBB";
			var groupRateEntry2 = groupClientRate2.GetRateEntryCollectionForCategory("AIR")[0];
			groupRateEntry2.TI_ContractNumber = "CCC";

			var shipment = CreateForwardingShipment(TransportModes.Air, localClient.PK, consignee.PK, "AUBNE", "USLAX", 1000);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			var ratingAdapter = shipment.RatingAdapter as ForwardingShipmentRatingAdapter;
			Factory.Save();

			#region expected rate

			var expectedClientRate = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 10m,
				},
			};

			var expectedGroupClientRate1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 20m,
				},
			};

			var expectedGroupClientRate2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
				},
			};

			#endregion

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "AAA", "BBB", "CCC" };
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("AAA"); // the contract number from client rate
			jobHeader.JH_ClientContractNumber = "";
			var message = "Should filter rates by Job's Client Contract Number (AAA => Client Rate).";
			AutorateAndAssert(message, expectedClientRate, shipment, localClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("AAA", jobHeader.JH_ClientContractNumber);

			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("BBB"); // the contract number from group client rate 1
			jobHeader.JH_ClientContractNumber = "";
			message = "Should filter rates by Job's Client Contract Number (BBB => Group Client Rate 1).";
			AutorateAndAssert(message, expectedGroupClientRate1, shipment, localClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("BBB", jobHeader.JH_ClientContractNumber);

			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("CCC"); // the contract number from group client rate 2
			jobHeader.JH_ClientContractNumber = "";
			message = "Should filter rates by Job's Client Contract Number (CCC => Group Client Rate 2).";
			AutorateAndAssert(message, expectedGroupClientRate2, shipment, localClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("CCC", jobHeader.JH_ClientContractNumber);
		}

		public void TestFilterAndPopulateShipmentContractNumber_MultipleChargeCodes_ReAutorate()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entryAAA = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			var entryBBB = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";

			var entryBlank = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 300);
			var bafRateLine = entryBlank.AddRateLine("BAF", FlatCalculator.Code);
			bafRateLine.GetCalculator<FlatCalculator>().BaseRate = 400;
			entryBlank.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 100);
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 400
				}
			};

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "", "AAA", "BBB" };
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("BBB");
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			var message = "Shipment CLC is not set. Should have non-duplicate charges from rates with and without contract number.";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("BBB", shipment.ShipmentJobHeader.JH_ClientContractNumber);

			var newFactory = Factory.CreateNewFactory();
			var bafRateLineInNewFactory = newFactory.Load<RateLine>(bafRateLine.PK);
			bafRateLineInNewFactory.GetCalculator<FlatCalculator>().BaseRate = 500;
			newFactory.Save();

			var expectedChargesAfterChangingBAFCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 500
				}
			};

			message = "Shipment CLC is set. Re-autorating without clearing charges should pickup same charge codes and update with new values.";
			AutorateAndAssert(message, expectedChargesAfterChangingBAFCharge, shipment, Consignee, autorateCosts: false, ratingContext: testRatingContext);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.Charges.RemoveAndDeleteAll();
			}

			message = "Shipment CLC is still set. Re-autorating when job charges is empty should recreate same charge codes";
			AutorateAndAssert(message, expectedChargesAfterChangingBAFCharge, shipment, Consignee, autorateCosts: false, ratingContext: testRatingContext);
		}

		public void TestFilterAndPopulateShipmentContractNumber_MultipleChargeCodes()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entryAAA = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 100);
			entryAAA.TI_ContractNumber = "AAA";

			var entryBBB = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 200);
			entryBBB.TI_ContractNumber = "BBB";
			var bafRateLine = entryBBB.AddRateLine("BAF", FlatCalculator.Code);
			bafRateLine.GetCalculator<FlatCalculator>().BaseRate = 400;

			var entryBlank = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "", "FRT", 300);
			entryBlank.TI_ContractNumber = "";
			var cafRateLine = entryBlank.AddRateLine("CAF", FlatCalculator.Code);
			cafRateLine.GetCalculator<FlatCalculator>().BaseRate = 500;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 100);
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 500
				},
			};

			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "", "AAA", "BBB" };
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("AAA");
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			const string message = @"Shipment CLC is not set.
Should have non-duplicate charges from rates with and without contract number.
Charges from rates unmatched contract number should not be created";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false, ratingContext: testRatingContext);

			AssertEquals("AAA", shipment.ShipmentJobHeader.JH_ClientContractNumber);
		}

		public void TestShipmentAdapterCarrierContractNumbers_DoesNotContainShipmentNumbers()
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 100);

			var conNumber1 = shipment.Numbers.AddNew();
			conNumber1.CE_EntryType = CustomsReferenceNumberCodes.CON;
			conNumber1.CE_EntryNum = "CON1";

			var conNumber2 = shipment.Numbers.AddNew();
			conNumber2.CE_EntryType = CustomsReferenceNumberCodes.CON;
			conNumber2.CE_EntryNum = "CON2";

			var shipmentRatingAdapter = shipment.RatingAdapter;
			AssertEquals("CONs on shipment are not used in autorating.", 0, shipmentRatingAdapter.CarrierContractNumbers.Count());
		}

		public void TestClientContractNumbers_ShouldAutorate()
		{
			// PART 1: Autorate shipment in Origin country (AU)

			var clientRateAtOrigin = Helper.NewClientRate(NewClient);
			var entry1 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1000, CurrencyCodes.Australia);
			entry1.TI_ContractNumber = "123";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUSYD", "NZAKL", 1000, 1);
			var auJob = new JobHeader.Loader(shipment).TryCreate();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 1000
				}
			};
			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false);
			AssertEquals("AU job should match the expected client contract number", "123", auJob.JH_ClientContractNumber);

			// PART 2: Autorate shipment in Destination country (NZ) with the same number entered manually

			var destinationCompany = Factory.NewWithValidTestData<GlbCompany>();
			destinationCompany.GC_Name = "NZ Company";
			destinationCompany.GC_RN_NKCountryCode = CountryCodes.NewZealand;
			var destinationBranch = destinationCompany.Branches.AddNew();
			destinationBranch.GB_Code = "AKL";
			destinationBranch.GB_BranchName = "NZAKL Branch";
			destinationBranch.GB_RL_NKHomePort = "NZAKL";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, destinationBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (var nzJob = new JobHeader.Loader(shipment).TryCreateWithMutex() as Job)
			{
				var chargeCodeDDOC = Helper.ChargeCodes["DDOC"];
				chargeCodeDDOC.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

				var clientRateAtDestination = Helper.NewClientRate(NewClient);
				var entry2 = clientRateAtDestination.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "NZAKL", "DDOC", 800, CurrencyCodes.NewZealand);
				entry2.TI_ContractNumber = "123";

				Factory.Save();

				nzJob.JH_ClientContractNumber = "123";

				expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalSellAmt = 800
					}
				};

				UnitTestUserNotification.Instance.ClearMessages();
				var reason = "Rate should be picked up because it matches job number";
				AutorateAndAssert(reason, expectedCharges, shipment, NewClient, job: nzJob, autorateCosts: false);

				reason = "The same numbers are differentiated by countries";
				const string expectedError = "Only one Client Contract Number can be used for Autorating Revenue.";
				AssertNotEquals(reason, expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPopulateShipmentContractNumber_OneNumber()
		{
			// Two rates - one with contract# 123, and one blank
			// Job contract# blank means rate with 123 applies, and rate with blank also applies
			// If there is only one non-blank number then it automatically is picked. User isn't given a choice.
			var clientRateAtOrigin = Helper.NewClientRate(NewClient);
			var entry1 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1100, CurrencyCodes.Australia);
			entry1.TI_ContractNumber = "123";
			var entry2 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1200, CurrencyCodes.Australia);
			entry2.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUSYD", "NZAKL", 1000, 1);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();

			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), null);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 1100
				}
			};
			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);

			AssertEquals("123", jobHeader.JH_ClientContractNumber);

			// Job contract# 123 means rate with 123 applies, rate with blank is skipped
			testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), null);
			jobHeader.JH_ClientContractNumber = "123";
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 1100
				}
			};
			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("123", jobHeader.JH_ClientContractNumber);
		}

		public void TestPopulateShipmentContractNumber_MultipleNumbers()
		{
			// Three rates - one with contract# 111, one with contract# 222, and one blank
			var clientRateAtOrigin = Helper.NewClientRate(NewClient);
			var entry1 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1100, CurrencyCodes.Australia);
			entry1.TI_ContractNumber = "AAA";
			var entry2 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1150, CurrencyCodes.Australia);
			entry2.TI_ContractNumber = "BBB";
			var entryBlank = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1200, CurrencyCodes.Australia);
			entryBlank.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUSYD", "NZAKL", 1000, 1);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();

			// Job contract# blank means all rates match.
			// User picks blank.
			var mockedDialogService = new Mock<IDialogService>();
			var expectedArgumentSetup = new[] { "", "AAA", "BBB" };
			mockedDialogService
				.SetupSequence(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("");
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 1200
				}
			};
			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("", jobHeader.JH_ClientContractNumber);

			var jobDataUpdater = (IJobDataUpdater)shipment.RatingAdapter;

			// User picks 222
			mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.SetupSequence(x => x.SelectSingleClientContractNumber(expectedArgumentSetup))
				.Returns("BBB");
			testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 1150
				}
			};

			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("BBB", jobHeader.JH_ClientContractNumber);
		}

		public void TestPopulateShipmentClientContractNumber_JobHasNumber_RateHasNoNumber()
		{
			var clientRateAtOrigin = Helper.NewClientRate(NewClient);
			var entry = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "NZAKL", "ODOC", 1000, CurrencyCodes.Australia);
			entry.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUSYD", "NZAKL", 1000, 1);
			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.JH_ClientContractNumber = "AAA";

				var expectedCharges = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalSellAmt = 1000
					}
				};
				AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false);
				AssertEquals("blank rate contract number does not update job", "AAA", job.JH_ClientContractNumber);
			}
		}

		#endregion

		public void TestQuotedBookingWithDifferentPackages()
		{
			var rate = Helper.NewClientRate(NewClient);

			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUBNE", "NZAKL");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "PLT");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;
			rateLine.TL_RX_NKCurrency = "AUD";

			var rateLine2 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "SHT");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 4;
			rateLine.TL_RX_NKCurrency = "AUD";

			Factory.Save();

			var oneOffQuote = CreateQuotedBooking(TransportModes.Air, "LSE", "CFR", NewClient, null, Consignee, null, "AUBNE", "NZAKL", 1m, 1m, QuotedBookingState.QuoteOnly);

			var p1 = oneOffQuote.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			p1.TPL_F3_NKPackType = "PLT";
			p1.TPL_PackLineCount = 79;
			var p2 = oneOffQuote.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			p2.TPL_F3_NKPackType = "SHT";
			p2.TPL_PackLineCount = 128;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 907m,
				}
			};

			AutorateAndAssert(expectedCharges, oneOffQuote, NewClient, autorateCosts: false);
			var oneOffQuoteCharges = (oneOffQuote.Job as Job).Charges.Cast<BaseCharge>().ToList();

			AssertEquals(1, oneOffQuoteCharges.Count);
			var paymentBasesForDisplay = ((IPaymentBasisViewCharge)oneOffQuoteCharges[0]).SellPaymentBasesView;

			AssertEquals("FRT|512|AUD|128|SHT|4|SHT|||UNT", paymentBasesForDisplay[0].ToString());
			AssertEquals("FRT|395|AUD|79|PLT|5|PLT|||UNT", paymentBasesForDisplay[1].ToString());
		}

		public void TestShipmentWithDifferentPackages()
		{
			var cnr = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(cnr);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "PLT");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;
			rateLine.TL_RX_NKCurrency = "AUD";

			var rateLine2 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "SHT");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 4;
			rateLine.TL_RX_NKCurrency = "AUD";

			var shipment = CreateForwardingShipment(TransportModes.Air, cnr.PK, ZGuid.Empty, "AUSYD", "USLAX", 10m);
			var p1 = shipment.OuterPackLines.AddNew();
			p1.JL_F3_NKPackType = "PLT";
			p1.JL_PackageCount = 79;
			var p2 = shipment.OuterPackLines.AddNew();
			p2.JL_F3_NKPackType = "SHT";
			p2.JL_PackageCount = 128;

			shipment.JS_INCO = "CIF";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 907m
				}
			};

			AutorateAndAssert(expected, shipment, cnr);
		}

		public void TestIncotermFilteringWorksForSupplementaryChargesWhenNoConsol()
		{
			var cne = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(cne);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "CN");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("DCART", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 554m;
			rateLine.TL_RX_NKCurrency = "AUD";

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, cne.PK, "AUSYD", "CNSHA", 10m);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 554m
				}
			};

			shipment.JS_INCO = "EXW";
			AutorateAndAssert(null, shipment, cne);

			shipment.JS_INCO = "DDP";
			AutorateAndAssert(expected, shipment, cne);
		}

		public void TestCommodityShouldBeObtainedFromContainerOnFCLShipment()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "BVG";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "CLK";

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUBNE", "GBSUN", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = "BVG";
			var rateLine1 = rateEntry1.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RateDesc = "BVG Commodity";
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 30m;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUBNE", "GBSUN", "", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = "CLK";
			var rateLine2 = rateEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine2.TL_RateDesc = "CLK Commodity";
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var rateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUBNE", "GBSUN", "", "20GP");
			var rateLine3 = rateEntry3.AddRateLine("OAQF", FlatCalculator.Code);
			rateLine3.TL_RateDesc = "Empty Commodity";
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			var shipment2 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			var shipment3 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment1);
			consol.Shipments.Add(shipment2);
			consol.Shipments.Add(shipment3);

			consol.JK_ConsolMode = ContainerModes.FCL;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment3.JS_PackingMode = ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_RC = Helper.Containers["20GP"].PK;
			container.JC_RH_NKContainerCommodityCode = "CLK";
			container.JC_ContainerMode = ContainerModes.FCL;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = "CLK";
			packLine1.JL_JC = container.PK;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = "BVG";
			packLine2.JL_JC = container.PK;

			var packLine3 = shipment3.OuterPackLines.AddNew();
			packLine3.JL_RH_NKCommodityCode = "";
			packLine3.JL_JC = container.PK;

			Factory.Save();

			var bvgCommodityCharge = new AssertionCharge
			{
				ChargeCode = "OCART",
				JR_Desc = "BVG Commodity",
				JR_LocalSellAmt = 30m
			};
			var cklCommodityCharge = new AssertionCharge
			{
				ChargeCode = "ODOC",
				JR_Desc = "CLK Commodity",
				JR_LocalSellAmt = 20m
			};

			var message = "PackLine and Container both use the CLK Commodity Code.";
			AutorateAndAssert(message, new[] { cklCommodityCharge }, shipment1, NewClient);

			message = "Has both commodity codes so we expect both rate lines to be matched";
			AutorateAndAssert(message, new[] { bvgCommodityCharge, cklCommodityCharge }, shipment2, NewClient);

			message = "If there's no commodity on the packLine, we should still use the container's commodity, not rateLine3.";
			AutorateAndAssert(message, new[] { cklCommodityCharge }, shipment3, NewClient);
		}

		public void TestCommodityShouldBeObtainedFromTheCorrectPackLine()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "BVG";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "CLK";

			var rate = Helper.NewClientRate(NewClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUBNE", "GBSUN", "", "");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine1 = rateEntry1.AddRateLine("OAQF", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 30m;

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUBNE", "GBSUN", "", "");
			rateEntry2.TI_RH_NKCommodityCode = "CLK";
			var rateLine2 = rateEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var shipment1 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			var shipment2 = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment1);
			consol.JK_ConsolMode = ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_RC = Helper.Containers["20GP"].PK;
			container.JC_RH_NKContainerCommodityCode = "CLK";

			var beverages = shipment1.OuterPackLines.AddNew();
			beverages.JL_PackageCount = 10;
			beverages.JL_ActualWeight = 20;
			beverages.JL_RH_NKCommodityCode = "BVG";
			beverages.JL_JC = container.PK;

			var clocks = shipment2.OuterPackLines.AddNew();
			clocks.JL_PackageCount = 5;
			clocks.JL_ActualWeight = 10;
			clocks.JL_JC = container.PK;
			clocks.JL_RH_NKCommodityCode = "CLK";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OAQF",
					JR_OSSellAmt = 30m
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 10m
				},
			};

			AutorateAndAssert(expected, shipment1, NewClient, autorateCosts: false);
			AutorateAndAssert(expected, shipment2, NewClient, autorateCosts: false);
		}

		public void TestShipmentAgainstGroupClientRate()
		{
			//=======
			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "CONSIGNOR";
			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "CLIENT";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "CONSIGNEE";
			var relatedParty = Helper.NewOrgHeader();
			relatedParty.OH_Code = "PARTY";

			//=======
			var newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ARNettingGroup;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "COM";

			newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "COM";

			newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			//=======
			newParty = consignee.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			//=======
			var rateTariffLevelConsignor = consignor.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			var rateTariffLevelLocalClient = localClient.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			var rateTariffLevelConsignee = consignee.CompanyData.RateTariffLevels.SetLevel("DEF", 1);

			//=======
			rateTariffLevelConsignor.P7_ApplyGroupRate = false;
			rateTariffLevelLocalClient.P7_ApplyGroupRate = false;
			rateTariffLevelConsignee.P7_ApplyGroupRate = false;

			//=======
			Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 40m);
			Helper.NewClientRateWithSingleRateLine(consignee, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 50m);
			Helper.NewClientRateWithSingleRateLine(relatedParty, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 60m);

			Factory.Save();

			//=======
			var shipment1 = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 100);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m
				}
			};
			AutorateAndAssert("No group rate applied. Rates are from local client and consignee", expectedCharges, shipment1, localClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment1, "Filter message", "Information: RateLine Filtered FRT-FLT-Client Rate PARTY	reason:	organization PARTY has been marked to not use group client rates");

			//=======
			rateTariffLevelConsignor.P7_ApplyGroupRate = true;
			Factory.Save();

			var shipment2 = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 100);
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m
				}
			};
			AutorateAndAssert("Group rate applied for Consignor so rates from Related Party come through", expectedCharges, shipment2, localClient, autorateCosts: false);

			//=======
			rateTariffLevelConsignee.P7_ApplyGroupRate = true;
			Factory.Save();

			var shipment3 = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 100);
			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 50m
				}
			};
			AutorateAndAssert("Group rate also applied for Consignee. Rates from Related Party come through but they are overridden by rates from Consignee.", expectedCharges, shipment3, localClient, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(shipment3, "Overridden message", "Information: RateLine Filtered FRT-FLT-Client Rate PARTY	reason:	overridden by FRT-FLT-Client Rate CONSIGNEE by Rate Type comparer");
		}

		public void TestShipmentControllingCustomerAsRelatedParty()
		{
			//=======
			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "CONSIGNOR";
			var localClient = Helper.NewOrgHeader();
			localClient.OH_Code = "CLIENT";
			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "CONSIGNEE";
			var relatedParty = Helper.NewOrgHeader();
			relatedParty.OH_Code = "CCUS";
			relatedParty.OH_IsControllingCustomer = true;

			//=======
			var newParty = consignor.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			newParty = consignee.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			newParty.PR_OH_RelatedParty = relatedParty.PK;
			newParty.CompanyLevel = "ENT";

			//=======
			var rateTariffLevelConsignor = consignor.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			var rateTariffLevelLocalClient = localClient.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			var rateTariffLevelConsignee = consignee.CompanyData.RateTariffLevels.SetLevel("DEF", 1);

			//=======
			rateTariffLevelConsignor.P7_ApplyGroupRate = false;
			rateTariffLevelLocalClient.P7_ApplyGroupRate = false;
			rateTariffLevelConsignee.P7_ApplyGroupRate = false;

			//=======
			Helper.NewClientRateWithSingleRateLine(localClient, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 40m);
			Helper.NewClientRateWithSingleRateLine(relatedParty, "AIR", "LSE", "USLAX", "AUBNE", "FRT", 60m);

			Factory.Save();

			//=======
			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "USLAX", "AUBNE", 100);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
			};
			AutorateAndAssert("No group rate, no controlling customer", expectedCharges, shipment, localClient, autorateCosts: false);

			//=======
			rateTariffLevelConsignor.P7_ApplyGroupRate = true;
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					RevenueCalculationDescription = @"FRT: Base Rate AUD 60.00

International Freight

Charge located in CCUS group client rate (Linked to client rate: CONSIGNOR) with the following details:"
				}
			};
			AutorateAndAssert("New rate from group client rate", expectedCharges, shipment, localClient, autorateCosts: false);

			//=======
			shipment.ControllingCustomerNameOrPK = relatedParty.PK.ToString();
			rateTariffLevelConsignor.P7_ApplyGroupRate = false;
			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
					RevenueCalculationDescription = @"FRT: Base Rate AUD 60.00

International Freight

Charge located in CCUS client rate with the following details:"
				}
			};
			AutorateAndAssert("New rate comes from controlling customer instead", expectedCharges, shipment, localClient, autorateCosts: false);
		}

		public void TestAutorateConsol_ClientContractNumber()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "FRT", 100);
			rateEntry.TI_ContractNumber = "CLC1";
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-1);
			rateEntry.TI_RateEndDate = ZDate.Today.AddDays(10);

			Factory.Save();

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX", PaymentType.Prepaid);
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			consol.Numbers.RemoveAndDeleteAll();
			var clcNumber = consol.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC1";

			var consolRatingAdapter = consol.RatingAdapter;
			AssertEquals("CLC is not used in consol ClientContractNumber", 0, consolRatingAdapter.ClientContractNumbers.Count());

			CreateJob(consol, "CONSOLJOB1");

			AutorateAndAssert("CLC should not be used in autorating consol", Array.Empty<AssertionCharge>(), consol, Consignee, autorateCosts: false);
		}

		public void TestFreightInclusiveChargeCode()
		{
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];
			var chargeCodeCAF = Helper.ChargeCodes["CAF"];

			var localClient = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(localClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			// main freight
			var rateLineFRT = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLineFRT.GetCalculator<FlatCalculator>().BaseRate = 200m;

			// pre-carriage / on-carriage
			var rateLineBAF = rateEntry.AddRateLine(chargeCodeBAF, FlatCalculator.Code);
			rateLineBAF.GetCalculator<FlatCalculator>().BaseRate = 500m;

			// inclusive freight
			var rateLineCAF = rateEntry.AddRateLine(chargeCodeCAF, FreightInclusiveCalculator.Code);
			var calculatorFRT = rateLineCAF.GetCalculator<FreightInclusiveCalculator>();
			calculatorFRT.FreightCalcType = FreightInclusiveCalculator.FreightCalcTypes.Included;
			calculatorFRT.ChargeCode = chargeCodeBAF.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = chargeCodeBAF.AC_Code,
					JR_OSSellAmt = 500m,
					RevenueCalculationDescription = $@"Inclusive charges:

{chargeCodeCAF.AC_Code} - {chargeCodeCAF.AC_DescMultilingual} (Included)"
				}
			};

			AutorateAndAssert("Charges created for main freight and inclusive freight", expectedCharges, shipment, localClient, autorateCosts: false);
		}

		public void TestAutorateConsol_SingleRoute_AWBShouldHaveRateFromStandardCostingRatherCarrierCosting()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var costingCarrier = Helper.NewCosting(carrier);
			costingCarrier.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "FRT", 100m);
			var standardCosting = Helper.NewCosting(null);
			standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "FRT", 200m);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.SetDefaultShippingLineAddress(carrier);

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert
				(
					"Billing tab should have rate from carrier specific costing",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
						new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m },
					},
					costsSupporter: consol,
					autorateRevenue: false
				);

				consol.PopulateAWB();
				var awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines
					.Cast<ExportAWBRateLine>()
					.Single(x => x.ER_RateChargeOrDiscount != 0);
				AssertEquals
				(
					"The AWB rate line should have rate from standard costing",
					200m,
					awbRateLine.ER_RateChargeOrDiscount
				);
			}
		}

		public void TestAutorateConsol_SingleRoute_StandardCostingChargeableWeightShouldbeBasedOnCarrierCalculatedChargeableWeight()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var costingCarrier = Helper.NewCosting(carrier);

			var entry =
				costingCarrier
				.AddRateEntry(
					RatingConstants.RateCategory.AIR,
					RateMode.LSE,
					origin: "AUSYD",
					destination: "CNSHA");
			entry.RateLines.RemoveAndDeleteAll();

			var freightCosting = entry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			var cmbCalculator = freightCosting.GetCalculator<CombinedCalculator>();
			cmbCalculator["-45"] = (ZDecimal)0.65;
			cmbCalculator["+45"] = (ZDecimal)0.55;
			cmbCalculator["+280"] = (ZDecimal)4.55;
			cmbCalculator["+350"] = (ZDecimal)0.90;
			cmbCalculator.UseHigherChargeableLowerRateRule = true;

			var standardCosting = Helper.NewCosting(null);
			var standardCostEntry =
				standardCosting
				.AddRateEntry(
					RatingConstants.RateCategory.AIR,
					RateMode.LSE,
					origin: "AUSYD",
					destination: "CNSHA");

			standardCostEntry.RateLines.RemoveAndDeleteAll();
			var standardCostLine = standardCostEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			standardCostLine.GetCalculator<CombinedCalculator>().PerUnit = 2M;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 310, 1m));

			Factory.Save();

			AutoCostAndAssert
			(
				"Costings should be calculated based on carrier's costing using higher break lower rate",
				expectedInvoicingCharges: null,
				expectedCosts: new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 315.00m,
						CostCalculationDescription = "FRT: 350 Kilogram(s) (HBLR is applied) @ AUD 0.90/KG",
					}
				},
				costsSupporter: consol,
				autorateRevenue: false
			);

			consol.PopulateAWB();
			var awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines[0];

			AssertEquals
			(
				"The AWB Chargeable Weight should be carrier's costing calculated chargeable weight",
				350m,
				awbRateLine.ER_ChargeableWeight
			);

			AssertEquals("Gross Weight is unmodified weight", 310m, awbRateLine.ER_GrossWeight);
		}

		public void TestAutorateConsol_SingleRoute_StandardAndCarrierCostingsWithNoUnitsUsesDefaultUnits()
		{
			var carrier = NewCarrierCreditor("CARRIER");
			var costingCarrier = Helper.NewCosting(carrier);

			var entry =
				costingCarrier
				.AddRateEntry(
					RatingConstants.RateCategory.AIR,
					RateMode.LSE,
					origin: "AUSYD",
					destination: "CNSHA");
			entry.RateLines.RemoveAndDeleteAll();

			var freightCostingMinimum = entry.AddRateLine("FRT", MinimumCalculator.Code);
			freightCostingMinimum.GetCalculator<MinimumCalculator>().MinimumValue = 5M;

			var standardCosting = Helper.NewCosting(null);
			var standardCostEntry =
				standardCosting
				.AddRateEntry(
					RatingConstants.RateCategory.AIR,
					RateMode.LSE,
					origin: "AUSYD",
					destination: "CNSHA");
			standardCostEntry.RateLines.RemoveAndDeleteAll();

			var standardCostLine = standardCostEntry.AddRateLine("FRT", MinimumCalculator.Code);
			standardCostLine.GetCalculator<MinimumCalculator>().MinimumValue = 8m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var consignor = Helper.NewOrgHeader(1);
			consol.Shipments.Add(CreateForwardingShipment(TransportModes.Air, consignor.PK, ZGuid.Empty, "AUSYD", "CNSHA", 310, 1m));

			Factory.Save();

			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.AIR;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				AutoCostAndAssert
				(
					"Should use costing with lower rate",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
									new AssertionCost
									{
										ChargeCode = "FRT",
										E6_OSCostAmount = 5M,
									}
					},
					costsSupporter: consol,
					autorateRevenue: false
				);
			}
		}

		public void TestAutorateConsol_SingleRoute_AWBShouldContainExcludedFromAutocostingLocalCostings()
		{
			var standardCosting = Helper.NewCosting(null);
			var costEntry = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert
				(
					"Billing tab should have rate from local standard costing",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
						new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m },
					},
					costsSupporter: consol,
					autorateRevenue: false
				);

				consol.PopulateAWB();
				var awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines
					.Cast<ExportAWBRateLine>()
					.Single(x => x.ER_RateChargeOrDiscount != 0);
				AssertEquals
				(
					"The AWB rate line should have rate from local standard costing",
					100m,
					awbRateLine.ER_RateChargeOrDiscount
				);

				costEntry.TI_IsExcludedFromAutoRating = true;
				Factory.Save();

				AutoCostAndAssert
				(
					"Billing tab should not have rate from local standard costing",
					expectedInvoicingCharges: null,
					expectedCosts: null,
					costsSupporter: consol,
					autorateRevenue: false
				);

				consol.PopulateAWB();
				awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines
					.Cast<ExportAWBRateLine>()
					.Single(x => x.ER_RateChargeOrDiscount != 0);
				AssertEquals
				(
					"The AWB rate line should have rate from local standard costing when excluded from autocosting",
					100m,
					awbRateLine.ER_RateChargeOrDiscount
				);
			}
		}

		public void TestAutorateConsol_SingleRoute_AWBShouldContainExcludedFromAutocostingGlobalCostings()
		{
			var globalCosting = Helper.NewGlobalCosting(null);
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var costEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine(globalChargeCode).GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert
				(
					"Billing tab should have rate from global costing",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
						new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m },
					},
					costsSupporter: consol,
					autorateRevenue: false
				);

				consol.PopulateAWB();
				var awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines
					.Cast<ExportAWBRateLine>()
					.Single(x => x.ER_RateChargeOrDiscount != 0);
				AssertEquals
				(
					"The AWB rate line should have rate from global standard costing",
					200m,
					awbRateLine.ER_RateChargeOrDiscount
				);

				costEntry.TI_IsExcludedFromAutoRating = true;
				Factory.Save();

				AutoCostAndAssert
				(
					"Billing tab should not have rate from global standard costing",
					expectedInvoicingCharges: null,
					expectedCosts: null,
					costsSupporter: consol,
					autorateRevenue: false
				);

				consol.PopulateAWB();
				awbRateLine = consol.AWBHeaderManager.AWBHeader.AWBRateLines
					.Cast<ExportAWBRateLine>()
					.Single(x => x.ER_RateChargeOrDiscount != 0);
				AssertEquals
				(
					"The AWB rate line should have rate from global standard costing when excluded from autocosting",
					200m,
					awbRateLine.ER_RateChargeOrDiscount
				);
			}
		}

		#region Aircraft Type

		#region Aircraft Type - Cost

		[TestDate(2020, 06, 01)]
		public void TestAutorateConsolCost_AircraftTypeFiltering_WhenConsolIsCargoOnly()
		{
			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 150).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 160).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 170).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 200).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 210).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 220).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "CNSHA", "USLAX", "WAR", 270).TI_AircraftType = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.CreditorPK = TransportProvider1.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_IsCargoOnly = true;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_IsCargoOnly = true;
			transport2.JW_TransportMode = TransportModes.Air;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);
			transport2.JW_CarrierBookingReference = "B";
			transport2.CarrierPK = TransportProvider1.PK;

			var transport3 = consol.Transports.AddNew("CNSHA", "USLAX");
			transport3.JW_IsCargoOnly = true;
			transport3.JW_TransportMode = TransportModes.Air;
			transport3.JW_ETA = new ZDateTime(2020, 06, 15);
			transport3.JW_ETD = new ZDateTime(2020, 06, 17);
			transport3.JW_CarrierBookingReference = "C";
			transport3.CarrierPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Consol's overall status", true, consol.JK_Calc_IsCargoOnly);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 150m,
					},
					new AssertionCost
					{
						ChargeCode = "CAF",
						E6_OSCostAmount = 200m,
					},
					new AssertionCost
					{
						ChargeCode = "WAR",
						E6_OSCostAmount = 270m,
					}
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("Should rate by routes", null, expectedCosts, consol, autorateRevenue: false);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 100m,
					}
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("Should rate by 1st Load and Last Discharge", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		[TestDate(2020, 06, 01)]
		public void TestAutorateConsolCost_AircraftTypeFiltering_WhenConsolIsNotCargoOnly()
		{
			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 150).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 160).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 170).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 200).TI_AircraftType = "CAO";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 210).TI_AircraftType = "PAX";
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "CNSHA", "CAF", 220).TI_AircraftType = "";

			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "CNSHA", "USLAX", "WAR", 270).TI_AircraftType = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.CreditorPK = TransportProvider1.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_IsCargoOnly = true;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_IsCargoOnly = false;
			transport2.JW_TransportMode = TransportModes.Air;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);
			transport2.JW_CarrierBookingReference = "B";
			transport2.CarrierPK = TransportProvider1.PK;

			var transport3 = consol.Transports.AddNew("CNSHA", "USLAX");
			transport3.JW_IsCargoOnly = true;
			transport3.JW_TransportMode = TransportModes.Air;
			transport3.JW_ETA = new ZDateTime(2020, 06, 15);
			transport3.JW_ETD = new ZDateTime(2020, 06, 17);
			transport3.JW_CarrierBookingReference = "C";
			transport3.CarrierPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Consol's overall status", false, consol.JK_Calc_IsCargoOnly);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 150m,
					},
					new AssertionCost
					{
						ChargeCode = "CAF",
						E6_OSCostAmount = 210m,
					},
					new AssertionCost
					{
						ChargeCode = "WAR",
						E6_OSCostAmount = 270m,
					}
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("Should rate by routes", null, expectedCosts, consol, autorateRevenue: false);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 110m,
					}
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutoCostAndAssert("Should rate by 1st Load and Last Discharge", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		public void TestAutorateConsolCost_AircraftTypeFiltering_OriginCharges_IsCargoOnly()
		{
			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "CLORG", 200).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "CLORG", 210).TI_AircraftType = AircraftType.PAX;

			Factory.Save();

			var (_, consol) = CreateShipmentAndConsol(TransportModes.Air, "AUSYD", "USLAX", isCargoOnly: true);
			AutoCostAndAssert
			(
				message: "IsCargoOnly consol should only match CAO - Cargo Aircraft Only",
				null,
				expectedCosts: new[]
				{
					new AssertionCost { ChargeCode = "BAF", E6_OSCostAmount = 100m, },
					new AssertionCost { ChargeCode = "CLORG", E6_OSCostAmount = 200m, },
				},
				consol,
				autorateRevenue: false
			);
		}

		public void TestAutorateConsolCost_AircraftTypeFiltering_OriginCharges_IsNotCargoOnly()
		{
			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "CLORG", 200).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "CLORG", 210).TI_AircraftType = AircraftType.PAX;

			Factory.Save();

			var (_, consol) = CreateShipmentAndConsol(TransportModes.Air, "AUSYD", "USLAX", isCargoOnly: false);
			AutoCostAndAssert
			(
				message: "IsCargoOnly consol should only match PAX - Passenger and Cargo",
				null,
				expectedCosts: new[]
				{
					new AssertionCost { ChargeCode = "BAF", E6_OSCostAmount = 110m, },
					new AssertionCost { ChargeCode = "CLORG", E6_OSCostAmount = 210m, },
				},
				consol,
				autorateRevenue: false
			);
		}

		public void TestAutorateConsolCost_AircraftTypeFiltering_DestinationCharges_IsCargoOnly()
		{
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD", "CLDST", 200).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD", "CLDST", 210).TI_AircraftType = AircraftType.PAX;
			Factory.Save();

			var (_, consol) = CreateShipmentAndConsol(TransportModes.Air, "USLAX", "AUSYD", isCargoOnly: true);
			AutoCostAndAssert
			(
				message: "IsCargoOnly consol should only match CAO - Cargo Aircraft Only",
				null,
				expectedCosts: new[]
				{
					new AssertionCost { ChargeCode = "BAF", E6_OSCostAmount = 100m, },
					new AssertionCost { ChargeCode = "CLDST", E6_OSCostAmount = 200m, }
				},
				consol,
				autorateRevenue: false
			);
		}

		public void TestAutorateConsolCost_AircraftTypeFiltering_DestinationCharges_IsNotCargoOnly()
		{
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD", "CLDST", 200).TI_AircraftType = AircraftType.CAO;
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "AUSYD", "CLDST", 210).TI_AircraftType = AircraftType.PAX;
			Factory.Save();

			var (_, consol) = CreateShipmentAndConsol(TransportModes.Air, "USLAX", "AUSYD", isCargoOnly: false);
			AutoCostAndAssert
			(
				message: "IsCargoOnly consol should only match PAX - Passenger and Cargo",
				null,
				expectedCosts: new[]
				{
					new AssertionCost { ChargeCode = "BAF", E6_OSCostAmount = 110m, },
					new AssertionCost { ChargeCode = "CLDST", E6_OSCostAmount = 210m, }
				},
				consol,
				autorateRevenue: false
			);
		}

		#endregion

		#region Aircraft Type - Revenue

		[TestDate(2020, 06, 01)]
		public void TestAutorateShipmentRevenue_AircraftTypeFiltering_WhenNoAirRoutingLegs()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100).TI_AircraftType = "CAO";
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110).TI_AircraftType = "PAX";
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120).TI_AircraftType = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.JS_INCO = "";
			var shipmentLeg1 = shipment.Transports.AddNew("AUSYD", "AUPER");
			shipmentLeg1.JW_TransportMode = TransportModes.Sea;
			shipmentLeg1.JW_ETA = new ZDateTime(2020, 06, 02);
			shipmentLeg1.JW_ETD = new ZDateTime(2020, 06, 03);

			var shipmentLeg2 = shipment.Transports.AddNew("AUPER", "USLAX");
			shipmentLeg2.JW_TransportMode = TransportModes.Sea;
			shipmentLeg2.JW_ETA = new ZDateTime(2020, 06, 03);
			shipmentLeg2.JW_ETD = new ZDateTime(2020, 06, 04);

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);

			var transport3 = consol.Transports.AddNew("CNSHA", "USLAX");
			transport3.JW_TransportMode = TransportModes.Sea;
			transport3.JW_ETA = new ZDateTime(2020, 06, 15);
			transport3.JW_ETD = new ZDateTime(2020, 06, 17);

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 120m,
				}
			};

			var message = "Shipment has no air routing legs so fall back to empty";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
		}

		[TestDate(2020, 06, 01)]
		public void TestAutorateShipmentRevenue_AircraftTypeFiltering_WhenShipmentIsCargoOnly()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120).TI_AircraftType = "";
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.JS_INCO = "";
			var shipmentLeg1 = shipment.Transports.AddNew("AUSYD", "AUPER");
			shipmentLeg1.JW_TransportMode = TransportModes.Air;
			shipmentLeg1.JW_IsCargoOnly = true;
			shipmentLeg1.JW_ETA = new ZDateTime(2020, 06, 02);
			shipmentLeg1.JW_ETD = new ZDateTime(2020, 06, 03);

			var shipmentLeg2 = shipment.Transports.AddNew("AUPER", "USLAX");
			shipmentLeg2.JW_TransportMode = TransportModes.Air;
			shipmentLeg2.JW_IsCargoOnly = true;
			shipmentLeg2.JW_ETA = new ZDateTime(2020, 06, 03);
			shipmentLeg2.JW_ETD = new ZDateTime(2020, 06, 04);

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_IsCargoOnly = true;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_IsCargoOnly = true;
			transport2.JW_TransportMode = TransportModes.Air;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);

			var transport3 = consol.Transports.AddNew("CNSHA", "USLAX");
			transport3.JW_IsCargoOnly = true;
			transport3.JW_TransportMode = TransportModes.Air;
			transport3.JW_ETA = new ZDateTime(2020, 06, 15);
			transport3.JW_ETD = new ZDateTime(2020, 06, 17);

			Factory.Save();

			AssertEquals("Shipment's route", 5, shipment.TransportsIncludingRelated.Count);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 120m,
				}
			};

			var message = "Shipment overall IsCargoOnly status is CAO but there is no entry with CAO aircraft type so fall back to empty";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100).TI_AircraftType = "CAO";
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110).TI_AircraftType = "PAX";

			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				}
			};

			message = "Shipment overall IsCargoOnly status is CAO and there is a matching rate";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
		}

		[TestDate(2020, 06, 01)]
		public void TestAutorateShipmentRevenue_AircraftTypeFiltering_WhenShipmentIsNotCargoOnly()
		{
			var clientRate = Helper.NewClientRate(NewClient);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 120).TI_AircraftType = "";
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.JS_INCO = "";
			var shipmentLeg1 = shipment.Transports.AddNew("AUSYD", "AUPER");
			shipmentLeg1.JW_TransportMode = TransportModes.Air;
			shipmentLeg1.JW_IsCargoOnly = true;
			shipmentLeg1.JW_ETA = new ZDateTime(2020, 06, 02);
			shipmentLeg1.JW_ETD = new ZDateTime(2020, 06, 03);

			var shipmentLeg2 = shipment.Transports.AddNew("AUPER", "USLAX");
			shipmentLeg2.JW_TransportMode = TransportModes.Air;
			shipmentLeg2.JW_IsCargoOnly = false;
			shipmentLeg2.JW_ETA = new ZDateTime(2020, 06, 03);
			shipmentLeg2.JW_ETD = new ZDateTime(2020, 06, 04);

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_VoyageFlight = "QF111";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.JW_IsCargoOnly = true;
			transport1.JW_ETA = new ZDateTime(2020, 06, 02);
			transport1.JW_ETD = new ZDateTime(2020, 06, 03);
			transport1.JW_IsLinked = true;

			var transport2 = consol.Transports.AddNew("AUMEL", "CNSHA");
			transport2.JW_IsCargoOnly = false;
			transport2.JW_TransportMode = TransportModes.Air;
			transport2.JW_ETA = new ZDateTime(2020, 06, 03);
			transport2.JW_ETD = new ZDateTime(2020, 06, 04);

			var transport3 = consol.Transports.AddNew("CNSHA", "USLAX");
			transport3.JW_IsCargoOnly = true;
			transport3.JW_TransportMode = TransportModes.Air;
			transport3.JW_ETA = new ZDateTime(2020, 06, 15);
			transport3.JW_ETD = new ZDateTime(2020, 06, 17);

			Factory.Save();

			AssertEquals("Shipment's route", 5, shipment.TransportsIncludingRelated.Count);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 120m,
				}
			};

			var message = "Shipment overall IsCargoOnly status is PAX but there is no entry with PAX aircraft type so fall back to empty";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100).TI_AircraftType = "CAO";
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 110).TI_AircraftType = "PAX";

			Factory.Save();

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 110m,
				}
			};

			message = "Shipment overall IsCargoOnly status is PAX and there is a matching rate";
			AutorateAndAssert(message, expectedCharges, shipment, NewClient, autorateCosts: false);
		}

		public void TestAutorateShipmentRevenue_AircraftTypeFiltering_OriginDestinationCharges_IsCargoOnly()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 200).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 210).TI_AircraftType = AircraftType.PAX;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "USLAX", "DDOC", 300).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "USLAX", "DDOC", 310).TI_AircraftType = AircraftType.PAX;
			Factory.Save();

			var (shipment, _) = CreateShipmentAndConsol(TransportModes.Air, "AUSYD", "USLAX", isCargoOnly: true);
			AutorateAndAssert
			(
				message: "GIVEN shipment with IsCargoOnly WHEN autorate THEN should match rate with CAO AircraftType - Cargo Aircraft Only",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSSellAmt = 100m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 300m, },
				},
				shipment,
				NewClient,
				autorateCosts: false
			);
		}

		public void TestAutorateShipmentRevenue_AircraftTypeFiltering_OriginDestinationCharges_IsNotCargoOnly()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 100).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 110).TI_AircraftType = AircraftType.PAX;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 200).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "", "ODOC", 210).TI_AircraftType = AircraftType.PAX;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "USLAX", "DDOC", 300).TI_AircraftType = AircraftType.CAO;
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", "USLAX", "DDOC", 310).TI_AircraftType = AircraftType.PAX;
			Factory.Save();

			var (shipment, _) = CreateShipmentAndConsol(TransportModes.Air, "AUSYD", "USLAX", isCargoOnly: false);
			AutorateAndAssert
			(
				message: "GIVEN shipment with Non IsCargoOnly WHEN autorate THEN should match rate with PAX AircraftType - Passenger and Cargo",
				expected: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSSellAmt = 110m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 210m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 310m, },
				},
				shipment,
				NewClient,
				autorateCosts: false
			);
		}

		(ForwardingShipment, ForwardingConsol) CreateShipmentAndConsol(string transportMode, string origin, string destination, bool isCargoOnly)
		{
			var shipment = CreateForwardingShipment(transportMode, Consignor.PK, Consignee.PK, origin, destination, 1000);
			var shipmentTransport = shipment.Transports.AddNew(origin, destination);
			shipmentTransport.JW_IsCargoOnly = isCargoOnly;

			var consol = CreateForwardingConsol(transportMode, origin, destination, TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.Transports[0].JW_IsCargoOnly = isCargoOnly;

			Factory.Save();

			AssertEquals("Consol IsCargoOnly", isCargoOnly, consol.JK_Calc_IsCargoOnly);

			return (shipment, consol);
		}

		#endregion

		#endregion

		#region Standalone Shipment Autorate Costs

		public void TestAutorateStandaloneShipment_MultirouteAutoCostingRegistry()
		{
			Helper.ChargeCodes["FRT"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["BAF"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["CAF"].AC_DepartmentFilterList = "ALL";

			var standardCosting = Helper.NewCosting(null);
			standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUPER", "FRT", 100m);
			standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 200m);
			standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUMEL", "AUPER", "CAF", 300m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUPER", 1000m);
			shipment.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.Transports.AddNew("AUSYD", "AUMEL"); // The most interesting
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_TransportMode = TransportModes.Air;

			var transport2 = shipment.Transports.AddNew("AUMEL", "AUPER");
			transport1.JW_CarrierBookingReference = "B";
			transport2.JW_TransportMode = TransportModes.Air;

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCostShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_LocalCostAmt = 200m,
					},
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_LocalCostAmt = 300m,
					},
				};

				AutorateAndAssert("When Multi-Route Auto-Costing is enabled we expect to autorate all route sets",
					expected, shipment, Consignee, autorateCosts: true, autorateRevenue: false, standaloneShipmentOnly: true);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCostShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Pre-Condition", transport1, shipment.MostInterestingTransport);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_LocalCostAmt = 200m,
					},
				};

				AutorateAndAssert("When Multi-Route Auto-Costing is NOT enabled we expect to autorate the most interesting route set",
					expected, shipment, Consignee, autorateCosts: true, autorateRevenue: false, standaloneShipmentOnly: true);
			}
		}

		public void TestAutorateStandaloneShipment_RouteCarrierCreditor()
		{
			Helper.ChargeCodes["FRT"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["BAF"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["CAF"].AC_DepartmentFilterList = "ALL";

			var routeCarrier = Helper.NewOrgHeader("RCRR");
			var routeCreditor = Helper.NewOrgHeader("RCRD");

			Helper.NewCosting(null) // Standard Rate
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 100m);
			Helper.NewCosting(routeCarrier) // Service Provider = Route Carrier
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 200m);
			Helper.NewCosting(routeCreditor) // Service Provider = Route Creditor
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "CAF", 300m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUPER", 1000m);
			shipment.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.Transports.AddNew("AUSYD", "AUMEL");
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.CarrierPK = routeCarrier.PK;
			transport1.CreditorPK = routeCreditor.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 100m,
					CostAccountCode = ZString.Empty, // Standard Rate
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_LocalCostAmt = 200m,
					CostAccountCode = routeCarrier.OH_Code, // Service Provider = Route Carrier
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_LocalCostAmt = 300m,
					CostAccountCode = routeCreditor.OH_Code, // Service Provider = Route Creditor
				},
			};

			AutorateAndAssert("Autorate Standalone Shipment should load costs based on route's Carrier/Creditors",
				expected, shipment, Consignee, autorateCosts: true, autorateRevenue: false, standaloneShipmentOnly: true);
		}

		public void TestAutorateStandaloneShipment_SameChargeCode_CreditorTakesPriorityOverCarrier()
		{
			Helper.ChargeCodes["FRT"].AC_DepartmentFilterList = "ALL";

			var routeCarrier = Helper.NewOrgHeader("RCRR");
			var routeCreditor = Helper.NewOrgHeader("RCRD");

			Helper.NewCosting(routeCarrier) // Service Provider = Route Carrier
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 100m);
			Helper.NewCosting(routeCreditor) // Service Provider = Route Creditor
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 200m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUPER", 1000m);
			shipment.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.Transports.AddNew("AUSYD", "AUMEL");
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.CarrierPK = routeCarrier.PK;
			transport1.CreditorPK = routeCreditor.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 200m,
					CostAccountCode = routeCreditor.OH_Code, // Service Provider = Route Creditor
				},
			};

			AutorateAndAssert("Autorate Standalone Shipment should load costs based on route's Carrier/Creditors",
				expected, shipment, Consignee, autorateCosts: true, autorateRevenue: false, standaloneShipmentOnly: true);
		}

		public void TestAutorateStandaloneShipment_RouteCarrierAgainstRateCarrier()
		{
			Helper.ChargeCodes["FRT"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["BAF"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["CAF"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes["WAR"].AC_DepartmentFilterList = "ALL";

			var routeCarrier = Helper.NewOrgHeader("RouteCR");
			var randomCarrier = Helper.NewOrgHeader("RandomCR");

			var standardCost = Helper.NewCosting(null);
			var rate1 = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "FRT", 100m);
			rate1.TI_OH_TransportProvider = routeCarrier.PK;
			var rate2 = standardCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "BAF", 200m);
			rate2.TI_OH_TransportProvider = randomCarrier.PK;

			var carrierSpecificCost = Helper.NewCosting(routeCarrier);
			var rate3 = carrierSpecificCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "CAF", 300m);
			rate3.TI_OH_TransportProvider = routeCarrier.PK;
			var rate4 = carrierSpecificCost.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "AUMEL", "WAR", 400m);
			rate4.TI_OH_TransportProvider = randomCarrier.PK;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUPER", 1000m);
			shipment.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.Transports.AddNew("AUSYD", "AUMEL");
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.CarrierPK = routeCarrier.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 100m,
					CostAccountCode = ZString.Empty, // Standard Rate
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_LocalCostAmt = 300m,
					CostAccountCode = routeCarrier.OH_Code, // Service Provider = Route Carrier
				},
			};

			AutorateAndAssert("Autorate Standalone Shipment should load costs based on route's Carrier/Creditors",
				expected, shipment, Consignee, autorateCosts: true, autorateRevenue: false, standaloneShipmentOnly: true);
		}

		public void TestAutorateStandaloneShipment_ShouldLoadConsolLevelCharges()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Transport Provider One";
			creditor.OH_IsCreditor = true;

			var rate = Helper.NewCosting(creditor);
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = false;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUPER", 1000m);
			shipment.Transports.RemoveAndDeleteAll();

			var transport1 = shipment.Transports.AddNew("AUSYD", "AUMEL");
			transport1.JW_CarrierBookingReference = "A";
			transport1.JW_TransportMode = TransportModes.Air;
			transport1.CreditorPK = creditor.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT" }, // Consol Level Charge
				new AssertionCharge { ChargeCode = "BAF" }, // Non-Consol Level Charge
			};

			AutorateAndAssert("When Multi-Route Auto-Costing is enabled we expect to autorate all route sets",
				expected,
				shipment,
				Consignee,
				autorateCosts: true,
				autorateRevenue: false,
				isConsolLevelChargeExcluded: false, // based on the menu item "Autorate Costs (Standalone with Consol Level Charge)"
				standaloneShipmentOnly: true);      // based on the menu item "Autorate Costs (Standalone with Consol Level Charge)"
		}

		#endregion

		public void TestAutorateConsol_ControllingCustomer_NoFallbacktoClient()
		{
			var globalCosting = Helper.NewGlobalCosting(null);
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var controllingCustomer = Helper.NewOrgHeader("CONT_CUST");

			var firstCostEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			firstCostEntry.RateLines.RemoveAndDeleteAll();
			firstCostEntry.AddRateLine(globalChargeCode).GetCalculator<FlatCalculator>().BaseRate = 100m;

			var secondCostEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			secondCostEntry.RateLines.RemoveAndDeleteAll();
			secondCostEntry.AddRateLine(globalChargeCode).GetCalculator<FlatCalculator>().BaseRate = 200m;
			secondCostEntry.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var thirdCostEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			thirdCostEntry.RateLines.RemoveAndDeleteAll();
			thirdCostEntry.AddRateLine(globalChargeCode).GetCalculator<FlatCalculator>().BaseRate = 300m;
			thirdCostEntry.TI_OH_ControllingCustomer = Consignor.PK;

			var shipmentCCUS = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			shipmentCCUS.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();
			var consolCCUS = shipmentCCUS.Consols.AddNew();
			consolCCUS.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipmentLC = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			var consolLC = shipmentLC.Consols.AddNew();
			consolLC.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert
				(
					"Controlling customer on shipment matches rate with controlling customer set",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
						new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m },
					},
					costsSupporter: consolCCUS,
					autorateRevenue: false
				);

				AutoCostAndAssert
				(
					"Local client on shipment does not match rate with controlling customer set",
					expectedInvoicingCharges: null,
					expectedCosts: new[]
					{
						new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m },
					},
					costsSupporter: consolLC,
					autorateRevenue: false
				);
			}
		}

		public void TestAutorateShipmentRevenue_ManualEnteredRevenueChargeShouldBeUpdated()
		{
			var revenueChargeCode = Helper.ChargeCodes.New("REVCHG", "Revenue Charge Code", ChargeCodeGroupList.Codes.Freight);
			revenueChargeCode.AC_ChargeType = ChargeType.Revenue;
			revenueChargeCode.AC_DepartmentFilterList = "ALL";
			revenueChargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var clientRate = Helper.NewClientRate(Consignee);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "AUSYD");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(revenueChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "AUSYD", 1000m);

			Factory.Save();

			using (var existingJobWithManualCharge = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				existingJobWithManualCharge.JH_OA_LocalChargesAddr = Consignee.MainAddress.PK;
				existingJobWithManualCharge.Charges.RemoveAndDeleteAll();
				var manualCharge = existingJobWithManualCharge.Charges.AddNew();
				manualCharge.JR_AC = revenueChargeCode.PK;
				manualCharge.JR_SellRatingOverrideComment = "Manual Rev Charge";
				manualCharge.JR_OSSellAmt = 100;
				manualCharge.JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates;

				Factory.Save();

				Assert("Precondition: Revenue charge should not have cost currency code.", manualCharge.JR_RX_NKCostCurrency.IsEmpty);
				AssertEquals("Precondition: Current company currency code.", CurrencyCodes.Australia, Env.CurrentCompany.LocalCurrency.Code);

				var expected = new[]
				{
					new AssertionCharge
					{
						JR_LocalCostAmt = 0,
						JR_RX_NKCostCurrency = ZString.Empty,
						JR_LocalSellAmt = 100,
						JR_OSSellAmt = 100
					},
					new AssertionCharge
					{
						JR_LocalCostAmt = 0,
						JR_RX_NKCostCurrency = ZString.Empty,
						JR_LocalSellAmt = 0,
						JR_OSSellAmt = 0,
						JR_RX_NKSellCurrency = "AUD"
					}
				};
				const string message = "There should not be any exception stopping Autorating. Sell amounts and currency should be updated";
				AutorateAndAssert(message, expected, shipment, Consignee, job: existingJobWithManualCharge, autorateCosts: false);
			}
		}

		public void TestAutorateConsol_ContractNumbersShouldNotBeCleared()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(0).PK;

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "", chargeCode.AC_Code, 100);
			costEntry.TI_ContractNumber = "CON";

			var clientRate = Helper.NewClientRate(Consignor);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "", chargeCode.AC_Code, 120);
			clientRateEntry.TI_ContractNumber = "CLC";

			Factory.Save();

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX", PaymentType.Prepaid);
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			shipment.Numbers.RemoveAndDeleteAll();
			var clcNumber = shipment.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC";

			consol.JK_CarrierContractNumber = "CON";

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					shipment,
					new[]
					{
						new AssertionCharge
						{
							JR_OSSellAmt = 120,
							JR_OSCostAmt = 100,
							ChargeCode = "FRT",
						}
					}
				}
			};
			var expectedCosts = new[]
			{
				new AssertionCost
				{
					E6_OSCostAmount = 100,
					ChargeCode = "FRT"
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutoCostAndAssert("", expectedCharges, expectedCosts, consol);
			AssertEquals("CON", consol.JK_CarrierContractNumber);
			AssertGreaterThan("Shipment numbers should not be empty.", shipment.Numbers.Count, 0);
		}

		public void TestAutorateShipmentRevenue_RateFound_ContractNumbersShouldNotBeCleared()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "", "FRT", 100);
			rateEntry.TI_ContractNumber = "CLC";

			Factory.Save();

			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX");
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			shipment.Numbers.RemoveAndDeleteAll();
			var clcNumber = shipment.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC";

			consol.JK_CarrierContractNumber = "CON";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 100,
					ChargeCode = "FRT",
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignor);
			AssertEquals("CON", consol.JK_CarrierContractNumber);
			AssertGreaterThan("Shipment numbers should not be empty after autorating.", shipment.Numbers.Count, 0);
		}

		public void TestAutorateShipmentRevenue_NoRate_ContractNumbersShouldNotBeCleared()
		{
			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX");
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			shipment.Numbers.RemoveAndDeleteAll();
			var clcNumber = shipment.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC";

			consol.JK_CarrierContractNumber = "CON";

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, Consignor, autorateCosts: false);
			AssertEquals("CON", consol.JK_CarrierContractNumber);
			AssertGreaterThan(shipment.Numbers.Count, 0);
		}

		public void TestAutorateShipmentCostAndRevenue_NoRate_ContractNumbersShouldNotBeCleared()
		{
			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX");
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			shipment.Numbers.RemoveAndDeleteAll();
			var clcNumber = shipment.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC";

			consol.JK_CarrierContractNumber = "CON";

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, Consignor);
			AssertEquals("CON", consol.JK_CarrierContractNumber);
			AssertGreaterThan(shipment.Numbers.Count, 0);
		}

		public void TestAutorateShipmentCost_NoRate_ContractNumbersShouldNotBeCleared()
		{
			var shipment = CreateTestShipmentWithAttachedConsolAndContainer("AUSYD", "USLAX");
			var consol = shipment.Consols.OfType<ForwardingConsol>().Single();

			shipment.Numbers.RemoveAndDeleteAll();
			var clcNumber = shipment.Numbers.AddNew();
			clcNumber.CE_EntryType = CustomsReferenceNumberCodes.CLC;
			clcNumber.CE_EntryNum = "CLC";

			consol.JK_CarrierContractNumber = "CON";

			AutorateAndAssert(Array.Empty<AssertionCharge>(), shipment, Consignor, autorateRevenue: false);
			AssertEquals("CON", consol.JK_CarrierContractNumber);
			AssertGreaterThan(shipment.Numbers.Count, 0);
		}

		public void TestAutorateConsol_ShouldNotFilterTACTRates()
		{
			TransportProvider1.OH_IsCreditor = true;
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 200, contractNumber: "ABC");

			var standardCosting = Helper.NewCosting(null);
			var costEntryIsTact = standardCosting.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL", "FRT", 100, QuantityUnit.M3);
			costEntryIsTact.TI_IsTact = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ActualWeight = 1;
			shipment.JS_ActualVolume = 1;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.CreditorPK = TransportProvider1.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_CarrierContractNumber = "ABC";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200,
				},
			};

			AutoCostAndAssert("Auto-rating should pick the more specific carrier costing", null, expected, consol, false);

			AssertEquals("TACT rate should be not filtered", (ZDecimal)100, consol.JK_ConsolChargeableRate);
		}

		#region Containers

		public void TestAutorateShipment_SameContainerInDifferentConsols()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD");
			rateEntry.TI_RH_NKCommodityCode = "AAA";
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100;
			Factory.Save();

			var shipment = CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.FCL, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "1111";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_RH_NKContainerCommodityCode = "AAA";

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "1111";
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_RH_NKContainerCommodityCode = "AAA";

			var packLine = shipment.OuterPackLines.AddNew();
			container1.AddPackLine(packLine);
			container2.AddPackLine(packLine);

			var expectedCharges = new[]
			{
				new AssertionCharge()
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100,
					RevenueCalculationDescription = "FRT: 1 Container(s) @ AUD 100.00/Container"
				}
			};
			var message = "The 2 containers have same container key - consider as 1 container.";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);

			container2.JC_ContainerNum = "2222";

			expectedCharges = new[]
			{
				new AssertionCharge()
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100,
					RevenueCalculationDescription = "FRT: 1 Container(s) @ AUD 100.00/Container"
				}
			};

			message = "The 2 containers have different container key - but still only one container per consol.";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
		}

		#endregion

		public void TestAutoratingConsol_WhenTransportLegsAreDeleted_NoExceptionIsThrown()
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.Transports[0].JW_ETD = new ZDateTime(2021, 06, 01);
			consol.Transports[0].JW_ETA = new ZDateTime(2021, 06, 01);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			AssertEquals(1, consol.Transports.Count);
			AssertEquals(1, ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets.Count);

			consol.Transports.RemoveAndDeleteAll();

			AssertEquals(0, ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets.Count);

			var autoRater = new FreightAutoRater(new RatingContext());
			AssertNoExceptionThrown(() => autoRater.AutoRate(new AutoRatingProxy(consol.RatingAdapter), CostSell.Cost));
		}

		public void TestAutoratingShipment_WhenChangeWeightAndAutoratingRevenueAgain_ThenGrossWeightInAWBShouldBeChanged()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "NZAKL");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = (ZDecimal)10m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "NZAKL", 10m);
			shipment.JS_ActualWeight = 10m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_ActualVolume = 1;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1666.67,
					ChargeCode = "FRT",
				}
			};

			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: true, autorateRevenue: true, deleteJobCharges: false);

			shipment.PopulateAWB();
			var awbRateLine = shipment.AWBHeaderManager.AWBHeader.AWBRateLines[0];

			AssertEquals(10m, awbRateLine.ER_GrossWeight);

			Factory.Save();

			shipment.JS_ActualWeight = 12m;

			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false, autorateRevenue: true, deleteJobCharges: false);

			shipment.PopulateAWB();
			awbRateLine = shipment.AWBHeaderManager.AWBHeader.AWBRateLines[0];

			AssertEquals("Gross Weight should be changed after auto rating again", 12m, awbRateLine.ER_GrossWeight);
		}

		#region Rounding on pack lines and rate lines

		public void TestRateLineRoundingAndAuditLogNoteContainsActualChargeCalculation_RoundUpTo1()
		{
			AssertRateLineRoundingAndAuditLogNoteContainsActualChargeCalculation(
				roundingType: RatingRoundingTypes.UpTo1,
				expectedSellAmount: 17730,
				expectedRevenueCalculationDescription: "ODOC: 1773 Kilogram(s) @ AUD 10.00/KG",
				expectedChargeCalculationLine: "\tODOC: 1773 Kilogram(s) @ AUD 10.00/KG"
			);
		}

		public void TestRateLineRoundingAndAuditLogNoteContainsActualChargeCalculation_RoundUpToHalf()
		{
			AssertRateLineRoundingAndAuditLogNoteContainsActualChargeCalculation(
				roundingType: RatingRoundingTypes.UpToHalf,
				expectedSellAmount: 17725,
				expectedRevenueCalculationDescription: "ODOC: 1772.5 Kilogram(s) @ AUD 10.00/KG",
				expectedChargeCalculationLine: "\tODOC: 1772.5 Kilogram(s) @ AUD 10.00/KG"
			);
		}

		void AssertRateLineRoundingAndAuditLogNoteContainsActualChargeCalculation(
			string roundingType,
			ZDecimal expectedSellAmount,
			string expectedRevenueCalculationDescription,
			string expectedChargeCalculationLine
		)
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", removeLines: true);
			var rateLine = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10;
			rateLine.TL_Rounding = roundingType;
			Factory.Save();

			// job chargeable = MAX(1644, 10.633 / 0.006) = 1772.167 but rate line rounding should be used instead
			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, Consignee.PK, "AUSYD", "USLAX", 1644, 10.633m);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = expectedSellAmount,
					RevenueCalculationDescription = expectedRevenueCalculationDescription
				}
			};
			const string message = "Should take rate line rounding for calculation.";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false);
			// Also test Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner.LogChargesCalculated
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should have charge calculation ", expectedChargeCalculationLine);
		}

		public void TestAutorateShipmentWithPackLineRounding_BankersRounding()
		{
			// 5 x 1.22 x 1.46 x 1.05 = 9.3513 => Rounded to 9.351, divided by 0.006 => 1558.5
			AssertAutorateShipmentWithPackLineRounding(
				defaultNumberOfDecimalsCollection: null,
				expectedSellAmount: 15585m,
				expectedRevenueCalculationDescription: "ODOC: 1558.5 Kilogram(s) @ AUD 10.00/KG");
		}

		public void TestAutorateShipmentWithPackLineRounding_DefaultNumberOfDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = Volume.CubicMetres;
			defaultNumberOfDecimals.TransportMode = TransportModes.Air;
			defaultNumberOfDecimals.NumberOfDecimals = 3;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;

			// 9.3513 => Rounded up to 9.352 with above configuration, divided by 0.006 => 1558.667
			AssertAutorateShipmentWithPackLineRounding(
				defaultNumberOfDecimalsCollection: collection,
				expectedSellAmount: 15586.67m,
				expectedRevenueCalculationDescription: "ODOC: 1558.667 Kilogram(s) @ AUD 10.00/KG");
		}

		void AssertAutorateShipmentWithPackLineRounding(
			DefaultNumberOfDecimalsCollection defaultNumberOfDecimalsCollection,
			ZDecimal expectedSellAmount,
			string expectedRevenueCalculationDescription)
		{
			var ratingHeader = Helper.NewClientRate(Consignee);

			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", removeLines: true);
			var rateLine = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 10;
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1m, 0m);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = expectedSellAmount,
					RevenueCalculationDescription = expectedRevenueCalculationDescription
				}
			};

			if (defaultNumberOfDecimalsCollection == null)
			{
				// PackLine volume rounding should use default rounding
				UpdatePackLine(shipment);
				AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

				return;
			}

			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultNumberOfDecimalsCollection))
			{
				// PackLine volume rounding should take the configuration from DefaultNumberOfDecimalPlaces
				UpdatePackLine(shipment);
				AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);
			}
		}

		static void UpdatePackLine(ForwardingShipment shipment)
		{
			var packLine = shipment.OuterPackLines[0];
			packLine.JL_UnitOfDimension = Length.Metres;

			packLine.JL_PackageCount = 5;
			packLine.JL_Length = 1.22m;
			packLine.JL_Height = 1.46m;
			packLine.JL_Width = 1.05m;

			// 5 x 1.22 x 1.46 x 1.05 = 9.3513 M3
			shipment.JS_ActualVolume = packLine.JL_ActualVolume;
			shipment.JS_ActualWeight = packLine.JL_ActualWeight;
		}

		#endregion

		#region TestAutoratingColoadCostWithImportExportCreditor

		public void TestAutoRatingCost_ForCoLoadExportConsol_ShouldBringCarrierExportCharges()
		{
			var origin = "AUSYD";
			var destination = "NZAKL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;

			Factory.Save();

			var costing = Helper.NewCosting(exportCreditorAddress);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_OH_Creditor = exportCreditorAddress.PK
				}
			};

			AutoCostAndAssert("Should Bring Export Creditor charges", null, expectedCosts, consol, false);

			var costing1 = Helper.NewCosting(coLoadWith);
			var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 300m;

			var rateLine2 = rateEntry1.AddRateLine("CAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 400m;

			var costing2 = Helper.NewCosting(carrier);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry2.AddRateLine("BAF", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 500m;

			var relatedPartyForCarrier = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForCarrier.OH_IsCreditor = true;

			var carrierRelatedParties = carrier.AllRelatedParties;
			var partyRecord = carrierRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord.PR_OH_RelatedParty = relatedPartyForCarrier.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = origin;

			var costing3 = Helper.NewCosting(null);
			var rateEntry3 = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine4 = rateEntry3.AddRateLine("WAR", FlatCalculator.Code);
			rateLine4.GetCalculator<FlatCalculator>().BaseRate = 600m;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_OH_Creditor = exportCreditorAddress.PK
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = exportCreditorAddress.PK //There is Carrier Export Creditor, so we should use it as creditor
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = relatedPartyForCarrier.PK // this is from carrier, creditor should be carrier’s Organisation > Details > Related Parties
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = exportCreditorAddress.PK //There is Carrier Export Creditor, so we should use it as creditor for general costing
				},
			};

			AutoCostAndAssert("Export Creditor takes priority over co-load with", null, expectedCosts, consol, false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Export Creditor should take priority over Co-load with", "Information: RateLine Filtered FRT-FLT-Costing TRASPROV2\treason:\toverridden by FRT-FLT-Costing TRASPROV1 by Creditors comparer");

			carrier.AllRelatedParties.RemoveAndDeleteAll();
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = coLoadWith.PK // no creditor export address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = coLoadWith.PK // no creditor export address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = carrier.PK // NO Carrier’s Organisation > Details > Related Parties with Direction = PIC/PAD, should fallback to carrier
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = coLoadWith.PK //There is NO Carrier Export Creditor, so we should fall back to Co-Load With
				},
			};

			Factory.Save();

			AutoCostAndAssert("Defaulting creditor when there is no export creditor", null, expectedCosts, consol, false);
		}

		public void TestAutoRatingCost_ForCoLoadImportConsol_ShouldBringCarrierImportCharges()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			var importCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			importCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;

			Factory.Save();

			var costing = Helper.NewCosting(importCreditorAddress);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierImportCreditorAddress.E2_OA_Address = importCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Import Creditor has value", importCreditorAddress.PK, consol.CarrierImportCreditor.PK);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m
				}
			};

			AutoCostAndAssert("Should Bring Import Creditor charges", null, expectedCosts, consol, false);

			var costing1 = Helper.NewCosting(coLoadWith);
			var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 300m;

			var rateLine2 = rateEntry1.AddRateLine("CAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 400m;

			var costing2 = Helper.NewCosting(carrier);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry2.AddRateLine("BAF", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 500m;

			var relatedPartyForCarrier = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForCarrier.OH_IsCreditor = true;

			var carrierRelatedParties = carrier.AllRelatedParties;
			var partyRecord = carrierRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord.PR_OH_RelatedParty = relatedPartyForCarrier.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = destination;

			var costing3 = Helper.NewCosting(null);
			var rateEntry3 = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine4 = rateEntry3.AddRateLine("WAR", FlatCalculator.Code);
			rateLine4.GetCalculator<FlatCalculator>().BaseRate = 600m;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_OH_Creditor = importCreditorAddress.PK
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = importCreditorAddress.PK //There is Carrier Import Creditor, so we should use it as creditor
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = relatedPartyForCarrier.PK // this is from carrier, creditor should be carrier’s Organisation > Details > Related Parties
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = importCreditorAddress.PK //There is Carrier Import Creditor, so we should use it as creditor for general costing
				},
			};

			AutoCostAndAssert("Import Creditor takes priority over co-load with", null, expectedCosts, consol, false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Import Creditor should take priority over Co-load with", "Information: RateLine Filtered FRT-FLT-Costing TRASPROV2\treason:\toverridden by FRT-FLT-Costing TRASPROV1 by Creditors comparer");

			carrier.AllRelatedParties.RemoveAndDeleteAll();
			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = coLoadWith.PK // no creditor import address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = coLoadWith.PK // no creditor import address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = carrier.PK // NO Carrier’s Organisation > Details > Related Parties with Direction = DLY/PAD, should fallback to carrier
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = coLoadWith.PK //There is NO Carrier Import Creditor, so we should fall back to Co-Load With
				},
			};

			Factory.Save();

			AutoCostAndAssert("Defaulting creditor when there is no import creditor", null, expectedCosts, consol, false);
		}

		#endregion

		#region Pickup & Delivery Transport

		public void TestAutoRatingCost_PickupTransportWithRelatedParty()
		{
			TransportProvider1.OH_IsCreditor = true;
			TransportProvider1.Factory.Save();

			var origin = "AUSYD";
			var destination = "NZAKL";
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("OTHC", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var pickupFrom = OrgHeader.New(Factory);
			pickupFrom.OH_Code = "VWG";
			var address1 = pickupFrom.Addresses.AddNew();
			address1.Address1 = "Address 1";
			address1.ClosestPort = "AUSYD";
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			var address2 = pickupFrom.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			address2.ClosestPort = "AUBNE";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 550m);
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = address2.PK;

			CreateForwardingConsol(TransportModes.Air, origin, destination, Consignor, shipment);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 200m,
					CostAccountCode = TransportProvider1.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var relatedPartyForPickupTransportSYD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForPickupTransportSYD.OH_IsCreditor = true;

			var pickupParties = TransportProvider1.AllRelatedParties;
			var partyRecord = pickupParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord.PR_OH_RelatedParty = relatedPartyForPickupTransportSYD.PK;
			partyRecord.PR_FreightTransportMode = shipment.JS_TransportMode;
			partyRecord.PR_FreightContainerMode = shipment.JS_PackingMode;
			partyRecord.PR_Location = address1.ClosestPort;

			var relatedPartyForPickupTransportBNE = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForPickupTransportBNE.OH_IsCreditor = true;

			var partyRecord1 = pickupParties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_OH_RelatedParty = relatedPartyForPickupTransportBNE.PK;
			partyRecord1.PR_FreightTransportMode = shipment.JS_TransportMode;
			partyRecord1.PR_FreightContainerMode = shipment.JS_PackingMode;
			partyRecord1.PR_Location = address2.ClosestPort;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 200m,
					CostAccountCode = relatedPartyForPickupTransportBNE.OH_Code // Pickup transport has related Party that matches with Pickup from closest port, Charge's creditor should be set to it
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var costing2 = Helper.NewCosting(relatedPartyForPickupTransportBNE);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 300m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OTHC",
					JR_OSCostAmt = 200m,
					CostAccountCode = relatedPartyForPickupTransportBNE.OH_Code // Pickup transport has related party, Charge's creditor should be set to it
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC", // Pickup transport has related party, should bring its cost
					JR_OSCostAmt = 300m,
					CostAccountCode = relatedPartyForPickupTransportBNE.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var rateLine3 = rateEntry2.AddRateLine("OTHC", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 400m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OTHC", // Pickup related party takes over pickup transport
					JR_OSCostAmt = 400m,
					CostAccountCode = relatedPartyForPickupTransportBNE.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC", // Pickup transport has related party, should bring its cost
					JR_OSCostAmt = 300m,
					CostAccountCode = relatedPartyForPickupTransportBNE.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);
		}

		public void TestAutoRatingCost_DeliveryTransportWithRelatedParty()
		{
			TransportProvider1.OH_IsCreditor = true;
			TransportProvider1.Factory.Save();

			var origin = "AUSYD";
			var destination = "NZAKL";
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("DCART", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var deliverTo = OrgHeader.New(Factory);
			deliverTo.OH_Code = "VWG";
			var address1 = deliverTo.Addresses.AddNew();
			address1.Address1 = "Address 1";
			address1.ClosestPort = "AUSYD";
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			var address2 = deliverTo.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			address2.ClosestPort = "AUBNE";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 550m);
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = address2.PK;

			CreateForwardingConsol(TransportModes.Air, origin, destination, Consignor, shipment);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSCostAmt = 200m,
					CostAccountCode = TransportProvider1.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var relatedPartyForDeliveryTransportSYD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForDeliveryTransportSYD.OH_IsCreditor = true;

			var deliveryParties = TransportProvider1.AllRelatedParties;
			var partyRecord = deliveryParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord.PR_OH_RelatedParty = relatedPartyForDeliveryTransportSYD.PK;
			partyRecord.PR_FreightTransportMode = shipment.JS_TransportMode;
			partyRecord.PR_FreightContainerMode = shipment.JS_PackingMode;
			partyRecord.PR_Location = address1.ClosestPort;

			var relatedPartyForDeliveryTransportBNE = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForDeliveryTransportBNE.OH_IsCreditor = true;

			var partyRecord1 = deliveryParties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_OH_RelatedParty = relatedPartyForDeliveryTransportBNE.PK;
			partyRecord1.PR_FreightTransportMode = shipment.JS_TransportMode;
			partyRecord1.PR_FreightContainerMode = shipment.JS_PackingMode;
			partyRecord1.PR_Location = address2.ClosestPort;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSCostAmt = 200m,
					CostAccountCode = relatedPartyForDeliveryTransportBNE.OH_Code // Delivery transport has related party matches with deliver to closest port, Charge's creditor should be set it
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var costing2 = Helper.NewCosting(relatedPartyForDeliveryTransportBNE);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LSE, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("DDOC", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 300m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSCostAmt = 200m,
					CostAccountCode = relatedPartyForDeliveryTransportBNE.OH_Code // Delivery transport has related party, Charge's creditor should be set to it
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC", // Delivery transport has related party, should bring its cost
					JR_OSCostAmt = 300m,
					CostAccountCode = relatedPartyForDeliveryTransportBNE.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);

			var rateLine3 = rateEntry2.AddRateLine("DCART", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 400m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART", // Delivery related party takes over delivery transport
					JR_OSCostAmt = 400m,
					CostAccountCode = relatedPartyForDeliveryTransportBNE.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC", // Delivery transport has related party, should bring its cost
					JR_OSCostAmt = 300m,
					CostAccountCode = relatedPartyForDeliveryTransportBNE.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region Consol Gross Weight Overridden

		public void TestConsolAutoRatingWhenContainerGrossWeightOverridden_NoPackline()
		{
			var carrier = TransportProvider1;
			var costingCarrier = Helper.NewCosting(carrier);
			const string AirContainerType = "LD-3";

			costingCarrier.AddRateEntryWithUnitRateLine(
				RatingConstants.RateCategory.AIR,
				RateMode.ULD,
				origin: "AUSYD",
				destination: "USLAX",
				chargeCode: "FRT",
				unitRateAmount: 15,
				unit: Weight.Kilograms,
				currency: "AUD",
				container: AirContainerType);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100m);
			new JobHeader.Loader(shipment).TryCreate();
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", carrier, shipment, prepaidCollect: PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.ULD;
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, AirContainerType)).PK;
			container.JC_GrossWeightUQ = Weight.Kilograms;
			container.JC_TareWeight = 50;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_GrossWeight = 765m;

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 765m * 15m,
					E6_OH_Creditor = carrier.PK
				}
			};

			AutoCostAndAssert("should use gross weight", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestConsolAutoRatingWhenContainerGrossWeightOverridden_ChargeableRounding()
		{
			var carrier = TransportProvider1;
			var costingCarrier = Helper.NewCosting(carrier);
			const string AirContainerType = "LD-3";

			var entry = costingCarrier.AddRateEntryWithUnitRateLine(
				RatingConstants.RateCategory.AIR,
				RateMode.ULD,
				origin: "AUSYD",
				destination: "USLAX",
				chargeCode: "FRT",
				unitRateAmount: 15,
				unit: Weight.Kilograms,
				currency: "AUD",
				container: AirContainerType);
			var line = entry.RateLines[0];
			line.TL_Rounding = RatingRoundingTypes.Chargeable;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100m);
			new JobHeader.Loader(shipment).TryCreate();
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", carrier, shipment, prepaidCollect: PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.ULD;
			var container1 = consol.Containers.AddNew();
			var airContainerTypePK = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, AirContainerType)).PK;
			container1.JC_RC = airContainerTypePK;
			container1.JC_GrossWeightUQ = Weight.Kilograms;
			container1.JC_IsGrossWeightOverridden = true;
			container1.JC_GrossWeight = 20;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = airContainerTypePK;

			// Packline with a volume equivalent to 100kg, and a weight of only 50kg
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container2.PK;
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_ActualWeightUQ = Weight.Kilograms;
			packLine1.JL_ActualWeight = 50;
			packLine1.JL_ActualVolumeUQ = Volume.CubicMetres;
			packLine1.JL_ActualVolume = 0.6m;

			// Container 1 is overridden and should only use the given weight of 20kg and will have no volume.
			// Container 2 is not overridden and should use the volume converted to weight of 100kg.
			// What we don't want is the total volume to be converted to weight, since that will exclude container 1.
			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = (100m + 20m) * 15m,
					E6_OH_Creditor = carrier.PK
				}
			};

			AutoCostAndAssert("should use gross weight", null, expectedCosts, consol, autorateRevenue: false);
		}

		#endregion

		#region Consol Autorating Preferences

		ForwardingConsol SetupForTestAutoratingDateOverride()
		{
			var carrier = TransportProvider1;
			var costingCarrier = Helper.NewCosting(carrier);
			var today = ZDate.Today;

			// FRT Start date (-10) < DST Start date (-5) < TODAY (0) < FRT End date (10) < DST End date (15)
			var rateEntryFRT = costingCarrier.AddRateEntryWithFlatRateLine(
				RatingConstants.RateCategory.AIR,
				RateMode.LSE,
				origin: "USLAX",
				destination: "AUSYD",
				chargeCode: "FRT",
				flatRateAmount: 100,
				currency: "AUD");
			rateEntryFRT.TI_RateStartDate = today.AddDays(-10);
			rateEntryFRT.TI_RateEndDate = today.AddDays(10);

			Helper.ChargeCodes.NewConsolChargeCode("DTEST", "Destination Test Code", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var rateEntryDST = costingCarrier.AddRateEntryWithFlatRateLine(
				RatingConstants.RateCategory.DST,
				RateMode.ALL,
				origin: "USLAX",
				destination: "AUSYD",
				chargeCode: "DTEST",
				flatRateAmount: 200,
				currency: "AUD");
			rateEntryDST.TI_RateStartDate = today.AddDays(-5);
			rateEntryDST.TI_RateEndDate = today.AddDays(15);

			Factory.Save();

			// an import job US > AU
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, "USLAX", "AUSYD", carrier, shipment, prepaidCollect: PaymentType.Prepaid);
			return consol;
		}

		[TestDate(2023, 4, 1)]
		public void TestAutoratingDateOverride_NoOverride()
		{
			var today = ZDate.Today;
			var consol = SetupForTestAutoratingDateOverride();
			var transport = consol.Transports[0];

			transport.JW_ETD = today;
			transport.JW_ETA = today;
			consol.AutoratingDate = ZDate.Empty;

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100,
					CostCalculationDescription = "Autorating Date:	01-Apr-23"
				},
				new AssertionCost
				{
					ChargeCode = "DTEST",
					E6_OSCostAmount = 200,
					CostCalculationDescription = "Autorating Date:	01-Apr-23"
				}
			};
			AutoCostAndAssert("No autorating date override, job date should be used and logged", null, expectedCosts, consol, autorateRevenue: false);
			Assert("Autorating Date Override is populated", consol.AutoratingDate.IsEmpty);
		}

		[TestDate(2023, 4, 1)]
		public void TestAutoratingDateOverride_Override_MultiModalRatingCostIsFalse() => AutoratingDateOverride_Override(false);

		[TestDate(2023, 4, 1)]
		public void TestAutoratingDateOverride_Override_MultiModalRatingCostIsTrue() => AutoratingDateOverride_Override(true);

		public void AutoratingDateOverride_Override(bool multiModalRatingCost)
		{
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, multiModalRatingCost))
			{
				var today = ZDate.Today;
				var consol = SetupForTestAutoratingDateOverride();
				var transport = consol.Transports[0];
				transport.JW_ETD = today;
				transport.JW_ETA = today.AddDays(2);

				// Test1 DateOverride (-11) < FRT Start date (-10) < DST Start date (-5) < TODAY (0) < FRT End date (10) < DST End date (15)
				var testDate = today.AddDays(-11);
				consol.AutoratingDate = testDate;
				AssertNoErrors("Autorating Date Override should have been set", consol.AutoratingDateInfo);

				var expectedCosts = Enumerable.Empty<AssertionCost>();
				AutoCostAndAssert("Autorating date override is too old", null, expectedCosts, consol, autorateRevenue: false);

				// Test2 FRT Start date (-10) < DateOverride (-8) < DST Start date (-5) < TODAY (0) < FRT End date (10) < DST End date (15)
				testDate = today.AddDays(-8);
				consol.AutoratingDate = testDate;

				expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 100,
						CostCalculationDescription = "Autorating Date:	24-Mar-23 (Override)"
					},
				};

				AutoCostAndAssert("Autorating date override in range only for FRT charge", null, expectedCosts, consol, autorateRevenue: false);
				AssertEquals("Autorating Date Override should not be changed", testDate, consol.AutoratingDate);

				// Test3 FRT Start date (-10) < DST Start date (-5) < DateOverride (-2) < TODAY (0) < FRT End date (10) < DST End date (15)
				testDate = today.AddDays(-2);
				consol.AutoratingDate = testDate;

				expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 100,
						CostCalculationDescription = "Autorating Date:	30-Mar-23 (Override)"
					},
					new AssertionCost
					{
						ChargeCode = "DTEST",
						E6_OSCostAmount = 200,
						CostCalculationDescription = "Autorating Date:	30-Mar-23 (Override)"
					},
				};

				AutoCostAndAssert("Autorating date override in range for both charges", null, expectedCosts, consol, autorateRevenue: false);

				// Test4 FRT Start date (-10) < DST Start date (-5) < TODAY (0) < FRT End date (10) < DateOverride (11) < DST End date (15)
				testDate = today.AddDays(11);
				consol.AutoratingDate = testDate;

				expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "DTEST",
						E6_OSCostAmount = 200,
						CostCalculationDescription = "Autorating Date:	12-Apr-23 (Override)"
					},
				};

				AutoCostAndAssert("Autorating date override in range only for DST charge", null, expectedCosts, consol, autorateRevenue: false);

				// Test5 FRT Start date (-10) < DST Start date (-5) < TODAY (0) < FRT End date (10) < DST End date (15) < DateOverride (18)
				testDate = today.AddDays(18);
				consol.AutoratingDate = testDate;

				expectedCosts = Enumerable.Empty<AssertionCost>();

				AutoCostAndAssert("Autorating date override is too far in future", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		#endregion

		#region Autorating Via Port Registry

		public void TestAutoratingViaPortRegistry_InUse_ForShipment()
		{
			var shipment = CreateForwardingShipment("SEA", Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 0);
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USCHI";

			var configurations = new AutoratingViaPortConfigurationCollection();
			var configuration = configurations.AddNew();
			configuration.JobType = "SHP";
			configuration.TransportMode = "ALL";
			var setting = configuration.Settings.AddNew();
			setting.Direction = "ALL";
			setting.ViaSourceOption = "1L";

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("1st Load has to be returned", "AUMEL", shipment.RatingAdapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("1st Load has to be returned", "AUMEL", shipment.RatingAdapter.GetVia(CostSell.Revenue)?.Code);
			}

			setting.ViaSourceOption = "LD";

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("Last Discharge has to be returned", "USCHI", shipment.RatingAdapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("Last Discharge has to be returned", "USCHI", shipment.RatingAdapter.GetVia(CostSell.Revenue)?.Code);
			}
		}

		public void TestAutoratingViaPortRegistry_InUse_ForConsol()
		{
			var shipment = CreateForwardingShipment("SEA", Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 0);
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USCHI";

			var configurations = new AutoratingViaPortConfigurationCollection();
			var configuration = configurations.AddNew();
			configuration.JobType = "FCN";
			configuration.TransportMode = "ALL";
			var setting = configuration.Settings.AddNew();
			setting.Direction = "ALL";
			setting.ViaSourceOption = "VL";

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("1st Load has to be returned", "AUMEL", consol.RatingAdapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("1st Load has to be returned", "AUMEL", consol.RatingAdapter.GetVia(CostSell.Revenue)?.Code);
			}

			setting.ViaSourceOption = "VD";

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				AssertEquals("Last Discharge has to be returned", "USCHI", consol.RatingAdapter.GetVia(CostSell.Cost)?.Code);
				AssertEquals("Last Discharge has to be returned", "USCHI", consol.RatingAdapter.GetVia(CostSell.Revenue)?.Code);
			}
		}

		#endregion
	}
}
