using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AviationSecuritySupportTest : CommonAviationSecuritySupportTest
	{
		#region Approved Shipper Status

		public void TestSetApprovedShipperStatus_Generic()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "NZAKL";
				transport1.JW_RL_NKDiscPort = "JMALP";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "JMALP";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Approved Shipper Status is not set for Transhipment", !shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status remains as 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_RL_NKOrigin = "JMKIN";
				transport1.JW_RL_NKLoadPort = "JMKIN";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				consignor.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);

				consol.Transports[0].JW_ATD = ZDateTime.Now.AddDays(-1);
				Assert("Can not set Approved Shipper status as the first air leg has departed.", !shipment.SetApprovedShipperStatus(""));

				consol.Transports[0].JW_ATD = ZDateTime.Empty;
				consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(-1);
				Assert("Can set Approved Shipper status as the first air leg has not departed.", shipment.SetApprovedShipperStatus(""));

				consol.Transports[0].JW_ATD = ZDateTime.Now.AddDays(1);
				consol.Transports[0].JW_ETD = ZDateTime.Empty;
				Assert("Can set Approved Shipper status as the first air leg has not departed.", shipment.SetApprovedShipperStatus(""));

				consol.Transports[0].JW_ATD = ZDateTime.Empty;
				consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
				Assert("Can set Approved Shipper status as the first air leg has not departed.", shipment.SetApprovedShipperStatus(""));

				transport1.JW_RL_NKLoadPort = "NZAKL";
				transport2.JW_RL_NKDiscPort = "JMWKF";
				shipment.JS_RL_NKDestination = "JMWKF";
				shipment.JS_RL_NKOrigin = "NZAKL";

				shipment.JS_InspectionTypeCode = "XRY";

				Assert("Can't set Approved Shipper Status for Import", !shipment.SetApprovedShipperStatus(""));
				AssertEquals("Approval status has not changed", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Inspection Status can only be calculated for Air shipments where the Origin Country/Region matches the current Login Country/Region.", shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
			}
		}

		public void TestSetApprovedShipperStatus_Japan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport1.JW_RL_NKDiscPort = "JPOSA";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "JPOSA";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Can set Approved Shipper Status for Transhipment", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'TRN'", "TRN", shipment.JS_InspectionTypeCode);

				shipment.JS_RL_NKOrigin = "JPTYO";
				transport1.JW_RL_NKLoadPort = "JPTYO";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				var countryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.JS_E_DEP = ZDateTime.Now;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);

				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(2);
				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' as approval will have expired before MAWB issue date", "UNK", shipment.JS_InspectionTypeCode);

				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_RL_NKDiscPort = "JPTYO";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_RL_NKOrigin = "AUBNE";

				shipment.JS_InspectionTypeCode = "XRY";

				Assert("Can't set Approved Shipper Status for Import", !shipment.SetApprovedShipperStatus(""));
				AssertEquals("Approval status has not changed", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Inspection Status can only be calculated for Air Exports or Transhipments.", shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
			}
		}

		public void TestSetApprovedShipperStatus_Japan_RedefaultWhenDatesChange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "JPOSA";
				transport1.JW_RL_NKDiscPort = "DEHAM";

				var countryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.JS_E_DEP = ZDateTime.Now;
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);
				AssertEquals("Recalculation message as Inspection Type has been automatically set", "Estimated Time of Departure has been changed", shipment.MostRecentInspectionTypeChangeReason);

				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(2);
				AssertEquals("Status is set to 'UNK' as approval will have expired before MAWB issue date", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Recalculation message as Inspection Type has been automatically set", "Master Bill Issue Date has been changed", shipment.MostRecentInspectionTypeChangeReason);

				consol.JK_MasterBillIssueDate = ZDateTime.Now;
				AssertEquals("Status is set back to 'APP' as the approval is valid on the MAWB Issue date", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "XRY";
				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(2);
				AssertEquals("We don't override user supplied values", "XRY", shipment.JS_InspectionTypeCode);
				AssertEquals("No recalculation message as Inspection Type has been manually set", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);

				shipment.SetApprovedShipperStatus("", true);
				AssertEquals("We can force redefaulting even if user has supplied values", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("No recalculation message as Inspection Type has been manually recalculated", ZString.Empty, shipment.MostRecentInspectionTypeChangeReason);
			}
		}

		public void TestSetApprovedShipperStatus_HongKong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport1.JW_RL_NKDiscPort = "HKHKG";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "HKHKG";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Can set Approved Shipper Status for Transhipment", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'TRN'", "TRN", shipment.JS_InspectionTypeCode);

				shipment.JS_RL_NKOrigin = "HKKWN";
				transport1.JW_RL_NKLoadPort = "HKKWN";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				var countryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.JS_E_DEP = ZDateTime.Now;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);

				consol.JK_MasterBillIssueDate = ZDateTime.Now.AddDays(2);
				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("MAWB issue date is used for calculating inspection status in Hong Kong", "UNK", shipment.JS_InspectionTypeCode);

				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_RL_NKDiscPort = "HKHKG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_RL_NKOrigin = "AUBNE";

				shipment.JS_InspectionTypeCode = "XRY";

				Assert("Can't set Approved Shipper Status for Import", !shipment.SetApprovedShipperStatus(""));
				AssertEquals("Approval status has not changed", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Inspection Status can only be calculated for Air Exports or Transhipments.", shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
			}
		}

		public void TestSetApprovedShipperStatus_HongKong_OriginInChina()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "CNSHA";
				transport1.JW_RL_NKDiscPort = "HKHKG";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "HKHKG";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestSetApprovedShipperStatus_China()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport1 = consol.Transports[0];
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Road;
				transport1.JW_RL_NKLoadPort = "CNSHA";
				transport1.JW_RL_NKDiscPort = "HKHKG";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "HKHKG";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.GetInspectionTypeCodeForCountry(Constants.CountryCodes.HongKong));

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("HK Status is set to 'UNK'", "UNK", shipment.GetInspectionTypeCodeForCountry(Constants.CountryCodes.HongKong));

				var countryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				countryData.OV_RN_NKClientCountryRelation = Constants.CountryCodes.HongKong;

				shipment.JS_E_DEP = ZDateTime.Now;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("HK Status is set to 'APP'", "APP", shipment.GetInspectionTypeCodeForCountry(Constants.CountryCodes.HongKong));
			}
		}

		public void TestSetApprovedShipperStatus_UnitedKingdom()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var airline = CreateOrg("AIR");
				var consignor = CreateOrg("CON");

				var shipment1 = SetupShipment();
				Assert("Precondition: passenger flight validation does not apply", !shipment1.AviationSecurity.PassengerFlightValidationApplies);

				shipment1.JS_InspectionTypeCode = "APP";
				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				SetOrgApproval(airline);
				SetOrgApproval(consignor);

				Assert("Automatic calculation of approved status is allowed because shipment is approved by certificated user", shipment1.AviationSecurity.SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment1));
				Assert("Can set Approved Shipper status", shipment1.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment1.JS_InspectionTypeCode);

				var shipment2 = SetupShipment();

				Assert("Precondition: additional conditions for approval are met", shipment2.AviationSecurity.SupplyChainSecurityConfiguration.CheckAdditionalConditionsForApprovedInspectionType(shipment2));
				Assert("Precondition: relevant orgs are approved for aviation security", shipment2.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);

				shipment2.JS_InspectionTypeCode = "UNK";
				Assert("Automatic calculation of approved status is not allowed until approved by certificated user", !shipment2.AviationSecurity.SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment2));
				Assert("Can set Approved Shipper Status for Export", shipment2.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment2.JS_InspectionTypeCode);

				shipment2.JS_InspectionTypeCode = "APP";
				shipment2.JS_E_DEP = ZDateTime.Now;
				Assert("Automatic calculation of approved status is allowed because shipment is approved by certificated user", shipment2.AviationSecurity.SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment2));
				AssertEquals("Status is set to 'APP'", "APP", shipment2.JS_InspectionTypeCode);

				ForwardingShipment SetupShipment()
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "GBBYS";
					shipment.JS_RL_NKDestination = "DEHAM";

					var consol = shipment.Consols.AddNew();
					consol.JK_OA_ShippingLineAddress = airline.MainAddress.PK;

					var transport1 = consol.Transports.AddNew();
					transport1.JW_TransportMode = Constants.TransportModes.Air;
					transport1.JW_RL_NKLoadPort = "GBBYS";
					transport1.JW_RL_NKDiscPort = "DEHAM";

					return shipment;
				}
			}
		}

		public void TestSetApprovedShipperStatus_Australia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "DEHAM";

				Assert("Can set Approved Shipper Status for Export", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				var countryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = consignor.PK;
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' as not a regulated agent", "UNK", shipment.JS_InspectionTypeCode);

				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyCountryData = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyCountryData.OV_OH_OrgHeader = orgProxy.PK;
				orgProxyCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				orgProxyCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

				orgProxy.Factory.Save();

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);

				var consol = shipment.Consols.AddNew();
				consol.JK_MasterBillIssueDate = ZDateTime.Now;
				var transport1 = consol.Transports.AddNew();
				var transport2 = consol.Transports.AddNew();

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport1.JW_RL_NKDiscPort = "JPOSA";

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_RL_NKLoadPort = "JPOSA";
				transport2.JW_RL_NKDiscPort = "DEHAM";

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' as consol sending forwarder has not been set", "UNK", shipment.JS_InspectionTypeCode);

				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP'", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestSetApprovedShipperStatus_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "AUBNE";

				Assert("Can set Approved Shipper Status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_IsHighRisk = true;

				Assert("Cannot set Approved Shipper Status for High Risk shipment", !shipment.SetApprovedShipperStatus("test"));
				AssertEquals("Status is set to 'UNK'", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Inspection Status cannot be calculated for High Risk shipments.", shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
			}
		}

		#region TestSetApprovedShipperStatusWithRelevantOrganisations

		public void TestSetApprovedShipperStatus_SingleRelevantOrganisationWARN_NotApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "WARN")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				AssertEquals("No organisations are known", "UNK", shipment.JS_InspectionTypeCode);
				AssertHasOrganisationWarning("Should contain a Warning as consigner is not approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_SingleRelevantOrganisationWARN_Approved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "WARN")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrgWithApproval("CON");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				AssertEquals("All WARN organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as everyone is approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_SingleRelevantOrganisationYES_NotApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				AssertEquals("No organisations are known", "UNK", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as nothing set to WARN", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_SingleRelevantOrganisationYES_Approved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrgWithApproval("CON");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				AssertEquals("All YES organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as nothing set to WARN", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsWARN_NoneApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "WARN"), ("TRS", "WARN")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				var transportCo = CreateOrg("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("No organisations are known", "UNK", shipment.JS_InspectionTypeCode);
				AssertHasOrganisationWarning("Should contain a Warning as organisations aren't approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsWARN_OneApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "WARN"), ("TRS", "WARN")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				var transportCo = CreateOrgWithApproval("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("At least one WARN organisation is known", "APP", shipment.JS_InspectionTypeCode);
				AssertHasOrganisationWarning("Should contain a Warning as Consignor is not approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsWARN_AllApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "WARN"), ("TRS", "WARN")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrgWithApproval("CON");
				var transportCo = CreateOrgWithApproval("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("All WARN organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as everyone is approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsYES_NoneApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				var transportCo = CreateOrg("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("No organisations are known", "UNK", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as nothing set to WARN", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsYES_OneApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrg("CON");
				var transportCo = CreateOrgWithApproval("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("At least one YES organisation is not known", "UNK", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as nothing set to WARN", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsYES_AllApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES")))
			{
				var shipment = CreateShipmentForEUShipperStatusTest();
				var consignor = CreateOrgWithApproval("CON");
				var transportCo = CreateOrgWithApproval("TRN");
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;

				AssertEquals("All YES organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as nothing set to WARN", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsMixed_AllApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES"), ("CFS", "WARN"), ("LOC", "WARN")))
			{
				var consignor = CreateOrgWithApproval("CON");       // YES
				var transportCo = CreateOrgWithApproval("TRN");     // YES
				var shipmentCfs = CreateOrgWithApproval("CFS_S");   // WARN
				var localClient = CreateOrgWithApproval("LOC");     // WARN

				var shipment = CreateShipmentForEUShipperStatusTest();
				var job = CreateShipmentJob(shipment);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = shipmentCfs.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				AssertEquals("All organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as everyone is approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsMixed_AllYESApproved_OneWARNApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES"), ("CFS", "WARN"), ("LOC", "WARN")))
			{
				var consignor = CreateOrgWithApproval("CON");       // YES
				var transportCo = CreateOrgWithApproval("TRN");     // YES
				var shipmentCfs = CreateOrgWithApproval("CFS_S");   // WARN
				var localClient = CreateOrg("LOC");                 // WARN

				var shipment = CreateShipmentForEUShipperStatusTest();
				var job = CreateShipmentJob(shipment);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = shipmentCfs.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				AssertEquals("All YES organisations are known", "APP", shipment.JS_InspectionTypeCode);
				AssertHasOrganisationWarning("Should contain a Warning as Local Client is not approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisationsMixed_AllWARNApproved_OneYESApproved()
		{
			using (SetShipmentInspectionOrganisationToUse_EU(("CON", "YES"), ("TRS", "YES"), ("CFS", "WARN"), ("LOC", "WARN")))
			{
				var consignor = CreateOrg("CON");                   // YES
				var transportCo = CreateOrgWithApproval("TRN");     // YES
				var shipmentCfs = CreateOrgWithApproval("CFS_S");   // WARN
				var localClient = CreateOrgWithApproval("LOC");     // WARN

				var shipment = CreateShipmentForEUShipperStatusTest();
				var job = CreateShipmentJob(shipment);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = shipmentCfs.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				AssertEquals("a YES organisation is not known", "UNK", shipment.JS_InspectionTypeCode);
				AssertNoOrganisationWarning("Should not contain a Warning as all WARN orgs are approved", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		void AssertNoOrganisationWarning(string exceptionMessage, ZPropertyInfo info)
		{
			var releventWarnings = info.GetWarnings().Where(w => w.Message.StartsWith(UnapprovedOrganisationMessagePrefix));
			Assert(exceptionMessage, !releventWarnings.Any());
		}

		void AssertHasOrganisationWarning(string exceptionMessage, ZPropertyInfo info)
		{
			var releventWarnings = info.GetWarnings().Where(w => w.Message.Contains(UnapprovedOrganisationMessagePrefix));
			Assert(exceptionMessage, releventWarnings.Any());
		}

		public void TestSetApprovedShipperStatus_MultipleRelevantOrganisations_WarningMessages()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";

				var consol = shipment.Consols.AddNew();
				var transport = consol.Transports.AddNew();

				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				var consignor = CreateOrg("CON");
				var localClient = CreateOrg("LOC");
				var transportCo = CreateOrg("TRN");
				var shipmentCfs = CreateOrg("CFS_S");
				var sendingAgent = CreateOrg("AGT");
				var airline = CreateOrg("AIR");
				var consolCfs = CreateOrg("CFS_C");

				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = shipmentCfs.PK;
				consol.JK_OA_PackDepotAddress = consolCfs.PK;
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				shipment.SetApprovedShipperStatus("");

				AssertEquals("At least one approved organisation must be included for an approval", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals(false, shipment.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);
				AssertEquals("The following Organizations are not Approved:\r\nLocal Client (LOC)", shipment.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval());
				AssertEquals("There are no relevant organisations", 0, shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().Count);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = transportCo.MainAddress.PK;
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				consol.JK_OA_ShippingLineAddress = airline.MainAddress.PK;
				shipment.SetApprovedShipperStatus("");

				AssertEquals("UNK", shipment.JS_InspectionTypeCode);
				AssertEquals(false, shipment.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);
				AssertEquals("The following Organizations are not Approved:\r\nLocal Client (LOC)\r\nConsol Sending Agent (AGT)", shipment.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval());
				AssertEquals(1, shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().Count);
				AssertEquals(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consignor (CON)
Shipment Pickup Transport Company (TRN)
Consol Airline (AIR)", shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().First());

				SetOrgApproval(consignor);
				SetOrgApproval(transportCo);
				shipment.SetApprovedShipperStatus("");

				AssertEquals("UNK", shipment.JS_InspectionTypeCode);
				AssertEquals(false, shipment.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);
				AssertEquals("The following Organizations are not Approved:\r\nLocal Client (LOC)\r\nConsol Sending Agent (AGT)", shipment.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval());
				AssertEquals(1, shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().Count);
				AssertEquals(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consol Airline (AIR)", shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().First());

				SetOrgApproval(airline);
				shipment.SetApprovedShipperStatus("");

				AssertEquals("All YES organisations have approval", "APP", shipment.JS_InspectionTypeCode);
				AssertEquals(true, shipment.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);
				AssertEquals("The following Organizations are not Approved:\r\nLocal Client (LOC)\r\nConsol Sending Agent (AGT)", shipment.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval());
				AssertEquals("All relevant organisations are approved", 0, shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().Count);

				SetOrgApproval(localClient);
				SetOrgApproval(sendingAgent);
				shipment.SetApprovedShipperStatus("");

				AssertEquals("APP", shipment.JS_InspectionTypeCode);
				AssertEquals(true, shipment.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity);
				AssertEquals("There are no organisations with warnings", ZString.Empty, shipment.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval());
				AssertEquals("All relevant organisations are approved", 0, shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().Count);
			}
		}

		IDisposable SetShipmentInspectionOrganisationToUse_EU(params (string, string)[] values)
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = "NO";

			foreach (var (org, code) in values)
			{
				settings[org].ValidationCode = code;
			}

			var registryToClear = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
			var countryToClear = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);

			return new DisposableAction(() =>
			{
				registryToClear.Dispose();
				countryToClear.Dispose();
			});
		}

		ForwardingShipment CreateShipmentForEUShipperStatusTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "HKHKG";

			return shipment;
		}

		JobHeader CreateShipmentJob(ForwardingShipment shipment)
		{
			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			return job;
		}

		#endregion

		[TestDate]
		public void TestGetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval_ApprovalWillHaveExpired()
		{
			var mockedDate = new DateTime(2012, 12, 25);
			TestDateAttribute.Date = mockedDate;
			AssertEquals("Precondition: date is mocked correctly", mockedDate, ZDateTime.Now);

			string partialError = "The following Organizations' known/approved status will lapse between the current date and the Shipment ETD of";
			string expectedError = FormattableString.Invariant($@"The following Organizations' known/approved status will lapse between the current date and the Shipment ETD of {ZDate.Today.AddDays(2).ToShortDateString()}, so an Inspection Type of Approved/Known shipper is not allowed:
Consignor (CON)");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.HongKong))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				var consignor = CreateOrg("CON");
				SetOrgApproval(consignor, AviationSecuritySchemeMembership.Codes.AccountConsignor);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.JS_E_DEP = ZDate.Today;
				shipment.JS_InspectionTypeCode = "APP";

				AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, partialError);

				shipment.JS_E_DEP = ZDate.Today.AddDays(1);
				shipment.JS_InspectionTypeCode = "APP";
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, partialError);

				shipment.JS_E_DEP = ZDate.Today.AddDays(2);
				shipment.JS_InspectionTypeCode = "APP";
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertHasError(shipment.JS_InspectionTypeCodeInfo, expectedError);
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, partialError);
			}
		}

		OrgHeader CreateOrg(string code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			return org;
		}

		void SetOrgApproval(OrgHeader org, string eXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor)
		{
			var approval = org.MainAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = eXApprovedOrMajorExporter;
		}

		OrgHeader CreateOrgWithApproval(string code)
		{
			var org = CreateOrg(code);
			SetOrgApproval(org);
			return org;
		}

		#endregion

		#region ShimentDateForAviationSecurity

		public void TestShipmentDateForAviationSecurity()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "CNSHA";

			AssertEquals("Unsaved shipment", ZDateTime.Now, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			AssertEquals("NOW", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);

			Factory.Save();

			AssertEquals("Saved shipment", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			AssertEquals("CRE", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);

			shipment.JS_E_DEP = ZDate.Today.AddDays(1);

			AssertEquals("Shipment with ETD", shipment.JS_E_DEP, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			AssertEquals("SHP", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);

			var preCarriageConsol = shipment.Consols.AddNew();
			preCarriageConsol.JK_TransportMode = "ROA";
			preCarriageConsol.Transports[0].JW_TransportMode = "ROA";
			preCarriageConsol.Transports[0].JW_ETD = ZDate.Today.AddDays(2);
			preCarriageConsol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			preCarriageConsol.Transports[0].JW_RL_NKDiscPort = "AUSYD";

			var airConsol = shipment.Consols.AddNew();
			airConsol.JK_TransportMode = "AIR";

			var transport1 = airConsol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_ETD = ZDate.Today.AddDays(3);
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";

			var transport2 = airConsol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_ETD = ZDate.Today.AddDays(4);
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "CNSHA";

			AssertEquals("Shipment with export air consol", transport1.JW_ETD, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			AssertEquals("CON", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);

			airConsol.JK_MasterBillIssueDate = ZDate.Today.AddDays(-1);

			AssertEquals("Shipment with export air consol with MAWB issue date", airConsol.JK_MasterBillIssueDate, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			AssertEquals("MBL", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);
		}

		#endregion

		#region ActualDepartureDate

		public void TestActualDepartureDate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "JPOSA";

			AssertEquals("Shipment has no consol or transports", ZDateTime.Empty, shipment.AviationSecurity.ActualDepartureDate);

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = "ROA";
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_ATD = ZDateTime.Today.AddDays(-9);

			AssertEquals("Transport mode is not air", ZDateTime.Empty, shipment.AviationSecurity.ActualDepartureDate);

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			transport2.JW_ETD = ZDateTime.Today.AddDays(-8);

			AssertEquals("Air transport has no ATD", ZDateTime.Empty, shipment.AviationSecurity.ActualDepartureDate);

			transport2.JW_ATD = ZDateTime.Today.AddDays(-7);

			AssertEquals("Actual departure date is from first air leg on shipment", ZDateTime.Today.AddDays(-7), shipment.AviationSecurity.ActualDepartureDate);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "AIR";

			var transport3 = consol.Transports[0];
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "JPTYO";
			transport3.JW_TransportMode = "AIR";
			transport3.JW_ETD = ZDateTime.Today.AddDays(-6);

			AssertEquals("Shipment is attached to a consol with no ATD", ZDateTime.Empty, shipment.AviationSecurity.ActualDepartureDate);

			transport3.JW_ATD = ZDateTime.Today.AddDays(-5);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "JPTYO";
			transport4.JW_RL_NKDiscPort = "JPOSA";
			transport4.JW_ATD = ZDateTime.Today.AddDays(-4);

			AssertEquals("Actual departure date is from consol's first air leg", ZDateTime.Today.AddDays(-5), shipment.AviationSecurity.ActualDepartureDate);
		}

		#endregion

		#region IsApprovedForAviationSecurity

		public void TestIsApprovedForAviationSecurity_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "ZACPE";
				shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				AssertEquals(false, shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				AssertEquals("JS_InspectionTypeCode - Empty", false, shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_InspectionTypeCode = "XRY";
				Assert(shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_IsHighRisk = true;
				shipment.JS_AdditionalInspectionTypeCode = "UNK";
				AssertEquals("High risk - Unknown", false, shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				Assert("Approved for high risk requirements", shipment.AviationSecurity.IsApprovedForAviationSecurity);
			}
		}

		#endregion

		#region Inspection Status

		public void TestHasUnknownInspectionStatus_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "CNSHA";

				shipment.JS_InspectionTypeCode = "UNK";
				Assert("Low risk UNK shipment", shipment.AviationSecurity.HasUnknownInspectionTypeCode);
				Assert("Low risk not approved", !shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_InspectionTypeCode = "PHS";
				Assert("Low risk non-UNK shipment", !shipment.AviationSecurity.HasUnknownInspectionTypeCode);
				Assert("Low risk approved", shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_IsHighRisk = true;
				AssertEquals("Precondition: UNK additional inspection", "UNK", shipment.JS_AdditionalInspectionTypeCode);
				Assert("High risk UNK shipment", shipment.AviationSecurity.HasUnknownInspectionTypeCode);
				Assert("High risk not approved", !shipment.AviationSecurity.IsApprovedForAviationSecurity);

				shipment.JS_AdditionalInspectionTypeCode = "XRY";
				Assert("High risk non-UNK shipment", !shipment.AviationSecurity.HasUnknownInspectionTypeCode);
				Assert("High risk approved", shipment.AviationSecurity.IsApprovedForAviationSecurity);
			}
		}

		#endregion

		#region High Risk Shipment

		public void TestIsHighRiskShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "NZAKL";

				Assert("EU but not high-risk", !shipment.AviationSecurity.IsHighRiskShipment);

				shipment.JS_IsHighRisk = true;
				Assert("High risk", shipment.AviationSecurity.IsHighRiskShipment);
			}

			var nonEUShipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			nonEUShipment.JS_TransportMode = "AIR";
			nonEUShipment.JS_RL_NKOrigin = "AUBNE";
			nonEUShipment.JS_RL_NKDestination = "NZAKL";

			Assert("Non-EU and not high-risk", !nonEUShipment.AviationSecurity.IsHighRiskShipment);

			nonEUShipment.JS_IsHighRisk = true;
			Assert("High risk does not apply to AU", !nonEUShipment.AviationSecurity.IsHighRiskShipment);
		}

		#endregion

		#region Licence

		public void TestAviationSecurityLicence()
		{
			TestAviationSecurityLicence("JP", "JPOSA", FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP);
			TestAviationSecurityLicence("HK", "HKHKG", FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK);
		}

		void TestAviationSecurityLicence(string countryCode, string origin, BooleanRegistryItem registryItem)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = "AUBNE";

				using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					string registryDisabledMessage = string.Format("Supply Chain Security must be enabled via the registry under '{0}/{1}'.", registryItem.Category, registryItem.Caption);

					Assert("Can't set approved shipper status if registry is disabled", !shipment.SetApprovedShipperStatus(""));
					AssertEquals(registryDisabledMessage, shipment.AviationSecurity.ReasonForAviationSecurityNotBeingAvailable);
					AssertEquals(registryDisabledMessage, shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
				}

				Assert("Can set approved shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Licence is enabled", ZString.Empty, shipment.AviationSecurity.ReasonForAviationSecurityNotBeingAvailable);

				shipment.JS_RL_NKOrigin = "FRPAR";
				Assert("Shipment is not valid for setting approved shipper status", !shipment.SetApprovedShipperStatus(""));
				AssertEquals("Licence is OK", ZString.Empty, shipment.AviationSecurity.ReasonForAviationSecurityNotBeingAvailable);
				AssertEquals("Can't set approved shipper status", "Inspection Status can only be calculated for Air Exports or Transhipments.", shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus);
			}
		}

		#endregion

		const string UnapprovedOrganisationMessagePrefix = "The following Organizations are not Approved:";
	}
}
