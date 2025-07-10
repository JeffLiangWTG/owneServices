using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.TransportBookings.Document;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	[TestedType(typeof(FFMMessageDataObjectWriterTest))]
	public sealed class FFMMessageDataObjectWriterTest : DataObjectWriterTest
	{
		FFMMessage mockFFMMessage;

		readonly string voyageNumber = "CS0001A";
		readonly string goodsDescription = "Test Goods";
		readonly string wayBillNumber = "724-47177421";
		readonly string origin = "ITMXP";
		readonly string destination = "THBKK";

		readonly string driverDocumentID = "12 123 123 123";
		readonly string regulatedAgentID = "0079-00";
		readonly string approvalCountryCode = "IT";

		readonly string consolSecurityStatus = "SPX";

		readonly ZDate leaveDate = new(2077, 1, 1);

		readonly string containerID1 = "CNT-1";

		FFMMessagePackage container1SubPackage1;
		readonly ZInt container1SubPackage1Quantity = 1;
		readonly ZDecimal container1SubPackage1Volume = 10.0m;
		readonly ZString container1SubPackage1VolumeUnit = "M3";
		readonly ZDecimal container1SubPackage1Weight = 100.0m;
		readonly ZString container1SubPackage1WeightUnit = "KG";

		readonly string containerID2 = "CNT-2";

		FFMMessagePackage container2SubPackage1;
		readonly ZInt container2SubPackage1Quantity = 2;
		readonly ZDecimal container2SubPackage1Volume = 20.0m;
		readonly ZString container2SubPackage1VolumeUnit = "M3";
		readonly ZDecimal container2SubPackage1Weight = 200.0m;
		readonly ZString container2SubPackage1WeightUnit = "KG";

		FFMMessagePackage container2SubPackage2;
		readonly ZInt container2SubPackage2Quantity = 3;
		readonly ZDecimal container2SubPackage2Volume = 90.0m;
		readonly ZString container2SubPackage2VolumeUnit = "M3";
		readonly ZDecimal container2SubPackage2Weight = 900.0m;
		readonly ZString container2SubPackage2WeightUnit = "KG";

		readonly string expectedPortOfLoading = "MXP";
		readonly string expectedPortOfDestination = "BKK";

		protected override void SetUp()
		{
			var context = new TransportBookingsCommonContext(Factory);

			container1SubPackage1 = new FFMMessagePackage()
			{
				ContainerNumber = containerID1,
				GoodsDescription = goodsDescription,
				WayBillNumber = wayBillNumber,
				Quantity = container1SubPackage1Quantity,
				Volume = container1SubPackage1Volume,
				VolumeMetric = container1SubPackage1VolumeUnit,
				Weight = container1SubPackage1Weight,
				WeightMetric = container1SubPackage1WeightUnit,
				PortOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = destination
				},
				PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = origin
				},
			};

			container2SubPackage1 = new FFMMessagePackage()
			{
				ContainerNumber = containerID2,
				GoodsDescription = goodsDescription,
				WayBillNumber = wayBillNumber,
				Quantity = container2SubPackage1Quantity,
				Volume = container2SubPackage1Volume,
				VolumeMetric = container2SubPackage1VolumeUnit,
				Weight = container2SubPackage1Weight,
				WeightMetric = container2SubPackage1WeightUnit,
				PortOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = destination
				},
				PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = origin
				},
			};

			container2SubPackage2 = new FFMMessagePackage()
			{
				ContainerNumber = containerID2,
				GoodsDescription = goodsDescription,
				WayBillNumber = wayBillNumber,
				Quantity = container2SubPackage2Quantity,
				Volume = container2SubPackage2Volume,
				VolumeMetric = container2SubPackage2VolumeUnit,
				Weight = container2SubPackage2Weight,
				WeightMetric = container2SubPackage2WeightUnit,
				PortOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = destination
				},
				PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = origin
				},
			};

			mockFFMMessage = new FFMMessage()
			{
				VoyageFlightNo = voyageNumber,
				FlightDate = leaveDate,
				RegulatedAgentID = regulatedAgentID,
				RegulatedAgentCountry = approvalCountryCode,
				SecurityStatusCode = consolSecurityStatus,
				DriverDocumentID = driverDocumentID,
				AirportOfDestinationCode = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = destination
				},
				PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = origin
				},
				Packages = new List<FFMMessagePackage>()
				{
					container1SubPackage1,
					container2SubPackage1,
					container2SubPackage2
				}
			};
		}

		public void TestPopulateDataObject()
		{
			var manager = new TransportBookingsDocDataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new FFMMessageDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(mockFFMMessage);

			CombineAssertions("Main shipment details invalid", () =>
			{
				AssertEquals("Voyage Number is incorrect.", voyageNumber, dataObject.VoyageFlightNo);

				var universalShipmentLeaveDate = dataObject.DateCollection.Find(d => d.Type == UniversalDataBuss.DataObjects.Universal.DateType.Departure && d.Value == leaveDate);
				AssertNotNull("Could not find the Flight Date within the XUS.", universalShipmentLeaveDate);
				AssertEquals("Flight Date should have IsEstimate flag set to true.", true, universalShipmentLeaveDate?.IsEstimate ?? false);

				AssertEquals("Airport Code is incorrect.", expectedPortOfLoading, dataObject.PortOfLoading?.Code);
				AssertEquals("Port of Destination is incorrect.", expectedPortOfDestination, dataObject.PortOfDestination?.Code);

				AssertEquals("Regulated Agent ID is incorrect.", regulatedAgentID, dataObject.CarrierDocumentsOverride.AWBHeader.AgentName);
				AssertEquals("Country of Regulated Agent is incorrect.", approvalCountryCode, dataObject.CarrierDocumentsOverride.AWBHeader.AgentPlace);
				AssertEquals("Security status code is incorrect.", consolSecurityStatus, dataObject.CarrierDocumentsOverride.AWBHeader.SpecialHandlingCode);

				var documentID = dataObject.InstructionCollection?.FirstOrDefault()?
					.InstructionPackingLineLinkCollection?.FirstOrDefault()?
					.ConfirmationCollection?.FirstOrDefault()?.DriverDocumentID;

				AssertEquals("The Driver DocumentID is incorrect.", driverDocumentID, documentID);

				AssertEquals("There should be exactly 3 packages lines.", 3, dataObject.SubShipmentCollection.Count);
			});

			var expectedContainer1SubPackage1 = dataObject.SubShipmentCollection.FirstOrDefault(s => s.ContainerCollection.Any(c => (c.ContainerNumber ?? ZString.Empty) == containerID1));
			AssertNotNull("Missing the package for container 1.", expectedContainer1SubPackage1);

			CombineAssertions("Incorrect details for Sub package 1 Container 1." ,() =>
			{
				AssertEquals("Sub package 1 has incorrect Way Bill Number", wayBillNumber, expectedContainer1SubPackage1.WayBillNumber);
				AssertEquals("Sub package 1 has incorrect Goods Description.", goodsDescription, expectedContainer1SubPackage1.GoodsDescription);
				AssertEquals("Sub package 1 has incorrect Quantity.", container1SubPackage1Quantity, expectedContainer1SubPackage1.TotalNoOfPacks);

				AssertEquals("Sub package 1 has incorrect Weight.", container1SubPackage1Weight, expectedContainer1SubPackage1.TotalWeight);
				AssertEquals("Sub package 1 has incorrect Weight Unit.", container1SubPackage1WeightUnit, expectedContainer1SubPackage1.TotalWeightUnit.Code);
				AssertEquals("Sub package 1 has incorrect Volume.", container1SubPackage1Volume, expectedContainer1SubPackage1.TotalVolume);
				AssertEquals("Sub package 1 has incorrect Volume Unit.", container1SubPackage1VolumeUnit, expectedContainer1SubPackage1.TotalVolumeUnit.Code);

				AssertEquals("Sub package 1 has incorrect Port of Loading.", expectedPortOfLoading, expectedContainer1SubPackage1.PortOfLoading?.Code);
				AssertEquals("Sub package 1 has incorrect Port of Destination.", expectedPortOfDestination, expectedContainer1SubPackage1.PortOfDestination?.Code);
			});

			var expectedContainer2SubPackage1 = dataObject.SubShipmentCollection.ToList<UniversalShipment>()
				.FindAll(s => s.ContainerCollection.Any(c => (c.ContainerNumber ?? ZString.Empty) == containerID2))
				.Find(s => s.TotalNoOfPacks == container2SubPackage1Quantity);
			AssertNotNull("Missing the sub package 1 for container 2.");

			CombineAssertions("Incorrect details for Sub package 1 Container 2.", () =>
			{
				AssertEquals("Sub package 1 has incorrect Way Bill Number", wayBillNumber, expectedContainer2SubPackage1.WayBillNumber);
				AssertEquals("Sub package 1 has incorrect Goods Description.", goodsDescription, expectedContainer2SubPackage1.GoodsDescription);
				AssertEquals("Sub package 1 has incorrect Quantity.", container2SubPackage1Quantity, expectedContainer2SubPackage1.TotalNoOfPacks);

				AssertEquals("Sub package 1 has incorrect Weight.", container2SubPackage1Weight, expectedContainer2SubPackage1.TotalWeight);
				AssertEquals("Sub package 1 has incorrect Weight Unit.", container2SubPackage1WeightUnit, expectedContainer2SubPackage1.TotalWeightUnit.Code);
				AssertEquals("Sub package 1 has incorrect Volume.", container2SubPackage1Volume, expectedContainer2SubPackage1.TotalVolume);
				AssertEquals("Sub package 1 has incorrect Volume Unit.", container2SubPackage1VolumeUnit, expectedContainer2SubPackage1.TotalVolumeUnit.Code);

				AssertEquals("Sub package 1 has incorrect Port of Loading.", expectedPortOfLoading, expectedContainer1SubPackage1.PortOfLoading?.Code);
				AssertEquals("Sub package 1 has incorrect Port of Destination.", expectedPortOfDestination, expectedContainer1SubPackage1.PortOfDestination?.Code);
			});

			var expectedContainer2SubPackage2 = dataObject.SubShipmentCollection.ToList<UniversalShipment>()
				.FindAll(s => s.ContainerCollection.Any(c => (c.ContainerNumber ?? ZString.Empty) == containerID2))
				.Find(s => s.TotalNoOfPacks == container2SubPackage2Quantity);

			AssertNotNull("Missing the sub package 2 for container 2.", expectedContainer2SubPackage2);

			CombineAssertions("Incorrect details for Sub package 2 Container 2.", () =>
			{
				AssertEquals("Sub package 2 has incorrect Way Bill Number", wayBillNumber, expectedContainer2SubPackage2.WayBillNumber);
				AssertEquals("Sub package 2 has incorrect Goods Description.", goodsDescription, expectedContainer2SubPackage2.GoodsDescription);
				AssertEquals("Sub package 2 has incorrect Quantity.", container2SubPackage2Quantity, expectedContainer2SubPackage2.TotalNoOfPacks);

				AssertEquals("Sub package 2 has incorrect Weight.", container2SubPackage2Weight, expectedContainer2SubPackage2.TotalWeight);
				AssertEquals("Sub package 2 has incorrect Weight Unit.", container2SubPackage2WeightUnit, expectedContainer2SubPackage2.TotalWeightUnit.Code);
				AssertEquals("Sub package 2 has incorrect Volume.", container2SubPackage2Volume, expectedContainer2SubPackage2.TotalVolume);
				AssertEquals("Sub package 2 has incorrect Volume Unit.", container2SubPackage2VolumeUnit, expectedContainer2SubPackage2.TotalVolumeUnit.Code);

				AssertEquals("Sub package 2 has incorrect Port of Loading.", expectedPortOfLoading, expectedContainer2SubPackage2.PortOfLoading?.Code);
				AssertEquals("Sub package 2 has incorrect Port of Destination.", expectedPortOfDestination, expectedContainer2SubPackage2.PortOfDestination?.Code);
			});
		}
	}
}
