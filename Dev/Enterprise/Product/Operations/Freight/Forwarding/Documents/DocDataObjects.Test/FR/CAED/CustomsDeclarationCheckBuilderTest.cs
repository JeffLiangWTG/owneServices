using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CustomsDeclarationCheckBuilderTest : TestCaseWithFactory
	{
		public void TestAPPlusCodesCurrentBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testOrgProxy = testObjectCreator.CreateOrgHeader("FRCOMP", true, true, "FRPAR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", GlbCompany.CurrentCompany, testOrgProxy);
			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "MAIN_BRANCH_SON123", Core.Constants.CountryCodes.France);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "MAIN_BRANCH_CI5123", Core.Constants.CountryCodes.France);

			var consolMode = "FCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);

			Factory.Save();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, consolMode, false, false);

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "MAIN_BRANCH_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "MAIN_BRANCH_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "EMPTY_BRANCH_SON123", Core.Constants.CountryCodes.France);
			testBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "EMPTY_BRANCH_CI5123", Core.Constants.CountryCodes.France);

			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "EMPTY_BRANCH_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "EMPTY_BRANCH_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}
		}

		public void TestAPPlusCodesCurrentBranchWithUnloco()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testOrgProxy = testObjectCreator.CreateOrgHeader("CACOMP", true, true, "CAQUE");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", GlbCompany.CurrentCompany, testOrgProxy);
			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "MAIN_BRANCH_SON123", Core.Constants.CountryCodes.France);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "MAIN_BRANCH_CI5123", Core.Constants.CountryCodes.France);

			var consolMode = "FCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);

			Factory.Save();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, consolMode, false, false);

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "MAIN_BRANCH_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "MAIN_BRANCH_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "EMPTY_BRANCH_SON123", Core.Constants.CountryCodes.France);
			testBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "EMPTY_BRANCH_CI5123", Core.Constants.CountryCodes.France);

			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "EMPTY_BRANCH_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "EMPTY_BRANCH_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}
		}

		public void TestAPPlusCodesCurrentCompany()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testOrgProxy = testObjectCreator.CreateOrgHeader("FRCOMP", true, true, "FRPAR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var testCompany = testObjectCreator.CreateNewCompany("YYY", "FR", orgProxy: testOrgProxy);
			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "MAIN_COMPANY_SON123", Core.Constants.CountryCodes.France);
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "MAIN_COMPANY_CI5123", Core.Constants.CountryCodes.France);

			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			Factory.Save();

			var consolMode = "FCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, consolMode, false, false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "MAIN_COMPANY_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "MAIN_COMPANY_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "EMPTY_COMPANY_SON123", Core.Constants.CountryCodes.France);
			testCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "EMPTY_COMPANY_CI5123", Core.Constants.CountryCodes.France);

			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "EMPTY_COMPANY_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "EMPTY_COMPANY_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}
		}

		public void TestAPPlusCodesCurrentCompanyWithUnloco()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testOrgProxy = testObjectCreator.CreateOrgHeader("CACOMP", true, true, "CAQUE");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var testCompany = testObjectCreator.CreateNewCompany("YYY", "CA", orgProxy: testOrgProxy);
			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "MAIN_COMPANY_SON123", Core.Constants.CountryCodes.France);
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "MAIN_COMPANY_CI5123", Core.Constants.CountryCodes.France);

			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			Factory.Save();

			var consolMode = "FCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, consolMode, false, false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "MAIN_COMPANY_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "MAIN_COMPANY_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "EMPTY_COMPANY_SON123", Core.Constants.CountryCodes.France);
			testCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "EMPTY_COMPANY_CI5123", Core.Constants.CountryCodes.France);

			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
				AssertEquals("SendingPartySONCode", "EMPTY_COMPANY_SON123", customsDeclarationCheck.SendingPartySONCode.Value);
				AssertEquals("SendingPartyCI5Code", "EMPTY_COMPANY_CI5123", customsDeclarationCheck.SendingPartyCI5Code.Value);
			}
		}

		public void TestBuildFCLExportSingleContainer()
		{
			var consolMode = "FCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, consolMode, false, false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
			AssertNotNull(customsDeclarationCheck);

			AssertionHelper.AssertAddressData(GlbCompany.CurrentCompany.OrgProxy?.MainAddress, customsDeclarationCheck.CurrentUser);

			AssertEquals("CTOSONCode", "EMPTY_CTO_SON123", customsDeclarationCheck.CTOSONCode.Value);
			AssertEquals("CTOCI5Code", "EMPTY_CTO_CI5123", customsDeclarationCheck.CTOCI5Code.Value);

			AssertEquals("ExportBrokerSIRETCode", "EMPTY_EXPBROKER_SIRET123", customsDeclarationCheck.DeclarantsSIRETNumber.Value);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, customsDeclarationCheck.ShipmentNumber);
			AssertEquals("TotalNumberOfPackages", shipment.JS_OuterPacks, customsDeclarationCheck.TotalNumberOfPacks);
			AssertEquals("PackingType", shipment.JS_F3_NKPackType, customsDeclarationCheck.PackageType.Code);

			AssertEquals("CommonAccessRef", "ExportCusRef1", customsDeclarationCheck.CommonAccessRef);

			AssertEquals("Container1 number", "TBNN1111111", customsDeclarationCheck.Containers.ElementAt(0).Number);

			AssertEquals("Port", "FRPAR", customsDeclarationCheck.Port.Code);
			AssertEquals("PortDuesCurrency", "EUR", customsDeclarationCheck.PortDuesCurrency.Code);

			AssertEquals("ContainerMode", "FCL", customsDeclarationCheck.ContainerMode.Code);
			AssertEquals("ShipmentType", "STD", customsDeclarationCheck.ShipmentType.Code);
			AssertEquals("PortOfDestination", "AUSYD", customsDeclarationCheck.PortOfDestination.Code);
			AssertEquals("PortOfOrigin", "FRPAR", customsDeclarationCheck.PortOfOrigin.Code);
		}

		public void TestBuildLCLImportMultipleContainers()
		{
			var consolMode = "LCL";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			PopulateShipment(shipment, "LCL", true, true, true);
			PopulateConsol(shipment, consolMode, true, true);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckImport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
			AssertNotNull(customsDeclarationCheck);

			AssertionHelper.AssertAddressData(GlbCompany.CurrentCompany.OrgProxy?.MainAddress, customsDeclarationCheck.CurrentUser);

			AssertEquals("CTOSONCode", "MAIN_CTO_SON123", customsDeclarationCheck.CTOSONCode.Value);
			AssertEquals("CTOCI5Code", "MAIN_CTO_CI5123", customsDeclarationCheck.CTOCI5Code.Value);

			AssertEquals("ExportBrokerSIRETCode", "MAIN_IMPBROKER_SIRET123", customsDeclarationCheck.DeclarantsSIRETNumber.Value);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, customsDeclarationCheck.ShipmentNumber);
			AssertEquals("TotalNumberOfPackages", shipment.JS_OuterPacks, customsDeclarationCheck.TotalNumberOfPacks);
			AssertEquals("PackingType", shipment.JS_F3_NKPackType, customsDeclarationCheck.PackageType.Code);

			AssertEquals("CommonAccessRef", "ECV00010001", customsDeclarationCheck.CommonAccessRef);

			AssertEquals("Container1 number", "TBNN1111111", customsDeclarationCheck.Containers.ElementAt(0).Number);
			AssertEquals("Container2 number", "TBNN2222222", customsDeclarationCheck.Containers.ElementAt(1).Number);
			AssertEquals("Container3 number", "TBNN3333333", customsDeclarationCheck.Containers.ElementAt(2).Number);

			AssertEquals("Port", "FRDUN", customsDeclarationCheck.Port.Code);
			AssertEquals("PortDuesCurrency", "EUR", customsDeclarationCheck.PortDuesCurrency.Code);

			AssertEquals("ContainerMode", "LCL", customsDeclarationCheck.ContainerMode.Code);
			AssertEquals("ShipmentType", "STD", customsDeclarationCheck.ShipmentType.Code);
			AssertEquals("PortOfDestination", "FRDUN", customsDeclarationCheck.PortOfDestination.Code);
			AssertEquals("PortOfOrigin", "AUSYD", customsDeclarationCheck.PortOfOrigin.Code);
		}

		public void TestContainerValidation()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, "FCL", false, false);

			shipment.Consols[0].Containers.RemoveAndDeleteAll();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var customsDeclarationCheckBuilder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = customsDeclarationCheckBuilder.Build();
			AssertNotNull(customsDeclarationCheck);

			AssertHasMessageError(customsDeclarationCheck.ContainerMessageInfo, "At least one container is required.");
		}

		public void TestCommonAccessRefValidation()
		{
			const string expectedError = "DOC number is required when the Shipment is packed into more than 1 container or ECV/ICV number when LCL.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, "FCL", true, false);

			AssertNull("Precondition: ExportConventional", shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, "FR"));
			AssertNull("Precondition: ImportConventional", shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ImportConventional, "FR"));

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var builder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = builder.Build();

			AssertEquals("CommonAccessRef is empty", "", customsDeclarationCheck.CommonAccessRef);
			AssertHasMessageError(customsDeclarationCheck.CommonAccessRefInfo, expectedError);

			shipment.Consols[0].Containers.Remove(shipment.Consols[0].Containers[2]);
			shipment.Consols[0].Containers.Remove(shipment.Consols[0].Containers[1]);

			customsDeclarationCheck = builder.Build();
			AssertEquals("CommonAccessRef is populated from container", "ExportCusRef1", customsDeclarationCheck.CommonAccessRef);
			AssertNoMessageError(customsDeclarationCheck.CommonAccessRefInfo, expectedError);
		}

		public void TestCustomsOfficCodeValidation()
		{
			const string expectedError = "Incorrect format - Customs Office Code should start with 'FR' followed by six numbers.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, "FCL", true, false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckExport
			};

			var builder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = builder.Build();

			var customsOfficeCode = customsDeclarationCheck.CustomsOfficeCode as CodeDescription;

			AssertEquals("Precondition", ZString.Empty, customsDeclarationCheck.CustomsOfficeCode.Code);
			AssertNoMessageError("No error when blank", customsOfficeCode.CodeInfo, expectedError);

			customsOfficeCode.Code = "AB123456";
			AssertHasMessageError("Doesn't start with FR", customsOfficeCode.CodeInfo, expectedError);

			customsOfficeCode.Code = "FR12AB56";
			AssertHasMessageError("Doesn't have all numberics following 'FR'", customsOfficeCode.CodeInfo, expectedError);

			customsOfficeCode.Code = "FR1234";
			AssertHasMessageError("Wrong number of numerics", customsOfficeCode.CodeInfo, expectedError);

			customsOfficeCode.Code = "";
			AssertNoMessageError("Code is optional", customsOfficeCode.CodeInfo, expectedError);

			customsOfficeCode.Code = "FR123456";
			AssertNoMessageError("Valid code", customsOfficeCode.CodeInfo, expectedError);
		}

		public void TestAddresses()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			PopulateShipment(shipment, "FCL", false, false, false);
			PopulateConsol(shipment, "FCL", true, false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.CustomsClearanceCheckImport
			};

			var builder = new CustomsDeclarationCheckBuilder(shipment, parameters);
			var customsDeclarationCheck = builder.Build();

			AssertionHelper.AssertAddressData(GlbCompany.CurrentCompany.OrgProxy?.MainAddress, customsDeclarationCheck.CurrentUser);
			AssertionHelper.AssertAddressData(shipment.Consols[0].SendingForwarderAddress, customsDeclarationCheck.SendingForwarderAddress);
			AssertionHelper.AssertAddressData(shipment.Consols[0].ReceivingForwarderAddress, customsDeclarationCheck.ReceivingForwarderAddress);
			AssertionHelper.AssertAddressData(shipment.ExportBroker.MainAddress, customsDeclarationCheck.ExportBrokerAddress);
			AssertionHelper.AssertAddressData(shipment.ImportBroker.MainAddress, customsDeclarationCheck.ImportBrokerAddress);
			AssertionHelper.AssertAddressData(shipment.Consols[0].DepartureCTOAddress, customsDeclarationCheck.DepartureCTOAddress);
			AssertionHelper.AssertAddressData(shipment.Consols[0].ArrivalCTOAddress, customsDeclarationCheck.ArrivalCTOAddress);
		}

		#region Implementation

		void PopulateShipment(ForwardingShipment shipment, ZString shipmentmode, ZBool includeECV, ZBool isImport, ZBool mainAddress)
		{
			PopulateBrokers(shipment, mainAddress);

			shipment.JS_PackingMode = shipmentmode;
			shipment.JS_ShipmentType = "STD";

			if (isImport)
			{
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "FRDUN";
				AssertEquals(true, shipment.IsImport());
			}
			else
			{
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "AUSYD";
				AssertEquals(false, shipment.IsImport());
			}

			shipment.JS_UniqueConsignRef = "S00001002";
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "PLT";

			foreach (PackLine packline in shipment.OuterPackLines)
			{
				packline.JL_RefNumber = "REF123";
			}

			if (includeECV)
			{
				var number = shipment.Numbers.AddNew();
				number.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
				number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				number.CE_EntryNum = "ECV00010001";
			}
		}

		void PopulateConsol(ForwardingShipment shipment, ZString consolMode, ZBool multipleContainers, ZBool mainAddress)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);

			consol.JK_ConsolMode = consolMode;
			consol.JK_RL_NKLoadPort = "FRPAR";
			consol.JK_RL_NKDischargePort = "FRDUN";

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg1.JW_RL_NKLoadPort = "FRPAR";
			transportLeg1.JW_RL_NKDiscPort = "FRDUN";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TBNN1111111";
			container1.JC_ExportDepotCustomsReference = "ExportCusRef1";
			container1.JC_ImportDepotCustomsReference = "ImportCusRef1";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			if (multipleContainers)
			{
				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "TBNN2222222";
				container2.JC_ExportDepotCustomsReference = "ExportCusRef2";
				container2.JC_ImportDepotCustomsReference = "ImportCusRef2";
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.SetContainer(consol, container2);

				var container3 = consol.Containers.AddNew();
				container3.JC_ContainerNum = "TBNN3333333";
				container3.JC_ExportDepotCustomsReference = "ExportCusRef3";
				container3.JC_ImportDepotCustomsReference = "ImportCusRef3";
				var packLine3 = shipment.OuterPackLines.AddNew();
				packLine3.SetContainer(consol, container3);
			}

			PopulateCTO(consol, mainAddress);
			PopulateForwarders(consol);
		}

		void PopulateBrokers(ForwardingShipment shipment, ZBool mainAddress)
		{
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			exportBroker.OH_FullName = "YUMMY";
			exportBroker.OH_RL_NKClosestPort = "FRPAR";
			exportBroker.MainAddress.Address1 = "Unit 200";
			exportBroker.MainAddress.Address2 = "55 Why Lane";
			exportBroker.MainAddress.City = "Paris";
			exportBroker.MainAddress.Postcode = "2000";
			exportBroker.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_FullName = "IMPBROKER";
			importBroker.OH_RL_NKClosestPort = "FRPAR";
			importBroker.MainAddress.Address1 = "Unit 500";
			importBroker.MainAddress.Address2 = "101 Why Lane";
			importBroker.MainAddress.City = "Paris";
			importBroker.MainAddress.Postcode = "2000";
			importBroker.MainAddress.OA_RN_NKCountryCode = "FR";
			shipment.JS_OH_ImportBroker = importBroker.PK;

			if (mainAddress)
			{
				exportBroker.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "MAIN_EXPBROKER_SIRET123", Core.Constants.CountryCodes.France);
				importBroker.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "MAIN_IMPBROKER_SIRET123", Core.Constants.CountryCodes.France);
			}
			else
			{
				exportBroker.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "EMPTY_EXPBROKER_SIRET123", Core.Constants.CountryCodes.France);
				importBroker.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "EMPTY_IMPBROKER_SIRET123", Core.Constants.CountryCodes.France);
			}
		}

		void PopulateCTO(ForwardingConsol consol, ZBool mainAddress)
		{
			var cto = Factory.NewWithValidTestData<OrgHeader>();
			cto.OH_FullName = "YUMMY";
			cto.OH_RL_NKClosestPort = "FRPAR";
			cto.MainAddress.Address1 = "Unit 200";
			cto.MainAddress.Address2 = "55 Why Lane";
			cto.MainAddress.City = "Paris";
			cto.MainAddress.Postcode = "2000";
			cto.MainAddress.OA_RN_NKCountryCode = "FR";
			consol.JK_OA_ArrivalCTOAddress = cto.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;

			if (mainAddress)
			{
				cto.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "MAIN_CTO_SON123", Core.Constants.CountryCodes.France);
				cto.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "MAIN_CTO_CI5123", Core.Constants.CountryCodes.France);
			}
			else
			{
				cto.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "EMPTY_CTO_SON123", Core.Constants.CountryCodes.France);
				cto.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "EMPTY_CTO_CI5123", Core.Constants.CountryCodes.France);
			}
		}

		void PopulateForwarders(ForwardingConsol consol)
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyCountrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France);
		}

		protected override void TearDown()
		{
			if (companyCountrySetter != null)
			{
				companyCountrySetter.Dispose();
				companyCountrySetter = null;
			}

			base.TearDown();
		}

		IDisposable companyCountrySetter;

		#endregion
	}
}
