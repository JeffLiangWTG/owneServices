using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	sealed class ExportNotificationBuilderTest : TestCaseWithFactory
	{
		public void TestSendingPartyCodeOfCurrentBranch_PSN()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("ANR", "AntwerpBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSNxxx", Constants.CountryCodes.Belgium);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNxxx", Constants.CountryCodes.Belgium);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Belgium);
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "PSNxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.CodeTypes.PortSystemNumber, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Belgium, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestSendingPartyCodeOfCurrentBranch_DUN()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("ANR", "AntwerpBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNxxx", Constants.CountryCodes.Australia);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Belgium);
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "DUNxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Australia, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestSendingPartyCodeOfCurrentBranch_EOR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("NLCOMP", true, true, "NLRTM");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
			var testCompany = testObjectCreator.CreateNewCompany("NL", "NL", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("RTM", "RotterdamBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Netherlands);
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "EORIxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Netherlands, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestSendingPartyCodeOfCurrentCompany_PSN()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "PSNxxx", Constants.CountryCodes.Belgium);
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNxxx", Constants.CountryCodes.Belgium);
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Belgium);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "PSNxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.CodeTypes.PortSystemNumber, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Belgium, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestSendingPartyCodeOfCurrentCompany_DUN()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNxxx", Constants.CountryCodes.Australia);
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Belgium);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "DUNxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Australia, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestSendingPartyCodeOfCurrentCompany_EOR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("NLCOMP", true, true, "NLRTM");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
			var testCompany = testObjectCreator.CreateNewCompany("NL", "NL", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Netherlands);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();
				AssertEquals("SendingPartyCode", "EORIxxx", exportNotification.SendingPartyCode.Value);
				AssertEquals("SendingPartyCode", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, exportNotification.SendingPartyCode.Type.Code);
				AssertEquals("SendingPartyCode", Constants.CountryCodes.Netherlands, exportNotification.SendingPartyCode.CountryOfIssue.Code);
			}
		}

		public void TestBuildExportContainers()
		{
			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("ConsolNumber", "C20201209", exportNotification.ConsolNumber);
			AssertEquals("PortOfOrigin", "BEANR", exportNotification.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "AUSYD", exportNotification.PortOfDestination.Code);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, exportNotification.ContainerMode.Code);
			AssertEquals("ShipmentType", Constants.TransportModes.Sea, exportNotification.ShipmentType.Code);
			AssertEquals("TransportModeToTerminal", Constants.TransportModes.Sea, exportNotification.TransportModeToTerminal.Code);
			AssertEquals("VesselType", Constants.VesselType.Barge, exportNotification.VesselType);
			AssertEquals("Terminal", "BEANR", exportNotification.Terminal);
			AssertEquals("BookingReference", "B20201025", exportNotification.BookingReference);
			AssertEquals("ContainerNumber", "TBNU1111111", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumber);

			AssertEquals("MRN", "MRN_CONT_111", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRN);
			AssertEquals("CustomsOfficeCode", "BE1010", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode);

			AssertEquals("MRN", "MRN222", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(1).MRN);
			AssertEquals("CustomsOfficeCode", "BE2020", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(1).CustomsOfficeCode);
		}

		public void TestBuildExportVehicles()
		{
			var consol = CreateConsol("RORO", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("ConsolNumber", "C20201209", exportNotification.ConsolNumber);
			AssertEquals("PortOfOrigin", "BEANR", exportNotification.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "AUSYD", exportNotification.PortOfDestination.Code);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, exportNotification.ContainerMode.Code);
			AssertEquals("ShipmentType", Constants.TransportModes.Sea, exportNotification.ShipmentType.Code);
			AssertEquals("TransportModeToTerminal", Constants.TransportModes.Sea, exportNotification.TransportModeToTerminal.Code);
			AssertEquals("VesselType", Constants.VesselType.Barge, exportNotification.VesselType);
			AssertEquals("Terminal", "BEANR", exportNotification.Terminal);
			AssertEquals("BookingReference", "B20201025", exportNotification.BookingReference);
			AssertEquals("VIN", "VIN11111117777777", exportNotification.PackLines.ElementAtOrDefault(0).VIN);

			AssertEquals("MRN", "MRN111", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRN);
			AssertEquals("CustomsOfficeCode", "BE1010", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode);
		}

		public void TestBuildVesselType()
		{
			var consol = CreateConsol("CONT", Constants.TransportModes.Road, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("TransportModeToTerminal", Constants.TransportModes.Road, exportNotification.TransportModeToTerminal.Code);
			AssertEquals("VesselType", "TR", exportNotification.VesselType);

			consol = CreateConsol("CONT", Constants.TransportModes.Rail, false);
			exportNotificationBuilder = new ExportNotificationBuilder(consol);
			exportNotification = exportNotificationBuilder.Build();

			AssertEquals("TransportModeToTerminal", Constants.TransportModes.Rail, exportNotification.TransportModeToTerminal.Code);
			AssertEquals("VesselType", "RL", exportNotification.VesselType);
		}

		public void TestAddresses()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
				var exportNotificationBuilder = new ExportNotificationBuilder(consol);
				var exportNotification = exportNotificationBuilder.Build();

				AssertionHelper.AssertAddressData(testCompany.OrgProxy?.MainAddress, exportNotification.SendingForwarderAddress);
				AssertionHelper.AssertAddressData(consol.DepartureCTOAddress, exportNotification.DepartureCTOAddress);
			}
		}

		public void TestCurrentUser()
		{
			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertionHelper.AssertCurrentUserAddressData(exportNotification.CurrentUser);
		}

		public void TestSendingPartycodeValidations()
		{
			const string expectedError = "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN or DUN or EOR.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertHasMessageError("Not filled in", exportNotification.SendingPartyCode.ValueInfo, expectedError);
		}

		public void TestMRNValidations()
		{
			const string expectedError = "Please enter Document Number in Shipment > Packing > Export Ref Number or Shipment > Basic Registration > Details > Entry Details.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("MRN", "MRN_CONT_111", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRN);
			AssertNoMessageError("No error", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRNInfo, expectedError);

			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRN = ZString.Empty;
			AssertHasMessageError("Not filled in", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).MRNInfo, expectedError);
		}

		public void TestCustomsDocumentCodeEmptyValidations()
		{
			const string expectedError = "Please select an entry type from drop down list.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertHasMessageError("Not filled in", ((CodeDescription)exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsDocumentCode).CodeInfo, expectedError);
		}

		public void TestCustomsDocumentCodeValidations()
		{
			const string expectedError = "Entry type does not match with a value of the drop down list. Please select an entry type from drop down list.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsDocumentCode.Code = "AZERTYUIOP";
			AssertHasMessageError("CustomsDocumentCode contains a value not present in the document codes", ((CodeDescription)exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsDocumentCode).CodeInfo, expectedError);
		}

		public void TestCustomsOfficeCodeValidations()
		{
			const string expectedFormatError = "Incorrect format - Office code must be maximum 8 characters long.";
			const string expectedMandatoryError = "Customs Office Code is required when Entry Type is 'T' (Transit declaration).";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("CustomsOfficeCode", "BE1010", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode);
			AssertNoMessageError("No error", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCodeInfo, expectedFormatError);

			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode = "123456789";
			AssertHasMessageError("To long", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCodeInfo, expectedFormatError);

			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsDocumentCode.Code = DocumentCodes.Codes.T;
			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode = "ABC";
			AssertNoMessageError("CustomsOfficeCode No Error", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCodeInfo, expectedMandatoryError);

			exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCode = string.Empty;
			AssertHasMessageError("CustomsOfficeCode Mandatory Error", exportNotification.PackLines.ElementAtOrDefault(0).MovementReferenceNumbers.ElementAtOrDefault(0).CustomsOfficeCodeInfo, expectedMandatoryError);
		}

		public void TestBookingReferenceValidations()
		{
			const string expectedFormatError = "Incorrect format - Booking reference must be maximum 35 characters long.";
			const string expectedMandatoryError = "Booking Reference is required.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("BookingReference", "B20201025", exportNotification.BookingReference);
			AssertNoMessageError("No error", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertNoMessageError("No Error", exportNotification.BookingReferenceInfo, expectedMandatoryError);

			exportNotification.BookingReference = "1234567890123456789012345678901234567890";
			AssertEquals("BookingReference", "1234567890123456789012345678901234567890", exportNotification.BookingReference);
			AssertHasMessageError("To long", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertNoMessageError("No Error", exportNotification.BookingReferenceInfo, expectedMandatoryError);

			exportNotification.BookingReference = string.Empty;
			AssertEquals("BookingReference", string.Empty, exportNotification.BookingReference);
			AssertNoMessageError("No error", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertHasMessageError("No Error", exportNotification.BookingReferenceInfo, expectedMandatoryError);

			exportNotification.BookingReference = "B20201025";
			exportNotification.IsFerryTerminal = true;
			AssertEquals("BookingReference", "B20201025", exportNotification.BookingReference);
			AssertNoMessageError("No error", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertNoMessageError("No Error", exportNotification.BookingReferenceInfo, expectedMandatoryError);

			exportNotification.BookingReference = "1234567890123456789012345678901234567890";
			AssertEquals("BookingReference", "1234567890123456789012345678901234567890", exportNotification.BookingReference);
			AssertHasMessageError("To long", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertNoMessageError("No Error", exportNotification.BookingReferenceInfo, expectedMandatoryError);

			exportNotification.BookingReference = string.Empty;
			AssertEquals("BookingReference", string.Empty, exportNotification.BookingReference);
			AssertNoMessageError("No error", exportNotification.BookingReferenceInfo, expectedFormatError);
			AssertHasMessageError("Empty and Ferry Terminal", exportNotification.BookingReferenceInfo, expectedMandatoryError);
		}

		public void TestContainerValidations()
		{
			const string expectedContainerFormatError = "Incorrect format - Container number must start with 4 letters, followed by 7 numbers.";
			const string expectedFerryMandatoryError = "Equipment Number is required.";
			const string expectedFerryFormatError = "Incorrect format - Equipment number must be maximum 17 characters long.";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("ContainerNumber", "TBNU1111111", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumber);
			AssertNoMessageError("No error", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedContainerFormatError);

			consol.Containers.Cast<ForwardingContainer>().ElementAtOrDefault(0).JC_ContainerNum = "TBNU";
			exportNotification = exportNotificationBuilder.Build();
			AssertHasMessageError("Doesn't have numbers", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedContainerFormatError);

			consol.Containers.Cast<ForwardingContainer>().ElementAtOrDefault(0).JC_ContainerNum = "1234567";
			exportNotification = exportNotificationBuilder.Build();
			AssertHasMessageError("Doesn't have characters", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedContainerFormatError);

			consol.DepartureCTOAddress.Header.OH_IsFerryWaterTerminal = true;
			consol.Containers.Cast<ForwardingContainer>().ElementAtOrDefault(0).JC_ContainerNum = "Ferry123";
			exportNotification = exportNotificationBuilder.Build();
			AssertNoMessageError("Ferry Equipment Number No error", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedFerryMandatoryError);

			consol.Containers.Cast<ForwardingContainer>().ElementAtOrDefault(0).JC_ContainerNum = string.Empty;
			exportNotification = exportNotificationBuilder.Build();
			AssertHasMessageError("Ferry Equipment Number Mandatory Error", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedFerryMandatoryError);

			consol.Containers.Cast<ForwardingContainer>().ElementAtOrDefault(0).JC_ContainerNum = "12345678901234567890";
			exportNotification = exportNotificationBuilder.Build();
			AssertHasMessageError("Ferry Equipment Number", exportNotification.PackLines.ElementAtOrDefault(0).ContainerNumberInfo, expectedFerryFormatError);
		}

		public void TestVINValidations()
		{
			const string expectedError = "Incorrect format - VIN must be 17 characters long.";

			var consol = CreateConsol("RORO", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("VIN", "VIN11111117777777", exportNotification.PackLines.ElementAtOrDefault(0).VIN);
			AssertNoMessageError("No error", exportNotification.PackLines.ElementAtOrDefault(0).VINInfo, expectedError);

			exportNotification.PackLines.ElementAtOrDefault(0).VIN = "VIN";
			AssertHasMessageError("Shorter than 17 characters", exportNotification.PackLines.ElementAtOrDefault(0).VINInfo, expectedError);

			exportNotification.PackLines.ElementAtOrDefault(0).VIN = "VIN111111177777779999999";
			AssertHasMessageError("Longer than 17 characters", exportNotification.PackLines.ElementAtOrDefault(0).VINInfo, expectedError);
		}

		public void TestTerminalValidations()
		{
			const string expectedError = "Terminal is required and needs to start with BEANR or BEZEE (Consol > Departure > CTO Address > Registration Code PSN).";

			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("Terminal", "BEANR", exportNotification.Terminal);
			AssertNoMessageError("No error", exportNotification.TerminalInfo, expectedError);
			exportNotification.Terminal = "BEZEE";
			AssertEquals("Terminal", "BEZEE", exportNotification.Terminal);
			AssertNoMessageError("No error", exportNotification.TerminalInfo, expectedError);

			exportNotification.Terminal = "BEANR5555566666";
			AssertNoMessageError("No error", exportNotification.TerminalInfo, expectedError);
			exportNotification.Terminal = "BEZEE5555566666";
			AssertNoMessageError("No error", exportNotification.TerminalInfo, expectedError);

			exportNotification.Terminal = "AUSYD";
			AssertHasMessageError("Not BEANR or BEZEE", exportNotification.TerminalInfo, expectedError);
			exportNotification.Terminal = "AUSYD5555566666";
			AssertHasMessageError("Not BEANR or BEZEE", exportNotification.TerminalInfo, expectedError);
		}

		public void TestTerminalFromPSNValueCTO()
		{
			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, true);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("Teminal PCS from CTO:", "BEANR1111122222", exportNotification.Terminal);
			AssertNoMessageErrors("No error", exportNotification.TerminalInfo);
		}

		public void TestIsFerryTerminal()
		{
			var consol = CreateConsol("CONT", Constants.TransportModes.Sea, true);
			var exportNotificationBuilder = new ExportNotificationBuilder(consol);
			var exportNotification = exportNotificationBuilder.Build();

			AssertEquals("Is Not Ferry Terminal:", false, exportNotification.IsFerryTerminal);

			consol.DepartureCTOAddress.Header.OH_IsFerryWaterTerminal = true;
			exportNotification = exportNotificationBuilder.Build();

			AssertEquals("Is Ferry Terminal:", true, exportNotification.IsFerryTerminal);
		}

		#region Implementation

		ForwardingConsol CreateConsol(ZString type, ZString transportModeToTerminal, ZBool terminalCodeFromPSNFromCTO)
		{
			var consol = Factory.New<ForwardingConsol>();

			if (type == "CONT")
			{
				consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			}
			else
			{
				consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			}

			consol.JK_UniqueConsignRef = "C20201209";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "BEANR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "B20201025";

			CreatePackingLinesAndContainers(consol, type);
			CreateTransports(consol, transportModeToTerminal);
			CreateConsolAddresses(consol, terminalCodeFromPSNFromCTO);

			return consol;
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol, ZString type)
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SH0001000";
			shipment1.JS_HouseBill = "H0000056";
			shipment1.JS_RL_NKOrigin = "BEANR";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_MarksAndNumbers = "Marks";
			shipment1.JS_GoodsDescription = "Goods description";
			shipment1.CustomsEntryNumberType = "MRN";
			shipment1.CustomsEntryNumber = "MRN111";
			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 2;
			packingLine1.JL_F3_NKPackType = "PLT";
			packingLine1.JL_ActualWeight = 200;
			packingLine1.JL_ActualWeightUQ = "KG";
			packingLine1.JL_ActualVolume = 300;
			packingLine1.JL_ActualVolumeUQ = "M3";
			packingLine1.JL_DetailedDescription = "pack1";
			packingLine1.JL_ContainerPackingOrder = 1;

			if (type == "CONT")
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TBNU1111111";

				packingLine1.SetContainer(consol, container);
				packingLine1.JL_ExportRefNumber = "MRN_CONT_111";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "SH0002000";
				shipment2.JS_HouseBill = "H0000056";
				shipment2.JS_RL_NKOrigin = "BEANR";
				shipment2.JS_RL_NKDestination = "AUSYD";
				shipment2.JS_F3_NKPackType = "PLT";
				shipment2.JS_MarksAndNumbers = "Marks";
				shipment2.JS_GoodsDescription = "Goods description";
				shipment2.CustomsEntryNumberType = "MRN";
				shipment2.CustomsEntryNumber = "MRN222";
				shipment2.OuterPackLines.RemoveAndDeleteAll();

				var packingLine2 = shipment2.OuterPackLines.AddNew();
				packingLine2.JL_PackageCount = 2;
				packingLine2.JL_F3_NKPackType = "PLT";
				packingLine2.JL_ActualWeight = 200;
				packingLine2.JL_ActualWeightUQ = "KG";
				packingLine2.JL_ActualVolume = 300;
				packingLine2.JL_ActualVolumeUQ = "M3";
				packingLine2.JL_DetailedDescription = "pack2";
				packingLine2.JL_ContainerPackingOrder = 1;

				packingLine2.SetContainer(consol, container);

				var declaration2 = Factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.CustomsOfficeCollection.RemoveAndDeleteAll();
				var officeOfExport2 = declaration2.CustomsOfficeCollection.AddNew();
				officeOfExport2.CY_Code = BelgianPortsConstants.EuOfficeCodesTypes.ActualExitOffice;
				officeOfExport2.CY_Data = "BE2020";

				shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			}
			else
			{
				packingLine1.JL_VehicleColor = "RED";
				packingLine1.JL_VehicleMake = "Maserati ";
				packingLine1.JL_VehicleModel = "Quattroporte";
				packingLine1.JL_VehicleNumberOfDoors = 5;
				packingLine1.JL_VehicleYear = 2020;
				packingLine1.JL_RefNumber = "VIN11111117777777";

				shipment1.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			}

			var declaration1 = Factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			declaration1.CustomsOfficeCollection.RemoveAndDeleteAll();
			var officeOfExport1 = declaration1.CustomsOfficeCollection.AddNew();
			officeOfExport1.CY_Code = BelgianPortsConstants.EuOfficeCodesTypes.OfficeOfExit;
			officeOfExport1.CY_Data = "BE1010";
		}

		void CreateTransports(ForwardingConsol consol, ZString transportModeToTerminal)
		{
			var preTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			preTransport.JW_LegOrder = 1;
			preTransport.JW_TransportMode = transportModeToTerminal;
			preTransport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			preTransport.JW_RL_NKLoadPort = "BEMAL";
			preTransport.JW_RL_NKDiscPort = "BEANR";
			preTransport.JW_ETD = new ZDateTime(2020, 10, 1);
			preTransport.JW_VoyageFlight = "CC789";

			if (transportModeToTerminal == Constants.TransportModes.Sea)
			{
				var preVessel = Factory.New<RefVessel>();
				preVessel.RV_Code = "Stoomboot van Zwarte Piet";

				preVessel.RV_VesselType = Constants.VesselType.Barge;
				preVessel.RV_LloydsNumber = "LYDS456";
				preVessel.RV_RadioCallSign = "Radio456";
				preTransport.JW_Vessel = preVessel.RV_Code;
			}

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 2;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "BEANR";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = "MSC Antwerp";
			transport.JW_VoyageFlight = "V111";
			transport.JW_ETD = new ZDateTime(2020, 10, 15);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Stoomboot van Sinterklaas";
			vessel.RV_VesselType = Constants.VesselType.CargoVessel;
			vessel.RV_LloydsNumber = "LYDS123";
			vessel.RV_RadioCallSign = "Radio123";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "CC456";
		}

		void CreateConsolAddresses(ForwardingConsol consol, ZBool terminalCodeFromPSNFromCTO)
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "MAERSK";
			sendingForwarder.OH_RL_NKClosestPort = "DKAAL";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Aalborg";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			departureCTO.OH_FullName = "YUMMY";
			departureCTO.OH_RL_NKClosestPort = "BEANR";
			departureCTO.MainAddress.Address1 = "Unit 200";
			departureCTO.MainAddress.Address2 = "55 Why Lane";
			departureCTO.MainAddress.City = "Antwerp";
			departureCTO.MainAddress.Postcode = "2000";
			departureCTO.MainAddress.OA_RN_NKCountryCode = "BE";
			departureCTO.MainAddress.Header.OH_IsFerryWaterTerminal = false;

			if (terminalCodeFromPSNFromCTO)
			{
				departureCTO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "BEANR1111122222", Constants.CountryCodes.Belgium);
			}

			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
		}

		#endregion
	}
}
