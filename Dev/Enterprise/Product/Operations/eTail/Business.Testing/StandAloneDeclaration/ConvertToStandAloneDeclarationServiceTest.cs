using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.eTail.Integration.HVLVConstants;
using EventReferenceConstants = CargoWise.EventReference.Constants;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(ConvertToStandAloneDeclarationService))]
	class ConvertToStandAloneDeclarationServiceTest : TestCaseWithFactory
	{
		public void TestConvertToStandAloneDeclaration_StripNonWesternEuropeanChararcters_DependOnRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUBNE";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

				var consignment1 = Factory.New<HVLVConsignment>();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_GoodsDescription = "Some test 中文goods";
				consignment1.Items.AddNew();

				var consignment2 = Factory.New<HVLVConsignment>();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_GoodsDescription = "Some test 中文goods";
				consignment2.Items.AddNew();

				Factory.Save();

				var service = new ConvertToStandAloneDeclarationService();
				service.ConvertToStandAloneDeclaration(consignment2);
				Factory.Save();

				AssertEquals("When Registry is closed, non English words in StandAloneDeclaration are removed", "Some test 中文goods", consignment2.StandAloneDeclarationForCurrentCompany.JE_GoodsDescription);

				using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					service.ConvertToStandAloneDeclaration(consignment1);
					Factory.Save();

					AssertEquals("When Registry is open, non English words in StandAloneDeclaration are removed", "Some test goods", consignment1.StandAloneDeclarationForCurrentCompany.JE_GoodsDescription);

					AssertEquals("non English words in Consignment are not removed", "Some test 中文goods", consignment1.HVC_GoodsDescription);
				}
			}
		}

		public void TestConvertToStandAloneDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			var shipmentDestinationCountry = consignment.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);
			AssertStandAloneDeclarationCreatedForConsignment(consignment, result);
		}

		public void TestConvertToStandAloneDeclaration_WithoutErrorReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "03", "CSTA - Description Test", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUSYD";
				departureConsol.JK_RL_NKDischargePort = "AUBNE";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_RN_NKConsigneeCountryCode = "NZ";
				consignment.HVC_ImportCustomsClearanceStatus = "03";
				consignment.Items.AddNew();

				Factory.Save();
				ErrorReporter.Clear();

				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);

				Assert("No error will be reported during StandAloneDeclaration conversion", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
		}

		public void TestConvertToStandAloneDeclaration_CADestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "CATOR");
		}

		public void TestConvertToStandAloneDeclaration_NZDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "NZAKL");
		}

		public void TestConvertToStandAloneDeclaration_USDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "USCHI");
		}

		public void TestConvertToStandAloneDeclaration_SGDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "SGSIN");
		}

		public void TestConvertToStandAloneDeclaration_TWDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "TWTPE");
		}

		public void TestConvertToStandAloneDeclaration_TRDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "TRIZM");
		}

		public void TestConvertToStandAloneDeclaration_ZADestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "ZADUR");
		}

		public void TestConvertToStandAloneDeclaration_BEDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "BEANR");
		}

		public void TestConvertToStandAloneDeclaration_CHDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "CHBLT");
		}

		public void TestConvertToStandAloneDeclaration_DEDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "DE26T");
		}

		public void TestConvertToStandAloneDeclaration_ESDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "ES9BJ");
		}

		public void TestConvertToStandAloneDeclaration_FRDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "FRBLV");
		}

		public void TestConvertToStandAloneDeclaration_GBDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "GBADF");
		}

		public void TestConvertToStandAloneDeclaration_IEDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "IE7CM");
		}

		public void TestConvertToStandAloneDeclaration_ITDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "ITAAA");
		}

		public void TestConvertToStandAloneDeclaration_NLDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "NL2LL");
		}

		public void TestConvertToStandAloneDeclaration_SEDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "SEASA");
		}

		public void TestConvertToStandAloneDeclaration_BRDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "BR8CR");
		}

		public void TestConvertToStandAloneDeclaration_CNDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "CNAQG");
		}

		public void TestConvertToStandAloneDeclaration_PLDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "PLWAW");
		}

		public void TestConvertToStandAloneDeclaration_AEDestination()
		{
			TestConvertToStandAloneDeclarationForDestination(destination: "AEAUH");
		}

		void TestConvertToStandAloneDeclarationForDestination(ZString destination)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = destination;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = destination;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Goods";
			consignment.Items.AddNew();

			Factory.Save();

			var shipmentDestinationCountry = consignment.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);
			AssertStandAloneDeclarationCreatedForConsignment(consignment, result);
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_NoShipment()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_GoodsDescription = "Some test goods";

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignment, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotAttachedToShipment);
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_HasNoConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignment, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentMissingTransportDetails);
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_MissingConsignment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignmentNotUsed = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentNotUsed.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentNotUsed.HVC_GoodsDescription = "Some test goods";

			AssertNull("Precondition: Declaration not created yet", consignmentNotUsed.StandAloneDeclarationForCurrentCompany);
			AssertEquals("Precondition: Consignment not yet saved", false, consignmentNotUsed.IsInDatabase);

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(new Guid(), Factory);

			AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignmentNotUsed, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentNotFound);
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_ImportAlreadyCleared()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;

			consignment.Items.AddNew();

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);

				AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignment, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasClearedImportCustomsStatus);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);

				AssertStandAloneDeclarationCreatedForConsignment(consignment, result);
			}
		}

		public void TestConvertToExportStandAloneDeclaration_ExportHasCleared_ErrorMessage_AlreadyCleared()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;

			consignment.Items.AddNew();

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);

				AssertStandAloneDeclarationCreatedForConsignment(consignment, result);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);

				AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignment, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasClearedExportCustomsStatus);
			}
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_InvalidDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "INBDI";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "INBDI";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			var shipmentDestinationCountry = consignment.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should NOT be supported for Stand Alone Declaration creation", false, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			AssertStandAloneDeclarationNotCreatedForConsignmentWithError(consignment, result, ConvertToStandAloneDeclarationResponse.ErrorMessages.ShipmentDestinationNotSupported);
		}

		public void TestConvertToStandAloneDeclaration_ErrorMessage_DeclarationAlreadyCreated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "Test Ref";
			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			CombineAssertions("Declaration should not be created when consignment is already linked to a stand alone declaration", () =>
			{
				AssertEquals("Convert should fail", false, result.ConversionSucceeded);
				AssertEquals("Should have correct error message", ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentHasExistingDeclaration, result.ConversionFailureReason);
				AssertEquals("Declaration not created", Guid.Empty, result.ConvertedStandAloneDeclaration);
				AssertEquals("Consignment still linked to existing declaration", declaration.PK, consignment.StandAloneDeclarationForCurrentCompany.PK);
			});
		}

		public void TestConvertToStandAloneDeclaration_WhenImportConsignment_CreatesTCILog()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();
			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment.DirectionOfTrade);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			var customsImportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
			AssertEquals("Log was not created before save", 0, customsImportLogs.Count());

			Factory.Save();

			AssertEquals("Precondition: Declaration is created successfully", result.ConvertedStandAloneDeclaration, consignment.StandAloneDeclarationForCurrentCompany.PK);

			customsImportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
			AssertEquals("Should have created a single Customs Import Declaration Event Log", 1, customsImportLogs.Count());

			var eventParameters = customsImportLogs.Single().Parameters;
			eventParameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);
			AssertEquals("Event reference number should be set to Stand Alone Declaration Job Number", consignment.StandAloneDeclarationForCurrentCompany.JobNumber, referenceNumber);
		}

		public void TestConvertToStandAloneDeclaration_WhenExportConsignment_CreatesTCELog()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();
			AssertEquals("Precondition: Should be Export Consignment", Directions.Export, consignment.DirectionOfTrade);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);

			var customsExportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDecCode);
			AssertEquals("Log was not created before save", 0, customsExportLogs.Count());

			Factory.Save();

			AssertEquals("Declaration is created successfully", result.ConvertedStandAloneDeclaration, consignment.StandAloneDeclarationForCurrentCompany.PK);

			customsExportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDecCode);
			AssertEquals("Should have created a single Customs Import Declaration Event Log", 1, customsExportLogs.Count());

			var eventParameters = customsExportLogs.Single().Parameters;
			eventParameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);
			AssertEquals("Event reference number should be set to Stand Alone Declaration Job Number", consignment.StandAloneDeclarationForCurrentCompany.JobNumber, referenceNumber);
		}

		public void TestConvertToStandAloneDeclaration_CreatesTRFLogOnDeclaration()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignmentForStandAloneDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentForStandAloneDeclaration.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentForStandAloneDeclaration.HVC_GoodsDescription = "Consignment For HVLV Consignment Declaration TRF Event Log Test";
			consignmentForStandAloneDeclaration.Items.AddNew();

			Factory.Save();

			var existingDeclarations = Factory.Load<BaseJobDeclaration>(new ZQuery());
			AssertEquals("There should be no BaseJobDeclaration before convert", 0, existingDeclarations.Length);

			var service = new ConvertToStandAloneDeclarationService();
			service.ConvertToStandAloneDeclaration(consignmentForStandAloneDeclaration);

			Factory.Save();

			var consignmentDeclaration = consignmentForStandAloneDeclaration.StandAloneDeclarationForCurrentCompany;
			AssertNotNull("Declaration is created successfully", consignmentDeclaration);

			var trfLogsForConsignmentDeclaration = consignmentDeclaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
			AssertEquals("There should be a single TRF Event Log", 1, trfLogsForConsignmentDeclaration.Count());

			var trfLogOnDeclaration = trfLogsForConsignmentDeclaration.First();
			trfLogOnDeclaration.Parameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.Type, out var trfLogOnDeclarationLogType);
			trfLogOnDeclaration.Parameters.TryGetValue(EventReferenceConstants.EventReferenceParameters.Codes.JobNumber, out var trfLogOnDeclarationReferenceNumber);
			CombineAssertions(() =>
			{
				AssertEquals("HVL", trfLogOnDeclarationLogType);
				AssertEquals(consignmentForStandAloneDeclaration.HVC_ConsignmentId, trfLogOnDeclarationReferenceNumber);
			});
		}

		public void TestConvertToStandAloneDeclaration_WhenSaveFailedAndSaveAgain_NoDuplicateTRFLog()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignmentForStandAloneDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentForStandAloneDeclaration.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentForStandAloneDeclaration.HVC_GoodsDescription = "Consignment For HVLV Consignment Declaration TRF Event Log Test";
			consignmentForStandAloneDeclaration.Items.AddNew();

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			service.ConvertToStandAloneDeclaration(consignmentForStandAloneDeclaration);
			arrivalConsol.JK_OA_SendingForwarderAddress = shipment.PK;

			AssertNull("Invalid FK for JK_OA_SendingForwarderAddress", Factory.Load<OrgAddress>(arrivalConsol.JK_OA_SendingForwarderAddress));

			try
			{
				Factory.Save();
			}
			catch (ZSaveException)
			{
				var declarationFailedToSave = consignmentForStandAloneDeclaration.StandAloneDeclarationForCurrentCompany;
				Assert("precondition: declaration is not saved", !declarationFailedToSave.IsInDatabase);

				var trfLogsOnDeclaration = declarationFailedToSave.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
				AssertEquals("When Save Action failed there should be one single TRF Event Log", 1, trfLogsOnDeclaration.Count());
				Assert("TRF log should not be saved", !trfLogsOnDeclaration.Single().IsInDatabase);
			}

			arrivalConsol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			Factory.Save();

			var declarationSaved = consignmentForStandAloneDeclaration.StandAloneDeclarationForCurrentCompany;
			Assert("declaration is saved", declarationSaved.IsInDatabase);

			var trfLogsOnSavedDeclaration = declarationSaved.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
			AssertEquals("When Save Again there should should still be one single TRF Event Log", 1, trfLogsOnSavedDeclaration.Count());
		}

		public void TestConvertingToStandAloneDeclaration_WhenUSSeaCargo_ThenSetTheMasterBillIssuerSCAC()
		{
			var shippingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = shippingOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ASDF";

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ShippingLineAddress = shippingOrg.MainAddress.PK;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();

			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var service = new ConvertToStandAloneDeclarationService();
				var result = service.ConvertToStandAloneDeclaration(consignment);
				Factory.Save();

				AssertNotNull("Precondition: Declaration was created successfully", consignment.StandAloneDeclarationForCurrentCompany);

				var usDeclaration = consignment.StandAloneDeclarationForCurrentCompany as Customs.US.Business.JobDeclaration;

				AssertEquals("The declaration's Issuer SCAC code should be ASDF", "ASDF", usDeclaration.MasterBillIssuerSCACCode);
			}
		}

		public void TestConvertToStandAloneDeclaration_DoesNotInvokeFactorySave()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			consignment.Items.AddNew();

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);
			var declaration = Factory.Load<BaseJobDeclaration>(result.ConvertedStandAloneDeclaration);

			CombineAssertions(() =>
			{
				AssertNotNull(declaration);
				AssertEquals(false, declaration.IsInDatabase);
				AssertEquals("consignment.HVC_JE_ImportDeclaration is not assigned until saved", ZGuid.Empty, consignment.HVC_JE_ImportDeclaration);

				consignment.Factory.Save();
				AssertEquals("consignment.HVC_JE_ImportDeclaration is assigned", declaration.PK, consignment.HVC_JE_ImportDeclaration);
			});
		}

		public void TestConvertToStandAloneDeclaration_WhenImportConsignment_PopulatesItemsLastUsageCode_WithImportDeclaration()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";
			var item = consignment.Items.AddNew();

			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment.DirectionOfTrade);

			Factory.Save();

			var shipmentDestinationCountry = consignment.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", expected: true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			var conversionResponse = service.ConvertToStandAloneDeclaration(consignment);
			Factory.Save();

			Assert(conversionResponse.ConversionSucceeded);
			AssertEquals("Should populate HVI_LastUsageCode", UsageCodes.ImportStandAloneDeclaration, item.HVI_LastUsageCode);
		}

		public void TestConvertToStandAloneDeclaration_WhenExportConsignment_PopulatesItemsLastUsageCodeWithExportDeclaration()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			var item = consignment.Items.AddNew();

			AssertEquals("Precondition: Should be Export Consignment", Directions.Export, consignment.DirectionOfTrade);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var result = service.ConvertToStandAloneDeclaration(consignment);
			Factory.Save();

			AssertEquals("Precondition: Declaration is created successfully", result.ConvertedStandAloneDeclaration, consignment.StandAloneDeclarationForCurrentCompany.PK);

			Assert(result.ConversionSucceeded);
			AssertEquals("Should populate HVI_LastUsageCode with ExportStandAloneDeclaration UsageCode", UsageCodes.ExportStandAloneDeclaration, item.HVI_LastUsageCode);
		}

		public void TestConvertToStandAloneDeclaration_WhenCancelled_DoesNotChangeLastUsageCode()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
			consignment.HVC_WaybillNumber = "1";
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item = consignment.Items.AddNew();

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);

			Factory.Save();

			item.HVI_LastUsageCode = "AAA";
			Factory.Save();

			var shipmentDestinationCountry = consignment.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", expected: true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();

			service.Completed += new CancelEventHandler(cancelConversion);
			var conversionResponse = service.ConvertToStandAloneDeclaration(consignment);

			AssertNull("Conversion should be cancelled", conversionResponse);
			AssertEquals("Should not change HVI_LastUsageCode", "AAA", item.HVI_LastUsageCode);
		}

		public void TestConvertToStandAloneDeclaration_WhenMergeConsignments_PopulatesItemsLastUsageCode()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "NZAKL";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
			arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "1";
			consignment1.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item1 = consignment1.Items.AddNew();

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "2";
			consignment2.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment2.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item2 = consignment2.Items.AddNew();

			var consignment3 = header.Consignments.AddNew();
			consignment3.HVC_WaybillNumber = "3";
			consignment3.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment3.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item3 = consignment3.Items.AddNew();

			AssertNull("Precondition: Declaration 1 not created yet", consignment1.StandAloneDeclarationForCurrentCompany);
			AssertNull("Precondition: Declaration 2 not created yet", consignment2.StandAloneDeclarationForCurrentCompany);
			AssertNull("Precondition: Declaration 3 not created yet", consignment3.StandAloneDeclarationForCurrentCompany);

			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment1.DirectionOfTrade);
			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment2.DirectionOfTrade);
			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment3.DirectionOfTrade);

			Factory.Save();

			var shipmentDestinationCountry = consignment1.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", expected: true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			var conversionResponse = service.ConvertToStandAloneDeclaration(consignment1, new[] { consignment2, consignment3 });
			Factory.Save();

			Assert(conversionResponse.ConversionSucceeded);
			AssertEquals("Should populate HVI_LastUsageCode 1", ExpectedImportDeclarationUsageCode, item1.HVI_LastUsageCode);
			AssertEquals("Should populate HVI_LastUsageCode 2", ExpectedImportDeclarationUsageCode, item2.HVI_LastUsageCode);
			AssertEquals("Should populate HVI_LastUsageCode 3", ExpectedImportDeclarationUsageCode, item3.HVI_LastUsageCode);
		}

		public void TestConvertToStandAloneDeclaration_WhenMergeConsignments_AndCancelled_DoesNotChangeItemsLastUsageCode()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "USLAX";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "1";
			consignment1.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item1 = consignment1.Items.AddNew();

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "2";
			consignment2.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment2.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item2 = consignment2.Items.AddNew();

			var consignment3 = header.Consignments.AddNew();
			consignment3.HVC_WaybillNumber = "3";
			consignment3.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment3.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var item3 = consignment3.Items.AddNew();

			Factory.Save();

			item1.HVI_LastUsageCode = "AAA";
			item2.HVI_LastUsageCode = "AAA";
			item3.HVI_LastUsageCode = "AAA";
			Factory.Save();

			var shipmentDestinationCountry = consignment1.ManifestedOnShipment.Destination.Country.Code;
			AssertEquals($"Precondition: Shipment destination ({shipmentDestinationCountry}) should be supported for Stand Alone Declaration creation", expected: true, ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipmentDestinationCountry));

			var service = new ConvertToStandAloneDeclarationService();
			service.Completed += new CancelEventHandler(cancelConversion);

			var conversionResponse = service.ConvertToStandAloneDeclaration(consignment1, new[] { consignment2, consignment3 });

			AssertNull("Conversion should be cancelled", conversionResponse);
			AssertEquals("Should not change HVI_LastUsageCode 1", "AAA", item1.HVI_LastUsageCode);
			AssertEquals("Should not change HVI_LastUsageCode 2", "AAA", item2.HVI_LastUsageCode);
			AssertEquals("Should not change HVI_LastUsageCode 3", "AAA", item3.HVI_LastUsageCode);
		}

		void cancelConversion(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		public void TestConvertToStandAloneDeclaration_WhenConsolIsAirAndDeclarationWithSameWaybillNumberExists_CanMergeIntoTheExistingDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<Customs.US.Business.JobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Air;
			declaration.JE_MasterBill = "123-456";
			declaration.JE_HouseBill = "100001";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "123-456";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USSYD";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.HVC_WaybillNumber = "100001";

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			var response = service.ConvertToStandAloneDeclaration(consignment);

			AssertStandAloneDeclarationCreatedForConsignment(consignment, response);
			AssertEquals("Merged into the existing declaration", declaration.PK, response.ConvertedStandAloneDeclaration);
		}

		public void TestImportDeclarationUsageCodeAndCategory()
		{
			AssertEquals("UsageCode", ExpectedImportDeclarationUsageCode, UsageCodes.ImportStandAloneDeclaration);
			AssertEquals("UsageCategory", "HVD", UsageCategories.LookupByUsageCode[ExpectedImportDeclarationUsageCode]);
		}

		public void TestExportDeclarationUsageCodeAndCategory()
		{
			AssertEquals("UsageCode", ExpectedExportDeclarationUsageCode, UsageCodes.ExportStandAloneDeclaration);
			AssertEquals("UsageCategory", "HVD", UsageCategories.LookupByUsageCode[ExpectedExportDeclarationUsageCode]);
		}

		public void TestConvertToStandAloneDeclaration_ClearCustomsStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "Test Code");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "Test Code", "HLD", "CSTEX");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();
			consignment.HVC_ImportCustomsClearanceStatus = "AAA";
			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment.DirectionOfTrade);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			service.ConvertToStandAloneDeclaration(consignment);
			Factory.Save();

			AssertNullOrEmpty("ImportCustomsClearanceStatus has been cleared", consignment.HVC_ImportCustomsClearanceStatus);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			consignment.HVC_ExportCustomsClearanceStatus = "AAA";
			AssertEquals("Precondition: Should be Export consignment", Directions.Export, consignment.DirectionOfTrade);

			Factory.Save();

			service.ConvertToStandAloneDeclaration(consignment);
			Factory.Save();

			AssertNullOrEmpty("ExportCustomsClearanceStatus has been cleared", consignment.HVC_ExportCustomsClearanceStatus);
		}

		public void TestConvertToStandAloneDeclaration_WhenCancelled_DoesNotClearCustomsStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "Test Code", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AAA", "Test Code", "HLD", "CSTEX");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.Items.AddNew();
			consignment.HVC_ImportCustomsClearanceStatus = "AAA";
			AssertEquals("Precondition: Should be Import consignment", Directions.Import, consignment.DirectionOfTrade);

			Factory.Save();

			var service = new ConvertToStandAloneDeclarationService();
			service.Completed += new CancelEventHandler(cancelConversion);

			service.ConvertToStandAloneDeclaration(consignment);
			AssertEquals("ImportCustomsClearanceStatus not changed", "AAA", consignment.HVC_ImportCustomsClearanceStatus);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			consignment.HVC_ImportCustomsClearanceStatus = "";
			consignment.HVC_ExportCustomsClearanceStatus = "AAA";
			AssertEquals("Precondition: Should be Export consignment", Directions.Export, consignment.DirectionOfTrade);

			Factory.Save();

			service.ConvertToStandAloneDeclaration(consignment);
			AssertEquals("ExportCustomsClearanceStatus not changed", "AAA", consignment.HVC_ExportCustomsClearanceStatus);
		}

		const string ExpectedImportDeclarationUsageCode = "DEC";

		const string ExpectedExportDeclarationUsageCode = "DEE";

		#region Implementation

		void AssertStandAloneDeclarationCreatedForConsignment(HVLVConsignment consignment, IConvertToStandAloneDeclarationResponse result)
		{
			consignment.Factory.Save();

			CombineAssertions("Convert to stand alone declaration should succeed", () =>
			{
				AssertEquals("Conversion successful", true, result.ConversionSucceeded);
				AssertEquals("Error message should be empty", true, string.IsNullOrEmpty(result.ConversionFailureReason));
				AssertNotEquals("Declaration is created", Guid.Empty, result.ConvertedStandAloneDeclaration);
				if (consignment.IsImport || consignment.IsExport)
				{
					AssertEquals("Declaration is linked to consignment", result.ConvertedStandAloneDeclaration, consignment.StandAloneDeclarationForCurrentCompany.PK);
				}
				else
				{
					Assert($"Declaration is not supposed to be linked as the direction '{consignment.DirectionOfTrade}' is not supported", false);
				}
			});
		}

		void AssertStandAloneDeclarationNotCreatedForConsignmentWithError(HVLVConsignment consignment, IConvertToStandAloneDeclarationResponse result, string expectedError)
		{
			CombineAssertions("Declaration should not be created when the consignment is not attached to a shipment", () =>
			{
				AssertEquals("Convert should fail", false, result.ConversionSucceeded);
				AssertEquals("Should have correct error message", expectedError, result.ConversionFailureReason);
				AssertEquals("Declaration not created", Guid.Empty, result.ConvertedStandAloneDeclaration);
				AssertNull("Consignment has no linked declaration", consignment.StandAloneDeclarationForCurrentCompany);
			});
		}

		#endregion
	}
}
