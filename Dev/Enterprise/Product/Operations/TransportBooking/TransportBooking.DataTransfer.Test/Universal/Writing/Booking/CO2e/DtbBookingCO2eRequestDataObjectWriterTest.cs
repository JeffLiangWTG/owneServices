using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingCO2eRequestDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
		public void TestPopulateBusinessObject_WhenTransportModeIsROA()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_TransportMode = "ROA";
			var package = Helper.CreatePackage("PKG1", null, 1, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var picOrgHeader = Helper.CreateOrgHeader(Factory, "PIC");
			var picAddress = PopulateAddress(picOrgHeader, "PIC", 112.78m, 4.673m, "2122", Constants.CountryCodes.Australia, "AUSYD");
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, picAddress);
			Helper.CreatePackageDivot(pic, package, 1);

			var dlvOrgHeader = Helper.CreateOrgHeader(Factory, "DLV");
			var dlvAddress = PopulateAddress(dlvOrgHeader, "DLV", 69.8346m, 9m, "2241", Constants.CountryCodes.NewZealand, "NZAKL");
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv, package, 1);

			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();
			Assert("Pre-condition: booking is valid when matching for CO2 calculation", result.IsValid);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull("tbDO", tbDO);
			AssertEquals("tbDO.InstructionCollection.Count", 2, tbDO.InstructionCollection.Count);
			AssertEquals("tbDO.TransportMode", Constants.TransportModes.Road, tbDO.TransportMode.Code);

			AssertEquals("tbDO.InstructionCollection[0].Sequence", 1, tbDO.InstructionCollection[0].Sequence);
			AssertAddress("tbDO.InstructionCollection[0].Address", picAddress, tbDO.InstructionCollection[0].Address);
			AssertWeightCollection("tbDO.InstructionCollection[0].WeightCollection", GetActionsForInstruction(result, pic), tbDO.InstructionCollection[0].WeightCollection);

			AssertEquals("tbDO.InstructionCollection[1].Sequence", 2, tbDO.InstructionCollection[1].Sequence);
			AssertAddress("tbDO.InstructionCollection[1].Address", dlvAddress, tbDO.InstructionCollection[1].Address);
			AssertWeightCollection("tbDO.InstructionCollection[1].WeightCollection", GetActionsForInstruction(result, dlv), tbDO.InstructionCollection[1].WeightCollection);
		}

		public void TestPopulateBusinessObject_WhenTransportModeIsRAI()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_TransportMode = "RAI";
			var package = Helper.CreatePackage("PKG1", null, 1, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var picOrgHeader = Helper.CreateOrgHeader(Factory, "PIC");
			var picAddress = PopulateAddress(picOrgHeader, "PIC", 112.78m, 4.673m, "2122", Constants.CountryCodes.Australia, "AUSYD");
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, picAddress);
			Helper.CreatePackageDivot(pic, package, 1);

			var dlvOrgHeader = Helper.CreateOrgHeader(Factory, "DLV");
			var dlvAddress = PopulateAddress(dlvOrgHeader, "DLV", 69.8346m, 9m, "2241", Constants.CountryCodes.NewZealand, "NZAKL");
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv, package, 1);

			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();
			Assert("Pre-condition: booking is valid when matching for CO2 calculation", result.IsValid);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull("tbDO", tbDO);
			AssertEquals("tbDO.InstructionCollection.Count", 2, tbDO.InstructionCollection.Count);
			AssertEquals("tbDO.TransportMode", Constants.TransportModes.Rail, tbDO.TransportMode.Code);

			AssertEquals("tbDO.InstructionCollection[0].Sequence", 1, tbDO.InstructionCollection[0].Sequence);
			AssertAddress("tbDO.InstructionCollection[0].Address", picAddress, tbDO.InstructionCollection[0].Address);
			AssertWeightCollection("tbDO.InstructionCollection[0].WeightCollection", GetActionsForInstruction(result, pic), tbDO.InstructionCollection[0].WeightCollection);

			AssertEquals("tbDO.InstructionCollection[1].Sequence", 2, tbDO.InstructionCollection[1].Sequence);
			AssertAddress("tbDO.InstructionCollection[1].Address", dlvAddress, tbDO.InstructionCollection[1].Address);
			AssertWeightCollection("tbDO.InstructionCollection[1].WeightCollection", GetActionsForInstruction(result, dlv), tbDO.InstructionCollection[1].WeightCollection);
		}

		public void TestPopulateBusinessObject_WhenTransportModeIsIWT()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_TransportMode = "IWT";
			var package = Helper.CreatePackage("PKG1", null, 1, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var picOrgHeader = Helper.CreateOrgHeader(Factory, "PIC");
			var picAddress = PopulateAddress(picOrgHeader, "PIC", 112.78m, 4.673m, "2122", Constants.CountryCodes.Australia, "AUSYD");
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, picAddress);
			Helper.CreatePackageDivot(pic, package, 1);

			var dlvOrgHeader = Helper.CreateOrgHeader(Factory, "DLV");
			var dlvAddress = PopulateAddress(dlvOrgHeader, "DLV", 69.8346m, 9m, "2241", Constants.CountryCodes.NewZealand, "NZAKL");
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv, package, 1);

			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();
			Assert("Pre-condition: booking is valid when matching for CO2 calculation", result.IsValid);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull("tbDO", tbDO);
			AssertEquals("tbDO.InstructionCollection.Count", 2, tbDO.InstructionCollection.Count);
			AssertEquals("tbDO.TransportMode", Constants.TransportModes.InlandWaterwayTransport, tbDO.TransportMode.Code);

			AssertEquals("tbDO.InstructionCollection[0].Sequence", 1, tbDO.InstructionCollection[0].Sequence);
			AssertAddress("tbDO.InstructionCollection[0].Address", picAddress, tbDO.InstructionCollection[0].Address);
			AssertWeightCollection("tbDO.InstructionCollection[0].WeightCollection", GetActionsForInstruction(result, pic), tbDO.InstructionCollection[0].WeightCollection);

			AssertEquals("tbDO.InstructionCollection[1].Sequence", 2, tbDO.InstructionCollection[1].Sequence);
			AssertAddress("tbDO.InstructionCollection[1].Address", dlvAddress, tbDO.InstructionCollection[1].Address);
			AssertWeightCollection("tbDO.InstructionCollection[1].WeightCollection", GetActionsForInstruction(result, dlv), tbDO.InstructionCollection[1].WeightCollection);
		}

		public void TestPopulateBusinessObject_InstructionMatching()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 5, 1000);
			var package2 = Helper.CreatePackage("PKG2", null, 10, 50);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var picOrgHeader = Helper.CreateOrgHeader(Factory, "PIC");
			var picAddress = PopulateAddress(picOrgHeader, "PIC", 112.78m, 4.673m, "2122", Constants.CountryCodes.Australia, "AUSYD");
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, picAddress);
			Helper.CreatePackageDivot(pic, package1, 1);

			var mltOrgHeader = Helper.CreateOrgHeader(Factory, "MLT");
			var mltAddress = PopulateAddress(mltOrgHeader, "MLT", 123.456m, 65.4321m, "3000", Constants.CountryCodes.Australia, "AUMEL");
			var mlt = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS, mltAddress);
			var mltDlvDivot = Helper.CreatePackageDivot(mlt, package1, 1);
			Helper.CreateConfirmation(mltDlvDivot, ConfirmationTypes.Codes.Delivery);
			var mltPicDivot = Helper.CreatePackageDivot(mlt, package2, 3);
			Helper.CreateConfirmation(mltPicDivot, ConfirmationTypes.Codes.PickUp);

			var dlvOrgHeader = Helper.CreateOrgHeader(Factory, "DLV");
			var dlvAddress = PopulateAddress(dlvOrgHeader, "DLV", 69.8346m, 9m, "2241", Constants.CountryCodes.NewZealand, "NZAKL");
			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv1, package1, 3);
			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv2, package2, 3);

			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();
			Assert("Pre-condition: booking is valid when matching for CO2 calculation", result.IsValid);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull(tbDO);
			AssertEquals("tbDO.TransportMode.Code", Constants.TransportModes.Road, tbDO.TransportMode.Code);
			AssertEquals("tbDO.InstructionCollection.Count", 3, tbDO.InstructionCollection.Count);

			AssertEquals("tbDO.InstructionCollection[0].Sequence", 1, tbDO.InstructionCollection[0].Sequence);
			AssertAddress("tbDO.InstructionCollection[0].Address", picAddress, tbDO.InstructionCollection[0].Address);
			AssertWeightCollection("tbDO.InstructionCollection[0].WeightCollection", GetActionsForInstruction(result, pic), tbDO.InstructionCollection[0].WeightCollection);

			AssertEquals("tbDO.InstructionCollection[1].Sequence", 2, tbDO.InstructionCollection[1].Sequence);
			AssertAddress("tbDO.InstructionCollection[1].Address", mltAddress, tbDO.InstructionCollection[1].Address);
			AssertWeightCollection("tbDO.InstructionCollection[1].WeightCollection", GetActionsForInstruction(result, mlt), tbDO.InstructionCollection[1].WeightCollection);

			AssertEquals("tbDO.InstructionCollection[2].Sequence", 4, tbDO.InstructionCollection[2].Sequence);
			AssertAddress("tbDO.InstructionCollection[2].Address", dlvAddress, tbDO.InstructionCollection[2].Address);
			AssertWeightCollection("tbDO.InstructionCollection[2].WeightCollection", GetActionsForInstruction(result, dlv2), tbDO.InstructionCollection[2].WeightCollection);
		}

		public void TestPopulateBusinessObject_EmptyContainerWeight()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var emptyContainer = Helper.CreatePackageContainer("PKG1", null, 1, 0, 0, "20HC");
			emptyContainer.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			emptyContainer.Container.K0_IsEmpty = true;
			var containerYardContainer = Helper.CreatePackageContainer("PKG2", null, 1, 100, 0, "20GP");
			containerYardContainer.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var regularContainer = Helper.CreatePackageContainer("PKG3", null, 1, 1000, 0, "20GP");
			regularContainer.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic1OrgHeader = Helper.CreateOrgHeader(Factory, "PIC1");
			var pic1Address = PopulateAddress(pic1OrgHeader, "PIC1", 112.78m, 4.673m, "2122", Constants.CountryCodes.Australia, "AUSYD");
			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pic1Address);
			Helper.CreatePackageDivot(pic1, emptyContainer, 1);

			var pic2OrgHeader = Helper.CreateOrgHeader(Factory, "PIC2");
			var pic2Address = PopulateAddress(pic2OrgHeader, "PIC2", 123.456m, 65.4321m, "3000", Constants.CountryCodes.Australia, "AUMEL");
			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, pic2Address);
			Helper.CreatePackageDivot(pic2, containerYardContainer, 1);

			var pic3OrgHeader = Helper.CreateOrgHeader(Factory, "PIC3");
			var pic3Address = PopulateAddress(pic3OrgHeader, "PIC3", 156.399m, 2.937m, "7000", Constants.CountryCodes.Australia, "AUHBA");
			var pic3 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.WHS, pic3Address);
			Helper.CreatePackageDivot(pic3, regularContainer, 1);

			var dlvOrgHeader = Helper.CreateOrgHeader(Factory, "DLV");
			var dlvAddress = PopulateAddress(dlvOrgHeader, "DLV", 69.8346m, 9m, "2241", Constants.CountryCodes.NewZealand, "NZAKL");
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, dlvAddress);
			Helper.CreatePackageDivot(dlv, emptyContainer, 1);
			Helper.CreatePackageDivot(dlv, containerYardContainer, 1);
			Helper.CreatePackageDivot(dlv, regularContainer, 1);

			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();
			Assert("Pre-condition: booking is valid when matching for CO2 calculation", result.IsValid);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)));
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull("tbDO", tbDO);
			AssertEquals("tbDO.InstructionCollection.Count", 4, tbDO.InstructionCollection.Count);

			AssertWeightCollection("tbDO.InstructionCollection[0].WeightCollection", GetActionsForInstruction(result, pic1), tbDO.InstructionCollection[0].WeightCollection);
			AssertWeightCollection("tbDO.InstructionCollection[1].WeightCollection", GetActionsForInstruction(result, pic2), tbDO.InstructionCollection[1].WeightCollection);
			AssertWeightCollection("tbDO.InstructionCollection[2].WeightCollection", GetActionsForInstruction(result, pic3), tbDO.InstructionCollection[2].WeightCollection);
			AssertWeightCollection("tbDO.InstructionCollection[3].WeightCollection", GetActionsForInstruction(result, dlv), tbDO.InstructionCollection[3].WeightCollection);
		}

		public void TestPopulateBusinessObject_IncludesParentJobId_WhenHostSupporterIsNotNull()
		{
			// Arrange
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)), shipment as ICO2eCalculationSupporter);
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNotNull("AddInfoCollection should not be null", tbDO.AddInfoCollection);
			AssertEquals(2, tbDO.AddInfoCollection.Count);
			AssertEquals("ParentJobType", tbDO.AddInfoCollection[0].Key);
			AssertEquals("ForwardingShipment", tbDO.AddInfoCollection[0].Value);
			AssertEquals("ParentJobId", tbDO.AddInfoCollection[1].Key);
			AssertEquals("S01", tbDO.AddInfoCollection[1].Value);
		}

		public void TestPopulateBusinessObject_AddInfoCollection_Is_Null_WhenHostSupporterIsNull()
		{
			// Arrange
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);

			// Act
			var writer = new DtbBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, booking)), null);
			var tbDO = writer.GetDataObject(booking);

			// Assert
			AssertNull("AddInfoCollection should be null", tbDO.AddInfoCollection);
		}

		void AssertAddress(string assertionMessage, OrgAddress expected, OrganizationAddress actual)
		{
			AssertEquals($"{assertionMessage}.GeoLocation.Latitude", expected.OA_Latitude, actual.GeoLocation.Latitude);
			AssertEquals($"{assertionMessage}.GeoLocation.Longitude", expected.OA_Longitude, actual.GeoLocation.Longitude);
			AssertEquals($"{assertionMessage}.Country.Code", expected.Country.Code, actual.Country.Code);
			AssertEquals($"{assertionMessage}.Country.Name", expected.Country.Description, actual.Country.Name);
			AssertEquals($"{assertionMessage}.Postcode", expected.Postcode, actual.Postcode);
			AssertEquals($"{assertionMessage}.City", expected.City, actual.City);
			AssertEquals($"{assertionMessage}.Port.Code", expected.ClosestPort, actual.Port.Code);
		}

		void AssertWeightCollection(string assertionMessage, List<ICO2eMatchAction> expected, DataObjectList<WeightData> actual)
		{
			AssertEquals(assertionMessage + " Count", expected.Count, actual.Count);
			for (var i = 0; i < expected.Count; i++)
			{
				var expectedDirection = expected[i].IsPickup ? DirectionType.Load : DirectionType.Unload;
				AssertEquals($"{assertionMessage}.WeightData[{i}].Direction", expectedDirection, actual[i].Direction);
				AssertEquals($"{assertionMessage}.WeightData[{i}].TotalWeight", expected[i].Weight, actual[i].TotalWeight);
				AssertEquals($"{assertionMessage}.WeightData[{i}].TotalWeightUnit.Code", expected[i].WeightUQ, actual[i].TotalWeightUnit.Code);
				AssertContainerProperties($"{assertionMessage}.WeightData[{i}]", expected[i], actual[i]);
			}
		}

		void AssertContainerProperties(string assertionMessage, ICO2eMatchAction expected, WeightData actual)
		{
			var divot = expected.GetDivot();
			if (divot.IsContainerised)
			{
				var container = divot.Package.Container;
				var numOfTEU = container.ContainerType.RC_TEU * expected.Quantity;
				var tonnesPerTEU = numOfTEU > 0 && !expected.IsEmptyContainer ? Constants.Weight.ConvertSafe(divot.Package.GoodsWeight, divot.WeightUQ, Constants.Weight.Tonnes) / numOfTEU : 0;
				var emptyWeightPerTEU = container.ContainerType.RC_TareWeight / (numOfTEU > 0 ? numOfTEU : 1);

				AssertEquals($"{assertionMessage}.ContainerId", divot.PackageID, actual.ContainerJobID);
				AssertEquals($"{assertionMessage}.TEU.NumberOfTEU", numOfTEU, actual.TEU.NumberOfTEU);
				AssertEquals($"{assertionMessage}.TEU.TonnesPerTEU", Utilities.Round(tonnesPerTEU, 6), actual.TEU.TonnesPerTEU);
				AssertEquals($"{assertionMessage}.TEU.ContainerEmptyWeightPerTEU", Utilities.Round(emptyWeightPerTEU, 6), actual.TEU.ContainerEmptyWeightPerTEU);
				AssertEquals($"{assertionMessage}.TEU.ContainerEmptyWeightPerTEUUnit.Code", "KG", actual.TEU.ContainerEmptyWeightPerTEUUnit.Code);
			}
			else
			{
				AssertNull($"{assertionMessage}.TEU - null when package contains no container", actual.TEU);
			}
		}

		List<ICO2eMatchAction> GetActionsForInstruction(CO2eMatchResult matchResult, DtbBookingInstruction instruction)
		{
			return matchResult.Actions.Where(x => x.GetDivot().Instruction.PKEquals(instruction)).ToList();
		}

		OrgAddress PopulateAddress(OrgHeader header, ZString shortCode, ZDecimal latitude, ZDecimal longitude, ZString postcode, ZString countryCode, ZString closestPort)
		{
			var address = Helper.CreateOrgAddress(Factory, header, shortCode);
			address.OA_Longitude = longitude;
			address.OA_Latitude = latitude;
			address.OA_PostCode = postcode;
			address.OA_RN_NKCountryCode = countryCode;
			address.ClosestPort = closestPort;
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			return address;
		}
	}
}
