using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods.Testing
{
	sealed class ForwardingUNDGDataItemCFRValidationTest : TestCaseWithFactory
	{
		public void TestCheckDI_DGWeightForUSCFR()
		{
			{
				const string maximumCargoMessage = "This quantity exceeds the maximum allowed for a Cargo Aircraft.";
				const string maximumPassengerMessage = "This quantity exceeds the maximum allowed for a Passenger Aircraft.";

				var substance = Factory.New<UNDGSubstanceCFR>();
				substance.CFR_UNNO = "3507";

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MostInterestingTransportForBinding[0].JW_IsCargoOnly = false;
				var shipment = consol.Shipments.AddNew();
				shipment.TransportsIncludingRelated[0].JW_IsCargoOnly = true;
				var packline = shipment.OuterPackLines.AddNew();
				var undgDataItem = packline.UNDGs.AddNew();
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_DGWeight = 0.05;
				undgDataItem.DI_UnitOfWeight = "KG";
				undgDataItem.RunPreSaveValidation();

				AssertNoError(undgDataItem.DI_DGWeightInfo, maximumCargoMessage);

				undgDataItem.DI_DGWeight = 3;
				undgDataItem.RunPreSaveValidation();

				AssertHasError(undgDataItem.DI_DGWeightInfo, maximumCargoMessage);

				undgDataItem.DI_DGWeight = 0.1;
				undgDataItem.RunPreSaveValidation();

				AssertNoError(undgDataItem.DI_DGWeightInfo, maximumCargoMessage);

				shipment.TransportsIncludingRelated[0].JW_IsCargoOnly = false;
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_DGWeight = 0.05;
				undgDataItem.DI_UnitOfWeight = "KG";
				undgDataItem.RunPreSaveValidation();

				AssertNoError(undgDataItem.DI_DGWeightInfo, maximumPassengerMessage);

				undgDataItem.DI_DGWeight = 3;
				undgDataItem.RunPreSaveValidation();

				AssertHasError(undgDataItem.DI_DGWeightInfo, maximumPassengerMessage);

				undgDataItem.DI_DGWeight = 0.1;
				undgDataItem.RunPreSaveValidation();

				AssertNoError(undgDataItem.DI_DGWeightInfo, maximumPassengerMessage);
			}

			{
				const string errorMessage = "When shipping this substance, the gross weight is to be entered.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MostInterestingTransportForBinding[0].JW_IsCargoOnly = true;
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();
				var undgDataItem = packline.UNDGs.AddNew();

				foreach (var unno in new[] { "2800", "3164", "3166", "3506", "8000" })
				{
					var substance = Factory.New<UNDGSubstanceCFR>();
					substance.CFR_UNNO = unno;

					Factory.Save();

					undgDataItem.DI_DG = substance.PK;
					undgDataItem.DI_DGWeight = 0;
					undgDataItem.DI_UnitOfWeight = "KG";
					undgDataItem.RunPreSaveValidation();

					AssertHasWarning(undgDataItem.DI_DGWeightInfo, errorMessage);

					undgDataItem.DI_DGWeight = 100;

					AssertHasWarning(undgDataItem.DI_DGWeightInfo, errorMessage);
				}
			}

			{
				const string errorMessage = "Either a weight or volume estimate is required for Salvage Packaging.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MostInterestingTransportForBinding[0].JW_IsCargoOnly = true;
				var shipment = consol.Shipments.AddNew();
				var packline = shipment.OuterPackLines.AddNew();
				var undgDataItem = packline.UNDGs.AddNew();

				var substance = Factory.New<UNDGSubstanceCFR>();
				substance.CFR_UNNO = "1234";

				Factory.Save();

				undgDataItem.DI_IsSalvagePackaging = true;
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_DGWeight = 0;
				undgDataItem.DI_UnitOfWeight = "KG";
				undgDataItem.RunPreSaveValidation();

				AssertHasWarning(undgDataItem.DI_DGWeightInfo, errorMessage);
				AssertHasWarning(undgDataItem.DI_DGVolumeInfo, errorMessage);

				undgDataItem.DI_DGWeight = 100;

				AssertNoWarning(undgDataItem.DI_DGWeightInfo, errorMessage);
				AssertNoWarning(undgDataItem.DI_DGVolumeInfo, errorMessage);
			}
		}

		#region DI_DG

		public void TestCheckDI_DGForUSCFR()
		{
			const string errorMessage = "This substance is forbidden for uplift by Air.";

			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "0421";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			consol.Shipments.Add(shipment);

			substance.CFR_CargoAirRailLimitType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(undgDataItem.DI_DGInfo, errorMessage);

			var transport = consol.MostInterestingTransportForBinding[0];
			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_IsCargoOnly = true;
			transport.JW_IsCargoOnly = false;

			substance.CFR_CargoAirRailLimitType = UNDGSubstanceLookups.LimitedQuantityTypes.NLTCode;
			substance.CFR_PAXAirRailLimitType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(undgDataItem.DI_DGInfo, errorMessage);
		}

		public void TestDI_DGValidationUsesCaseInsensitiveCFRPrefix()
		{
			var consol = GetConsolForCFRTesting("na");
			var shipment = consol.Shipments[0];
			var undgDataItem = shipment.OuterPackLines[0].UNDGs[0];

			var naErrorMessage = "DG substances with NA codes are only allowed for domestic movements within the United States or movements between the United States and Canada.";

			AssertDI_DGHasError("CFR validation should occur regardless of the prefix's case.", undgDataItem, naErrorMessage);
		}

		public void TestDI_DGValidationForNASubstances()
		{
			var consol = GetConsolForCFRTesting(UNDGSubstanceCFR.Prefixes.NA);
			var shipment = consol.Shipments[0];
			var consolTransport = consol.Transports[0];
			var undgDataItem = shipment.OuterPackLines[0].UNDGs[0];

			var naErrorMessage = "DG substances with NA codes are only allowed for domestic movements within the United States or movements between the United States and Canada.";

			AssertDI_DGHasError("CFR substance with NA prefix not permitted when no transport legs on the shipment.", undgDataItem, naErrorMessage);

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USJFK";
			transport.JW_RL_NKDiscPort = "CAAUX";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US and Canada exists.", undgDataItem, naErrorMessage);

			transport.JW_RL_NKLoadPort = "CAAUX";
			transport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US and Canada exists.", undgDataItem, naErrorMessage);

			transport.JW_RL_NKLoadPort = "USLAX";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg within US exists.", undgDataItem, naErrorMessage);

			transport.JW_RL_NKLoadPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US and US Territory exists.", undgDataItem, naErrorMessage);

			transport.JW_RL_NKDiscPort = "CAAUX";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US Territory and Canada exists.", undgDataItem, naErrorMessage);

			transport.JW_RL_NKLoadPort = "CAAUX";
			transport.JW_RL_NKDiscPort = "AUSYD";

			AssertDI_DGHasError("CFR substance with NA prefix not permitted when no transport leg within US or between US and Canada exists.", undgDataItem, naErrorMessage);

			consolTransport.JW_RL_NKLoadPort = "USLAX";
			consolTransport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg within US exists on related consol.", undgDataItem, naErrorMessage);

			consolTransport.JW_RL_NKDiscPort = "CAUUX";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US and Canada exists on related consol.", undgDataItem, naErrorMessage);

			consolTransport.JW_RL_NKLoadPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US Territory and Canada exists on related consol.", undgDataItem, naErrorMessage);

			consolTransport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with NA prefix permitted when transport leg between US and US Territory exists on related consol.", undgDataItem, naErrorMessage);
		}

		public void TestDI_DGValidationForIDSubstances()
		{
			var consol = GetConsolForCFRTesting(UNDGSubstanceCFR.Prefixes.ID);
			var shipment = consol.Shipments[0];
			var consolTransport = consol.Transports[0];
			var undgDataItem = shipment.OuterPackLines[0].UNDGs[0];

			var idErrorMessage = "49 CFR DG Substances with ID codes are only allowed for domestic and international movements from/to the United States.";

			AssertDI_DGHasError("CFR substance with ID prefix not permitted when no transport legs on the shipment.", undgDataItem, idErrorMessage);

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USJFK";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg from US exists.", undgDataItem, idErrorMessage);

			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg to US exists.", undgDataItem, idErrorMessage);

			transport.JW_RL_NKLoadPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg between US and US Territory exists.", undgDataItem, idErrorMessage);

			transport.JW_RL_NKLoadPort = "USLAX";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg within US exists.", undgDataItem, idErrorMessage);

			transport.JW_RL_NKLoadPort = "AUPER";
			transport.JW_RL_NKDiscPort = "AUSYD";

			AssertDI_DGHasError("CFR substance with ID prefix not permitted when no transport leg from or to US exists.", undgDataItem, idErrorMessage);

			consolTransport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg to US exists on related consol.", undgDataItem, idErrorMessage);

			consolTransport.JW_RL_NKDiscPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg to US Territory exists on related consol.", undgDataItem, idErrorMessage);

			consolTransport.JW_RL_NKLoadPort = "USJFK";

			AssertDI_DGNoError("CFR substance with ID prefix permitted when transport leg from US to US Territory exists on related consol.", undgDataItem, idErrorMessage);
		}

		public void TestDI_DGValidationForUNSubstances()
		{
			var consol = GetConsolForCFRTesting(UNDGSubstanceCFR.Prefixes.UN);
			var shipment = consol.Shipments[0];
			var consolTransport = consol.Transports[0];
			var undgDataItem = shipment.OuterPackLines[0].UNDGs[0];

			var unErrorMessage = "49 CFR DG Substances with UN codes are only allowed for domestic and international movements from/to the United States.";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when no transport legs but shipment from US.", undgDataItem, unErrorMessage);

			shipment.JS_RL_NKOrigin = "AUSYD";

			AssertDI_DGHasError("CFR substance with UN prefix not permitted when no transport legs and shipment not from/to US or US Territory.", undgDataItem, unErrorMessage);

			shipment.JS_RL_NKOrigin = "PRGUY";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when no transport legs but shipment from US Territory.", undgDataItem, unErrorMessage);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when no transport legs but shipment to US.", undgDataItem, unErrorMessage);

			shipment.JS_RL_NKDestination = "PRGUY";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when no transport legs but shipment to US Territory.", undgDataItem, unErrorMessage);

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUPER";

			shipment.JS_RL_NKDestination = "AUPER";
			shipment.JS_RL_NKOrigin = "USLAX";

			AssertDI_DGHasError("CFR substance with UN prefix not permitted when transport legs exist but none from/to US or US Territory.", undgDataItem, unErrorMessage);

			shipment.JS_RL_NKOrigin = "AUSYD";
			transport.JW_RL_NKLoadPort = "USJFK";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg from US exists.", undgDataItem, unErrorMessage);

			transport.JW_RL_NKLoadPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg from US Territory exists.", undgDataItem, unErrorMessage);

			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = "USJFK";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg to US exists.", undgDataItem, unErrorMessage);

			transport.JW_RL_NKDiscPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg to US Territory exists.", undgDataItem, unErrorMessage);

			transport.JW_RL_NKDiscPort = "AUSYD";

			AssertDI_DGHasError("CFR substance with UN prefix not permitted when no transport leg from/to US or US Territory exists.", undgDataItem, unErrorMessage);

			consolTransport.JW_RL_NKLoadPort = "USJFK";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg from US exists on related consol.", undgDataItem, unErrorMessage);

			consolTransport.JW_RL_NKDiscPort = "PRGUY";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg from US to US Territory exists on related consol.", undgDataItem, unErrorMessage);

			consolTransport.JW_RL_NKLoadPort = "AUSYD";

			AssertDI_DGNoError("CFR substance with UN prefix permitted when transport leg to US Territory exists on related consol.", undgDataItem, unErrorMessage);
		}

		public void TestDI_DGValidationDefaultsCFRPrefixToUNWhenEmpty()
		{
			var consol = GetConsolForCFRTesting(string.Empty);
			var shipment = consol.Shipments[0];
			shipment.JS_RL_NKOrigin = "AUSYD";
			var undgDataItem = shipment.OuterPackLines[0].UNDGs[0];

			var unErrorMessage = "49 CFR DG Substances with UN codes are only allowed for domestic and international movements from/to the United States.";

			AssertDI_DGHasError("CFR substance with no prefix should be assumed to be \"UN\", and UN CFR substances not permitted when no transport legs and shipment not from/to US or US Territory.", undgDataItem, unErrorMessage);
		}

		ForwardingConsol GetConsolForCFRTesting(string cfrPrefix)
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_Prefix = cfrPrefix;
			cfrSubstance.CFR_UNNO = "1001";

			Factory.Save();

			var consolTransport = consol.Transports.AddNew();
			consolTransport.JW_RL_NKLoadPort = "NZAKL";
			consolTransport.JW_RL_NKDiscPort = "AUPER";

			var packLine = shipment.OuterPackLines.AddNew();
			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;

			Factory.Save();

			return consol;
		}

		void AssertDI_DGHasError(string message, UNDGDataItem undgDataItem, string expectedNotification)
		{
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError(message, undgDataItem.DI_DGInfo, expectedNotification);
		}

		void AssertDI_DGNoError(string message, UNDGDataItem undgDataItem, string expectedNotification)
		{
			undgDataItem.Validation.ValidateDI_DG();

			AssertNoError(message, undgDataItem.DI_DGInfo, expectedNotification);
		}

		#endregion

		#region DI_RadioactiveLabelCategory

		public void TestCheckDI_RadioactiveLabelCategory()
		{
			const string expectedWarningMessage =
				"49 CFR Section 172.403 (C) (1) advises any package containing a Highway Route Controlled Quantity must be labeled as RADIOACTIVE YELLOW-III.";

			var cfrSubstance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			cfrSubstance.CFR_PrimaryClass = RadioactiveConstants.RadioactiveClass;
			Factory.Save();

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.DI_IsHighwayRouteControlledQuantity = ZBool.True;

			undgDataItem.Validation.ValidateDI_RadioactiveLabelCategory();
			AssertNoWarning(undgDataItem.DI_RadioactiveLabelCategoryInfo, expectedWarningMessage);

			undgDataItem.DI_RadioactiveLabelCategory = ZString.Empty;
			AssertHasWarning(undgDataItem.DI_RadioactiveLabelCategoryInfo, expectedWarningMessage);
		}

		#endregion

		#region DI_TechnicalName

		public void TestDGTechnicalNameValidationIfSP441()
		{
			const string errorMessage =
				"Technical Name is required for the Ocean Booking for substances of the IMO Standard with Special Provision 274 and/or 318, and substances of the CFR Standard with Special Provision 441.";

			var subs = Factory.New<UNDGSubstanceCFR>();
			subs.CFR_UNNO = "3082";
			subs.CFR_Variant = "a";
			subs.CFR_SpecialProvisions = "8 146 173 335 441";

			Factory.Save();

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.LinkDefault(subs);
			undgDataItem.Validation.ValidateDI_TechnicalName();
			AssertHasMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.DI_TechnicalName = "Killer Vanila";
			undgDataItem.Validation.ValidateDI_TechnicalName();
			AssertNoMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);
		}

		#endregion
	}
}
