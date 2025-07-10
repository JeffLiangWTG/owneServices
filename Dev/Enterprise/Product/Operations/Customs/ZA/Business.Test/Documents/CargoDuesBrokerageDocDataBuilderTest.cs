using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using static Enterprise.MasterFiles.Business.OrgCusCode;
using TransportBizoT = Enterprise.Freight.Business.Transport;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects.Testing
{
	sealed class CargoDuesBrokerageDocDataBuilderTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			var parameters = new DocDataObjectParameters("CargoDuesBrokerage - Load Coastwise", "data-store");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			Assert(true);
		}

		public void TestIsContainersCheckboxesDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsContainerised);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsContainerised);
			}
		}

		public void TestIsDeepSeaDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = NonZAPort1;
				declaration.JE_RL_NKPortOfLoading = NonZAPort2;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsDeepSea);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = ZAPort1;
				declaration.JE_RL_NKPortOfLoading = NonZAPort1;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsDeepSea);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = NonZAPort1;
				declaration.JE_RL_NKPortOfLoading = ZAPort1;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsDeepSea);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = ZAPort1;
				declaration.JE_RL_NKPortOfLoading = ZAPort2;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsDeepSea);
			}
		}

		public void TestIsTranshipDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var transport = declaration.Transports.AddNew();
				transport.JW_RL_NKLoadPort = NonZAPort1;
				transport.JW_RL_NKDiscPort = ZAPort1;
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport = declaration.Transports.AddNew();
				transport.JW_RL_NKLoadPort = ZAPort1;
				transport.JW_RL_NKDiscPort = NonZAPort2;
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsTranship);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var transport = declaration.Transports.AddNew();
				transport.JW_RL_NKLoadPort = ZAPort1;
				transport.JW_RL_NKDiscPort = ZAPort2;
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsTranship);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsTranship);
			}
		}

		public void TestIsCoastwiseDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = NonZAPort1;
				declaration.JE_RL_NKPortOfLoading = NonZAPort2;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsCoastwise);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = ZAPort1;
				declaration.JE_RL_NKPortOfLoading = ZAPort2;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsCoastwise);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = NonZAPort1;
				declaration.JE_RL_NKPortOfLoading = ZAPort2;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsCoastwise);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = ZAPort2;
				declaration.JE_RL_NKPortOfLoading = NonZAPort1;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsCoastwise);
			}
		}

		public void TestIsBulkDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsBulk);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsBulk);
			}
		}

		public void TestIsBreakBulkDefaults()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(false, result.IsBreakBulk);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals(true, result.IsBreakBulk);
			}

			Assert(true);
		}

		public void TestContainerModeDefaults()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			var result = BuildWithDefaultParameters(declaration);
			AssertEquals("BLK", result.ContainerMode.Code);
			AssertEquals("Bulk", result.ContainerMode.Description);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			result = BuildWithDefaultParameters(declaration);
			AssertEquals("BBK", result.ContainerMode.Code);
			AssertEquals("Break Bulk", result.ContainerMode.Description);
		}

		public void TestAgentDefaults()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_AgentOverride = agent.PK;
			var result = BuildWithDefaultParameters(declaration);
			AssertAddressData(agent.MainAddress, result.Agent);
		}

		public void TestSimpleMappingsDefaults()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			const string expLloydsNumber = "9999";
			const string expRadioCallsign = "1111";
			const string origin = ZAPort1;
			const string expOriginCountry = CountryCodes.SouthAfrica;
			const string destination = "AUSYD";
			const string expDestinationCountry = CountryCodes.Australia;
			const string expCustomsRegNo = "12345";
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ABCD";
			declaration.JE_VesselName = vessel.RV_Code;
			var customCode = declaration.ShippingLine.CustomsCodes.AddNew();
			customCode.OK_CustomsRegNo = expCustomsRegNo;
			customCode.OK_CodeType = CodeTypes.CarrierCode;
			customCode.OK_RN_NKCodeCountry = declaration.CountryCode;
			declaration.JE_RL_NKOrigin = origin;
			declaration.JE_RL_NKFinalDestination = destination;
			declaration.JE_RadioCallSign = expRadioCallsign;
			declaration.Vessel.RV_LloydsNumber = expLloydsNumber;
			var result = BuildWithDefaultParameters(declaration);
			AssertEquals("imo number is as expected", expLloydsNumber, result.IMONumber);
			AssertEquals(expRadioCallsign, result.RadioCallSign);
			AssertEquals(expOriginCountry, result.PlaceOfReceipt.Country.Code);
			AssertEquals(expDestinationCountry, result.PlaceOfDelivery.Country.Code);
			AssertEquals(expCustomsRegNo, result.CarrierCode);
		}

		public void TestPopulateCustomsContainerTerminalOperator()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = departureCTOAddress.MainAddress.PK;
			var result = BuildWithDefaultParameters(declaration);
			AssertAddressData(declaration.ContainerTerminalOperatorDocAddress, result.CustomsContainerTerminalOperator);
		}

		public void TestIsExportDocument()
		{
			void helper(string docTitle, bool isExport)
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var parameters = new DocDataObjectParameters(docTitle, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var result = builder.Build();
				AssertEquals(isExport, result.IsExportDocument);
			}

			helper(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, false);
			helper(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise, false);
			helper(DeclarationDocumentConstants.DocumentNames.CargoDuesExport, true);
			helper(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, true);
		}

		public void TestCarrierCode()
		{
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var result = BuildWithDefaultParameters(declaration);
				Assert(result.CarrierCode.IsEmpty);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				var customCode = declaration.ShippingLine.CustomsCodes.AddNew();
				customCode.OK_CustomsRegNo = "CCC123";
				customCode.OK_CodeType = CodeTypes.CarrierCode;
				customCode.OK_RN_NKCodeCountry = declaration.CountryCode;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals("CCC123", result.CarrierCode);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				var customCode = declaration.ShippingLine.CustomsCodes.AddNew();
				customCode.OK_CustomsRegNo = "TNP123";
				customCode.OK_CodeType = SouthAfricaCodeTypes.TNP;
				customCode.OK_RN_NKCodeCountry = declaration.CountryCode;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals("TNP123", result.CarrierCode);
			}

			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				var customCode = declaration.ShippingLine.CustomsCodes.AddNew();
				customCode.OK_CustomsRegNo = "TNP123";
				customCode.OK_CodeType = SouthAfricaCodeTypes.TNP;
				customCode.OK_RN_NKCodeCountry = declaration.CountryCode;
				customCode = declaration.ShippingLine.CustomsCodes.AddNew();
				customCode.OK_CustomsRegNo = "CCC123";
				customCode.OK_CodeType = CodeTypes.CarrierCode;
				customCode.OK_RN_NKCodeCountry = declaration.CountryCode;
				var result = BuildWithDefaultParameters(declaration);
				AssertEquals("TNP123", result.CarrierCode);
			}
		}

		public void TestEtaEtdDefaults()
		{
			var etdDate = new ZDateTime(2019, 1, 1);
			var etaDate = new ZDateTime(2029, 5, 6);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DateAtOrigin = etdDate;
			declaration.JE_DateAtFinalDestination = etaDate;
			var result = BuildWithDefaultParameters(declaration);
			AssertEquals(etaDate, result.Eta);
			AssertEquals(etdDate, result.Etd);
		}

		public void TestPortOfLoadingAndPortOfDischarge()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var transport = declaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = NonZAPort1;
			transport.JW_RL_NKDiscPort = NonZAPort2;
			transport.JW_TransportMode = TransportModes.Sea;
			transport = declaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = NonZAPort2;
			transport.JW_RL_NKDiscPort = ZAPort1;
			transport.JW_TransportMode = TransportModes.Sea;
			transport = declaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = ZAPort1;
			transport.JW_RL_NKDiscPort = ZAPort2;
			transport.JW_TransportMode = TransportModes.Sea;
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
			AssertEquals("discharge port of last SEA leg", ZAPort2, result.PortOfDischarge.Code);
			AssertEquals("load port of first SEA leg", NonZAPort1, result.PortOfLoading.Code);
		}

		public void TestTransportsDefaults()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var (importOrDischargeLeg, exportOrLoadLeg, beforeImportOrDischargeLeg, afterExportOrLoadLeg) = AddTransportsToDeclaration(declaration);
			void AssertTransportsDefaults(string docTitle, TransportBizoT mainLeg, TransportBizoT onForwardingLeg, TransportBizoT preCarriageLeg, RefUNLOCO expServicePort, ZString expTNPAArrivalNumber)
			{
				var parameters = new DocDataObjectParameters(docTitle, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var result = builder.Build();
				AssertEquals(result.ServicePort.Code, expServicePort.Code);
				AssertEquals(result.Transports.Main.Vessel.Name, mainLeg.JW_Vessel);
				AssertEquals(result.Transports.Main.VoyageFlightNumber, mainLeg.JW_VoyageFlight);
				AssertEquals(result.Transports.OnForwarding?.Vessel.Name, onForwardingLeg?.JW_Vessel);
				AssertEquals(result.Transports.OnForwarding?.VoyageFlightNumber, onForwardingLeg?.JW_VoyageFlight);
				AssertEquals(result.Transports.PreCarriage?.Vessel.Name, preCarriageLeg?.JW_Vessel);
				AssertEquals(result.Transports.PreCarriage?.VoyageFlightNumber, preCarriageLeg?.JW_VoyageFlight);
				AssertEquals(result.TNPAArrivalNumber, expTNPAArrivalNumber);
			}

			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			sailing1.Origin.JA_DepartReference = "ASDF5678";
			sailing1.Destination.JB_ArrivalReference = "QWER1234";
			exportOrLoadLeg.JW_JX = sailing1.PK;
			var importArrivalNumber = exportOrLoadLeg.Sailing.Destination.JB_ArrivalReference;
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			sailing2.Origin.JA_DepartReference = "ASDF5678";
			sailing2.Destination.JB_ArrivalReference = "QWER1234";
			importOrDischargeLeg.JW_JX = sailing2.PK;
			var exportArrivalNumber = importOrDischargeLeg.Sailing.Origin.JA_DepartReference;
			AssertTransportsDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, importOrDischargeLeg, null, beforeImportOrDischargeLeg, importOrDischargeLeg.DiscPort, importArrivalNumber);
			AssertTransportsDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesExport, exportOrLoadLeg, afterExportOrLoadLeg, null, exportOrLoadLeg.LoadPort, exportArrivalNumber);
			AssertTransportsDefaults(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise, importOrDischargeLeg, null, beforeImportOrDischargeLeg, importOrDischargeLeg.DiscPort, importArrivalNumber);
			AssertTransportsDefaults(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, exportOrLoadLeg, afterExportOrLoadLeg, null, exportOrLoadLeg.LoadPort, exportArrivalNumber);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestPopulateTransportLegs()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var (importOrDischargeLeg, exportOrLoadLeg, beforeImportOrDischargeLeg, afterExportOrLoadLeg) = AddTransportsToDeclaration(declaration);
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			sailing1.Origin.JA_DepartReference = "ASDF5678";
			sailing1.Destination.JB_ArrivalReference = "QWER1234";
			exportOrLoadLeg.JW_JX = sailing1.PK;
			var importArrivalNumber = exportOrLoadLeg.Sailing.Destination.JB_ArrivalReference;
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			sailing2.Origin.JA_DepartReference = "ASDF5678";
			sailing2.Destination.JB_ArrivalReference = "QWER1234";
			importOrDischargeLeg.JW_JX = sailing2.PK;
			var exportArrivalNumber = importOrDischargeLeg.Sailing.Origin.JA_DepartReference;

			importOrDischargeLeg.JW_RL_NKDiscPort = ZString.Empty;
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
		}

		public void TestContainerOperatorDefaults()
		{
			void AssertContainerOperatorDefaults(string docTitle)
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				var parameters = new DocDataObjectParameters(docTitle, "data-store");
				declaration.JE_VesselAgent = "VESS";
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var cargoDuesBrokerage = builder.Build();

				AssertEquals("containerOperator", string.Empty, cargoDuesBrokerage.ContainerOperator);
				AssertHasMessageError("Mesage Error", cargoDuesBrokerage.ContainerOperatorInfo, "Container Operator/Vessels Agent is required.");

				var ccc = declaration.ShippingLine.CustomsCodes.AddNew();
				ccc.OK_CustomsRegNo = "CCC123";
				ccc.OK_CodeType = CodeTypes.CarrierCode;
				ccc.OK_RN_NKCodeCountry = declaration.CountryCode;

				builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				cargoDuesBrokerage = builder.Build();

				AssertEquals("containerOperator", "CCC123", cargoDuesBrokerage.ContainerOperator);
				AssertNoMessageErrors("No Message Errors", cargoDuesBrokerage.ContainerOperatorInfo);

				var tnp = declaration.ShippingLine.CustomsCodes.AddNew();
				tnp.OK_CustomsRegNo = "TNP123";
				tnp.OK_CodeType = SouthAfricaCodeTypes.TNP;
				tnp.OK_RN_NKCodeCountry = declaration.CountryCode;

				builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				cargoDuesBrokerage = builder.Build();

				AssertEquals("containerOperator", "TNP123", cargoDuesBrokerage.ContainerOperator);
				AssertNoMessageErrors("No Message Errors", cargoDuesBrokerage.ContainerOperatorInfo);
			}

			AssertContainerOperatorDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesImport);
			AssertContainerOperatorDefaults(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise);
			AssertContainerOperatorDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesExport);
			AssertContainerOperatorDefaults(DeclarationDocumentConstants.DocumentNames.LoadCoastwise);
		}

		public void TestTerminalDefaults()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var (importOrDischargeLeg, exportOrLoadLeg, _, _) = AddTransportsToDeclaration(declaration);
			var oa1 = Factory.NewWithValidTestData<OrgAddress>();
			var oa2 = Factory.NewWithValidTestData<OrgAddress>();
			exportOrLoadLeg.JW_OA_DepartureLocation = oa1.PK;
			importOrDischargeLeg.JW_OA_ArrivalLocation = oa2.PK;
			declaration.ContainerTerminalOperatorDocAddress.AddressCode = "should not find I";
			AssertNotNull(importOrDischargeLeg.ArrivalLocation.CustomsCodes);
			AssertNotNull(exportOrLoadLeg.DepartureLocation.CustomsCodes);
			foreach (var customsCode in new[] { importOrDischargeLeg.ArrivalLocation.CustomsCodes.AddNew(), exportOrLoadLeg.DepartureLocation.CustomsCodes.AddNew() })
			{
				customsCode.OK_CodeType = SouthAfricaCodeTypes.BGV;
				customsCode.OK_CustomsRegNo = "should not find II";
			}

			var expImportCode = "import";
			var expExportCode = "export";
			var importCode = importOrDischargeLeg.ArrivalLocation.CustomsCodes.AddNew();
			importCode.OK_CodeType = SouthAfricaCodeTypes.TNP;
			importCode.OK_CustomsRegNo = expImportCode;
			var exportCode = exportOrLoadLeg.DepartureLocation.CustomsCodes.AddNew();
			exportCode.OK_CodeType = SouthAfricaCodeTypes.TNP;
			exportCode.OK_CustomsRegNo = expExportCode;
			AssertEquals("import or discharge leg was correctly setup", 2, importOrDischargeLeg.ArrivalLocation.CustomsCodes.Count);
			AssertEquals("export or load leg was correctly setup", 2, exportOrLoadLeg.DepartureLocation.CustomsCodes.Count);
			void AssertTerminalDefaults(string docTitle, ZString expCustomsRegNo)
			{
				var parameters = new DocDataObjectParameters(docTitle, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var result = builder.Build();
				AssertEquals("terminal uses the expected registry number", expCustomsRegNo, result.Terminal);
			}

			AssertTerminalDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, expImportCode);
			AssertTerminalDefaults(DeclarationDocumentConstants.DocumentNames.CargoDuesExport, expExportCode);
			AssertTerminalDefaults(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise, expImportCode);
			AssertTerminalDefaults(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, expExportCode);
		}

		public void TestTerminalExportsFallback()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AddTransportsToDeclaration(declaration);
			var addrCode = "abc";
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.MainAddress.OA_Address1 = "My Address";
			organization.CustomsCodes.AddNew(SouthAfricaCodeTypes.TNP, addrCode);
			declaration.ContainerTerminalOperatorDocAddress.AddressCode = addrCode;
			declaration.ContainerTerminalOperatorDocAddress.OrganisationPK = organization.PK;
			void AssertTerminalExportsFallback(string docTitle, ZString expTerminalCode)
			{
				var parameters = new DocDataObjectParameters(docTitle, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var result = builder.Build();
				AssertEquals("terminal uses the expected CTO code as a fallback", expTerminalCode, result.Terminal);
			}

			AssertTerminalExportsFallback(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, addrCode);
			AssertTerminalExportsFallback(DeclarationDocumentConstants.DocumentNames.CargoDuesExport, addrCode);
			AssertTerminalExportsFallback(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise, addrCode);
			AssertTerminalExportsFallback(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, addrCode);
		}

		public void TestTNPAOrderNumber()
		{
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			string myRfn = "123";
			logProvider.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, $"|MST=Cargo Dues - Import|DEP=TNPA|RFN={myRfn}");
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store", null, logProvider);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
			AssertEquals(myRfn, result.TNPAOrderNumber);
		}

		public void TestAllDocTitlesExistInDatabase()
		{
			void AssertDocTitlesExist(string docTitle)
			{
				var query = new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, docTitle);
				var pivot = Factory.LoadTop1<StmMenuTemplatePivot>(query);
				AssertNotNull(pivot);
			}

			foreach (var field in typeof(DeclarationDocumentConstants.DocumentNames).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
			{
				AssertDocTitlesExist((string)field.GetValue(null));
			}
		}

		public void TestContainersWhenContainerised()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ContainerMode = ContainerModes.Containerised;
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "containerNumber1";
			container1.CO_RC = refContainer1.PK;
			container1.CO_Weight = 1.00;
			var container2 = declaration.CusContainers.AddNew();
			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			container2.CO_ContainerNumber = "containerNumber1";
			container2.CO_RC = refContainer2.PK;
			container2.JobContainer.JC_IsEmptyContainer = true;
			container2.CO_Weight = 2000;
			container2.CO_WeightUQ = Weight.Grams;
			AddTransportsToDeclaration(declaration);
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
			AssertEquals("Total Number of packs", 0, result.TotalNumberOfPacks);
			AssertEquals(2, result.Containers.Count);
			var firstContainer = result.Containers.ToArray()[0];
			AssertEquals("Container Prefix & Number is correct", container1.CO_ContainerNumber, firstContainer.Number);
			AssertEquals("Number of Packages is correct", 0, firstContainer.PackCount);
			AssertEquals("Type of Packs is correct", container1.Container.RC_Code, firstContainer.Type.Code);
			AssertEquals("Gross mass is correct", container1.CO_Weight, firstContainer.GrossWeight.Value);
			AssertEquals("Gross mass unit is correct", ZUnit.Weight.KG.Code, firstContainer.GrossWeight.Unit.Code);
			var secondContainer = result.Containers.ToArray()[1];
			AssertEquals("Container Prefix & Number is correct", container2.CO_ContainerNumber, secondContainer.Number);
			AssertEquals("Number of Packages is correct", 0, secondContainer.PackCount);
			AssertEquals("Type of Packs is correct", container2.Container.RC_Code, secondContainer.Type.Code);
			AssertEquals("Gross mass is correct", (ZDecimal)2.00, secondContainer.GrossWeight.Value);
			AssertEquals("Gross mass unit is correct", ZUnit.Weight.KG.Code, secondContainer.GrossWeight.Unit.Code);
		}

		public void TestGoodsWhenBulkOrBreakBulk()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MarksAndNumbersShort = "marksAndNumbers";
			declaration.JE_TotalNoOfPacks = 5;
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.JE_GoodsDescription = "goodsDescription";
			declaration.JE_TotalWeight = 3000;
			declaration.JE_TotalWeightUnit = Weight.Grams;
			void AssertGoods(string containerMode)
			{
				declaration.JE_ContainerMode = containerMode;
				var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				var result = builder.Build();
				AssertEquals("Total Number of packs", 5, result.TotalNumberOfPacks);
				AssertEquals("ShipmentPackingInfos' count", 1, result.ShipmentPackingInfos.Count);
				AssertEquals(1, result.ShipmentPackingInfos.First().GoodsInfoCollection.Count);
				var firstContainer = result.ShipmentPackingInfos.First().GoodsInfoCollection.ToArray()[0];
				AssertEquals("Container Prefix & Number is correct", declaration.JE_MarksAndNumbersShort, firstContainer.MarksAndNos);
				AssertEquals("Number of Packages is correct", declaration.JE_TotalNoOfPacks, firstContainer.NumberOfPacks);
				AssertEquals("Type of Packs is correct", declaration.JE_TotalNoOfPacksPackType, firstContainer.PackType);
				AssertEquals("Description of goods is correct", declaration.JE_GoodsDescription, firstContainer.GoodsDescription);
				AssertEquals("Gross mass is correct", (ZDecimal)3.00, firstContainer.GrossMass.Value);
				AssertEquals("Gross mass unit is correct", ZUnit.Weight.KG.Code, firstContainer.GrossMass.Unit.Code);
			}

			AssertGoods(ContainerModes.BreakBulk);
			AssertGoods(ContainerModes.Bulk);
		}

		public void TestCalculateTotalNumberOfPacks()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ContainerMode = ContainerModes.Containerised;
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "containerNumber1";
			container1.CO_RC = refContainer1.PK;
			container1.CO_Weight = 1.00;
			var container2 = declaration.CusContainers.AddNew();
			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			container2.CO_ContainerNumber = "containerNumber1";
			container2.CO_RC = refContainer2.PK;
			container2.JobContainer.JC_IsEmptyContainer = true;
			container2.CO_Weight = 2000;
			container2.CO_WeightUQ = Weight.Grams;
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
			AssertEquals("Total Number of Packs defaults as 0", 0, result.TotalNumberOfPacks);
			var containers = result.Containers.ToArray();
			AssertEquals(2, containers.Length);
			containers[0].PackCount = 3;
			containers[1].PackCount = 6;
			AssertEquals("Summary of each containers", 9, result.TotalNumberOfPacks);
		}

		public void TestPopulateCurrentUser()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var cargoDues = builder.Build();
			AssertCurrentUserAddressData(cargoDues.CurrentUser);
		}

		public void TestDuesCollectionElementsAreInstantiated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
			var result = builder.Build();
			for (int i = 1; i <= 8; ++i)
			{
				var propertyName = "DuesCollectionElement" + i.ToString();
				var propInfo = typeof(CargoDues).GetProperty(propertyName);
				AssertNotNull("DuesCollectionElement should be instantiated as non-null", propInfo.GetValue(result));
			}
		}

		public void TestValidationSimpleMappings()
		{
			AssertHasMessageError(CargoDues.IMONumberInfo, "IMO Number is required.");
			AssertHasMessageError(CargoDues.CarrierCodeInfo, "Carrier Code is required.");
			AssertHasMessageError(CargoDues.RadioCallSignInfo, "Radio Call Sign is required.");
			AssertNoMessageError(CargoDues.PlaceOfDelivery.Country.NameInfo, "Country/Region of Destination is required.");
			AssertNoMessageError(CargoDues.PlaceOfReceipt.Country.NameInfo, "Country/Region of Origin is required.");
			CargoDues.IMONumber = "IMO111";
			CargoDues.CarrierCode = "CODE111";
			CargoDues.RadioCallSign = "RADIO111";
			CargoDues.PlaceOfDelivery.Country.Name = "";
			CargoDues.PlaceOfReceipt.Country.Name = "";
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.IMONumberInfo, "IMO Number is required.");
			AssertNoMessageError(CargoDues.CarrierCodeInfo, "Carrier Code is required.");
			AssertNoMessageError(CargoDues.RadioCallSignInfo, "Radio Call Sign is required.");
			AssertHasMessageError(CargoDues.PlaceOfDelivery.Country.NameInfo, "Country/Region of Destination is required.");
			AssertHasMessageError(CargoDues.PlaceOfReceipt.Country.NameInfo, "Country/Region of Origin is required.");
		}

		public void TestValidationEtaEtd()
		{
			AssertHasMessageError(CargoDues.EtaInfo, "ETD/ETA is required.");
			AssertHasMessageError(CargoDues.EtdInfo, "ETD/ETA is required.");
			CargoDues.Eta = ZDateTime.Now;
			CargoDues.Etd = ZDateTime.Now;
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.EtaInfo, "ETD/ETA is required.");
			AssertNoMessageError(CargoDues.EtdInfo, "ETD/ETA is required.");
		}

		public void TestValidationVesselAndOnCarrierInformation()
		{
			CombineAssertions(() =>
			{
				AssertHasMessageError(CargoDues.TNPAArrivalNumberInfo, "TNPA Arrival Number is required.");
				AssertHasMessageError(CargoDues.Transports.Main.Vessel.NameInfo, "Vessel Name is required.");
				AssertHasMessageError(CargoDues.Transports.Main.VoyageFlightNumberInfo, "Voyage number is required.");
				AssertHasMessageError(CargoDues.TerminalInfo, "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization > Config > Registration Numbers/Codes as TNP code for country/region ZA.");
				AssertHasMessageError(CargoDues.ClientRefInfo, "Client Reference is required.");
				AssertNoMessageError(CargoDues.ServicePort.NameInfo, "Service Port is required.");
				AssertNoMessageError(CargoDues.PortOfDischarge.NameInfo, "Port of Discharge is required.");
				AssertNoMessageError(CargoDues.PortOfLoading.NameInfo, "Port of Loading is required.");
			});
			CargoDues.TNPAArrivalNumber = "TPNAA111";
			CargoDues.Transports.Main.Vessel.Name = "VESS11";
			CargoDues.Transports.Main.VoyageFlightNumber = "VOYA11";
			CargoDues.Terminal = "1234";
			CargoDues.ServicePort.Name = ZString.Empty;
			CargoDues.PortOfLoading.Name = ZString.Empty;
			CargoDues.PortOfDischarge.Name = ZString.Empty;
			CargoDues.ClientRef = "REF111";
			CargoDues.ValidateAllIncludingChildren();
			CombineAssertions(() =>
			{
				AssertNoMessageError(CargoDues.TNPAArrivalNumberInfo, "TNPA Arrival Number is required.");
				AssertNoMessageError(CargoDues.Transports.Main.Vessel.NameInfo, "Vessel Name is required.");
				AssertNoMessageError(CargoDues.Transports.Main.VoyageFlightNumberInfo, "Voyage number is required.");
				AssertNoMessageError(CargoDues.TerminalInfo, "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization > Config > Registration Numbers/Codes as TNP code for country/region ZA.");
				AssertNoMessageError(CargoDues.ClientRefInfo, "Client Reference is required.");
				AssertHasMessageError(CargoDues.ServicePort.NameInfo, "Service Port is required.");
				AssertHasMessageError(CargoDues.PortOfDischarge.NameInfo, "Port of Discharge is required.");
				AssertHasMessageError(CargoDues.PortOfLoading.NameInfo, "Port of Loading is required.");
			});
		}

		public void TestValidationContainerOperator()
		{
			AssertHasMessageError(CargoDues.ContainerOperatorInfo, "Container Operator/Vessels Agent is required.");
			CargoDues.ContainerOperator = "ABC";
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.ContainerOperatorInfo, "Container Operator/Vessels Agent is required.");
		}

		public void TestValidationTNPANumbers()
		{
			AssertHasMessageError(CargoDues.TNPAAccountNumberInfo, "TNPA Account Number is required.");
			CargoDues.TNPAAccountNumber = "ACCOUNT111";
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.TNPAAccountNumberInfo, "TNPA Account Number is required.");
		}

		public void TestValidationContainersIfContainerised()
		{
			AssertEquals("Goods should be containerised for this test", true, CargoDues.IsContainerised);
			AssertHasMessageError(CargoDues.Containers.ToArray()[0].NumberInfo, "Container Number is required.");
			AssertNoMessageError(CargoDues.Containers.ToArray()[0].Type.CodeInfo, "Container Type is required.");
			AssertNoMessageError(CargoDues.CargoDuesWarningPlaceHolderInfo, "Container Number is required.");
			CargoDues.Containers.ToArray()[0].Number = "1111";
			CargoDues.Containers.ToArray()[0].Type.Code = ZString.Empty;
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.Containers.ToArray()[0].NumberInfo, "Container Number is required.");
			AssertHasMessageError(CargoDues.Containers.ToArray()[0].Type.CodeInfo, "Container Type is required.");
			AssertNoMessageError(CargoDues.CargoDuesWarningPlaceHolderInfo, "Container Number is required.");
			CargoDues.Containers = new List<Container>();
			CargoDues.ValidateAllIncludingChildren();
			AssertHasMessageError(CargoDues.CargoDuesWarningPlaceHolderInfo, "Container Number is required.");
		}

		public void TestValidationGoodsIfNotContainerised()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_ContainerMode = ContainerModes.BreakBulk;
			var parameters1 = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesExport, "data-store");
			var builder1 = new CargoDuesBrokerageDocDataBuilder(declaration1, parameters1);
			var cargoDuesBreakBulk = builder1.Build();
			var shipment = cargoDuesBreakBulk.ShipmentPackingInfos.First();
			AssertEquals("Goods should not be containerised for this test", false, cargoDuesBreakBulk.IsContainerised);
			AssertHasMessageError(shipment.GoodsInfoCollection.ToArray()[0].MarksAndNosInfo, "Marks and Numbers required.");
			AssertNoMessageError(cargoDuesBreakBulk.CargoDuesWarningPlaceHolderInfo, "Marks and Numbers required.");
			shipment.GoodsInfoCollection.ToArray()[0].MarksAndNos = "1111";
			cargoDuesBreakBulk.ValidateAllIncludingChildren();
			AssertNoMessageError(shipment.GoodsInfoCollection.ToArray()[0].MarksAndNosInfo, "Marks and Numbers required.");
			AssertNoMessageError(cargoDuesBreakBulk.CargoDuesWarningPlaceHolderInfo, "Marks and Numbers required.");
			cargoDuesBreakBulk.ShipmentPackingInfos = new List<ShipmentPackingInfo>();
			cargoDuesBreakBulk.ValidateAllIncludingChildren();
			AssertHasMessageError(cargoDuesBreakBulk.CargoDuesWarningPlaceHolderInfo, "Marks and Numbers required.");
		}

		public void TestValidationWayBillNumber()
		{
			AssertHasMessageError(CargoDues.WayBillNumberInfo, "Bill of Lading/Mates Receipt is required.");
			CargoDues.WayBillNumber = "abc123456";
			CargoDues.ValidateAllIncludingChildren();
			AssertNoMessageError(CargoDues.WayBillNumberInfo, "Bill of Lading/Mates Receipt is required.");
		}

		public void TestValidationCargoDuesWarningPlaceHolder()
		{
			AssertHasWarning(CargoDues.CargoDuesSectionTitleInfo, "This section is for information only and is not included in Cargo Dues EDI message.");
		}

		public void TestResetTotalNumberOfPacksWhenPackCountWasOverridden()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ContainerMode = ContainerModes.Containerised;
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "container A";
			container1.CO_RC = refContainer1.PK;
			container1.CO_Weight = 1.00;
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "container B";
			container2.CO_Weight = 2000;
			container2.CO_WeightUQ = Weight.Kilograms;
			IDynamicData GetDynamicCargoDuesDataObject()
			{
				var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
				var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);
				return builder.Build().MakeDocDataDynamic();
			}

			IDynamicDataCollection GetDynamicContainers(IDynamicData cargoDues) => (IDynamicDataCollection)cargoDues.GetDynamicProperty(nameof(CargoDues.Containers));
			IDynamicData GetDynamicTotalNumberOfPacks(IDynamicData cargoDues) => cargoDues.GetDynamicProperty(nameof(CargoDues.TotalNumberOfPacks));
			IDynamicData GetDynamicPackCount(IDynamicData dynamicContainer) => dynamicContainer.GetDynamicProperty(nameof(Container.PackCount));
			var dynamicCargoDues = GetDynamicCargoDuesDataObject();
			var dynamicContainers = GetDynamicContainers(dynamicCargoDues);
			AssertEquals("Precondition: containers has 2 elements", 2, dynamicContainers.Count());
			var dynamicTotalPacks = GetDynamicTotalNumberOfPacks(dynamicCargoDues);
			AssertEquals("Precondition: total packs", 0, dynamicTotalPacks.Value);
			var dynamicContainerA = dynamicContainers.ElementAt(0);
			var dynamicContainerB = dynamicContainers.ElementAt(1);
			var packCountContainerA = GetDynamicPackCount(dynamicContainerA);
			var packCountContainerB = GetDynamicPackCount(dynamicContainerB);
			packCountContainerA.SetValue(1);
			packCountContainerB.SetValue(2);
			AssertEquals("Total packs has been updated", 3, dynamicTotalPacks.Value);
			var xml = dynamicCargoDues.GetOverriddenValuesXml();
			var dynamicCargoDues2 = GetDynamicCargoDuesDataObject();
			dynamicCargoDues2.MergeDataFromXml(xml);
			var dynamicContainers2 = GetDynamicContainers(dynamicCargoDues2);
			var dynamicContainerA2 = dynamicContainers2.ElementAt(0);
			var dynamicContainerB2 = dynamicContainers2.ElementAt(1);
			var packCountContainerA2 = GetDynamicPackCount(dynamicContainerA2);
			var packCountContainerB2 = GetDynamicPackCount(dynamicContainerB2);
			AssertEquals("PackCount has value from override", 1, packCountContainerA2.Value);
			AssertEquals("PackCount has value from override", 2, packCountContainerB2.Value);
			var dynamicTotalPacks2 = GetDynamicTotalNumberOfPacks(dynamicCargoDues2);
			AssertEquals("Total packs has correct value", 3, dynamicTotalPacks2.Value);
			dynamicCargoDues2.CancelChanges();
			AssertEquals("PackCount should be reset", 0, packCountContainerA2.Value);
			AssertEquals("PackCount should be reset", 0, packCountContainerB2.Value);
			AssertEquals("Total packs should be reset", 0, dynamicTotalPacks2.Value);
		}

		public void TestPopulateContainersWhenForwardingContainerAndCusContainerHaveDifferentValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ContainerMode = ContainerModes.Containerised;

			var refCusContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refCusContainer1.RC_Code = "Cus1";
			refCusContainer1.RC_ISOType = "CISO";
			refCusContainer1.RC_ContainerType = "RFG";
			var refForwardingContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refForwardingContainer1.RC_Code = "Fwd1";
			refForwardingContainer1.RC_ISOType = "FISO";
			refForwardingContainer1.RC_ContainerType = "DRY";

			var cusContainer1 = declaration.CusContainers.AddNew();
			cusContainer1.CO_RC = refCusContainer1.PK;
			cusContainer1.CO_ContainerNumber = "containerNumber1";
			cusContainer1.CO_WeightUQ = Weight.Grams;
			cusContainer1.CO_Weight = 1000;

			cusContainer1.JobContainer.JC_RC = refForwardingContainer1.PK;
			cusContainer1.JobContainer.JC_ContainerNum = "JobContainerNumber1";
			cusContainer1.JobContainer.JC_GrossWeightUQ = Weight.Kilograms;
			cusContainer1.JobContainer.JC_GrossWeight = 2000;

			CombineAssertions(() =>
			{
				Assert(cusContainer1.CO_ContainerNumber.EqualsIgnoringCase("containerNumber1"));
				AssertEquals(1000M, cusContainer1.CO_Weight);
				AssertEquals(Weight.Grams, cusContainer1.CO_WeightUQ);
				AssertEquals("Cus1", cusContainer1.Container.RC_Code);
				AssertEquals("CISO", cusContainer1.Container.RC_ISOType);
				AssertEquals("RFG", cusContainer1.Container.RC_ContainerType);

				Assert(cusContainer1.JobContainer.JC_ContainerNum.EqualsIgnoringCase("JobContainerNumber1"));
				AssertEquals(2000M, cusContainer1.JobContainer.JC_GrossWeight);
				AssertEquals(Weight.Kilograms, cusContainer1.JobContainer.JC_GrossWeightUQ);
				AssertEquals("Fwd1", cusContainer1.JobContainer.Container.RC_Code);
				AssertEquals("FISO", cusContainer1.JobContainer.Container.RC_ISOType);
				AssertEquals("DRY", cusContainer1.JobContainer.Container.RC_ContainerType);
			});

			var refForwardingContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refForwardingContainer2.RC_Code = "Fwd2";

			var cusContainer2 = declaration.CusContainers.AddNew();

			cusContainer2.JobContainer.JC_RC = refForwardingContainer2.PK;
			cusContainer2.JobContainer.JC_ContainerNum = "JobContainerNumber2";
			cusContainer2.JobContainer.JC_GrossWeightUQ = Weight.Kilograms;
			cusContainer2.JobContainer.JC_GrossWeight = 2000;

			cusContainer2.CO_RC = ZGuid.Empty;
			cusContainer2.CO_ContainerNumber = "containerNumber2";
			cusContainer2.CO_WeightUQ = Weight.Grams;
			cusContainer2.CO_Weight = 3000;

			CombineAssertions(() =>
			{
				Assert(cusContainer2.CO_ContainerNumber.EqualsIgnoringCase("containerNumber2"));
				AssertEquals(3000M, cusContainer2.CO_Weight);
				AssertEquals(Weight.Grams, cusContainer2.CO_WeightUQ);
				AssertNull(cusContainer2.Container);

				Assert(cusContainer2.JobContainer.JC_ContainerNum.EqualsIgnoringCase("containerNumber2"));
				AssertEquals(3000M, cusContainer2.JobContainer.JC_GrossWeight);
				AssertEquals(Weight.Grams, cusContainer2.JobContainer.JC_GrossWeightUQ);
				AssertNull(cusContainer2.JobContainer.Container);
			});

			var parameters = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters);

			var containers = builder.Build().Containers.ToArray();
			AssertEquals(2, containers.Length);

			CombineAssertions(() =>
			{
				var container1 = containers.Single(x => x.Number.EqualsIgnoringCase("containerNumber1"));

				AssertEquals("Cus1", container1.Type.Code);
				AssertEquals("CISO", container1.Type.ISOCode);
				AssertEquals("RFG", container1.Type.Type.Code);
				AssertEquals(1M, container1.GrossWeight.Value);
				AssertEquals(Weight.Kilograms, container1.GrossWeight.Unit.Code);

				var container2 = containers.Single(x => x.Number.EqualsIgnoringCase("containerNumber2"));

				AssertNullOrEmpty(container2.Type.Code);
				AssertEquals(3M, container2.GrossWeight.Value);
				AssertEquals(Weight.Kilograms, container2.GrossWeight.Unit.Code);
			});
		}

		#region TestCargoDuesInformation

		public void TestCargoDuesInformation()
		{
			var taxRate = AccTaxRate.Helper.FindTaxRate(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.Factory.Save();

			RunCargoDueInformationTestingCore(DeclarationDocumentConstants.DocumentNames.CargoDuesExport);
			RunCargoDueInformationTestingCore(DeclarationDocumentConstants.DocumentNames.CargoDuesImport);
			RunCargoDueInformationTestingCore(DeclarationDocumentConstants.DocumentNames.LoadCoastwise);
			RunCargoDueInformationTestingCore(DeclarationDocumentConstants.DocumentNames.DischargeCoastwise);
		}

		void RunCargoDueInformationTestingCore(string documentName)
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerMode = ContainerModes.Containerised;

			var docDataObjectParameters = new DocDataObjectParameters(documentName, "data-store");

			var cargoDues = BuildWithDefaultParameters(declaration1, docDataObjectParameters);

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", ZDecimal.Zero, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", ZDecimal.Zero, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", ZDecimal.Zero, cargoDues.TotalR);
			});

			AddDataLinkedEvent(declaration1, "39996", documentName);
			cargoDues = BuildWithDefaultParameters(declaration1, docDataObjectParameters);

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, "Desc1", "1111");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, "abc");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", 39996m, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", 5999.40m, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", 45995.40m, cargoDues.TotalR);
			});

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ContainerMode = ContainerModes.Containerised;
			AddDataLinkedEvent(declaration2, "xxx", documentName);
			cargoDues = BuildWithDefaultParameters(declaration2, docDataObjectParameters);

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, "Desc1", "1111");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, "abc");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", ZDecimal.Zero, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", ZDecimal.Zero, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", ZDecimal.Zero, cargoDues.TotalR);
			});
		}

		void AssertCargoDueInformation(DuesCollectionRow cargoDue, ZString description, ZString amount)
		{
			AssertEquals("Description", description, cargoDue.Description);
			AssertEquals("Factor", ZString.Empty, cargoDue.Factor);
			AssertEquals("Rate", ZString.Empty, cargoDue.Rate);
			AssertEquals("Amount", amount, cargoDue.Amount);
		}

		void AddDataLinkedEvent(JobDeclaration declaration, string totalCargoDuesAmount, string documentName)
		{
			var cargoDuesDocumentEventXML = GetCargoDuesDocumentEventXML(declaration.JE_DeclarationReference, totalCargoDuesAmount, documentName);

			declaration.Logs.CreateOrRecreateEventLog(
				Events.MessageAccepted,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddDays(-1),
				ZString.Empty,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			var dataLinkedLog = declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.MessageAccepted.Code).First();

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(cargoDuesDocumentEventXML);

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dataLinkedLog.PK;
			pivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
		}

		string GetCargoDuesDocumentEventXML(string declarationKey, string totalCargoDuesAmount, string documentName) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Cargo Dues - Import</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>{declarationKey}</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2020-12-16T16:39:00</EventTime>
		<EventType>MAA</EventType>
		<EventParameters>
			<Department>TNPA</Department>
			<MessageType>{documentName}</MessageType>
			<ReferenceNumber>3707720274</ReferenceNumber>
		</EventParameters>
		<EventReference>Order Confirmed</EventReference>
		<ContextCollection>
			<Context>
				<Type>TNPA Order Number</Type>
				<Value>3707720274</Value>
			</Context>
			<Context>
				<Type>Total Cargo Dues Amount</Type>
				<Value>{totalCargoDuesAmount}</Value>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>1</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
						<Value>Desc1</Value>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
						<Value>1111</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>2</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
						<Value></Value>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
						<Value></Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>3</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>4</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Amount</Type>
						<Value>abc</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>5</Value>
				<SubContextCollection>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>6</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		CargoDues cargoDues;
		CargoDues CargoDues
		{
			get
			{
				if (cargoDues == null)
				{
					var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
					declaration1.JE_ContainerMode = ContainerModes.Containerised;
					var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
					declaration1.CusContainers.RemoveAll();
					var container1 = declaration1.CusContainers.AddNew();
					container1.CO_ContainerNumber = ZString.Empty;
					container1.CO_RC = refContainer1.PK;
					container1.CO_Weight = 1.00;
					AddTransportsToDeclaration(declaration1);
					var parameters1 = new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.CargoDuesImport, "data-store");
					var builder1 = new CargoDuesBrokerageDocDataBuilder(declaration1, parameters1);
					cargoDues = builder1.Build();
				}

				return cargoDues;
			}
		}

		CargoDues BuildWithDefaultParameters(JobDeclaration declaration, IDocDataObjectParameters parameters = null)
		{
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, parameters ?? new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, "data-store"));
			return builder.Build();
		}

		Tuple<TransportBizoT, TransportBizoT, TransportBizoT, TransportBizoT> AddTransportsToDeclaration(JobDeclaration declaration)
		{
			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = NonZAPort1;
			transport1.JW_RL_NKDiscPort = NonZAPort2;
			transport1.JW_TransportMode = TransportModes.Sea;
			var transport2 = declaration.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = NonZAPort2;
			transport2.JW_RL_NKDiscPort = ZAPort1;
			transport2.JW_TransportMode = TransportModes.Sea;
			var exportOrLoadLeg = declaration.Transports.AddNew();
			exportOrLoadLeg.JW_RL_NKLoadPort = ZAPort1;
			exportOrLoadLeg.JW_RL_NKDiscPort = NonZAPort1;
			exportOrLoadLeg.JW_TransportMode = TransportModes.Sea;
			var afterExportOrLoadLeg = declaration.Transports.AddNew();
			afterExportOrLoadLeg.JW_RL_NKLoadPort = NonZAPort1;
			afterExportOrLoadLeg.JW_RL_NKDiscPort = NonZAPort2;
			afterExportOrLoadLeg.JW_TransportMode = TransportModes.Sea;
			var beforeImportOrDischargeLeg = declaration.Transports.AddNew();
			beforeImportOrDischargeLeg.JW_RL_NKLoadPort = NonZAPort2;
			beforeImportOrDischargeLeg.JW_RL_NKDiscPort = NonZAPort1;
			beforeImportOrDischargeLeg.JW_TransportMode = TransportModes.Sea;
			var importOrDischargeLeg = declaration.Transports.AddNew();
			importOrDischargeLeg.JW_RL_NKLoadPort = NonZAPort1;
			importOrDischargeLeg.JW_RL_NKDiscPort = ZAPort2;
			importOrDischargeLeg.JW_TransportMode = TransportModes.Sea;
			return Tuple.Create(importOrDischargeLeg, exportOrLoadLeg, beforeImportOrDischargeLeg, afterExportOrLoadLeg);
		}

		const string ZAPort1 = "ZA2WC";
		const string ZAPort2 = "ZA3WC";
		const string NonZAPort1 = "ADALV";
		const string NonZAPort2 = "ADCAN";
	}
}
