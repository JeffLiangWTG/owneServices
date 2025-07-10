using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterData.Business.Tests
{
	public class RelatedOrgsAndRolesHelperTest : TestCaseWithFactory
	{
		public void TestGetRelatedOrgPartyScreeningStatuses()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress1 = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress);
			var jobDocAddress2 = CreateJobDocAddress(shipments.PK, ZGuid.Empty, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress);

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_OH = orgHeader3.PK;

			var screenStatus1 = CreateScreenStatus(orgHeader1.PK);
			var screenStatus2 = CreateScreenStatus(orgHeader1.PK);
			var screenStatus3 = CreateScreenStatus(orgHeader2.PK);
			var screenStatus4 = CreateScreenStatus(orgHeader3.PK);
			var screenStatus5 = CreateScreenStatus(shipments.PK, JobShipmentSchema.Constants.Prefix);

			Factory.Save();

			var vessel1ScreeningParties = ((IScreeningPartyProvider)vessel1).ScreeningParties;
			var vessel2ScreeningParties = ((IScreeningPartyProvider)vessel2).ScreeningParties;

			CombineAssertions("Mock Screening Parties for Vessel", () =>
			{
				AssertEquals(2, vessel1ScreeningParties.Length);
				AssertNotNull(vessel1ScreeningParties.SingleOrDefault(u => u.Header != null));
				AssertNotNull(vessel1ScreeningParties.SingleOrDefault(u => u.Header == null));

				AssertEquals(1, vessel2ScreeningParties.Length);
				AssertNotNull(vessel2ScreeningParties.SingleOrDefault(u => u.Header == null));
			});

			var screeningParties = new[]
			{
				new ScreeningParty(shipments, "Consignor Documentary Address", jobDocAddress1),
				new ScreeningParty(shipments, "BBB", jobDocAddress2),

				new ScreeningParty(shipments, "CCC", orgHeader1),
				new ScreeningParty(shipments, "DDD", orgHeader1),

				new ScreeningParty(shipments, "EEE", orgHeader2),

				new ScreeningParty(shipments, "FFF", orgHeader4),

				vessel1ScreeningParties[0],
				vessel1ScreeningParties[1],
				vessel2ScreeningParties[0]
			};

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties, relatedJobPKs: new[] { shipments.PK }).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(6, collection.Length);
				AssertEquals($"{orgHeader1.OH_Code}({jobDocAddress1.AddressDescription}|CCC|DDD)", collection.Single(u => u.PJ_ParentID == orgHeader1.PK && u.PK == screenStatus1.PK).RelatedOrganization);
				AssertEquals($"{orgHeader1.OH_Code}({jobDocAddress1.AddressDescription}|CCC|DDD)", collection.Single(u => u.PJ_ParentID == orgHeader1.PK && u.PK == screenStatus2.PK).RelatedOrganization);
				AssertEquals($"{orgHeader2.OH_Code}(EEE)", collection.Single(u => u.PJ_ParentID == orgHeader2.PK && u.PK == screenStatus3.PK).RelatedOrganization);
				AssertEquals($"{orgHeader3.OH_Code}(Shipping Provider)", collection.Single(u => u.PJ_ParentID == orgHeader3.PK && u.PK == screenStatus4.PK).RelatedOrganization);
				AssertEquals(true, collection.Any(u => u.PJ_ParentID == shipments.PK && u.PK == screenStatus5.PK));
			});
		}

		public void TestGetRelatedOrgPartyScreeningStatusesForJobDocAddress()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignorAddress = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress);

			var consigneeAddress = CreateJobDocAddress(shipments.PK, orgHeader2.MainAddress.PK, AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var notifyParty1Address = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.NotifyParty);
			var notifyParty2Address = CreateJobDocAddress(shipments.PK, orgHeader2.MainAddress.PK, AutoDocAddressTypes.Codes.NotifyParty2);
			var notifyParty3Address = CreateJobDocAddress(shipments.PK, orgHeader3.MainAddress.PK, AutoDocAddressTypes.Codes.NotifyParty3);

			var otherAddress1 = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.AdditionalDeliveryAddress);
			var otherAddress2 = CreateJobDocAddress(shipments.PK, orgHeader2.MainAddress.PK, AutoDocAddressTypes.Codes.AssuredPartyDocumentaryAddress);
			var otherAddress3 = CreateJobDocAddress(shipments.PK, ZGuid.Empty, AutoDocAddressTypes.Codes.CarrierBookingAgent);

			var screenStatus1 = CreateScreenStatus(orgHeader1.PK);
			var screenStatus2 = CreateScreenStatus(orgHeader2.PK);
			var screenStatus3 = CreateScreenStatus(orgHeader3.PK);

			Factory.Save();

			var screeningParties = new[]
			{
				new ScreeningParty(shipments, "1", consignorAddress),
				new ScreeningParty(shipments, "2", consigneeAddress),

				new ScreeningParty(shipments, "3", notifyParty1Address),
				new ScreeningParty(shipments, "4", notifyParty2Address),
				new ScreeningParty(shipments, "5", notifyParty3Address),

				new ScreeningParty(shipments, "6", otherAddress1),
				new ScreeningParty(shipments, "7", otherAddress2),
				new ScreeningParty(shipments, "8", otherAddress3),
			};

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(3, collection.Length);
				AssertEquals($"{orgHeader1.OH_Code}({consignorAddress.AddressDescription}|{notifyParty1Address.AddressDescription}|{otherAddress1.AddressDescription})", collection.Single(u => u.PJ_ParentID == orgHeader1.PK && u.PK == screenStatus1.PK).RelatedOrganization);
				AssertEquals($"{orgHeader2.OH_Code}({consigneeAddress.AddressDescription}|{notifyParty2Address.AddressDescription}|{otherAddress2.AddressDescription})", collection.Single(u => u.PJ_ParentID == orgHeader2.PK && u.PK == screenStatus2.PK).RelatedOrganization);
				AssertEquals($"{orgHeader3.OH_Code}({notifyParty3Address.AddressDescription})", collection.Single(u => u.PJ_ParentID == orgHeader3.PK && u.PK == screenStatus3.PK).RelatedOrganization);
			});
		}

		public void TestGetEmptyRelatedOrgPartyScreeningStatuses()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			CombineAssertions(() =>
			{
				AssertEquals(false, RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, null).Any());
				AssertEquals(false, RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, new[] { new ScreeningParty(shipments, "AAA", orgHeader) }).Any());
			});
		}

		public void TestNotAddMultiDesc()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var otherAddress1 = CreateJobDocAddress(shipments.PK, orgHeader.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			var otherAddress2 = CreateJobDocAddress(shipments.PK, orgHeader.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			var screenStatus1 = CreateScreenStatus(orgHeader.PK);

			var screeningParties = new[]
			{
				new ScreeningParty(shipments, "1", otherAddress1),
				new ScreeningParty(shipments, "2", otherAddress2)
			};

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Length);
				AssertEquals($"{orgHeader.OH_Code}({otherAddress1.AddressDescription})", collection.Single(u => u.PJ_ParentID == orgHeader.PK && u.PK == screenStatus1.PK).RelatedOrganization);
			});
		}

		public void TestGetRelatedOrgPartyScreeningStatusesForOverrideJobDocAddress()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			var overrideAddress1 = CreateJobDocAddress(shipments.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCTOAddress, true);
			var overrideAddress2 = CreateJobDocAddress(shipments.PK, orgHeader2.MainAddress.PK, AutoDocAddressTypes.Codes.NotifyParty3, true);

			var screenStatus1 = CreateScreenStatus(orgHeader1.PK);
			var screenStatus2 = CreateScreenStatus(orgHeader2.PK);
			var screenStatus3 = CreateScreenStatus(overrideAddress1.PK, JobDocAddressSchema.Constants.Prefix);
			var screenStatus4 = CreateScreenStatus(overrideAddress2.PK, JobDocAddressSchema.Constants.Prefix);

			Factory.Save();

			var screeningParties = new[]
			{
				new ScreeningParty(shipments, "1", jobDocAddress),
				new ScreeningParty(shipments, "2", orgHeader2),
				new ScreeningParty(shipments, "3", overrideAddress1),
				new ScreeningParty(shipments, "4", overrideAddress2),
				new ScreeningParty(shipments, "5", overrideAddress2) // Mock Same Override Address
			};

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(4, collection.Length);
				AssertEquals($"{orgHeader1.OH_Code}({jobDocAddress.AddressDescription})", collection.Single(u => u.PJ_ParentID == orgHeader1.PK && u.PK == screenStatus1.PK).RelatedOrganization);
				AssertEquals($"{orgHeader2.OH_Code}(2)", collection.Single(u => u.PJ_ParentID == orgHeader2.PK && u.PK == screenStatus2.PK).RelatedOrganization);
				AssertEquals($"Override Address({overrideAddress1.AddressDescription})", collection.Single(u => u.PJ_ParentID == overrideAddress1.PK && u.PK == screenStatus3.PK).RelatedOrganization);
				AssertEquals($"Override Address({overrideAddress2.AddressDescription})", collection.Single(u => u.PJ_ParentID == overrideAddress2.PK && u.PK == screenStatus4.PK).RelatedOrganization);
			});
		}

		public void TestNotProcessEmptyJobDocAddress()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();
			var emptyJobDocAddress = CreateJobDocAddress(shipments.PK, ZGuid.Empty, AutoDocAddressTypes.Codes.ArrivalCTOAddress);
			var screenStatus = CreateScreenStatus(emptyJobDocAddress.PK, JobDocAddressSchema.Constants.Prefix);
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Precondition", !emptyJobDocAddress.IsInDatabase);
				Assert("Precondition", emptyJobDocAddress.IsEmpty);
				Assert("Precondition", screenStatus.IsInDatabase);
				AssertEquals("Precondition", emptyJobDocAddress.PK, screenStatus.PJ_ParentID);
			});

			var screeningParties = new[]
			{
				new ScreeningParty(shipments, "1", emptyJobDocAddress)
			};

			Assert("Key should be empty", screeningParties[0].Key.IsEmpty);

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			AssertEquals("Should not process empty job doc address", 0, collection.Length);
		}

		public void TestGetRelatedOrgPartyScreeningStatusesForVessel()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_OH = orgHeader.PK;
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			var screenStatus1 = CreateScreenStatus(vessel1.PK, RefVesselSchema.Constants.Prefix);
			var screenStatus2 = CreateScreenStatus(vessel2.PK, RefVesselSchema.Constants.Prefix);
			Factory.Save();

			var vessel1ScreeningParties = ((IScreeningPartyProvider)vessel1).ScreeningParties;
			var vessel2ScreeningParties = ((IScreeningPartyProvider)vessel2).ScreeningParties;

			var screeningParties = ((IScreeningPartyProvider)vessel1).ScreeningParties.Concat(((IScreeningPartyProvider)vessel2).ScreeningParties).ToArray();

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(2, collection.Length);
				AssertEquals($"{vessel1.RV_Code}(Vessel)", collection.Single(u => u.PJ_ParentID == vessel1.PK && u.PK == screenStatus1.PK).RelatedOrganization);
				AssertEquals($"{vessel2.RV_Code}(Vessel)", collection.Single(u => u.PJ_ParentID == vessel2.PK && u.PK == screenStatus2.PK).RelatedOrganization);
			});
		}

		public void TestGetRelatedOrgPartyScreeningStatusesForNotLinkedVessel()
		{
			var shipments = (BusinessObject)Factory.New<IForwardingShipment>();
			var notLinkedVessel = Factory.New<ITransport>();
			notLinkedVessel.ParentType = shipments.GetType();
			var screenStatus = CreateScreenStatus((notLinkedVessel as BusinessObject).PK, (notLinkedVessel as BusinessObject).TablePrefix);
			Factory.Save();

			var screeningParties = ((IScreeningPartyProvider)notLinkedVessel).ScreeningParties;

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Length);
				AssertEquals($"{notLinkedVessel.JW_Vessel}(Vessel)", collection.Single(u => u.PJ_ParentID == (notLinkedVessel as BusinessObject).PK && u.PK == screenStatus.PK).RelatedOrganization);
			});
		}

		public void TestGetRelatedOrgPartyScreeningStatusesWhenParentIsDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "CN";

			var declaration = Factory.New<IBaseJobDeclaration>() as BusinessObject;
			(declaration as IBaseJobDeclaration).JE_GB = branch.PK;
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress = CreateJobDocAddress(declaration.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCFSAddress);
			var overrideAddress = CreateJobDocAddress(declaration.PK, orgHeader1.MainAddress.PK, AutoDocAddressTypes.Codes.ArrivalCTOAddress, true);

			var screenStatus1 = CreateScreenStatus(orgHeader1.PK);
			var screenStatus2 = CreateScreenStatus(orgHeader2.PK);
			var screenStatus3 = CreateScreenStatus(overrideAddress.PK, JobDocAddressSchema.Constants.Prefix);
			Factory.Save();

			var screeningParties = new[]
			{
				new ScreeningParty(declaration, "Description 1", jobDocAddress),
				new ScreeningParty(declaration, "Description 2", orgHeader2),
				new ScreeningParty(declaration, "Description 3", overrideAddress),
				new ScreeningParty(declaration, "Description 4", orgHeader1),
			};

			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(3, collection.Length);
				AssertEquals($"{orgHeader2.OH_Code}(CN - Description 2)", collection.Single(u => u.PJ_ParentID == orgHeader2.PK && u.PK == screenStatus2.PK).RelatedOrganization);
				AssertEquals($"{orgHeader1.OH_Code}(CN - {jobDocAddress.AddressDescription}|CN - Description 4)", collection.Single(u => u.PJ_ParentID == orgHeader1.PK && u.PK == screenStatus1.PK).RelatedOrganization);
				AssertEquals($"Override Address(CN - {overrideAddress.AddressDescription})", collection.Single(u => u.PJ_ParentID == overrideAddress.PK && u.PK == screenStatus3.PK).RelatedOrganization);
			});
		}

		public void TestGetRelatedOrgPartyScreeningStatusCollectionExcludeComplianceRiskSnapshots()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var plugInBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			plugInBizO.RefreshData();
			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(plugInBizO, (string.Empty, string.Empty, string.Empty));

			var notLinkedVessel = Factory.New<ITransport>();
			notLinkedVessel.ParentType = shipment.GetType();
			notLinkedVessel.JW_Vessel = "Vessel1";
			var screenStatus = CreateScreenStatus((notLinkedVessel as BusinessObject).PK, (notLinkedVessel as BusinessObject).TablePrefix);

			Factory.Save();

			var screeningParties = new[] { new ScreeningParty(shipment, "123", notLinkedVessel as IScreeningPartyForVessel) };
			var collection = RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, screeningParties, shipment.PK).ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Length);
				AssertEquals($"Vessel1(123)", collection.Single(u => u.PJ_ParentID == (notLinkedVessel as BusinessObject).PK && u.PK == screenStatus.PK).RelatedOrganization);
				Assert("Should exclude Compliance Risk Snapshot (CRS) logs", collection.All(u => u.PJ_Status != DeniedPartyConstants.LogsScreeningStatus.ComplianceRiskSnapshot));
			});
		}

		#region Implementation

		JobDocAddress CreateJobDocAddress(ZGuid parentPK, ZGuid addressPK, ZString addressType, bool overrideAddress = false)
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = addressPK;
			jobDocAddress.E2_ParentID = parentPK;
			jobDocAddress.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobDocAddress.E2_AddressType = addressType;
			jobDocAddress.E2_AddressOverride = overrideAddress;

			return jobDocAddress;
		}

		StmEntityScreeningLog CreateScreenStatus(ZGuid parentID, string tablePrefix = null)
		{
			var status = Factory.New<StmEntityScreeningLog>();
			status.PJ_ParentID = parentID;
			status.PJ_ParentTableCode = tablePrefix ?? OrgHeaderSchema.Constants.Prefix;

			return status;
		}

		#endregion
	}
}
