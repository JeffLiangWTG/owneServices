using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	class ForwardingExportAWBHeaderValidationTest : ExportAWBHeaderValidationTest
	{
		#region Additional Security Information

		public void TestCheckEH_ScheduledArrivalDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.VisualCheck;

				var consol = shipment.Consols.AddNew();
				Factory.Save();
				AssertEquals(ScreeningMethods.Codes.VisualCheck, shipment.JS_InspectionTypeCode);

				var awbHeader = consol.AWBHeader;
				awbHeader.EH_AgentApprovalNumber = "BLAH123";
				awbHeader.EH_RN_NKAgentApprovalCountryCode = Constants.CountryCodes.UnitedKingdom;
				var awbActions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All);
				awbActions.SendFWB = true;
				awbActions.PrintConsignmentSecurityDeclaration = true;

				var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				awbHeader.CargoSecurityScreeningMethods.AddNew().EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;
				AssertEquals(securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings("EH_SecurityStatus != SPX", awbHeader.EH_ScheduledArrivalDateInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft, awbHeader.EH_SecurityStatus);
				consol.JK_OverrideSecurityDeclarationDefaults = true;
				Assert(consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);

				var addressCountryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalNumber = "12345-01";
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				Assert(!consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);
				AssertEquals(ZDateTimeOffset.Empty, awbHeader.EH_ScheduledArrivalDate);
				awbHeader.Validation.ValidateAll();
				AssertHasWarning("When EH_SecurityStatus is SPX and EH_ScheduledArrivalDate is blank, show the warning on the CTO Date field regardless if the override is ticked or not", awbHeader.EH_ScheduledArrivalDateInfo, "For known cargo, the scheduled date/time of arrival at the terminal is required per the CAA (Civil Aviation Authority).");

				consol.JK_OverrideSecurityDeclarationDefaults = false;
				AssertHasWarning("When EH_SecurityStatus is SPX and EH_ScheduledArrivalDate is blank, show the warning on the CTO Date field regardless if the override is ticked or not", awbHeader.EH_ScheduledArrivalDateInfo, "For known cargo, the scheduled date/time of arrival at the terminal is required per the CAA (Civil Aviation Authority).");

				consol.JK_OverrideSecurityDeclarationDefaults = true;
				awbHeader.EH_ScheduledArrivalDate = ZDateTimeOffset.Now;
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings(awbHeader.EH_ScheduledArrivalDateInfo);

				consol.JK_OverrideSecurityDeclarationDefaults = false;
				awbHeader.PopulateSecurityDeclarationIfNotOverridden();
				AssertEquals(ZDateTimeOffset.Empty, awbHeader.EH_ScheduledArrivalDate);
				AssertHasWarning("When EH_SecurityStatus is SPX and EH_ScheduledArrivalDate is blank, show the warning on the CTO Date field regardless if the override is ticked or not", awbHeader.EH_ScheduledArrivalDateInfo, "For known cargo, the scheduled date/time of arrival at the terminal is required per the CAA (Civil Aviation Authority).");
			}
		}

		public void TestCheckEH_AdditionalSecurityInformationStatement()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				var consol = shipment.Consols.AddNew();
				consol.IsAWBValuesOverriddenProperty = true;
				Factory.Save();
				var awbHeader = consol.AWBHeader;
				consol.JK_OverrideSecurityDeclarationDefaults = true;
				awbHeader.Validation.ValidateAll();
				AssertHasWarning(awbHeader.EH_AdditionalSecurityInformationStatementInfo, "Please select the statement that best describes the examination or clearance details of the cargo.");
				awbHeader.EH_AdditionalSecurityInformationStatement = "Invalid statement";
				awbHeader.Validation.ValidateAll();
				AssertHasError(awbHeader.EH_AdditionalSecurityInformationStatementInfo, "Enter a valid selection.");

				awbHeader.EH_AdditionalSecurityInformationStatement = AWBAdditionalSecurityStatementList.Codes.Regulation441JA;
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings(awbHeader.EH_AdditionalSecurityInformationStatementInfo);
				awbHeader.EH_AdditionalSecurityInformationStatement = AWBAdditionalSecurityStatementList.Codes.Regulation441J;
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings(awbHeader.EH_AdditionalSecurityInformationStatementInfo);
				awbHeader.EH_AdditionalSecurityInformationStatement = AWBAdditionalSecurityStatementList.Codes.NotRequired;
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings(awbHeader.EH_AdditionalSecurityInformationStatementInfo);
				awbHeader.EH_AdditionalSecurityInformationStatement = AWBAdditionalSecurityStatementList.Codes.Received;
				awbHeader.Validation.ValidateAll();
				AssertNoWarnings(awbHeader.EH_AdditionalSecurityInformationStatementInfo);

				consol.JK_OverrideSecurityDeclarationDefaults = false;
				AssertEquals(ZString.Empty, awbHeader.EH_AdditionalSecurityInformationStatement);
				AssertHasWarning(awbHeader.EH_AdditionalSecurityInformationStatementInfo, "Please select the statement that best describes the examination or clearance details of the cargo.");
			}
		}

		#endregion

		#region Organisation with ICS2 Country

		protected OrgHeader GenerateOrganisationIsIcs2Country()
		{
			var country = Factory.New<RefCountry>();
			country.Code = "GB";
			Assert(country.IsIcs2Member);

			RefUNLOCO consignorPort = Factory.New<RefUNLOCO>();
			consignorPort.RL_Code = "SGFAK";
			consignorPort.RL_RN_NKCountryCode = country.RN_Code;

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = consignorPort.RL_Code;
			return organisation;
		}

		protected OrgHeader GenerateOrganisationIsNotIcs2Country()
		{
			var country = Factory.New<RefCountry>();
			country.Code = "AU";
			Assert(!country.IsIcs2Member);

			RefUNLOCO consignorPort = Factory.New<RefUNLOCO>();
			consignorPort.RL_Code = "SGFAL";
			consignorPort.RL_RN_NKCountryCode = country.RN_Code;

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = consignorPort.RL_Code;
			return organisation;
		}

		#endregion
	}
}
