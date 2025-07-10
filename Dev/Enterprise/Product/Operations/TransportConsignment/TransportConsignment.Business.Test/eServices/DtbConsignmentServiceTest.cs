using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentServiceTest : TestCaseWithFactory
	{
		#region GetDefaultValuesForConsignment

		public void TestGetDefaultValuesForConsignment_WhenBothAddressesNotSpecified()
		{
			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(ZGuid.Empty, ZGuid.Empty, Constants.ContainerModes.FTL);

			AssertResult(string.Empty, string.Empty, result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupIsFound_ButNotDelivery()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, ZGuid.Empty, Constants.ContainerModes.FTL);

			AssertResult("IN1", "SL1", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupIsFound_ButNotDelivery_AndValuesMissing()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, string.Empty, string.Empty);
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, ZGuid.Empty, Constants.ContainerModes.FTL);

			AssertResult(string.Empty, string.Empty, result);
		}

		public void TestGetDefaultValuesForConsignment_WhenDeliveryIsFound_ButNotPickup()
		{
			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN1", "SL1");
			SetOrgAsConsignee(deliveryOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(ZGuid.Empty, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("IN2", "SL2", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenDeliveryIsFound_ButNotPickup_AndValuesMissing()
		{
			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN1", "SL1");
			SetOrgAsConsignee(deliveryOrg, string.Empty, string.Empty);

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(ZGuid.Empty, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult(string.Empty, string.Empty, result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_ButNoRelationshipsDefined()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("IN4", "SL4", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_ButNoRelationshipsDefined_AndDeliveryValuesMissing()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, string.Empty, string.Empty);
			SetOrgAsConsignee(deliveryOrg, string.Empty, string.Empty);

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("IN1", "SL1", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_AndTransportModeIsRoad_AndContainerModeMatchesJobType()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.CountryCodes.Australia, "IN5", "SL5");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FTL, Constants.CountryCodes.India, "IN6", "SL6");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Air, Constants.ContainerModes.AIR, Constants.CountryCodes.Canada, "IN7", "SL7");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("IN6", "SL6", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_AndTransportModeIsRoad_AndContainerModeDoesNotMatchJobType()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.CountryCodes.Australia, "IN5", "SL5");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("No relationship with a matching container mode was found, so we should fall back to the delivery org's values.", "IN4", "SL4", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_AndTransportModeIsRoad_AndContainerModeIsBlank()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.CountryCodes.Australia, "IN5", "SL5");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, string.Empty, Constants.CountryCodes.India, "IN6", "SL6");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FTL, Constants.CountryCodes.Canada, "IN7", "SL7");

			Factory.Save();

			var service = new DtbConsignmentService();

			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);
			AssertResult("The relationship with a matching job type/container mode should be selected over the one with the blank container mode.", "IN7", "SL7", result);

			result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.LTL);
			AssertResult("There is no relationship with a matching job type/container mode, but we do have one with a blank container mode, so we can fall back to that one.", "IN6", "SL6", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_AndTransportModeIsAll()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.CountryCodes.Australia, "IN5", "SL5");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.All, Constants.ContainerModes.LTL, Constants.CountryCodes.India, "IN6", "SL6");

			Factory.Save();

			var service = new DtbConsignmentService();

			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);
			AssertResult("There is no Road relationship with a matching job type/container mode, but we do have one for ALL transport modes, so we can fall back to that one.", "IN6", "SL6", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryFound_AndTransportModeIsNotRoadOrAll()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Air, Constants.ContainerModes.FTL, Constants.CountryCodes.Australia, "IN5", "SL5");

			Factory.Save();

			var service = new DtbConsignmentService();

			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);
			AssertResult("There is no relationship with Road or All transport mode, so we must fall back to the delivery org values.", "IN4", "SL4", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryOrgsAreConsignorAndConsigneeAppropriately()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignee(deliveryOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("IN2", "SL2", result);
		}

		public void TestGetDefaultValuesForConsignment_WhenPickupAndDeliveryOrgsAreNotConsignorAndConsigneeAppropriately()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();
			var result = service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);

			AssertResult("The pickup org is not a consinor and the delivery org is not a consignee, so no results should be found.", string.Empty, string.Empty, result);
		}

		public void TestGetDefaultValuesForConsignment_WhenBothAddressesSpecified_DbHits()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			var deliveryOrg = CreateDeliveryOrganization();
			SetOrgAsConsignor(deliveryOrg, "IN3", "SL3");
			SetOrgAsConsignee(deliveryOrg, "IN4", "SL4");

			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.Road, Constants.ContainerModes.FCL, Constants.CountryCodes.Australia, "IN5", "SL5");
			CreateRelationship(pickupOrg, deliveryOrg, Constants.TransportModes.All, Constants.ContainerModes.LTL, Constants.CountryCodes.India, "IN6", "SL6");

			Factory.Save();

			var service = new DtbConsignmentService();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupBuyLinkTrnModeSchema.Constants.TableName, 2 }, // allowed 2 so we can use business object collections since the logic is quite complex
				{ OrgSupplierBuyerLinkSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories("It would be easy to end up with two hits per table since we are looking at two organizations, but let's do it with just one per table.", expectedHits))
			{
				service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, deliveryOrg.MainAddress.PK, Constants.ContainerModes.FTL);
			}
		}

		public void TestGetDefaultValuesForConsignment_WhenOneAddressNotSpecified_DbHits()
		{
			var pickupOrg = CreatePickupOrganization();
			SetOrgAsConsignor(pickupOrg, "IN1", "SL1");
			SetOrgAsConsignee(pickupOrg, "IN2", "SL2");

			Factory.Save();

			var service = new DtbConsignmentService();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupBuyLinkTrnModeSchema.Constants.TableName, 0 },
				{ OrgSupplierBuyerLinkSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories("It would be easy to end up with two hits per table since we are looking at two organizations, but let's do it with just one per table.", expectedHits))
			{
				service.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK, ZGuid.Empty, Constants.ContainerModes.FTL);
			}
		}

		public void TestGetDefaultValuesForConsignment_WhenNoAddressesSpecified_DbHits()
		{
			var service = new DtbConsignmentService();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 0 },
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgSupBuyLinkTrnModeSchema.Constants.TableName, 0 },
				{ OrgSupplierBuyerLinkSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsForAllFactories("No addresses were specified, so we shouldn't do any queries.", expectedHits))
			{
				service.GetDefaultValuesForConsignment(ZGuid.Empty, ZGuid.Empty, Constants.ContainerModes.FTL);
			}
		}

		#endregion

		#region Implementation

		static void AssertResult(string message, string expectedIncoterm, string expectedServiceLevel, Dictionary<string, string> result)
		{
			var actualIncoterm = result["INC"];
			AssertEquals("Expected incoterm", expectedIncoterm, actualIncoterm);

			var actualServiceLevel = result["SRV"];
			AssertEquals("Expected service level", expectedServiceLevel, actualServiceLevel);

			AssertEquals(message, 2, result.Count);
		}

		static void AssertResult(string expectedIncoterm, string expectedServiceLevel, Dictionary<string, string> result)
		{
			AssertResult(null, expectedIncoterm, expectedServiceLevel, result);
		}

		OrgHeader CreatePickupOrganization()
		{
			var pickupOrg = Factory.New<OrgHeader>();
			pickupOrg.OH_Code = "PICORG";

			return pickupOrg;
		}

		OrgHeader CreateDeliveryOrganization()
		{
			var pickupOrg = Factory.New<OrgHeader>();
			pickupOrg.OH_Code = "DLVORG";

			return pickupOrg;
		}

		static void SetOrgAsConsignor(OrgHeader org, string orgIncoterm, string orgServiceLevel)
		{
			org.OH_IsConsignor = true;
			org.MiscServ.OM_EXDefaultIncoTerm = orgIncoterm;
			org.MiscServ.OM_RS_NKEXDefaultServiceLevel = orgServiceLevel;
		}

		static void SetOrgAsConsignee(OrgHeader org, string orgIncoterm, string orgServiceLevel)
		{
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMDefaultINCOTerm = orgIncoterm;
			org.MiscServ.OM_RS_NKIMDefaultServiceLevel = orgServiceLevel;
		}

		static void CreateRelationship(OrgHeader pickupOrg, OrgHeader deliveryOrg, string transportMode, string containerMode, string countryCode, string incoterm, string serviceLevel)
		{
			var link = pickupOrg.Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = pickupOrg.PK;
			link.OL_OH_Buyer = deliveryOrg.PK;
			link.OL_RN_NKImporterCountry = countryCode;

			var mode = link.OrgSupBuyLinkTrnModes.AddNew();
			mode.PF_TransportMode = transportMode;
			mode.PF_ContainerMode = containerMode;
			mode.PF_IncoTerm = incoterm;
			mode.PF_RS_NKDefaultServiceLevel = serviceLevel;
		}

		#endregion
	}
}
