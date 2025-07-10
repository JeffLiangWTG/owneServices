using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class CargoSecurityExportAWBHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEH_AgentApprovalNumber()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			header.Validation.ValidateEH_AgentApprovalNumber();
			AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "Enter the identifier of the Regulated Agent issuing the security status.");

			header.EH_AgentApprovalNumber = "1234";
			AssertNoMessageError(header.EH_AgentApprovalNumberInfo, "Enter the identifier of the Regulated Agent issuing the security status.");

			header.EH_AgentApprovalNumber = "RA333A3";
			AssertHasWarning(header.EH_AgentApprovalNumberInfo, "Regulated Agent’s approval number should only contain capital letters, numbers or hyphens. The \"RA\" prefix of the code is not required here.");

			header.EH_AgentApprovalNumber = "24343";
			AssertNoWarnings(header.EH_AgentApprovalNumberInfo);

			header.EH_AgentApprovalNumber = "A837A45";
			AssertNoWarnings(header.EH_AgentApprovalNumberInfo);

			header.EH_AgentApprovalNumber = "82738-01";
			AssertNoWarnings(header.EH_AgentApprovalNumberInfo);

			header.EH_AgentApprovalNumber = "8989b9a";
			AssertHasWarning(header.EH_AgentApprovalNumberInfo, "Regulated Agent’s approval number should only contain capital letters, numbers or hyphens. The \"RA\" prefix of the code is not required here.");

			header.EH_AgentApprovalNumber = ZString.Empty;
			AssertNoWarnings(header.EH_AgentApprovalNumberInfo);
		}

		public void TestCheckEH_AgentApprovalNumber_SecurityDeclarationCannotBeIssued_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "MYKUL";

				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var knownShipperDetails = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_OH_OrgHeader = orgProxy.PK;
				knownShipperDetails.OV_EXApprovedOrMajorExporter = "AA";
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_EXApprovalNumber = "12345-67";

				knownShipperDetails.Factory.Save();

				consol.JK_OA_SendingForwarderAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy)).PK;

				var header = consol.AWBHeader;
				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "A Security Declaration cannot be issued for this consignment because the Sending Agent does not match the login (issuing) company.");

				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertNoMessageError(header.EH_AgentApprovalNumberInfo, "A Security Declaration cannot be issued for this consignment because the Sending Agent does not match the login (issuing) company.");
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "The Sending Agent is an AACA so cannot clear cargo.");

				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				knownShipperDetails.Factory.Save();

				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertNoMessageError(header.EH_AgentApprovalNumberInfo, "The Sending Agent is an AACA so cannot clear cargo.");
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "Issuing Sending Agent does not have a regulated status for air cargo security.");

				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_EXApprovedOrMajorExporter = "RA";
				knownShipperDetails.OV_EXApprovalNumber = "12345-67";
				knownShipperDetails.Factory.Save();

				header.Populate();
				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertNoMessageErrors(header.EH_AgentApprovalNumberInfo);
			}
		}

		public void TestCheckEH_AgentApprovalNumber_SecurityDeclarationCannotBeIssued_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				const string expectedError = "The Regulated Agent Identifier must be in the following format: 'NNNNN-NN'. For example: 00002-01.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var header = consol.AWBHeader;
				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertEquals("Precondition", "GB", header.EH_RN_NKAgentApprovalCountryCode);
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_AgentApprovalNumber = "XYZ-AB";
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_AgentApprovalNumber = "12345-67";
				AssertNoMessageError("Valid approval number", header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_RN_NKAgentApprovalCountryCode = "IT";
				header.EH_AgentApprovalNumber = "XYZ-AB";
				AssertNoMessageError("Approval Number Validation is not used for Italy", header.EH_AgentApprovalNumberInfo, expectedError);
			}
		}

		public void TestValidateEH_RN_NKAgentApprovalCountryCode()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();
			header.EH_RN_NKAgentApprovalCountryCode = "ZA";

			header.Validation.ValidateEH_RN_NKAgentApprovalCountryCode();
			AssertHasWarning(header.EH_RN_NKAgentApprovalCountryCodeInfo, "The country/region here is different from the currently logged in country/region.");

			header.EH_RN_NKAgentApprovalCountryCode = GlbCompany.CurrentCompany.Country.Code;
			AssertNoWarning(header.EH_RN_NKAgentApprovalCountryCodeInfo, "The country/region here is different from the currently logged in country/region.");
		}

		public void TestValidateEH_AdditionalScreeningMethods()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			var line = header.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ScreeningMethod = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;

			header.Validation.ValidateEH_AdditionalScreeningMethods();
			AssertHasMessageError(header.EH_AdditionalScreeningMethodsInfo, "One or more of the Shipments have Inspection type \"AOM\" - Subjected to any other means.  Please specify the alternative method of screening used.");

			line.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment;

			header.Validation.ValidateEH_AdditionalScreeningMethods();
			AssertNoMessageError(header.EH_AdditionalScreeningMethodsInfo, "One or more of the Shipments have Inspection type \"AOM\" - Subjected to any other means.  Please specify the alternative method of screening used.");
		}

		public void TestValidateEH_SecurityStatus()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			header.Validation.ValidateEH_SecurityStatus();
			AssertHasMessageError(header.EH_SecurityStatusInfo, "Security Status is required. This can be entered on Consol > Docs > Security Status.");

			var securityStatus = header.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			header.Validation.ValidateEH_SecurityStatus();
			AssertHasMessageError(header.EH_SecurityStatusInfo, "The Special Handling Code on Consol > AWB is NSC. The Security Declaration can only be issued if the cargo is secured. Check the security status of the attached Shipments.");

			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

			header.Validation.ValidateEH_SecurityStatus();
			AssertNoMessageError(header.EH_SecurityStatusInfo, "The Special Handling Code on Consol > AWB is NSC. The Security Declaration can only be issued if the cargo is secured. Check the security status of the attached Shipments.");
		}

		public void TestValidateEH_SecurityStatus_AccountConsignor()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<ConsolExportAWBHeaderForTest>();

				var securityStatus = header.AWBSpecialHandlingItems.AddNew();
				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

				var line = header.ExportAWBSecurityStatusLines.AddNew();
				line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.AccountConsignor;

				header.Validation.ValidateEH_SecurityStatus();
				AssertHasMessageError(header.EH_SecurityStatusInfo, "This code is not valid if any of the shipments attached have been received from an Account Consignor. This defaults from AWB > Special Handling Codes.");

				line.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

				header.Validation.ValidateEH_SecurityStatus();
				AssertNoMessageError(header.EH_SecurityStatusInfo, "This code is not valid if any of the shipments attached have been received from an Account Consignor. This defaults from AWB > Special Handling Codes.");

				line.Delete();

				header.Validation.ValidateEH_SecurityStatus();
				AssertNoMessageError(header.EH_SecurityStatusInfo, "This code is not valid if any of the shipments attached have been received from an Account Consignor. This defaults from AWB > Special Handling Codes.");
			}
		}

		public void TestEH_SecurityStatusIssuedBy()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();
			var errorMessage = "Person Screening is required.";

			header.EH_SecurityStatusIssuedBy = "";

			AssertHasMessageError(header.EH_SecurityStatusIssuedByInfo, errorMessage);

			header.EH_SecurityStatusIssuedBy = "Roger Rabbit";

			AssertNoMessageError(header.EH_SecurityStatusIssuedByInfo, errorMessage);
		}

		public void TestEH_SecurityStatusIssuedBy_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var header = Factory.New<ConsolExportAWBHeaderForTest>();
				var warningMessage = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

				header.EH_GS_NKSecurityStatusIssuedByCode = "ZZ";
				AssertHasWarning(header.EH_SecurityStatusIssuedByInfo, warningMessage);

				var certificate = header.SecurityStatusIssuedBy.Certificates.AddNew();
				certificate.XZ_Type = "CO1";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				header.EH_SecurityStatusIssuedBy = "Roger Rabbit";

				AssertNoWarning(header.EH_SecurityStatusIssuedByInfo, warningMessage);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var header = Factory.New<ConsolExportAWBHeaderForTest>();
				var warningMessage = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

				header.EH_GS_NKSecurityStatusIssuedByCode = "ZZ";
				AssertNoWarning(header.EH_SecurityStatusIssuedByInfo, warningMessage);
			}
		}

		public void TestEH_SecurityStatusIssueDate()
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			header.EH_SecurityStatusIssueDate = ZDateTime.Empty;

			AssertHasMessageError(header.EH_SecurityStatusIssueDateInfo, "This defaults from the Consol > Docs > Master Bill Issue Date field. Check it has been entered.");

			header.EH_SecurityStatusIssueDate = ZDateTime.Today;

			AssertNoMessageError(header.EH_SecurityStatusIssueDateInfo, "This defaults from the Consol > Docs > Master Bill Issue Date field. Check it has been entered.");
		}

		public void TestEH_SecurityStatusRequired()
		{
			var errorMessageForRequired = "Security Status is required. This can be entered on Consol > Docs > Security Status.";

			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			header.Validation.ValidateEH_SecurityStatus();
			AssertHasMessageError(header.EH_SecurityStatusInfo, errorMessageForRequired);

			var securityStatus = header.AWBSpecialHandlingItems.AddNew();
			foreach (var code in new AWBSpecialHandlingCodeDescriptionPairList().GetAllCodes())
			{
				securityStatus.EP_SpecialHandling = code;
				header.Validation.ValidateEH_SecurityStatus();
				if (securityStatus.IsSecurityStatus)
				{
					AssertNoErrorContaining($"{code} should have the error message", header.EH_SecurityStatusInfo, errorMessageForRequired);
				}
				else
				{
					AssertHasMessageError($"{code} should have the error message", header.EH_SecurityStatusInfo, errorMessageForRequired);
				}
			}

			header.Delete();
		}

		#region Implementaion

		sealed class ConsolExportAWBHeaderForTest : ConsolExportAWBHeader
		{
			public ConsolExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ExportAWBHeaderValidation GetNewValidation()
			{
				var validation = base.GetNewValidation();
				validation.Add(new CargoSecurityExportAWBHeaderValidation(this));

				return validation;
			}
		}

		#endregion
	}
}
