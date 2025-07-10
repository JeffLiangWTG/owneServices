using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Moq;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	static class AMSInterfaceTestHelper
	{
		internal static Mock<IEntity> GetEntity(ZString entityIDCode, ZString name, ZString codeQualifier, ZString idCode, ZString addressLine1, ZString addressLine1Part2, ZString addressLine2, ZString addressLine2Part2, ZString cityName, ZString stateProvince, ZString postalCode, ZString countryCode)
		{
			var entityMock = new Mock<IEntity>();
			entityMock.Setup(m => m.EntityCode).Returns(entityIDCode);
			entityMock.Setup(m => m.EntityName).Returns(name);
			entityMock.Setup(m => m.CodeQualifier).Returns(codeQualifier);
			entityMock.Setup(m => m.IDCode).Returns(idCode);
			entityMock.Setup(m => m.AddressLine1).Returns(addressLine1);
			entityMock.Setup(m => m.AddressLine1Part2).Returns(addressLine1Part2);
			entityMock.Setup(m => m.AddressLine2).Returns(addressLine2);
			entityMock.Setup(m => m.AddressLine2Part2).Returns(addressLine2Part2);
			entityMock.Setup(m => m.CityName).Returns(cityName);
			entityMock.Setup(m => m.StateProvince).Returns(stateProvince);
			entityMock.Setup(m => m.PostalCode).Returns(postalCode);
			entityMock.Setup(m => m.CountryCode).Returns(countryCode);
			var notifyPartyContactMock = new Mock<INotifyPartyContact>();
			notifyPartyContactMock.Setup(m => m.ContactName).Returns("BOB THE BUILDER");
			notifyPartyContactMock.Setup(m => m.CommNumberQualifier).Returns("TE");
			notifyPartyContactMock.Setup(m => m.CommunicationsNumber).Returns("1059654684");
			notifyPartyContactMock.Setup(m => m.CommNumberQualifier2).Returns("WP");
			notifyPartyContactMock.Setup(m => m.CommunicationsNumber2).Returns("9686448456");
			entityMock.Setup(m => m.AdminContact).Returns(notifyPartyContactMock.Object);
			return entityMock;
		}

		public static Mock<IACEBillManifestMessageAttachee> GetManifestForACE(bool fullData)
		{
			var manifestMock = new Mock<IACEBillManifestMessageAttachee>();
			manifestMock.Setup(m => m.CarrierCode).Returns("SD23");
			manifestMock.Setup(m => m.ModeOfTransportationCode).Returns("40");
			manifestMock.Setup(m => m.ConveyanceCountryCode).Returns("AU");
			manifestMock.Setup(m => m.VoyageNumber).Returns("V234");
			manifestMock.Setup(m => m.UniqueVoyageIdentifier).Returns("SD23DJV234");
			manifestMock.Setup(m => m.ManifestSequenceNumber).Returns("1");
			manifestMock.Setup(m => m.ConveyanceCode).Returns("1234567");
			var portMock = new Mock<IPort>();
			portMock.Setup(m => m.DistrictPortOfUnladingCode).Returns("5946");
			portMock.Setup(m => m.OriginalEstimatedDate).Returns(ZDate.BrettsBirthday);
			manifestMock.Setup(m => m.PortDetails).Returns(portMock.Object);

			var billMock1 = new Mock<IACEBillOfLading>();
			billMock1.Setup(m => m.IssuerCode).Returns("BDKL");
			billMock1.Setup(m => m.BillOfLadingSequenceNumber).Returns("BDKLHB1");
			billMock1.Setup(m => m.ForeignPort).Returns("56842");
			billMock1.Setup(m => m.ManifestQuantity).Returns(245m);
			billMock1.Setup(m => m.ManifestUnits).Returns("NO");
			billMock1.Setup(m => m.Weight).Returns(15250m);
			billMock1.Setup(m => m.WeightUnit).Returns("KG");
			billMock1.Setup(m => m.BillOfLadingStatusIndicator).Returns("B");
			billMock1.Setup(m => m.IsMasterInbond).Returns(fullData);
			billMock1.Setup(m => m.PlaceOfReceiptByCarrier).Returns("SYDNEY");
			billMock1.Setup(m => m.SecondNotifyParty1).Returns("SC53");
			billMock1.Setup(m => m.SecondNotifyParty2).Returns("SC98");
			billMock1.Setup(m => m.LastForeignPortBeforeDepartingForTheUS).Returns("56975");
			billMock1.Setup(m => m.ModeOfTransportationFromThePlacePriorToLoading).Returns("20");
			billMock1.Setup(m => m.MethodOfPaymentForTransportation).Returns("CHK");
			billMock1.Setup(m => m.ContractualPossessionForeignPort).Returns("56231");
			if (fullData)
			{
				billMock1.Setup(m => m.Volume).Returns(23m);
				var shipmentReferenceDetail1Mock = new Mock<IShipmentReferenceDetail>();
				shipmentReferenceDetail1Mock.Setup(m => m.Qualifier).Returns("M");
				shipmentReferenceDetail1Mock.Setup(m => m.ReferenceIdentifier).Returns("MASTERXTN1");
				var shipmentReferenceDetail2Mock = new Mock<IShipmentReferenceDetail>();
				shipmentReferenceDetail2Mock.Setup(m => m.Qualifier).Returns("H");
				shipmentReferenceDetail2Mock.Setup(m => m.ReferenceIdentifier).Returns("HOUSEXTN1");
				billMock1.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns(new IShipmentReferenceDetail[] { shipmentReferenceDetail1Mock.Object, shipmentReferenceDetail2Mock.Object });
			}
			else
			{
				billMock1.Setup(m => m.Volume).Returns(0m);
				billMock1.Setup(m => m.ShipmentReferenceDetails(It.IsAny<ActionCode>())).Returns((System.Collections.Generic.IEnumerable<IShipmentReferenceDetail>)null);
			}

			var entity1Mock = GetEntity("SH", "SHIPPER NAME", "17", "ID23423", "SHIPPER ADDRESS 1", "SHIPPER ADDRESS 2", "SHIPPER ADDRESS 3", "SHIPPER ADDRESS 4", "SHIPPER CITY", "NS", "2000", "AU");
			var entity2Mock = GetEntity("CN", "CONSIGNEE NAME", "1", "ID89564", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CONSIGNEE ADDRESS 3", "CONSIGNEE ADDRESS 4", "CONSIGNEE CITY", "CA", "60025", "US");
			var entity3Mock = GetEntity("N1", "NOTIFY PARTY 1 NAME", "98", "ID986575", "NOTIFY PARTY 1 ADDRESS 1", "NOTIFY PARTY 1 ADDRESS 2", "NOTIFY PARTY 1 ADDRESS 3", "NOTIFY PARTY 1 ADDRESS 4", "NOTIFY PARTY 1 CITY", "NY", "96632", "US");
			var entity4Mock = GetEntity("N2", "NOTIFY PARTY 2 NAME", "36", "ID876534", "NOTIFY PARTY 2 ADDRESS 1", "NOTIFY PARTY 2 ADDRESS 2", "NOTIFY PARTY 2 ADDRESS 3", "NOTIFY PARTY 2 ADDRESS 4", "NOTIFY PARTY 2 CITY", "VI", "3005", "AU");
			billMock1.Setup(m => m.Entities(It.IsAny<ActionCode>())).Returns(new IEntity[] { entity1Mock.Object, entity2Mock.Object, entity3Mock.Object, entity4Mock.Object });
			var inBOndMock = new Mock<IMovemenDetails>();
			inBOndMock.Setup(m => m.PreviousInBondNumber).Returns("ITPRV234");
			inBOndMock.Setup(m => m.InbondEntryType).Returns("62");
			if (fullData)
			{
				inBOndMock.Setup(m => m.InBondQuantity).Returns(245);
			}
			else
			{
				inBOndMock.Setup(m => m.InBondQuantity).Returns(200);
			}

			inBOndMock.Setup(m => m.IsBTAFDA).Returns(fullData);
			inBOndMock.Setup(m => m.ConventionalInbondNumber).Returns("INB12346");
			inBOndMock.Setup(m => m.InbondCarrierCode).Returns("SD98");
			inBOndMock.Setup(m => m.USPortOfDestination).Returns("2354");
			inBOndMock.Setup(m => m.ForeignDestination).Returns("69542");
			inBOndMock.Setup(m => m.Value).Returns(23212212);
			inBOndMock.Setup(m => m.BondedCarrierID).Returns("965-63-5685");
			inBOndMock.Setup(m => m.PaperlessInbondNumber).Returns("VINB53264");
			inBOndMock.Setup(m => m.ExportVesselName).Returns(fullData ? "EXPORT VESSEL NAME" : "");
			inBOndMock.Setup(m => m.ArrivalDateTime).Returns(new ZDateTime(2012, 2, 3, 18, 44, 53));
			inBOndMock.Setup(m => m.ExportDateTime).Returns(new ZDateTime(2013, 4, 6, 17, 36, 46));
			inBOndMock.Setup(m => m.TOLInBondCarrierCode).Returns("TOL1");
			inBOndMock.Setup(m => m.TOLBondedCarrierID).Returns("TOL-INBCARID");
			inBOndMock.Setup(m => m.TOLDateTime).Returns(new ZDateTime(2012, 7, 9, 10, 22, 8));
			inBOndMock.Setup(m => m.TOLCityName).Returns("BOB'S CITY");
			inBOndMock.Setup(m => m.TOLStateCode).Returns("CA");
			var container1Mock = GetACEContainer("C1" + "BDKLHB1", fullData);
			var container2Mock = GetACEContainer("C2" + "BDKLHB1", !fullData);
			billMock1.Setup(m => m.Containers).Returns(new IACEContainer[] { container1Mock.Object, container2Mock.Object });
			manifestMock.Setup(m => m.BillOfLadingDetails).Returns(billMock1.Object);
			return manifestMock;
		}

		public static Mock<IACEContainer> GetACEContainer(ZString containerNo, bool fullData)
		{
			var containerMock = new Mock<IACEContainer>();
			containerMock.Setup(m => m.ContainerEquipmentNo).Returns(containerNo);
			containerMock.Setup(m => m.SealNumber1).Returns("SEAL1");
			containerMock.Setup(m => m.SealNumber2).Returns("SEAL2");
			containerMock.Setup(m => m.ContainerEquipmentDescriptionCode).Returns("20");
			containerMock.Setup(m => m.ContainerEquipmentLength).Returns(20);
			containerMock.Setup(m => m.Height).Returns("21");
			containerMock.Setup(m => m.Width).Returns("22");
			containerMock.Setup(m => m.ContainerEquipmentType).Returns("20FR");
			containerMock.Setup(m => m.LoadEmptyStatusCode).Returns("L");
			containerMock.Setup(m => m.TypeOfServiceCode).Returns("CS");
			var vehicleDetail1Mock = new Mock<IVehicleDetails>();
			vehicleDetail1Mock.Setup(m => m.VIN).Returns("VIN1234658");
			var vehicleDetail2Mock = new Mock<IVehicleDetails>();
			vehicleDetail2Mock.Setup(m => m.VIN).Returns("");
			containerMock.Setup(m => m.VehicleDetails).Returns(new IVehicleDetails[] { vehicleDetail1Mock.Object, vehicleDetail2Mock.Object });

			var commodity1Mock = new Mock<ICargoDescription>();
			if (fullData)
			{
				commodity1Mock.Setup(m => m.HarmonizedNumber).Returns("1010101012");
				commodity1Mock.Setup(m => m.Value).Returns(123);
				commodity1Mock.Setup(m => m.Weight).Returns(32424);
				commodity1Mock.Setup(m => m.WeightUnit).Returns("KG");
			}
			else
			{
				commodity1Mock.Setup(m => m.HarmonizedNumber).Returns(ZString.Empty);
				commodity1Mock.Setup(m => m.Value).Returns(0);
				commodity1Mock.Setup(m => m.Weight).Returns(0);
				commodity1Mock.Setup(m => m.WeightUnit).Returns(ZString.Empty);
			}

			commodity1Mock.Setup(m => m.MarksAndNumbers).Returns("MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LINE AND NOT ONE LINE");
			commodity1Mock.Setup(m => m.PieceCount).Returns(10m);
			commodity1Mock.Setup(m => m.Description).Returns("DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE AGAIN I SAID DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE");
			commodity1Mock.Setup(m => m.C4Number).Returns("C4NUMBER12");
			commodity1Mock.Setup(m => m.ManifestUnitCode).Returns("PCE");
			commodity1Mock.Setup(m => m.CountryCode).Returns("AU");

			var commodity2Mock = new Mock<ICargoDescription>();
			if (!fullData)
			{
				commodity2Mock.Setup(m => m.HarmonizedNumber).Returns("2013456875");
				commodity2Mock.Setup(m => m.Value).Returns(123);
				commodity2Mock.Setup(m => m.Weight).Returns(32424);
				commodity2Mock.Setup(m => m.WeightUnit).Returns("KG");
			}
			else
			{
				commodity2Mock.Setup(m => m.HarmonizedNumber).Returns(ZString.Empty);
				commodity2Mock.Setup(m => m.Value).Returns(0);
				commodity2Mock.Setup(m => m.Weight).Returns(0);
				commodity2Mock.Setup(m => m.WeightUnit).Returns(ZString.Empty);
			}

			commodity2Mock.Setup(m => m.MarksAndNumbers).Returns("MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LINE AND NOT ONE LINE AGAIN I SAID MARKS AND NUMBERS THAT SHOULD BE IN AT LEAST TWO LINE AND NOT ONE LINE");
			commodity2Mock.Setup(m => m.PieceCount).Returns(10m);
			commodity2Mock.Setup(m => m.Description).Returns("DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE AGAIN I SAID DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE");
			commodity2Mock.Setup(m => m.C4Number).Returns("C4NUMBER12");
			commodity2Mock.Setup(m => m.ManifestUnitCode).Returns("PCE");
			commodity2Mock.Setup(m => m.CountryCode).Returns("AU");

			containerMock.Setup(m => m.Commondities).Returns(new ICargoDescription[] { commodity1Mock.Object, commodity2Mock.Object });
			var hazardous1Mock = new Mock<IHazardousMaterial>();
			hazardous1Mock.Setup(m => m.HazMatCode).Returns("UN114561");
			hazardous1Mock.Setup(m => m.HazMatClass).Returns("CL32");
			hazardous1Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous1Mock.Setup(m => m.HazMatClassificationDesc).Returns("SHIPPING NAME 1");
			hazardous1Mock.Setup(m => m.ContactName).Returns("BOB THE BUILDER");
			if (fullData)
			{
				hazardous1Mock.Setup(m => m.FlashPointTemp).Returns(-32m);
				hazardous1Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.True);
				hazardous1Mock.Setup(m => m.HazMatDesc).Returns("HAZRDOUS DESCRIPTION THAT IS LONG AND SHOULD BE AT LEAST TWO LINE AND NOT ONE LINE");
				hazardous1Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.True);
			}
			else
			{
				hazardous1Mock.Setup(m => m.FlashPointTemp).Returns(0m);
				hazardous1Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.False);
				hazardous1Mock.Setup(m => m.HazMatDesc).Returns(ZString.Empty);
				hazardous1Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.False);
			}

			var hazardous2Mock = new Mock<IHazardousMaterial>();
			hazardous2Mock.Setup(m => m.HazMatCode).Returns("R9658");
			hazardous2Mock.Setup(m => m.HazMatClass).Returns("CL69");
			hazardous2Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous2Mock.Setup(m => m.HazMatClassificationDesc).Returns("SHIPPING NAME 2");
			hazardous2Mock.Setup(m => m.ContactName).Returns("WENDY THE DESTROYER");
			if (fullData)
			{
				hazardous2Mock.Setup(m => m.FlashPointTemp).Returns(45m);
				hazardous2Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.True);
				hazardous2Mock.Setup(m => m.HazMatDesc).Returns("HAZRDOUS DESCRIPTION");
				hazardous2Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.True);
			}
			else
			{
				hazardous2Mock.Setup(m => m.FlashPointTemp).Returns(0m);
				hazardous2Mock.Setup(m => m.IsFlashPointTempRelevant).Returns(ZBool.False);
				hazardous2Mock.Setup(m => m.HazMatDesc).Returns(ZString.Empty);
				hazardous2Mock.Setup(m => m.IsHazRelevant).Returns(ZBool.False);
			}

			var hazardous3Mock = new Mock<IHazardousMaterial>();
			hazardous3Mock.Setup(m => m.HazMatCode).Returns("1381E");
			hazardous3Mock.Setup(m => m.HazMatClass).Returns("4.2");
			hazardous3Mock.Setup(m => m.HazMatQualifier).Returns("U");
			hazardous3Mock.Setup(m => m.HazMatDesc).Returns("PHOSPHORUS");
			hazardous3Mock.Setup(m => m.ContactName).Returns("AARON");
			hazardous3Mock.Setup(m => m.FlashPointTemp).Returns(0m);
			hazardous3Mock.Setup(m => m.HazMatClassificationDesc).Returns(string.Empty);
			containerMock.Setup(m => m.HazardousMaterials).Returns(new IHazardousMaterial[] { hazardous1Mock.Object, hazardous2Mock.Object, hazardous3Mock.Object });
			return containerMock;
		}

		public static void PopulateINPM01(Mock<IACEBillManifestMessageAttachee> manifestMock)
		{
			manifestMock.Setup(m => m.CarrierCode).Returns("SD23");
			manifestMock.Setup(m => m.ModeOfTransportationCode).Returns("40");
			manifestMock.Setup(m => m.ConveyanceCountryCode).Returns("AU");
			manifestMock.Setup(m => m.VoyageNumber).Returns("V234");
			manifestMock.Setup(m => m.ManifestSequenceNumber).Returns("1");
			manifestMock.Setup(m => m.ConveyanceCode).Returns("1234567");
		}

		public static Mock<IPort> PopulateBasicPortMock()
		{
			var portMock = new Mock<IPort>();
			portMock.Setup(m => m.DistrictPortOfUnladingCode).Returns("5946");
			portMock.Setup(m => m.OriginalEstimatedDate).Returns(ZDate.BrettsBirthday);
			return portMock;
		}
	}
}
