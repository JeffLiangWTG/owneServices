using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Integration.Freight;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Document.Testing
{
	public class FFMMessageBuilderTest : DtbBookingTestCaseWithFactory
	{
		readonly string voyageNumber = "CS0001A";
		readonly string goodsDescription = "Test Goods";
		readonly string wayBillNumber = "724-47177421";
		readonly string vessel = "SHIPPER";
		readonly string shipmentOrigin = "ITMXP";
		readonly string shipmentDestination = "THBKK";
		readonly string documentID = "12 123 123 123";
		readonly string regulatedAgentID = "0079-00";
		readonly string approvalCountryCode = "IT";
		readonly string consolSecurityStatus = "SPX";

		readonly string direction = "DLV";

		readonly ZDate leaveDate = new(2077, 1, 1);
		readonly ZDate arriveDate = new(2077, 1, 2);

		readonly string packagePackageType = "PKG";
		readonly string containerType = "AMA-2Q";

		readonly string containerID1 = "ContainerWithOneSubPackage";

		readonly ZInt container1SubPackage1Quantity = 1;
		readonly ZDecimal container1SubPackage1Volume = 7.5m;
		readonly ZString container1SubPackage1VolumeUnit = "CY";
		readonly ZDecimal container1SubPackage1Weight = 55.5m;
		readonly ZString container1SubPackage1WeightUnit = "LB";

		TestContainerData containerDetails1;

		readonly string containerID2 = "ContainerWithTwoSubPackage";

		readonly ZInt container2SubPackage1Quantity = 1;
		readonly ZDecimal container2SubPackage1Volume = 10.0m;
		readonly ZString container2SubPackage1VolumeUnit = "M3";
		readonly ZDecimal container2SubPackage1Weight = 100.0m;
		readonly ZString container2SubPackage1WeightUnit = "KG";

		readonly ZInt container2SubPackage2Quantity = 2;
		readonly ZDecimal container2SubPackage2Volume = 40.0m;
		readonly ZString container2SubPackage2VolumeUnit = "M3";
		readonly ZDecimal container2SubPackage2Weight = 400.0m;
		readonly ZString container2SubPackage2WeightUnit = "KG";

		TestContainerData containerDetails2;

		readonly ZInt loosePackage1Quantity = 3;
		readonly ZDecimal loosePackage1Volume = 90.0m;
		readonly ZString loosePackage1VolumeUnit = "M3";
		readonly ZDecimal loosePackage1Weight = 900.0m;
		readonly ZString loosePackage1WeightUnit = "KG";

		TestPackageData loosePackageDetails1;

		readonly string expectedPortOfLoading = "MXP";
		readonly string expectedPortOfDestination = "BKK";

		IContext context;

		IFFMDetails validFFMDetails;
		IFFMDetails badFFMDetailsMissingDetails;
		IFFMDetails ffmDetailsWithoutPackages;

		TestPackageDetails validPackageDetails;
		TestPackageDetails badPackageDetails;

		TestRefUNLOCO PortOfLoading;
		TestRefUNLOCO PortOfDestination;

		protected override void SetUp()
		{
			base.SetUp();
			containerDetails1 = new()
			{
				ContainerID = containerID1,
				ContainerType = containerType,
				SubPackages = new()
				{
					new TestPackageData()
					{
						Quantity = container1SubPackage1Quantity,
						Type = packagePackageType,
						Volume = container1SubPackage1Volume,
						VolumeMetric = container1SubPackage1VolumeUnit,
						Weight = container1SubPackage1Weight,
						WeightMetric = container1SubPackage1WeightUnit,
					}
				}
			};

			containerDetails2 = new()
			{
				ContainerID = containerID2,
				ContainerType = containerType,
				SubPackages = new()
				{
					new TestPackageData()
					{
						Quantity = container2SubPackage1Quantity,
						Type = packagePackageType,
						Volume = container2SubPackage1Volume,
						VolumeMetric = container2SubPackage1VolumeUnit,
						Weight = container2SubPackage1Weight,
						WeightMetric = container2SubPackage1WeightUnit
					},
					new TestPackageData()
					{
						Quantity = container2SubPackage2Quantity,
						Type = packagePackageType,
						Volume = container2SubPackage2Volume,
						VolumeMetric = container2SubPackage2VolumeUnit,
						Weight = container2SubPackage2Weight,
						WeightMetric = container2SubPackage2WeightUnit
					}
				},
			};

			loosePackageDetails1 = new()
			{
				Quantity = loosePackage1Quantity,
				Type = packagePackageType,
				Volume = loosePackage1Volume,
				VolumeMetric = loosePackage1VolumeUnit,
				Weight = loosePackage1Weight,
				WeightMetric = loosePackage1WeightUnit,
			};

			context = new TransportBookingsCommonContext(Factory);

			PortOfLoading = new()
			{
				PK = ZGuid.NewZGuid(),
				RL_RN_NKCountryCode = "IT",
				RL_Code = "ITMXP",
				RL_IATA = "MXP"
			};

			PortOfDestination = new()
			{
				PK = ZGuid.NewZGuid(),
				RL_RN_NKCountryCode = "TH",
				RL_Code = "THBKK",
				RL_IATA = "BKK"
			};

			validPackageDetails = new()
			{
				ContainerNumber = containerID1,
				GoodsDescription = goodsDescription,
				Quantity = container1SubPackage1Quantity,
				Volume = container1SubPackage1Volume,
				VolumeMetric = container1SubPackage1VolumeUnit,
				Weight = container1SubPackage1Weight,
				WeightMetric = container1SubPackage1WeightUnit,
				PortOfDestination = PortOfDestination,
				PortOfLoading = PortOfLoading,
				WayBillNumber = wayBillNumber
			};

			validFFMDetails = new TestFFMDetails()
			{
				FlightDate = ZDateTime.Now,
				VoyageFlightNo = voyageNumber,
				RegulatedAgentID = regulatedAgentID,
				AirportOfDestinationCode = PortOfDestination,
				PortOfLoading = PortOfLoading,
				RegulatedAgentCountry = approvalCountryCode,
				SecurityStatusCode = consolSecurityStatus,
				Packages = new List<IFFMPackageDetails>() { validPackageDetails }
			};

			badPackageDetails = new()
			{
				ContainerNumber = ZString.Empty,
				GoodsDescription = ZString.Empty,
				Quantity = ZInt.Zero,
				Volume = ZDecimal.Zero,
				Weight = ZDecimal.Zero,
				PortOfDestination = null,
				PortOfLoading = null,
				WayBillNumber = ZString.Empty
			};

			badFFMDetailsMissingDetails = new TestFFMDetails()
			{
				FlightDate = ZDateTime.Empty,
				VoyageFlightNo = ZString.Empty,
				RegulatedAgentID = ZString.Empty,
				AirportOfDestinationCode = null,
				PortOfLoading = null,
				Packages = new List<IFFMPackageDetails>() { badPackageDetails }
			};

			ffmDetailsWithoutPackages = new TestFFMDetails()
			{
				FlightDate = ZDateTime.Now,
				VoyageFlightNo = voyageNumber,
				RegulatedAgentID = regulatedAgentID,
				AirportOfDestinationCode = PortOfDestination,
				PortOfLoading = PortOfLoading,
				Packages = new List<IFFMPackageDetails>()
			};
		}

		public void TestBuildFFMMessageFFMDetails()
		{
			var ffmDetails = CreateTestData(containers: new() { containerDetails1 });

			var ffmMessageBuilder = new FFMMessageBuilder(ffmDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			CombineAssertions("The generated FFM Message appears to be incorrect.", () =>
			{
				AssertEquals("Voyage Number is wrong.", voyageNumber, ffmMessage.VoyageFlightNo);
				AssertEquals("Flight Date is wrong.", leaveDate, ffmMessage.FlightDate);
				AssertEquals("Port of Loading is wrong.", expectedPortOfLoading, ffmMessage.PortOfLoading.IATACode);
				AssertEquals("Airport of Destination Code is wrong.", expectedPortOfDestination, ffmMessage.AirportOfDestinationCode.IATACode);

				AssertEquals("Regulated Agent ID is wrong.", regulatedAgentID, ffmMessage.RegulatedAgentID);
				AssertEquals("Regulated Agent Country is wrong.", approvalCountryCode, ffmMessage.RegulatedAgentCountry);

				AssertEquals("Cargo secure status is wrong.", consolSecurityStatus, ffmMessage.SecurityStatusCode);
			});
		}

		public void TestBuildFFMMessageWithCorrectContainerDetails()
		{
			var ffmDetails = CreateTestData(containers: new() { containerDetails1 });

			var ffmMessageBuilder = new FFMMessageBuilder(ffmDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			var containerDetails = ffmMessage.Packages.FirstOrDefault(c => c.ContainerNumber == containerID1);

			AssertNotNull("Container 1", containerDetails);

			CombineAssertions("The container details are incorrect.", () =>
			{
				AssertEquals("Container Number is wrong", containerID1, containerDetails.ContainerNumber);
				AssertEquals("Container Way Billing Number is wrong", wayBillNumber, containerDetails.WayBillNumber);
				AssertEquals("Container Port of Destination is wrong", expectedPortOfDestination, containerDetails.PortOfDestination.IATACode);
				AssertEquals("Container Port of Loading is wrong", expectedPortOfLoading, containerDetails.PortOfLoading.IATACode);
				AssertEquals("Container Goods Description is wrong", goodsDescription, containerDetails.GoodsDescription);

				AssertEquals("Container Quantity is wrong", container1SubPackage1Quantity, containerDetails.Quantity);
				AssertEquals("Container Weight is wrong", container1SubPackage1Weight, containerDetails.Weight);
				AssertEquals("Container Weight Metric is wrong", container1SubPackage1WeightUnit, containerDetails.WeightMetric);
				AssertEquals("Container Volume is wrong", container1SubPackage1Volume, containerDetails.Volume);
				AssertEquals("Container Volume Metric is wrong", container1SubPackage1VolumeUnit, containerDetails.VolumeMetric);
			});
		}

		public void TestBuildFFMMessageWithMultipleContainers()
		{
			var ffmDetails = CreateTestData(containers: new() { containerDetails1, containerDetails2 });

			var ffmMessageBuilder = new FFMMessageBuilder(ffmDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			var container1Details = ffmMessage.Packages.FirstOrDefault(c => c.ContainerNumber == containerID1);
			AssertNotNull("Container 1", container1Details);

			CombineAssertions("The container 1 details are incorrect.", () =>
			{
				AssertEquals("Container 1 Number is wrong", containerID1, container1Details.ContainerNumber);
				AssertEquals("Container 1 Way Billing Number is wrong", wayBillNumber, container1Details.WayBillNumber);
				AssertEquals("Container 1 Port of Destination is wrong", expectedPortOfDestination, container1Details.PortOfDestination.IATACode);
				AssertEquals("Container 1 Port of Loading is wrong", expectedPortOfLoading, container1Details.PortOfLoading.IATACode);
				AssertEquals("Container 1 Goods Description is wrong", goodsDescription, container1Details.GoodsDescription);

				AssertEquals("Container 1 Quantity is wrong", container1SubPackage1Quantity, container1Details.Quantity);
				AssertEquals("Container 1 Weight is wrong", container1SubPackage1Weight, container1Details.Weight);
				AssertEquals("Container 1 Weight Metric is wrong", container1SubPackage1WeightUnit, container1Details.WeightMetric);
				AssertEquals("Container 1 Volume is wrong", container1SubPackage1Volume, container1Details.Volume);
				AssertEquals("Container 1 Volume Metric is wrong", container1SubPackage1VolumeUnit, container1Details.VolumeMetric);
			});

			var container2SubPackage1Details = ffmMessage.Packages.FirstOrDefault(c => c.ContainerNumber == containerID2 && c.Quantity == container2SubPackage1Quantity);
			AssertNotNull("Container 2 sub package 1", container2SubPackage1Details);

			CombineAssertions("The container 2 sub package 1 details are incorrect.", () =>
			{
				AssertEquals("Container 2 sub package 1 is wrong", containerID2, container2SubPackage1Details.ContainerNumber);
				AssertEquals("Container 2 sub package 1 Way Billing Number is wrong", wayBillNumber, container2SubPackage1Details.WayBillNumber);
				AssertEquals("Container 2 sub package 1 Port of Destination is wrong", expectedPortOfDestination, container2SubPackage1Details.PortOfDestination.IATACode);
				AssertEquals("Container 2 sub package 1 Port of Loading is wrong", expectedPortOfLoading, container2SubPackage1Details.PortOfLoading.IATACode);
				AssertEquals("Container 2 sub package 1 Goods Description is wrong", goodsDescription, container2SubPackage1Details.GoodsDescription);

				AssertEquals("Container 2 sub package 1 Quantity is wrong", container2SubPackage1Quantity, container2SubPackage1Details.Quantity);
				AssertEquals("Container 2 sub package 1 Weight is wrong", container2SubPackage1Weight, container2SubPackage1Details.Weight);
				AssertEquals("Container 2 sub package 1 Weight Metric is wrong", container2SubPackage1WeightUnit, container2SubPackage1Details.WeightMetric);
				AssertEquals("Container 2 sub package 1 Volume is wrong", container2SubPackage1Volume, container2SubPackage1Details.Volume);
				AssertEquals("Container 2 sub package 1 Volume Metric is wrong", container2SubPackage1VolumeUnit, container2SubPackage1Details.VolumeMetric);
			});

			var container2SubPackage2Details = ffmMessage.Packages.FirstOrDefault(c => c.ContainerNumber == containerID2 && c.Quantity == container2SubPackage2Quantity);
			AssertNotNull("Container 2 sub package 2", container2SubPackage2Details);

			CombineAssertions("The container 2 sub package 2 details are incorrect.", () =>
			{
				AssertEquals("Container 2 sub package 2 is wrong", containerID2, container2SubPackage2Details.ContainerNumber);
				AssertEquals("Container 2 sub package 2 Way Billing Number is wrong", wayBillNumber, container2SubPackage2Details.WayBillNumber);
				AssertEquals("Container 2 sub package 2 Port of Destination is wrong", expectedPortOfDestination, container2SubPackage2Details.PortOfDestination.IATACode);
				AssertEquals("Container 2 sub package 2 Port of Loading is wrong", expectedPortOfLoading, container2SubPackage2Details.PortOfLoading.IATACode);
				AssertEquals("Container 2 sub package 2 Goods Description is wrong", goodsDescription, container2SubPackage2Details.GoodsDescription);

				AssertEquals("Container 2 sub package 2 Quantity is wrong", container2SubPackage2Quantity, container2SubPackage2Details.Quantity);
				AssertEquals("Container 2 sub package 2 Weight is wrong", container2SubPackage2Weight, container2SubPackage2Details.Weight);
				AssertEquals("Container 2 sub package 2 Weight Metric is wrong", container2SubPackage2WeightUnit, container2SubPackage2Details.WeightMetric);
				AssertEquals("Container 2 sub package 2 Volume is wrong", container2SubPackage2Volume, container2SubPackage2Details.Volume);
				AssertEquals("Container 2 sub package 2 Volume Metric is wrong", container2SubPackage2VolumeUnit, container2SubPackage2Details.VolumeMetric);
			});
		}

		public void TestBuildFFMMessageWithLoosePackage()
		{
			var ffmDetails = CreateTestData(loosePackages: new() { loosePackageDetails1 });

			var ffmMessageBuilder = new FFMMessageBuilder(ffmDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			var nonLoosePackage = ffmMessage.Packages.FirstOrDefault(c => c.ContainerNumber != ZString.Empty);
			AssertNull($"Non-loose package", nonLoosePackage);

			var loosePackage = ffmMessage.Packages.FirstOrDefault();
			AssertNotNull("Loose package", loosePackage);

			CombineAssertions("Loose package details are incorrect", () =>
			{
				AssertEquals("Loose packages should not have a container number.", ZString.Empty, loosePackage.ContainerNumber);
				AssertEquals("Loose package Way Billing Number is wrong", wayBillNumber, loosePackage.WayBillNumber);
				AssertEquals("Loose package Port of Destination is wrong", expectedPortOfDestination, loosePackage.PortOfDestination.IATACode);
				AssertEquals("Loose package Port of Loading is wrong", expectedPortOfLoading, loosePackage.PortOfLoading.IATACode);
				AssertEquals("Loose package Goods Description is wrong", goodsDescription, loosePackage.GoodsDescription);

				AssertEquals("Loose package Quantity is wrong", loosePackage1Quantity, loosePackage.Quantity);
				AssertEquals("Loose package Weight is wrong", loosePackage1Weight, loosePackage.Weight);
				AssertEquals("Loose package Weight Metric is wrong", loosePackage1WeightUnit, loosePackage.WeightMetric);
				AssertEquals("Loose package Volume is wrong", loosePackage1Volume, loosePackage.Volume);
				AssertEquals("Loose package Volume Metric is wrong", loosePackage1VolumeUnit, loosePackage.VolumeMetric);
			});
		}

		IFFMDetails CreateTestData(
			List<TestContainerData> containers = null,
			List<TestPackageData> loosePackages = null
			)
		{
			var booking = CreateDtbBookingWithShipmentConsolidation();

			var selectedConfirmation = CreateConfirmationWithPackagesOnBooking(
				booking: booking,
				containers: containers,
				loosePackages: loosePackages);

			return new DtbBookingConfirmationFFMDetailsProvider(selectedConfirmation);
		}

		DtbBooking CreateDtbBookingWithShipmentConsolidation()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = shipmentOrigin;
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = shipmentDestination;
			bookingShipment[JobShipmentSchema.JS_RL_NKLoadPort] = shipmentOrigin;
			bookingShipment[JobShipmentSchema.JS_RL_NKDischargePort] = shipmentDestination;

			var bookingConsol = Factory.New<IForwardingConsol>();
			bookingConsol.AddShipment((IForwardingShipment)bookingShipment);

			var consolBusinessObject = (BusinessObject)bookingConsol;
			consolBusinessObject[JobConsolSchema.JK_RL_NKLoadPort] = shipmentOrigin;
			consolBusinessObject[JobConsolSchema.JK_RL_NKDischargePort] = shipmentDestination;
			consolBusinessObject[JobConsolSchema.JK_TransportMode] = Core.Constants.TransportModes.Air;
			consolBusinessObject[JobConsolSchema.JK_OverrideWaybillDefaults] = ZBool.True;

			var awbSpecialHandlingItems = ((IBusinessObjectCollection)(consolBusinessObject)["AWBSpecialHandlingItems"]).AddNew();
			awbSpecialHandlingItems[JobConsolAWBSpecialHandlingSchema.JKH_Code] = "SPX";

			var voyage = (BusinessObject)Factory.New<IJobVoyage>();
			voyage[JobVoyageSchema.JV_RV_NKVessel] = vessel;
			voyage[JobVoyageSchema.JV_VoyageFlight] = voyageNumber;
			var origin = (BusinessObject)Factory.New<IVoyageOrigin>();
			origin[JobVoyOriginSchema.JA_E_DEP] = leaveDate;
			origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = shipmentOrigin;

			var destination = (BusinessObject)Factory.New<IVoyageDestination>();
			destination[JobVoyDestinationSchema.JB_E_ARV] = arriveDate;
			destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = shipmentDestination;

			var sailing = (BusinessObject)Factory.New<IJobSailing>();
			sailing[JobSailingSchema.JX_JA] = origin.PK;
			sailing[JobSailingSchema.JX_JB] = destination.PK;

			var transport = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)bookingConsol)["Transports"])[0];
			transport[JobConsolTransportSchema.JW_RL_NKDiscPort] = shipmentOrigin;
			transport[JobConsolTransportSchema.JW_RL_NKLoadPort] = shipmentDestination;
			transport[JobConsolTransportSchema.JW_ETD] = leaveDate;
			transport[JobConsolTransportSchema.JW_VoyageFlight] = voyageNumber;
			transport[JobConsolTransportSchema.JW_ETA] = arriveDate;
			transport[JobConsolTransportSchema.JW_JX] = sailing.PK;

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)consolBusinessObject);
			tbConsol.KB_GoodsDescription = goodsDescription;
			tbConsol.KB_JobDirection = direction;

			var booking = Helper.CreateBooking(tbConsol);

			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.HouseBill, wayBillNumber);
			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.MasterBill, wayBillNumber);

			var awbHeader = (BusinessObject)consolBusinessObject["AWBHeader"];
			awbHeader[ExportAWBHeaderSchema.EH_ParentID] = bookingConsol.PK;
			awbHeader[ExportAWBHeaderSchema.EH_AgentApprovalNumber] = regulatedAgentID;
			awbHeader[ExportAWBHeaderSchema.EH_RN_NKAgentApprovalCountryCode] = approvalCountryCode;

			Factory.Save();
			return booking;
		}

		DtbBookingConfirmation CreateConfirmationWithPackagesOnBooking(
			DtbBooking booking,
			List<TestContainerData> containers = null,
			List<TestPackageData> loosePackages = null)
		{
			var packageJob = booking.PackageJob;
			List<PkgPackage> pkgPackages = new();

			if (containers != null)
			{
				foreach (var container in containers)
				{
					var pkgPackage = Helper.CreatePackage(container.ContainerID, 1, "CNT");
					pkgPackage.Container.K0_RC_ContainerType = Helper.LoadRefContainer(container.ContainerType).PK;
					pkgPackage.KP_KJ_ParentPackageJob = packageJob.PK;
					pkgPackages.Add(pkgPackage);

					foreach (var containerSubPackage in container.SubPackages)
					{
						var innerPkgPackage = Helper.CreatePackage(ZString.Empty, containerSubPackage.Quantity, containerSubPackage.Type);
						pkgPackage.Packages.Add(innerPkgPackage);
						innerPkgPackage.KP_Weight = containerSubPackage.Weight;
						innerPkgPackage.KP_WeightUQ = containerSubPackage.WeightMetric;
						innerPkgPackage.KP_Volume = containerSubPackage.Volume;
						innerPkgPackage.KP_VolumeUQ = containerSubPackage.VolumeMetric;
					}
				}
			}

			if (loosePackages != null)
			{
				foreach (var loosePackage in loosePackages)
				{
					var pkgPackage = Helper.CreatePackage(ZString.Empty, loosePackage.Quantity, loosePackage.Type);
					pkgPackage.KP_KJ_ParentPackageJob = packageJob.PK;
					pkgPackage.KP_Weight = loosePackage.Weight;
					pkgPackage.KP_WeightUQ = loosePackage.WeightMetric;
					pkgPackage.KP_Volume = loosePackage.Volume;
					pkgPackage.KP_VolumeUQ = loosePackage.VolumeMetric;
					pkgPackages.Add(pkgPackage);
				}
			}

			var orgType = "CFS";
			var instruction = Helper.CreateInstruction(booking, direction, orgType, null);

			foreach (var pkgPackage in pkgPackages)
			{
				_ = Helper.CreatePackageDivot(instruction, pkgPackage);
			}

			var confirmation = Helper.CreateConfirmation(instruction, direction);
			confirmation.KK_DocumentID = documentID;

			Factory.Save();
			return confirmation;
		}

		public void TestNoErrorMessageOnValidFFMDetails()
		{
			var ffmMessageBuilder = new FFMMessageBuilder(validFFMDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			Assert("The FFM Details are valid but the FFM Message builder returned an error.", !ffmMessage.HasErrors);
		}

		public void TestErrorMessagesProvidedOnInvalidFFMDetails()
		{
			var ffmMessageBuilder = new FFMMessageBuilder(badFFMDetailsMissingDetails, context);
			var ffmMessage = ffmMessageBuilder.Build();

			Assert("The FFM Details are invalid but the FFM Message builder did not indicate that.", ffmMessage.HasErrors);
			var errorMessages = ffmMessage.GetErrors();

			var voyageFlightErrorExist = errorMessages.Any(e => e.Message.Contains("Missing the Voyage Flight Number."));
			var flightDateErrorExist = errorMessages.Any(e => e.Message.Contains("Missing the Flight Date."));
			var regulatedAgentErrorExist = errorMessages.Any(e => e.Message.Contains("Missing the ID of the Regulated Agent."));
			var portOfDestinationErrorExist = errorMessages.Any(e => e.Message.Contains("Missing The Port of Loading for this FFM Message."));
			var portOfAirportCodeErrorExist = errorMessages.Any(e => e.Message.Contains("Missing The Airport Code for this FFM Message."));

			var packageWayBillNumberErrorExist = errorMessages.Any(e => e.Message.Contains("A package is missing its Way Bill Number."));
			var packageGoodsDescriptionErrorExist = errorMessages.Any(e => e.Message.Contains("A package is missing the descriptions for its goods."));
			var packageQuantityErrorExist = errorMessages.Any(e => e.Message.Contains("A package's quantity must be set above zero."));
			var packageWeightErrorExist = errorMessages.Any(e => e.Message.Contains("A package's volume must be set above zero."));
			var packageWeightMetricErrorExist = errorMessages.Any(e => e.Message.Contains("A package's weight did not specify a metric."));
			var packageVolumeErrorExist = errorMessages.Any(e => e.Message.Contains("A package's weight must be set above zero."));
			var packageVolumeMetricErrorExist = errorMessages.Any(e => e.Message.Contains("A package's volume did not specify a metric."));
			var packagePortOfLoadingErrorExist = errorMessages.Any(e => e.Message.Contains("Missing a package's Port of Loading."));
			var packagePortOfDestinationErrorExist = errorMessages.Any(e => e.Message.Contains("Missing a package's Port of Destination."));

			CombineAssertions("Missing reported error for FFM Message", () =>
			{
				Assert("Error about missing voyage flight number was not reported.", voyageFlightErrorExist);
				Assert("Error about missing flight date was not reported.", flightDateErrorExist);
				Assert("Error about missing Regulated Agent ID was not reported.", regulatedAgentErrorExist);
				Assert("Error about missing Port of Loading was not reported.", portOfDestinationErrorExist);
				Assert("Error about missing Airport Code was not reported.", portOfAirportCodeErrorExist);

				Assert("Error about package missing Way Bill Number not reported.", packageWayBillNumberErrorExist);
				Assert("Error about package missing Goods Description not reported.", packageGoodsDescriptionErrorExist);
				Assert("Error about package missing Quantity not reported.", packageQuantityErrorExist);
				Assert("Error about package missing Weight not reported.", packageWeightErrorExist);
				Assert("Error about package missing Weight Metric not reported.", packageWeightMetricErrorExist);
				Assert("Error about package missing Volume not reported.", packageVolumeErrorExist);
				Assert("Error about package missing Volume Metric not reported.", packageVolumeMetricErrorExist);
				Assert("Error about package missing Port of Loading was not reported.", packagePortOfLoadingErrorExist);
				Assert("Error about package missing Port of Destination was not reported.", packagePortOfDestinationErrorExist);
			});
		}

		public void TestFFMMessageMissingPackagesGeneratesError()
		{
			var ffmMessageBuilder = new FFMMessageBuilder(ffmDetailsWithoutPackages, context);
			var ffmMessage = ffmMessageBuilder.Build();

			Assert("The FFM Details are invalid but the FFM Message builder did not indicate that.", ffmMessage.HasErrors);
			var errorMessages = ffmMessage.GetErrors();

			var noPackagesErrorExist = errorMessages.Any(e => e.Message.Contains("FFM message has no packages."));
			Assert("Error Generated FFM Message not having any packages was not reported.", noPackagesErrorExist);
		}
	}
}
