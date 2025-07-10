using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed class DtbBookingConfirmationFFMDetailsProviderTest : DtbBookingTestCaseWithFactory
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
		readonly string direction = "DLV";
		readonly string securityStatusCode = "DIP";

		readonly ZDate leaveDate = new(2077, 1, 1);
		readonly ZDate arriveDate = new(2077, 1, 2);

		readonly string packagePackageType = "PKG";
		readonly string containerType = "AMA-2Q";

		readonly string containerID1 = "ContainerWithOneSubPackage";

		readonly ZInt container1SubPackage1Quantity = 1;
		readonly ZDecimal container1SubPackage1Volume = 7.5m;
		readonly ZString container1SubPackage1VolumeMetric = "M3";
		readonly ZDecimal container1SubPackage1Weight = 55.5m;
		readonly ZString container1SubPackage1WeightMetric = "KG";

		TestContainerData containerDetails1;

		readonly string containerID2 = "ContainerWithTwoSubPackage";

		readonly ZInt container2SubPackage1Quantity = 1;
		readonly ZDecimal container2SubPackage1Volume = 20.0m;
		readonly ZString container2SubPackage1VolumeMetric = "CY";
		readonly ZDecimal container2SubPackage1Weight = 1000.0m;
		readonly ZString container2SubPackage1WeightMetric = "LB";

		readonly ZInt container2SubPackage2Quantity = 2;
		readonly ZDecimal container2SubPackage2Volume = 70.0m;
		readonly ZString container2SubPackage2VolumeMetric = "CY";
		readonly ZDecimal container2SubPackage2Weight = 4000.0m;
		readonly ZString container2SubPackage2WeightMetric = "LB";

		TestContainerData containerDetails2;

		readonly ZInt loosePackage1Quantity = 5;
		readonly ZDecimal loosePackage1Volume = 50.0m;
		readonly ZString loosePackage1VolumeMetric = "M3";
		readonly ZDecimal loosePackage1Weight = 500.0m;
		readonly ZString loosePackage1WeightMetric = "KG";

		TestPackageData loosePackageDetails1;

		readonly ZInt loosePackage2Quantity = 8;
		readonly ZDecimal loosePackage2Volume = 80.0m;
		readonly ZString loosePackage2VolumeMetric = "M3";
		readonly ZDecimal loosePackage2Weight = 800.0m;
		readonly ZString loosePackage2WeightMetric = "KG";

		TestPackageData loosePackageDetails2;

		readonly ZInt loosePackage3Quantity = 13;
		readonly ZDecimal loosePackage3Volume = 130.0m;
		readonly ZString loosePackage3VolumeMetric = "M3";
		readonly ZDecimal loosePackage3Weight = 1300.0m;
		readonly ZString loosePackage3WeightMetric = "KG";

		TestPackageData loosePackageDetails3;

		readonly string expectedPortOfLoading = "MXP";
		readonly string expectedPortOfDestination = "BKK";

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
						VolumeMetric = container1SubPackage1VolumeMetric,
						Weight = container1SubPackage1Weight,
						WeightMetric = container1SubPackage1WeightMetric,
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
						VolumeMetric = container2SubPackage1VolumeMetric,
						Weight = container2SubPackage1Weight,
						WeightMetric = container2SubPackage1WeightMetric,
					},
					new TestPackageData()
					{
						Quantity = container2SubPackage2Quantity,
						Type = packagePackageType,
						Volume = container2SubPackage2Volume,
						VolumeMetric = container2SubPackage2VolumeMetric,
						Weight = container2SubPackage2Weight,
						WeightMetric = container2SubPackage2WeightMetric,
					}
				},
			};

			loosePackageDetails1 = new()
			{
				Quantity = loosePackage1Quantity,
				Type = packagePackageType,
				Volume = loosePackage1Volume,
				VolumeMetric = loosePackage1VolumeMetric,
				Weight = loosePackage1Weight,
				WeightMetric = loosePackage1WeightMetric,
			};

			loosePackageDetails2 = new()
			{
				Quantity = loosePackage2Quantity,
				Type = packagePackageType,
				Volume = loosePackage2Volume,
				VolumeMetric = loosePackage2VolumeMetric,
				Weight = loosePackage2Weight,
				WeightMetric = loosePackage2WeightMetric,
			};

			loosePackageDetails3 = new()
			{
				Quantity = loosePackage3Quantity,
				Type = packagePackageType,
				Volume = loosePackage3Volume,
				VolumeMetric = loosePackage3VolumeMetric,
				Weight = loosePackage3Weight,
				WeightMetric = loosePackage3WeightMetric,
			};
		}

		public void TestGenerateFFMDetailsFromDtbBookingWithForwardingConsolParentAndSingleConfirmation()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingConsol, containers: new() { containerDetails1 });

			CombineAssertions("Generated FFM details from confirmation (booking parent is Forwarding Consol, Goods Description not blank) appears to be invalid", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong RegulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertGreaterThan("No packages.", ffmDetails.Packages.Count, 0);
				AssertCollectionContains("Missing Correct Container ID.", containerID1, ffmDetails.Packages.Select(p => p.ContainerNumber));
				AssertCollectionContains("Missing Correct Goods Description.", goodsDescription, ffmDetails.Packages.Select(p => p.GoodsDescription));
				AssertCollectionContains("Missing Correct Container weight.", container1SubPackage1Weight, ffmDetails.Packages.Select(p => p.Weight));
				AssertCollectionContains("Missing Correct Container weight metric.", container1SubPackage1WeightMetric, ffmDetails.Packages.Select(p => p.WeightMetric));
				AssertCollectionContains("Missing Correct Container volume.", container1SubPackage1Volume, ffmDetails.Packages.Select(p => p.Volume));
				AssertCollectionContains("Missing Correct Container volume metric.", container1SubPackage1VolumeMetric, ffmDetails.Packages.Select(p => p.VolumeMetric));
				AssertCollectionContains("Missing Correct Container port loading.", expectedPortOfLoading, ffmDetails.Packages.Select(p => p.PortOfLoading.RL_IATA));
				AssertCollectionContains("Missing Correct Container port discharge.", expectedPortOfDestination, ffmDetails.Packages.Select(p => p.PortOfDestination.RL_IATA));
				AssertCollectionContains("Missing Correct Container Way Bill Number.", wayBillNumber, ffmDetails.Packages.Select(p => p.WayBillNumber));
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingWithForwardingConsolParentBlankGoodsDescriptionAndSingleConfirmation()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingConsol, containers: new() { containerDetails1 }, blankGoodsDescription: true);

			CombineAssertions("Generated FFM details from confirmation (booking parent is Forwarding Consol, Goods Description blank) appears to be invalid", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong RegulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertGreaterThan("No packages.", ffmDetails.Packages.Count, 0);
				AssertCollectionContains("Missing Correct Container ID.", containerID1, ffmDetails.Packages.Select(p => p.ContainerNumber));
				AssertCollectionContains("Missing Correct Goods Description.", "CONSOL", ffmDetails.Packages.Select(p => p.GoodsDescription));
				AssertCollectionContains("Missing Correct Container weight.", container1SubPackage1Weight, ffmDetails.Packages.Select(p => p.Weight));
				AssertCollectionContains("Missing Correct Container weight metric.", container1SubPackage1WeightMetric, ffmDetails.Packages.Select(p => p.WeightMetric));
				AssertCollectionContains("Missing Correct Container volume.", container1SubPackage1Volume, ffmDetails.Packages.Select(p => p.Volume));
				AssertCollectionContains("Missing Correct Container volume metric.", container1SubPackage1VolumeMetric, ffmDetails.Packages.Select(p => p.VolumeMetric));
				AssertCollectionContains("Missing Correct Container port loading.", expectedPortOfLoading, ffmDetails.Packages.Select(p => p.PortOfLoading.RL_IATA));
				AssertCollectionContains("Missing Correct Container port discharge.", expectedPortOfDestination, ffmDetails.Packages.Select(p => p.PortOfDestination.RL_IATA));
				AssertCollectionContains("Missing Correct Container Way Bill Number.", wayBillNumber, ffmDetails.Packages.Select(p => p.WayBillNumber));
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingWithForwardingShipmentParentAndSingleConfirmation()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingShipment, containers: new() { containerDetails1 });

			CombineAssertions("Generated FFM details from confirmation (booking parent is Forwarding Shipment, Goods Description not blank) appears to be invalid", () =>
			{
				AssertEquals("Voyage number should be empty.", ZString.Empty, ffmDetails.VoyageFlightNo);
				AssertNull("Loading port should be null.", ffmDetails.PortOfLoading);
				AssertEquals("Flight date should be empty.", ZDateTime.Empty, ffmDetails.FlightDate);
				AssertNull("Destination port should be null.", ffmDetails.AirportOfDestinationCode);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", ZString.Empty, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", ZString.Empty, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", ZString.Empty, ffmDetails.SecurityStatusCode);
				AssertGreaterThan("No packages.", ffmDetails.Packages.Count, 0);
				AssertCollectionContains("Missing Correct Container ID.", containerID1, ffmDetails.Packages.Select(p => p.ContainerNumber));
				AssertCollectionContains("Missing Correct Goods Description.", goodsDescription, ffmDetails.Packages.Select(p => p.GoodsDescription));
				AssertCollectionContains("Missing Correct Container weight.", container1SubPackage1Weight, ffmDetails.Packages.Select(p => p.Weight));
				AssertCollectionContains("Missing Correct Container weight metric.", container1SubPackage1WeightMetric, ffmDetails.Packages.Select(p => p.WeightMetric));
				AssertCollectionContains("Missing Correct Container volume.", container1SubPackage1Volume, ffmDetails.Packages.Select(p => p.Volume));
				AssertCollectionContains("Missing Correct Container volume metric.", container1SubPackage1VolumeMetric, ffmDetails.Packages.Select(p => p.VolumeMetric));
				AssertCollectionContains("Missing Container with null port of loading.", null, ffmDetails.Packages.Select(p => p.PortOfLoading));
				AssertCollectionContains("Missing Container with null port of discharge.", null, ffmDetails.Packages.Select(p => p.PortOfDestination));
				AssertCollectionContains("Missing Correct Container Way Bill Number.", wayBillNumber, ffmDetails.Packages.Select(p => p.WayBillNumber));
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingWithForwardingShipmentParentBlankGoodsDescriptionAndSingleConfirmation()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingShipment, containers: new() { containerDetails1 }, blankGoodsDescription: true);

			CombineAssertions("Generated FFM details from confirmation (booking parent is Forwarding Shipment, Goods Description blank) appears to be invalid", () =>
			{
				AssertEquals("Voyage number should be empty.", ZString.Empty, ffmDetails.VoyageFlightNo);
				AssertNull("Loading port should be null.", ffmDetails.PortOfLoading);
				AssertEquals("Flight date should be empty.", ZDateTime.Empty, ffmDetails.FlightDate);
				AssertNull("Destination port should be null.", ffmDetails.AirportOfDestinationCode);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", ZString.Empty, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", ZString.Empty, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", ZString.Empty, ffmDetails.SecurityStatusCode);
				AssertGreaterThan("No packages.", ffmDetails.Packages.Count, 0);
				AssertCollectionContains("Missing Correct Container ID.", containerID1, ffmDetails.Packages.Select(p => p.ContainerNumber));
				AssertCollectionContains("Missing Correct Goods Description.", "OTHER", ffmDetails.Packages.Select(p => p.GoodsDescription));
				AssertCollectionContains("Missing Correct Container weight.", container1SubPackage1Weight, ffmDetails.Packages.Select(p => p.Weight));
				AssertCollectionContains("Missing Correct Container weight metric.", container1SubPackage1WeightMetric, ffmDetails.Packages.Select(p => p.WeightMetric));
				AssertCollectionContains("Missing Correct Container volume.", container1SubPackage1Volume, ffmDetails.Packages.Select(p => p.Volume));
				AssertCollectionContains("Missing Correct Container volume metric.", container1SubPackage1VolumeMetric, ffmDetails.Packages.Select(p => p.VolumeMetric));
				AssertCollectionContains("Missing Container with null port of loading.", null, ffmDetails.Packages.Select(p => p.PortOfLoading));
				AssertCollectionContains("Missing Container with null port of discharge.", null, ffmDetails.Packages.Select(p => p.PortOfDestination));
				AssertCollectionContains("Missing Correct Container Way Bill Number.", wayBillNumber, ffmDetails.Packages.Select(p => p.WayBillNumber));
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingWithForwardingConsolParentAndMultipleConfirmations()
		{
			var ffmDetails = CreateTestDataWithMultipleConfirmations(
				selectedConfirmationContainers: new() { containerDetails1 },
				nonSelectConfirmationContainers: new() { containerDetails2 });

			CombineAssertions("Generated FFM details from confirmation appears to be invalid", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertGreaterThan("No packages.", ffmDetails.Packages.Count, 0);
				AssertCollectionContains("Missing Correct Container ID.", containerID1, ffmDetails.Packages.Select(p => p.ContainerNumber));
				AssertCollectionContains("Missing Correct Goods Description.", goodsDescription, ffmDetails.Packages.Select(p => p.GoodsDescription));
				AssertCollectionContains("Missing Correct Container weight.", container1SubPackage1Weight, ffmDetails.Packages.Select(p => p.Weight));
				AssertCollectionContains("Missing Correct Container weight metric.", container1SubPackage1WeightMetric, ffmDetails.Packages.Select(p => p.WeightMetric));
				AssertCollectionContains("Missing Correct Container volume.", container1SubPackage1Volume, ffmDetails.Packages.Select(p => p.Volume));
				AssertCollectionContains("Missing Correct Container volume metric.", container1SubPackage1VolumeMetric, ffmDetails.Packages.Select(p => p.VolumeMetric));
				AssertCollectionContains("Missing Correct Container port loading.", expectedPortOfLoading, ffmDetails.Packages.Select(p => p.PortOfLoading.RL_IATA));
				AssertCollectionContains("Missing Correct Container port discharge.", expectedPortOfDestination, ffmDetails.Packages.Select(p => p.PortOfDestination.RL_IATA));
				AssertCollectionContains("Missing Correct Container Way Bill Number.", wayBillNumber, ffmDetails.Packages.Select(p => p.WayBillNumber));
			});

			CombineAssertions("Generated FFM details appears to have details for a confirmation that was not selected for the FFM message", () =>
			{
				AssertEquals($"FFM Message is expected to only have {containerDetails1.SubPackages.Count} packages", containerDetails1.SubPackages.Count, ffmDetails.Packages.Count);
				AssertCollectionNotContains($"FFM should not contain a container with ID: {containerID2}", containerID2, ffmDetails.Packages.Select(p => p.ContainerNumber));
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingConfirmationLoosePackages()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingConsol, loosePackages: new() { loosePackageDetails1, loosePackageDetails2, loosePackageDetails3 });

			CombineAssertions("Generated FFM details from confirmation appears to be invalid", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertEquals("No packages is expected to be 3.", 3, ffmDetails.Packages.Count);
			});

			var expectedLoosePackage1 = ffmDetails.Packages.FirstOrDefault(p => p.Quantity == loosePackage1Quantity && p.Volume == loosePackage1Volume && p.Weight == loosePackage1Weight);
			AssertNotNull("Loose package 1 is missing", expectedLoosePackage1);

			var expectedLoosePackage2 = ffmDetails.Packages.FirstOrDefault(p => p.Quantity == loosePackage2Quantity && p.Volume == loosePackage2Volume && p.Weight == loosePackage2Weight);
			AssertNotNull("Loose package 2 is missing", expectedLoosePackage2);

			var expectedLoosePackage3 = ffmDetails.Packages.FirstOrDefault(p => p.Quantity == loosePackage3Quantity && p.Volume == loosePackage3Volume && p.Weight == loosePackage3Weight);
			AssertNotNull("Loose package 3 is missing", expectedLoosePackage3);
		}

		public void TestGenerateFFMDetailsFromDtbBookingConfirmationWithContainerAndLoosePackages()
		{
			var ffmDetails = CreateTestData(
				parentType: BookingParentType.ForwardingConsol,
				containers: new() { containerDetails1 },
				loosePackages: new() { loosePackageDetails1 });

			CombineAssertions("Generated FFM details from confirmation appears to be invalid.", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertEquals("No packages is expected to be 2.", 2, ffmDetails.Packages.Count);
			});

			var expectedContainerPackage = ffmDetails.Packages.FirstOrDefault(p => p.ContainerNumber == containerID1);
			AssertNotNull("Missing container.", expectedContainerPackage);

			CombineAssertions("Container package details are incorrect.", () =>
			{
				AssertEquals("Wrong Goods Description.", goodsDescription, expectedContainerPackage.GoodsDescription);
				AssertEquals("Wrong Container Quantity.", container1SubPackage1Quantity, expectedContainerPackage.Quantity);
				AssertEquals("Wrong Container weight.", container1SubPackage1Weight, expectedContainerPackage.Weight);
				AssertEquals("Wrong Container volume.", container1SubPackage1Volume, expectedContainerPackage.Volume);
				AssertEquals("Wrong Container port loading.", expectedPortOfLoading, expectedContainerPackage.PortOfLoading.RL_IATA);
				AssertEquals("Wrong Container port discharge.", expectedPortOfDestination, expectedContainerPackage.PortOfDestination.RL_IATA);
				AssertEquals("Wrong Container Way Bill Number.", wayBillNumber, expectedContainerPackage.WayBillNumber);
			});

			var expectedLoosePackage = ffmDetails.Packages.FirstOrDefault(p => p.ContainerNumber == ZString.Empty);
			AssertNotNull("Missing Loose Package.", expectedLoosePackage);

			CombineAssertions("container package details are incorrect", () =>
			{
				AssertEquals("Wrong Goods Description.", goodsDescription, expectedLoosePackage.GoodsDescription);
				AssertEquals("Wrong Loose Package Quantity.", loosePackage1Quantity, expectedLoosePackage.Quantity);
				AssertEquals("Wrong Loose Package weight.", loosePackage1Weight, expectedLoosePackage.Weight);
				AssertEquals("Wrong Loose Package weight metric.", loosePackage1WeightMetric, expectedLoosePackage.WeightMetric);
				AssertEquals("Wrong Loose Package volume.", loosePackage1Volume, expectedLoosePackage.Volume);
				AssertEquals("Wrong Loose Package volume metric.", loosePackage1VolumeMetric, expectedLoosePackage.VolumeMetric);
				AssertEquals("Wrong Loose Package port loading.", expectedPortOfLoading, expectedLoosePackage.PortOfLoading.RL_IATA);
				AssertEquals("Wrong Loose Package port discharge.", expectedPortOfDestination, expectedLoosePackage.PortOfDestination.RL_IATA);
				AssertEquals("Wrong Loose Package Way Bill Number.", wayBillNumber, expectedLoosePackage.WayBillNumber);
			});
		}

		public void TestGenerateFFMDetailsFromDtbBookingConfirmationWithContainerContainingMultipleSubPackages()
		{
			var ffmDetails = CreateTestData(parentType: BookingParentType.ForwardingConsol, containers: new() { containerDetails2 });

			CombineAssertions("Generated FFM details from confirmation appears to be invalid", () =>
			{
				AssertEquals("Wrong voyage number.", voyageNumber, ffmDetails.VoyageFlightNo);
				AssertEquals("Wrong loading port.", expectedPortOfLoading, ffmDetails.PortOfLoading.RL_IATA);
				AssertEquals("Wrong flight date.", leaveDate, ffmDetails.FlightDate);
				AssertEquals("Wrong destination.", expectedPortOfDestination, ffmDetails.AirportOfDestinationCode.RL_IATA);
				AssertEquals("Wrong DriverDocumentID", documentID, ffmDetails.DriverDocumentID);
				AssertEquals("Wrong regulatedAgentID", regulatedAgentID, ffmDetails.RegulatedAgentID);
				AssertEquals("Wrong RegulatedAgentCountry", approvalCountryCode, ffmDetails.RegulatedAgentCountry);
				AssertEquals("Wrong SecurityStatusCode", securityStatusCode, ffmDetails.SecurityStatusCode);
				AssertEquals("Number of packages is expected to be 2.", 2, ffmDetails.Packages.Count);
			});

			var expectedSubPackage1 = ffmDetails.Packages.FirstOrDefault(p => p.Quantity == container2SubPackage1Quantity && p.Volume == container2SubPackage1Volume && p.Weight == container2SubPackage1Weight);
			AssertNotNull("Sub Package 1 is missing", expectedSubPackage1);

			CombineAssertions("Sub Package 1 details are incorrect", () =>
			{
				AssertEquals("Sub Package 1 is missing its container ID", containerID2, expectedSubPackage1.ContainerNumber);
				AssertEquals("Wrong Goods Description.", goodsDescription, expectedSubPackage1.GoodsDescription);
				AssertEquals("Wrong Sub Package 1 Quantity.", container2SubPackage1Quantity, expectedSubPackage1.Quantity);
				AssertEquals("Wrong Sub Package 1 weight.", container2SubPackage1Weight, expectedSubPackage1.Weight);
				AssertEquals("Wrong Sub Package 1 weight metric.", container2SubPackage1WeightMetric, expectedSubPackage1.WeightMetric);
				AssertEquals("Wrong Sub Package 1 volume.", container2SubPackage1Volume, expectedSubPackage1.Volume);
				AssertEquals("Wrong Sub Package 1 volume metric.", container2SubPackage1VolumeMetric, expectedSubPackage1.VolumeMetric);
				AssertEquals("Wrong Sub Package 1 port loading.", expectedPortOfLoading, expectedSubPackage1.PortOfLoading.RL_IATA);
				AssertEquals("Wrong Sub Package 1 port discharge.", expectedPortOfDestination, expectedSubPackage1.PortOfDestination.RL_IATA);
				AssertEquals("Wrong Sub Package 1 Way Bill Number.", wayBillNumber, expectedSubPackage1.WayBillNumber);
			});

			var expectedSubPackage2 = ffmDetails.Packages.FirstOrDefault(p => p.Quantity == container2SubPackage2Quantity && p.Volume == container2SubPackage2Volume && p.Weight == container2SubPackage2Weight);
			AssertNotNull("Sub Package 2 is missing", expectedSubPackage2);

			CombineAssertions("Sub Package 2 details are incorrect", () =>
			{
				AssertEquals("Sub Package 2 is missing its container ID", containerID2, expectedSubPackage2.ContainerNumber);
				AssertEquals("Wrong Goods Description.", goodsDescription, expectedSubPackage2.GoodsDescription);
				AssertEquals("Wrong Sub Package 2 Quantity.", container2SubPackage2Quantity, expectedSubPackage2.Quantity);
				AssertEquals("Wrong Sub Package 2 weight.", container2SubPackage2Weight, expectedSubPackage2.Weight);
				AssertEquals("Wrong Sub Package 2 weight metric.", container2SubPackage2WeightMetric, expectedSubPackage2.WeightMetric);
				AssertEquals("Wrong Sub Package 2 volume.", container2SubPackage2Volume, expectedSubPackage2.Volume);
				AssertEquals("Wrong Sub Package 2 volume metric.", container2SubPackage2VolumeMetric, expectedSubPackage2.VolumeMetric);
				AssertEquals("Wrong Sub Package 2 port loading.", expectedPortOfLoading, expectedSubPackage2.PortOfLoading.RL_IATA);
				AssertEquals("Wrong Sub Package 2 port discharge.", expectedPortOfDestination, expectedSubPackage2.PortOfDestination.RL_IATA);
				AssertEquals("Wrong Sub Package 2 Way Bill Number.", wayBillNumber, expectedSubPackage2.WayBillNumber);
			});
		}

		IFFMDetails CreateTestData(
			BookingParentType parentType,
			List<TestContainerData> containers = null,
			List<TestPackageData> loosePackages = null,
			bool blankGoodsDescription = false
			)
		{
			var booking = CreateDtbBookingWithParent(parentType, blankGoodsDescription: blankGoodsDescription);

			var selectedConfirmation = CreateConfirmationWithPackagesOnBooking(
				booking: booking,
				containers: containers,
				loosePackages: loosePackages);

			return new DtbBookingConfirmationFFMDetailsProvider(selectedConfirmation);
		}

		IFFMDetails CreateTestDataWithMultipleConfirmations(List<TestContainerData> selectedConfirmationContainers, List<TestContainerData> nonSelectConfirmationContainers)
		{
			var booking = CreateDtbBookingWithParent(BookingParentType.ForwardingConsol);

			var selectedConfirmation = CreateConfirmationWithPackagesOnBooking(
				booking: booking,
				containers: selectedConfirmationContainers);

			var nonSelectedConfirmation = CreateConfirmationWithPackagesOnBooking(
				booking: booking,
				containers: nonSelectConfirmationContainers);

			return new DtbBookingConfirmationFFMDetailsProvider(selectedConfirmation);
		}

		DtbBooking CreateDtbBookingWithParent(BookingParentType parentType, bool blankGoodsDescription = false)
		{
			var shipmentBusinessObject = (BusinessObject)Factory.New<IForwardingShipment>();
			shipmentBusinessObject[JobShipmentSchema.JS_IsShipping] = true;
			shipmentBusinessObject[JobShipmentSchema.JS_IsForwardRegistered] = false;
			shipmentBusinessObject[JobShipmentSchema.JS_IsBooking] = true;
			shipmentBusinessObject[JobShipmentSchema.JS_RL_NKOrigin] = shipmentOrigin;
			shipmentBusinessObject[JobShipmentSchema.JS_RL_NKDestination] = shipmentDestination;
			shipmentBusinessObject[JobShipmentSchema.JS_RL_NKLoadPort] = shipmentOrigin;
			shipmentBusinessObject[JobShipmentSchema.JS_RL_NKDischargePort] = shipmentDestination;

			var forwardingConsol = Factory.New<IForwardingConsol>();
			forwardingConsol.AddShipment((IForwardingShipment)shipmentBusinessObject);

			var consolBusinessObject = (BusinessObject)forwardingConsol;
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

			var transport = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)forwardingConsol)["Transports"])[0];
			transport[JobConsolTransportSchema.JW_RL_NKDiscPort] = shipmentOrigin;
			transport[JobConsolTransportSchema.JW_RL_NKLoadPort] = shipmentDestination;
			transport[JobConsolTransportSchema.JW_ETD] = leaveDate;
			transport[JobConsolTransportSchema.JW_VoyageFlight] = voyageNumber;
			transport[JobConsolTransportSchema.JW_ETA] = arriveDate;
			transport[JobConsolTransportSchema.JW_JX] = sailing.PK;

			var bookingParent = (IDtbBookingParent)(parentType == BookingParentType.ForwardingConsol ? consolBusinessObject : shipmentBusinessObject);
			var tbConsol = Helper.CreateConsolidation(bookingParent);
			tbConsol.KB_GoodsDescription = blankGoodsDescription ? ZString.Empty : goodsDescription;
			tbConsol.KB_JobDirection = direction;

			var booking = Helper.CreateBooking(tbConsol);

			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.HouseBill, wayBillNumber);
			_ = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(CargoWise.Definitions.AdditionalReferenceTypes.Codes.MasterBill, wayBillNumber);

			var awbHeader = (BusinessObject)consolBusinessObject["AWBHeader"];
			awbHeader[ExportAWBHeaderSchema.EH_ParentID] = forwardingConsol.PK;
			awbHeader[ExportAWBHeaderSchema.EH_AgentApprovalNumber] = regulatedAgentID;
			awbHeader[ExportAWBHeaderSchema.EH_RN_NKAgentApprovalCountryCode] = approvalCountryCode;

			consolBusinessObject["SecurityStatusCode"] = securityStatusCode;

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

		enum BookingParentType
		{
			ForwardingConsol,
			ForwardingShipment,
		}
	}
}
