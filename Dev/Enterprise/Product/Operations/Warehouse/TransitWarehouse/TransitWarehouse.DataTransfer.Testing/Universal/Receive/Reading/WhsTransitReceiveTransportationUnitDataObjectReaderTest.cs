using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveTransportationUnitDataObjectReaderTest : TransitUniversalTestCase
	{
		#region TestPopulateBizO_CreateNewHeader

		public void TestPopulateBizO_CreateNewHeader()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Header reference should be generated from number fountain.", "TR00000001", header.WRH_ReferenceNumber);
			AssertEquals("Warehouse must be set to the one that passed into the reader.", warehouse.GetValue(WhsWarehouseSchema.PK), header.WRH_WW_Warehouse);
			AssertEquals("Vehicle reference should match the container number.", "C1", header.WRH_VehicleReference);
		}

		public void TestPopulateBizO_SetRTUStagingLocation()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			AssertNotEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);

			var tempInboundDockDoor = warehouse.WW_DefaultInboundDockDoor;
			warehouse.WW_DefaultInboundDockDoor = ZGuid.Empty;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("RTU's StagingLocation should be empty", ZGuid.Empty, header.WRH_WL_StagingLocation);

			warehouse.WW_DefaultInboundDockDoor = tempInboundDockDoor;
			header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("RTU's StagingLocation should be warehouse.DefaultInboundDockDoorLocation", warehouse.WW_DefaultInboundDockDoor, header.WRH_WL_StagingLocation);
		}

		public void TestPopulateBizO_RTUHasStagingLocation_DoNotUpdate()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			AssertNotEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotNull("RTU's StagingLocation is not null", header.WRH_WL_StagingLocation);
			AssertEquals("RTU's StagingLocation should be warehouse.DefaultInboundDockDoorLocation", header.WRH_WL_StagingLocation, warehouse.WW_DefaultInboundDockDoor);

			var ddlLocationType = Helper.CreateLocationType("DOD", LocationClasses.Codes.DDL);
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			header.WRH_WL_StagingLocation = locationA1.PK;
			Factory.SaveForTesting();

			var header1 = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("RTU's StagingLocation is already exist", header.WRH_WL_StagingLocation, header1.WRH_WL_StagingLocation);
			AssertNotEquals("RTU's StagingLocation dose not update", warehouse.WW_DefaultInboundDockDoor, header1.WRH_WL_StagingLocation);
		}

		public void TestPopulateBizO_CreateNewHeader_AddditionalReferences()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var additionalReferencesForRTU = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, header.PK));
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MB1", WarehouseAdditionalReferenceTypes.Descriptions.MasterBill);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingConsolNumber);
			AssertEquals(2, additionalReferencesForRTU.Length);
		}

		public void TestPopulateBizO_CreateNewHeader_CreatesPivotToASNIfRequired()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Should attach a pivot to the RTU and ASN", 1, pivots.Length);
			AssertEquals("Should attach the pivot to the RTU", rtu.PK, pivots[0].WAR_WRH_TransitReceiveTransportationUnit);
			AssertEquals("Should attach the pivot to the ASN", asn.PK, pivots[0].WAR_WRP_TransitReceiveASN);
			AssertEquals("System create type should not be empty.", false, pivots[0].WAR_SystemCreateTimeUtc.IsEmpty);
			AssertEquals("System create user should not be empty.", false, pivots[0].WAR_SystemCreateUser.IsEmpty);
			AssertEquals("System last edit time should not be empty.", false, pivots[0].WAR_SystemLastEditTimeUtc.IsEmpty);
			AssertEquals("System last edit user should not be empty.", false, pivots[0].WAR_SystemLastEditUser.IsEmpty);

			AssertNoExceptionThrown(() => Factory.SaveForTesting());
			var newFactory = new UniversalObjectFactory();

			var rtuInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, asn, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var pivotsInNewFactory = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Should leave the pivot unchanged.", 1, pivotsInNewFactory.Length);
			AssertEquals("Should leave the pivot unchanged", pivots[0].PK, pivotsInNewFactory[0].PK);
			AssertEquals("Should leave the pivot unchanged", rtu.PK, pivotsInNewFactory[0].WAR_WRH_TransitReceiveTransportationUnit);
			AssertEquals("Should leave the pivot unchanged", asn.PK, pivotsInNewFactory[0].WAR_WRP_TransitReceiveASN);
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_CreateContainerRecord()
		{
			var businessObjectFactory = Factory.BOFactory;
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var packages = header.PackageJob.Packages;

			AssertContainsExactElementsInAnyOrder(new string[] { PkgUnit.Container }, packages.Select(p => p.KP_F3_NKPackType));
			var package = packages.SingleOrDefault();
			AssertEquals("C1", package?.KP_PackageID);
			var container = package.Container;
			AssertNotNull(container);
			AssertEquals("40GP", container.ContainerType.RC_Code);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var headerInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();

			AssertEquals("A reimported RTU should not create a duplicate container", 1, headerInNewFactory.PackageJob.Packages.Count);
			var packageInNewFactory = headerInNewFactory.PackageJob.Packages.Single();
			AssertEquals("C1", packageInNewFactory?.KP_PackageID);
			var containerInNewFactory = packageInNewFactory.Container;
			AssertNotNull(containerInNewFactory);
			AssertEquals("40GP", containerInNewFactory.ContainerType.RC_Code);

			var containerPackage = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			AssertEquals(TransitWarehouseSecurityStatuses.Codes.NotRequired, containerPackage.WPS_SecurityStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, containerPackage.WPS_CustomsStatus);
		}

		#region TestPopulateBizO_IsTransportProviderIsKnown

		#region Known Address and Approved Location

		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Empty, isKnownAlsoApproved: false, isKnown: true, "Is known when certificate type is CertifiedHaulier 'CH' and before Year 2027.");

		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Empty, isKnownAlsoApproved: true, isKnown: true, "Is known when certificate type is CertifiedHaulier 'CH' and before Year 2027.");

		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Today.AddDays(-2), isKnownAlsoApproved: false, isKnown: false, "Is not known if OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_CertifiedHaulier_Expired_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("CH", ZDate.Today.AddDays(-2), isKnownAlsoApproved: true, isKnown: false, "Is not known if OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Empty, isKnownAlsoApproved: false, isKnown: false, "Is not known when certificate type is RegulatedAgentACENotice 'RA', and Approved Location does not match Known address.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Empty, isKnownAlsoApproved: true, isKnown: true, "Is known when certificate type is RegulatedAgentACENotice 'RA', and Approved Location matches Known address.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Today.AddDays(-2), isKnownAlsoApproved: false, isKnown: false, "Is not known when certificate type is RegulatedAgentACENotice 'RA', and Approved Location does not match Known address.");

		public void TestPopulateBizO_TransportProviderIsKnown_RegulatedAgentACENotice_ApprovedLocation_Expired_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("RA", ZDate.Today.AddDays(-2), isKnownAlsoApproved: true, isKnown: false, "Is not known if OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Empty, isKnownAlsoApproved: false, isKnown: true, "Is known when certificate type is ApprovedHaulier 'AH'.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Empty, isKnownAlsoApproved: true, isKnown: true, "Is known when certificate type is ApprovedHaulier 'AH'.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_Expired() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Today.AddDays(-2), isKnownAlsoApproved: false, isKnown: false, "Is not known when OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_Expired_KnownAlsoApproved() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Today.AddDays(-2), isKnownAlsoApproved: true, isKnown: false, "Is not known when OrgCountryData has expired.");

		public void TestPopulateBizO_TransportProviderIsKnown_ApprovedHaulier_NotExpired() =>
			TestPopulateBizO_TransportProviderIsKnown("AH", ZDate.Today.AddDays(2), isKnownAlsoApproved: false, isKnown: true, "Is known when OrgCountryData has not expired.");

		protected void TestPopulateBizO_TransportProviderIsKnown(string exApprovedOrMajorExporter, ZDate expiredDate, bool isKnownAlsoApproved, bool isKnown, string reason)
		{
			var checkCertifiedHaulier = DateTime.Now < new DateTime(2027, 01, 01);
			isKnown = isKnown && (!(exApprovedOrMajorExporter == "CH") || checkCertifiedHaulier);

			var warehouse = Data.Warehouse;
			warehouse.WW_TransitSecurityProcessingRequired = true;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			var rtuTransportCompanyAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.DepartureCFSLocalTransportAddress));
			rtuTransportCompanyAddress.Address1 = "2804 Fudrucker Way";
			rtuTransportCompanyAddress.AddressShortCode = rtuTransportCompanyAddress.Address1;
			consolDO.OrganizationAddressCollection.Add(rtuTransportCompanyAddress);

			var crbAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ClientRequestedBillingParty));
			crbAddress.Address1 = "3804 Fudrucker Way";
			crbAddress.AddressShortCode = crbAddress.Address1;
			consolDO.OrganizationAddressCollection.Add(crbAddress);

			var org = Data.Orgs.CRAHOLSYD;
			var transportOrgAddress = org.Addresses.AddNew();
			transportOrgAddress.Address1 = rtuTransportCompanyAddress.Address1.Value;
			transportOrgAddress.OA_RN_NKCountryCode = rtuTransportCompanyAddress.Country.Code.Value;
			var crbOrgAddress = org.Addresses.AddNew();
			crbOrgAddress.Address1 = crbAddress.Address1.Value;
			crbOrgAddress.OA_RN_NKCountryCode = crbAddress.Country.Code.Value;

			var knownAddress = isKnownAlsoApproved ? transportOrgAddress : org.MainAddress;
			var orgCountryData = knownAddress.KnownShipperDetails.AddNew();
			orgCountryData.OV_OH_OrgHeader = org.PK;
			orgCountryData.OV_EXApprovedOrMajorExporter = exApprovedOrMajorExporter;
			orgCountryData.OV_EXApprovalExpiryDate = expiredDate;

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.SaveForTesting();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var transportCompanyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(transportCompanyAddress);
			AssertEquals(reason, isKnown, header.WRH_TransportProviderIsKnown);
		}

		#endregion		

		#region Org Country Data

		public void TestPopulateBizO_TransportProviderIsKnown_Country_EU_EU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.EuropeanUnion, CountryCodes.France, isKnown: true, "Is known when Company is an EU country, and Org Country Data is European Union.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_EU_NonEU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.EuropeanUnion, CountryCodes.SouthAfrica, isKnown: false, "Is not known when Company is not an EU country, and Org Country Data is European Union.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_UK() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.UnitedKingdom, CountryCodes.UnitedKingdom, isKnown: true, "Is known when UK Country Codes match.");

		public void TestPopulateBizO_TransportProviderIsKnown_Country_NonEU() =>
			TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(CountryCodes.SouthAfrica, CountryCodes.SouthAfrica, isKnown: true, "Is known when Country Codes match.");

		public void TestPopulateBizO_TransportProviderIsKnown_EUCountryCode(string approvedCountry, string currentCountry, bool isKnown, string message)
		{
			var warehouse = Data.Warehouse;
			warehouse.WW_TransitSecurityProcessingRequired = true;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;

			GlbCompany.CurrentCompany.SetCountry(currentCountry);
			var orgAddress = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.DepartureCFSLocalTransportAddress));
			consolDO.SetOrganizationAddressCollection(() => new() { orgAddress });

			var org = Data.Orgs.WUFSHIJNB;
			var orgCountryData = org.MainAddress.KnownShipperDetails.AddNew();
			orgCountryData.OV_OH_OrgHeader = org.PK;
			orgCountryData.OV_RN_NKClientCountryRelation = approvedCountry;
			orgCountryData.OV_EXApprovedOrMajorExporter = "AH";

			WarehouseDataRegistry.Instance.DriverSecurityCertificationCheckingActivated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.SaveForTesting();

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var transportCompanyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull(transportCompanyAddress);
			AssertEquals(message, isKnown, header.WRH_TransportProviderIsKnown);
		}

		#endregion

		#endregion

		#region TestPopulateBizO_PopulateBillToParty

		public void TestPopulateBizO_PopulateBillToParty_ArrivalWarehouse()
		{
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ReceivingForwarderAddress));
			var consolDO = Data.HeaderDataObject;
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var containerDO = Data.CreateContainer("C1", 0);
			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in RTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in RTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ReceivingForwarderAddress));
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in RTU in Arrival Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in RTU in Arrival Warehouse should be overridden", Data.Orgs.INTHEMSYD.MainAddress.PK, billToPartyAddress2.E2_OA_Address);
		}

		public void TestPopulateBizO_PopulateBillToParty_DepartureWarehouse()
		{
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });
			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.SendingForwarderAddress));
			var consolDO = Data.HeaderDataObject;
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var containerDO = Data.CreateContainer("C1", 0);
			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in RTU in Departure Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in RTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.SendingForwarderAddress));
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in RTU in Departure Warehouse should be same as Consol's ReceivingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in RTU in Departure Warehouse should be overridden", Data.Orgs.INTHEMSYD.MainAddress.PK, billToPartyAddress2.E2_OA_Address);
		}

		#endregion

		#region TestPopulateBizO_CannotUpdateTransportCompanyAddress

		public void TestPopulateBizO_CannotUpdateDepartureCFSLocalTransportAddress() =>
			TestPopulateBizO_CannotUpdateTransportCompanyAddress(nameof(DocAddressType.DepartureCFSLocalTransportAddress));

		public void TestPopulateBizO_CannotUpdateArrivalCFSLocalTransportAddress() =>
			TestPopulateBizO_CannotUpdateTransportCompanyAddress(nameof(DocAddressType.ArrivalCFSLocalTransportAddress));

		public void TestPopulateBizO_CannotUpdateTransportCompanyAddress(string addressType)
		{
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo
				{ RecipientRoles = new[] { new RecipientRoleDetail { Type = addressType == "ArrivalCFSLocalTransportAddress" ? RecipientRoleType.ATW : RecipientRoleType.DTW } } });
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);

			var org = addressType == "ArrivalCFSLocalTransportAddress" ? Data.Orgs.Warehouse_INTHEMSYD : Data.Orgs.Warehouse_WUFSHIJNB;
			org.AddressType = addressType;
			var consolDO = Data.HeaderDataObject;
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });

			var tempInboundDockDoor = warehouse.WW_DefaultInboundDockDoor;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();

			header.WRH_WL_StagingLocation = tempInboundDockDoor;
			Helper.CreatePackageState(header, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);

			consolDO.OrganizationAddressCollection
				.Where(address => address.AddressType.GetValueOrDefault().ToString() == addressType).FirstOrDefault()
				.Address1 = "New Address";

			Factory.SaveForTesting();

			AssertExceptionThrown<DataObjectReadFailureException>("Cannot update the RTU Transport Company Address. Some packages have already been unloaded.",
				() => new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject());
		}

		#endregion

		#endregion

		#region TestPopulateBizO_LinkMatchingHeader

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingHeader_MandatoryFields()
		{
			var warehouse = Data.Warehouse;
			var matchingRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(matchingRTU, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";

			AssertMatchedRTU(warehouse, containerDO, consolDO, matchingRTU.PK, "RTU with matching Warehouse and Container Number must be matched.");
			AssertNotMatchedRTU(Data.WarehouseCRAHOLSYD, containerDO, consolDO, matchingRTU.PK, "Should not match RTUs from other warehouses.");
			containerDO.ContainerNumber = "C3";
			AssertNotMatchedRTU(warehouse, containerDO, consolDO, matchingRTU.PK, "Should not match RTUs with a different container number.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_WithoutConsolNumberOrMasterBill_DoesNotMatch()
		{
			var warehouse = Data.Warehouse;
			var matchingRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO);
			consolDO.WayBillNumber = null;

			AssertNotMatchedRTU(Data.WarehouseCRAHOLSYD, containerDO, consolDO, matchingRTU.PK, "Should not match RTUs when the Consol has no Consol Number or Master Bill.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AdditionalReference_DBHits()
		{
			var warehouse = Data.Warehouse;
			var matchingRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(matchingRTU, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingRTU, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var matchingRTU2 = Helper.CreateReceiveTransportationUnitWithContainerType("2", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(matchingRTU2, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingRTU2, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";

			var newFactory = new UniversalObjectFactory();
			new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			AssertDbHits(new Dictionary<string, int>() { { CusEntryNumSchema.Constants.TableName, 1 } }, newFactory.BOFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingHeader_IgnoresRTUsOlderThan30Days()
		{
			var warehouse = Data.Warehouse;

			var oldRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(oldRTU, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			oldRTU.WRH_SystemCreateTimeUtc = new ZDateTime(ZDateTime.UtcNow.AddDays(-32));

			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";

			AssertNotMatchedRTU(warehouse, containerDO, consolDO, oldRTU.PK, "RTUs older than 30 days should not be matched.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingHeader_UsesMostRecent()
		{
			var warehouse = Data.Warehouse;

			var matchingRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(matchingRTU, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			matchingRTU.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11);
			var oldRTUWithin30Days = Helper.CreateReceiveTransportationUnitWithContainerType("2", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(oldRTUWithin30Days, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			oldRTUWithin30Days.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			AssertMatchedRTU(warehouse, containerDO, consolDO, matchingRTU.PK, "The most recent RTU is matched.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingHeader_Fallback()
		{
			var warehouse = Data.Warehouse;

			var rtuWithAllReferences = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, containerID: "C1", vehicleRef: "C1");
			Helper.CreateAdditionalReference(rtuWithAllReferences, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rtuWithAllReferences, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			rtuWithAllReferences.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);
			// Setup RTUs with a weaker but more recent match
			var rtuWithConsolNumber = Helper.CreateReceiveTransportationUnitWithContainerType("2", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, containerID: "C1", vehicleRef: "C1");
			Helper.CreateAdditionalReference(rtuWithConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			rtuWithConsolNumber.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11);
			var rtuWithMasterBill = Helper.CreateReceiveTransportationUnitWithContainerType("3", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, containerID: "C1", vehicleRef: "C1");
			Helper.CreateAdditionalReference(rtuWithMasterBill, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			rtuWithMasterBill.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 12);
			// Setup RTUs that should not match
			var rtuWithDifferentMasterBill = Helper.CreateReceiveTransportationUnitWithContainerType("4", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, containerID: "C1", vehicleRef: "C1");
			Helper.CreateAdditionalReference(rtuWithDifferentMasterBill, "MB2", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rtuWithDifferentMasterBill, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			rtuWithDifferentMasterBill.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 13);
			var rtuWithDifferentConsolNumber = Helper.CreateReceiveTransportationUnitWithContainerType("5", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, containerID: "C1", vehicleRef: "C1");
			Helper.CreateAdditionalReference(rtuWithDifferentConsolNumber, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rtuWithDifferentConsolNumber, "C2000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			rtuWithDifferentConsolNumber.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 14);

			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";
			AssertMatchedRTU(warehouse, containerDO, consolDO, rtuWithAllReferences.PK, "RTU with both MasterBill and ConsolNumber should be matched.");
			rtuWithAllReferences.Reload();
			rtuWithAllReferences.Delete();
			Factory.SaveForTesting();

			AssertMatchedRTU(warehouse, containerDO, consolDO, rtuWithConsolNumber.PK, "RTU should fallback to ConsolNumber.");
			rtuWithConsolNumber.Reload();
			rtuWithConsolNumber.Delete();
			Factory.SaveForTesting();

			AssertMatchedRTU(warehouse, containerDO, consolDO, rtuWithMasterBill.PK, "RTU should fallback to MasterBill.");
			rtuWithMasterBill.Reload();
			rtuWithMasterBill.Delete();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("Precondition.", 2, rtus.Length);

			new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();

			newFactory.SaveForTesting();
			rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("RTUs with contradicting references should not be matched.", 3, rtus.Length);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_Reimport_DoesNotDuplicateReferences()
		{
			var warehouse = Data.Warehouse;

			var matchingRTU = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "C1");
			Helper.CreateAdditionalReference(matchingRTU, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingRTU, string.Empty, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);

			Factory.SaveForTesting();

			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000", runSheet: "R1");
			consolDO.WayBillNumber = "MB1";

			AssertMatchedRTU(warehouse, containerDO, consolDO, matchingRTU.PK, "RTU should be matched.");

			var newFactory = new UniversalObjectFactory();
			var loadedRTU = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			var additionalReferencesForRTU = newFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadedRTU.PK));
			AssertEquals("RTU references should not be duplicated", 2, additionalReferencesForRTU.Length);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, AdditionalReferenceTypes.Codes.MasterBill, "MB1", AdditionalReferenceTypes.Descriptions.MasterBill);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingConsolNumber);
		}

		#endregion

		#region TestParametersNotNull

		public void TestParametersNotNull()
		{
			var warehouse = Data.Warehouse;
			var consolDO = Data.HeaderDataObject;
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			AssertExceptionThrown<ArgumentNullException>("Container must not be null", () => new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, null, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy));
			var containerDO = Data.CreateContainer("C1", 0);
			AssertExceptionThrown<ArgumentNullException>("Warehouse cannot be empty", () => new WhsTransitReceiveTransportationUnitDataObjectReader(null, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy));
			containerDO.ContainerNumber = null;
			AssertExceptionThrown<ArgumentNullException>("Container Number must not be empty", () => new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy));
			containerDO.ContainerNumber = ZString.Empty;
			AssertExceptionThrown<ArgumentException>("Container Number must not be empty", () => new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy));
		}

		#endregion

		#region TestImportIsSuccessfulIfMatchedRTUHasArrivedPackages_MandatoryRTUPropertiesCannotBeUpdated

		[TestDate(2024, 10, 14)]
		public void TestImportIsSuccessfulIfMatchedRTUHasArrivedPackages_MandatoryRTUPropertiesCannotBeUpdated()
		{
			var warehouse = Data.Warehouse;
			var containerDO = Data.CreateContainer("C1", 0);
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			AssertEquals(containerDO.ContainerNumber, rtu.WRH_VehicleReference);
			AssertEquals(warehouse.GetValue(WhsWarehouseSchema.PK), rtu.WRH_WW_Warehouse);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			Helper.CreateAdditionalReference(rtu, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreatePackageState(rtu, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreatePackageState(rtu, 1, "PLT", "PKG-2", TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();

			var rtuInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var pivotsInNewFactory = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("RTU Mandatory fields cannot be updated", rtu.WRH_ReferenceNumber, rtuInNewFactory.WRH_ReferenceNumber);
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		#endregion

		#region TestPopulateBizO_AddsLogForNewRTU

		public void TestPopulateBizO_AddsLogForNewRTU_PremiseIDNotMatched_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var asn = Helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "RandomID", "Lloyds", "Voyage", "Vessel");

			// set the warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);
			AssertExceptionThrown<DataObjectReadFailureException>("Premised ID not matched to warehouse.",
@"The premise ID 'RandomID' did not match the warehouse 'Transit Warehouse' premise ID (CCP) 'PREMISEID'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewRTU_PremiseIDAndVoyageFlightNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var asn = Helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.LloydsIMO = "Lloyds";

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an RTU because of below errors. Correct them and try again.
Premise ID is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_VesselLloydsAndPremiseIDNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var asn = Helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.VoyageFlightNo = "Voyage";

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an RTU because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Premise ID is not found.
", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_VesselLloydsAndVoyageFlightNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var asn = Helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = "PremiseID"
							},
						});

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, asn, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an RTU because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		void PopulateShipmentObjectWithSeaCargoOutturnData(Shipment shipment, string premiseId, string lloydsNumber, string voyageFlightNumber, string vesselName)
		{
			shipment.LloydsIMO = lloydsNumber;
			shipment.VoyageFlightNo = voyageFlightNumber;
			shipment.VesselName = vesselName;
			shipment.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = premiseId
							},
						});
		}

		#endregion

		#region TestPopulateBizO_ULDPackageExtension_AIR

		public void TestPopulateBizO_ULDPackageExtension_AIR()
		{
			TestPopulateBizO_ULDPackageExtension(TransportUnitTypes.ULD, "AIR");
		}

		public void TestPopulateBizO_ULDPackageExtension_SEA()
		{
			TestPopulateBizO_ULDPackageExtension(TransportUnitTypes.Container, "SEA");
		}

		public void TestPopulateBizO_ULDPackageExtension_ROA()
		{
			var warehouse = Data.Warehouse;
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, "ROA"));
			var containerDO = Data.CreateContainer("C1", 0, refContainer.RC_Code, "ROA");
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var packages = header.PackageJob.Packages;
			var containerPackage = packages.Single();
			var packageStatesForRTUPackage = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, containerPackage.PK));
			AssertEquals(0, packageStatesForRTUPackage.Length);
			AssertEquals(TransportUnitTypes.Vehicle, header.WRH_UnitType);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var headerInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var packagesInNewFactory = headerInNewFactory.PackageJob.Packages;
			var containerPackageInNewFactory = packagesInNewFactory.Single();
			AssertEquals(containerPackage.PK, containerPackageInNewFactory.PK);
			var packageStatesForRTUPackageInNewFactory = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, containerPackageInNewFactory.PK));
			AssertEquals(0, packageStatesForRTUPackageInNewFactory.Length);
			AssertEquals(TransportUnitTypes.Vehicle, header.WRH_UnitType);
		}

		void TestPopulateBizO_ULDPackageExtension(string expectedUnitType, string transportMode)
		{
			var warehouse = Data.Warehouse;
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, transportMode));
			var containerDO = Data.CreateContainer("C1", 0, refContainer.RC_Code, transportMode);
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var uldPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			var packages = header.PackageJob.Packages;
			var containerPackage = packages.Single();
			AssertEquals(expectedUnitType, header.WRH_UnitType);
			AssertEquals("C1", containerPackage.KP_PackageID);
			AssertEquals(containerPackage.PK, uldPackageState.WPS_KP_Package);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var headerInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var uldPackageStateInNewFactory = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, headerInNewFactory.PackageExtension.KPN_KP_Package));
			AssertEquals(expectedUnitType, headerInNewFactory.WRH_UnitType);
			AssertEquals(uldPackageState.PK, uldPackageStateInNewFactory.PK);
			AssertEquals(containerPackage.PK, uldPackageStateInNewFactory.WPS_KP_Package);
			AssertEquals(true, uldPackageStateInNewFactory.WPS_IsHandlingUnit);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, uldPackageStateInNewFactory.WPS_Status);
		}

		#endregion

		#region TestPopulateBizO_ULDPackageExtension_ChangeContainerDeliveryMode

		public void TestPopulateBizO_ULDPackageExtension_ChangeContainerDeliveryMode()
		{
			var warehouse = Data.Warehouse;
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, "SEA"));
			var containerDO = Data.CreateContainer("C1", 0, refContainer.RC_Code, "SEA");
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var uldPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			var packages = header.PackageJob.Packages;
			var containerPackage = packages.Single();
			AssertEquals(TransportUnitTypes.Container, header.WRH_UnitType);
			AssertEquals("C1", containerPackage.KP_PackageID);
			AssertEquals(containerPackage.PK, uldPackageState.WPS_KP_Package);
			Factory.SaveForTesting();

			containerDO.DeliveryMode = "AIR";
			var newFactory = new UniversalObjectFactory();
			var headerInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var uldPackageStateInNewFactory = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, headerInNewFactory.PackageExtension.KPN_KP_Package));
			AssertEquals(TransportUnitTypes.Container, headerInNewFactory.WRH_UnitType);
			AssertEquals(uldPackageState.PK, uldPackageStateInNewFactory.PK);
			AssertEquals(containerPackage.PK, uldPackageStateInNewFactory.WPS_KP_Package);
			AssertEquals(true, uldPackageStateInNewFactory.WPS_IsHandlingUnit);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, uldPackageStateInNewFactory.WPS_Status);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region TestPopulateBizO_InvalidContainerType

		public void TestPopulateBizO_InvalidContainerType()
		{
			var warehouse = Data.Warehouse;
			var containerDOWithInvalidContainerType = Data.CreateContainer("C1", 0, "XXX", "SEA");
			var consolDO = Data.HeaderDataObject;

			var readerForContainerWithID = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDOWithInvalidContainerType, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);
			AssertExceptionThrown<DataObjectReadFailureException>("Container does not have a supported Container Type.", () => readerForContainerWithID.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBizO_ContainerTypeUpdated

		public void TestPopulateBizO_ContainerTypeUpdated()
		{
			var warehouse = Data.Warehouse;

			var containerDO = Data.CreateContainer("C1", 0, "20GP", "SEA");
			var consolDO = Data.HeaderDataObject;

			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			Helper.CreateStmUniversalJobLink(header.PK.ToGuid(), "WRH", "GB1", "GateMovementBooking");

			var cntPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			var packages = header.PackageJob.Packages;
			var containerPackage = packages.Single();
			AssertEquals(TransportUnitTypes.Container, header.WRH_UnitType);
			AssertEquals("C1", containerPackage.KP_PackageID);
			AssertEquals(containerPackage.PK, cntPackageState.WPS_KP_Package);
			Factory.SaveForTesting();

			containerDO.ContainerType = new ContainerType { Code = "40GP" };
			var newFactory = new UniversalObjectFactory();
			var headerInNewFactory = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var uldPackageStateInNewFactory = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, headerInNewFactory.PackageExtension.KPN_KP_Package));

			var rtuEventLog = headerInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ContainerTypeUpdatedCode).SingleOrDefault();
			AssertNotNull("Should create Container Type updated log for rtu", rtuEventLog);
			AssertEquals("RTU Container Type updated event must be logged.", "|TYP=ContainerType|NEW=40GP|OLD=20GP", rtuEventLog.SL_Reference);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region Logs

		public void TestPopulateBizO_AddsLogForNewRTU()
		{
			var containerDO = Data.CreateContainer("CNT-1", 0);
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);
			reader.ReadIntoBusinessObject();

			AssertContains("Should add a log when creating an RTU.", "RTU matching failed - No RTU created within the last 30 days could be found with the provided Container Number CNT-1, Warehouse TWH, Master Bill MB1, and Consol Number C1000000.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewRTU_CreatedFromConsolWithoutReferences()
		{
			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject);
			Data.HeaderDataObject.WayBillNumber = null;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(Data.Warehouse, containerDO, Data.HeaderDataObject, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy);
			reader.ReadIntoBusinessObject();

			AssertContains("Should add a log when creating an RTU without a Consol Number or Master Bill.", @"RTU matching failed - A Master Bill or Consol Number was not provided for the Consolidation of Consignments.", Logger.Logs);
		}

		void AssertLogsForMatchedRTU(bool hasMasterBill, bool hasConsolNumber, string errorMessage, string expectedLog)
		{
			var warehouse = Data.Warehouse;
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "CNT-1");
			rtu.WRH_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);
			Factory.SaveForTesting();

			if (hasConsolNumber)
			{
				Helper.CreateAdditionalReference(rtu, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			}

			if (hasMasterBill)
			{
				Helper.CreateAdditionalReference(rtu, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			}

			Factory.SaveForTesting();

			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000");
			Data.HeaderDataObject.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			var newFactory = new UniversalObjectFactory();
			var reader = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, Data.HeaderDataObject, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy);
			var matchedRTU = reader.ReadIntoBusinessObject();

			AssertEquals("Precondition", rtu.PK, matchedRTU.PK);
			AssertContains(errorMessage, expectedLog, Logger.Logs);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedRTU() => AssertLogsForMatchedRTU(
			hasMasterBill: true,
			hasConsolNumber: true,
			errorMessage: "Should add a log when matching an RTU.",
			expectedLog: @"RTU 1 was matched - It was created within the last 30 days and has the provided Container Number CNT-1, Warehouse TWH, Master Bill MB1, and Consol Number C1000000.");

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedRTU_WithNoMasterBill() => AssertLogsForMatchedRTU(
			hasMasterBill: false,
			hasConsolNumber: true,
			errorMessage: "Should add a log when matching an RTU without a Master Bill.",
			expectedLog: @"RTU 1 was matched - It was created within the last 30 days and has the provided Container Number CNT-1, Warehouse TWH, and Consol Number C1000000.");

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedRTU_WithNoConsolNumber() => AssertLogsForMatchedRTU(
			hasMasterBill: true,
			hasConsolNumber: false,
			errorMessage: "Should add a log when matching an RTU without a Consol Number.",
			expectedLog: @"RTU 1 was matched - It was created within the last 30 days and has the provided Container Number CNT-1, Warehouse TWH, and Master Bill MB1.");

		#endregion

		#region Asserts

		void AssertMatchedRTU(IColumnIndexer warehouse, Container containerDO, Shipment consolDO, ZGuid expectedRTUPK, string errorMessage)
		{
			var newFactory = new UniversalObjectFactory();
			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			AssertEquals(errorMessage, expectedRTUPK, header.PK);
			newFactory.SaveForTesting();
		}

		void AssertNotMatchedRTU(IColumnIndexer warehouse, Container containerDO, Shipment consolDO, ZGuid expectedRTUPK, string errorMessage)
		{
			var newFactory = new UniversalObjectFactory();
			var header = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, newFactory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			AssertNotEquals(errorMessage, expectedRTUPK, header.PK);
			newFactory.SaveForTesting();
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			Data.SetupForForwardingImport();
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
