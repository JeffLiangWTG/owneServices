using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationUSTest : SupplyChainSecurityConfigurationTest
	{
		public override void TestIsOnlyForDevelopers()
		{
			var configuration = GetNewSupplyChainSecurityConfigurationToTest();
			AssertEquals("Should remove this test case when this configuration is available for customers to use", true, configuration.IsOnlyForDevelopers);
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestUseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry()
		{
			Assert(SupplyChainSecurityConfiguration.UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry);
		}

		public void TestApprovalNumberAllowed()
		{
			Assert("YES allows approval number", SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber("YES"));
			Assert("NO allows approval number", SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber("NO"));
		}

		#region CheckJL_InspectionTypeCode_DestinationValidationCore

		public void TestCheckJL_InspectionTypeCode_DestinationValidationCore()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "USPHL";

				var packingLine = shipment.OuterPackLines.AddNew();
				packingLine.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				packingLine.Validation.ValidateAll();
				AssertNoWarnings(packingLine.JL_InspectionTypeCodeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var expectedWarning = "This Shipment is destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages.";
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "CNSIN";
				shipment.JS_RL_NKDestination = "USPHL";

				var packingLine = shipment.OuterPackLines.AddNew();
				packingLine.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				packingLine.Validation.ValidateAll();
				AssertHasWarning(packingLine.JL_InspectionTypeCodeInfo, expectedWarning);
			}
		}

		#endregion

		#region Known Shipper Filter

		public override void TestKnownShipperFilter()
		{
			AssertEquals("TSA Known Shipper Status", SupplyChainSecurityConfiguration.KnownShipperFilterText);
			AssertEquals("TSA Known Shipper Status", SupplyChainSecurityConfiguration.KnownShipperFilterDescription);

			AssertEquals("All", SupplyChainSecurityConfiguration.KnownShipperFilterList["ALL"].Description);
			AssertEquals("Known Shipper", SupplyChainSecurityConfiguration.KnownShipperFilterList["YES"].Description);
			AssertEquals("Unknown Shipper", SupplyChainSecurityConfiguration.KnownShipperFilterList["NO"].Description);
			AssertEquals("Every code + All should be in the list", 3, SupplyChainSecurityConfiguration.KnownShipperFilterList.Count);
		}

		#endregion

		#region Approval Number Filter

		public override void TestApprovalNumberFilter()
		{
			AssertEquals("TSA ID Number", SupplyChainSecurityConfiguration.ApprovalNumberFilterText);
			AssertEquals("TSA ID Number", SupplyChainSecurityConfiguration.ApprovalNumberFilterDescription);
		}

		#endregion

		#region Consignment Security Declaration

		public override void TestUseConsignmentSecurityDeclaration()
		{
			AssertEquals("Should not use Consignment Security Declaration", false, SupplyChainSecurityConfiguration.UseConsignmentSecurityDeclaration);
		}

		public override void TestAllowIncludeECSD()
		{
			AssertEquals("Should not allow include eCSD", false, SupplyChainSecurityConfiguration.AllowIncludeECSD);
		}

		#endregion

		#region Consol has domestic transport leg

		public void TestIsPackLevelScreeningRequired_WhenConsolHasDomesticTransportLegs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var result = CreateUSExportShipmentAndItsConsolWithDomesticLeg("SVSAL");
				Assert("Should be false when consol is export from US and has a domestic leg in US", !SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(result.Consol));
			}
		}

		public void TestJL_InspectionTypeCode_ReadOnly_WhenConsolHasDomesticTransportLeg()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var result = CreateUSExportShipmentAndItsConsolWithDomesticLeg("AUSYD");
				result.Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
				Assert("Should be true when consol is export from US and has a domestic leg in US", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(result.Shipment));
			}
		}

		public void TestValidateSendFWBPackLineInspectionTypes_WhenConsolHasDomesticTransportLeg()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var expectedWarning = "This Consol is destined for El Salvador, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between El Salvador and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

				var result = CreateUSExportShipmentAndItsConsolWithDomesticLeg("SVSAL");

				var awbActions = new ConsolAWBActions(result.Consol, Business.AWB.AWBActions.ActionsModeType.All);
				awbActions.DateLastSent = "";
				awbActions.SendFWB = ZBool.False;

				SupplyChainSecurityConfiguration.ValidateSendFWBPackLineInspectionTypes(result.Consol, awbActions.SendFWBInfo);

				AssertNoError(awbActions.SendFWBInfo, expectedWarning);
				AssertNoWarning(awbActions.SendFWBInfo, expectedWarning);

				var firstTransport = result.Consol.Transports[0];
				firstTransport.JW_RL_NKLoadPort = "AUSYD";
				SupplyChainSecurityConfiguration.ValidateSendFWBPackLineInspectionTypes(result.Consol, awbActions.SendFWBInfo);
				AssertHasWarning(awbActions.SendFWBInfo, expectedWarning);
			}
		}

		public void TestCheckEAS_ScreeningMethod_PackLevelScreeningValidation_WhenConsolHasDomesticTransportLegs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var expectedWarning = "Shipment/s  are destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";

				var result = CreateUSExportShipmentAndItsConsolWithDomesticLeg("SVSAL");

				var line = Factory.New<ExportAWBSecurityStatusLine>();
				line.EAS_EH = result.Consol.AWBHeader.PK;
				line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				line.EAS_ApprovalNumber = "";
				line.EAS_ScreeningMethod = FreightDataRegistry.AviationSecurity_Unknown_Code;
				line.Validation.ValidateAll();
				AssertNoMessageError(line.EAS_ScreeningMethodInfo, expectedWarning);

				var firstTransport = result.Consol.Transports[0];
				firstTransport.JW_RL_NKLoadPort = "AUSYD";
				line.Validation.ValidateAll();
				AssertHasMessageError(line.EAS_ScreeningMethodInfo, expectedWarning);
			}
		}

		(ForwardingShipment Shipment, ForwardingConsol Consol) CreateUSExportShipmentAndItsConsolWithDomesticLeg(string destination)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USCHS";
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = "AIR";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USCHS";
			consol.JK_RL_NKDischargePort = destination;

			var packingLine = shipment.OuterPackLines.AddNew();

			var firstTransport = consol.Transports[0];
			firstTransport.JW_RL_NKLoadPort = "USCHS";
			firstTransport.JW_RL_NKDiscPort = "USPHL";

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_RL_NKLoadPort = "USPHL";
			secondTransport.JW_RL_NKDiscPort = destination;
			return (shipment, consol);
		}

		#endregion

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationUS();
		}

		#endregion
	}
}
