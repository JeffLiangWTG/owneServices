using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobShipmentCRMSecurityProviderTest : CRMSecurityProviderTest<ForwardingShipment>
	{
		protected override CRMSecurityProvider<ForwardingShipment> GetNewProviderForTest() => new JobShipmentCRMSecurityProvider();

		public void TestGetOSMGQueryEnhancedSecurityLevel()
		{
			var provider = GetNewProviderForTest();
			provider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			provider.OSMGSecurityLevelRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.OSMGSecurityLevels.Enhanced);

			var filters = new ModuleFilterCollection();
			ProviderForTest.AddCRMSecurityFilterStrips(Factory, filters);

			var expectedQueryAdo = $"(SELECT HasAccess FROM HasEnhancedAccessOSMGShipment(JS_PK, CONVERT('{GlbCompany.CurrentCompany.PK}', 'System.Guid'), CONVERT('{GlbStaff.CurrentUser.PK}', 'System.Guid'))) = 1";
			var query = filters.GetFilterQuery(filters);

			AssertContains(expectedQueryAdo, query.LiteralTextADO);
		}

		protected override IEnumerable<ForwardingShipment> GetTestObjectWithoutStaffAssignment()
		{
			var obj = Factory.NewWithValidTestData<ForwardingShipment>();
			return new ForwardingShipment[] { obj };
		}

		protected override IEnumerable<ForwardingShipment> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobHeader1.JH_ParentID = forwardingShipment.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new ForwardingShipment[] { forwardingShipment };
		}

		protected override IEnumerable<ForwardingShipment> GetTestObjectWithBizObjStaffAssignment()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobHeader1.JH_ParentID = forwardingShipment.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new ForwardingShipment[] { forwardingShipment };
		}

		protected override IEnumerable<ForwardingShipment> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = "U01";
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobHeader1.JH_ParentID = forwardingShipment.PK;
			jobHeader1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new ForwardingShipment[] { forwardingShipment };
		}

		protected override void AddStaffAssignmentForCompany(ForwardingShipment obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var job = obj.Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, obj.PK));
			var assignment = job.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}

		#region Security Level Tests

		protected override IEnumerable<(ForwardingShipment testObject, bool isVisibleOnStandardLevel, bool isVisibleOnEnhancedLevel, string assertionMessage)> GetTestDataForOrgSecurityGroupsSecurityLevel()
		{
			var consignee_NoOSMG = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: false);
			var consignee_OSMG_UserNotInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: false);
			var consignee_OSMG_UserInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: true);
			var consignee_NoOSMG_StaffAssignment = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: false);
			var staffAssignment = consignee_NoOSMG_StaffAssignment.StaffAssignments.AddNew();
			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var consignor_NoOSMG = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: false);
			var consignor_OSMG_UserNotInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: false);
			var consignor_OSMG_UserInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: true);

			var controllingCustomer_NoOSMG = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: false);
			var controllingCustomer_OSMG_UserNotInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: false);
			var controllingCustomer_OSMG_UserInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: true);

			var localCharges_NoOSMG = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: false);
			var localCharges_OSMG_UserNotInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: false);
			var localCharges_OSMG_UserInGroup = GetOrgForOSMGSecurityLevel(Factory, hasOSMG: true, isUserInGroup: true);

			var shipment1 = GetShipment(Factory, consignee_NoOSMG, consignor_NoOSMG, controllingCustomer_NoOSMG, localCharges_NoOSMG);
			yield return (shipment1, false, false, "Shipment1: no OSMG groups set up => shipment should not be visible");

			var shipment2 = GetShipment(Factory, consignee_OSMG_UserNotInGroup, consignor_OSMG_UserNotInGroup, controllingCustomer_OSMG_UserNotInGroup, localCharges_OSMG_UserNotInGroup);
			yield return (shipment2, false, false, "Shipment2: with OSMG groups set up, but user is not in the groups => shipment should not be visible");

			var shipment3 = GetShipment(Factory, consignee_OSMG_UserInGroup, consignor_OSMG_UserInGroup, controllingCustomer_OSMG_UserNotInGroup, localCharges_OSMG_UserNotInGroup);
			yield return (shipment3, true, false, "Shipment3: with OSMG groups set up and user is in some of these groups => shipment should be visible with Standard Security Level but not with Enhanced Security Level");

			var shipment4 = GetShipment(Factory, consignee_OSMG_UserInGroup, consignor_OSMG_UserInGroup, controllingCustomer_OSMG_UserInGroup, localCharges_OSMG_UserInGroup);
			yield return (shipment4, true, true, "Shipment4: with OSMG groups set up and user is in all of these groups => shipment should be visible on Standard and Enhanced Security Levels");

			var shipment5 = GetShipment(Factory, consignee_OSMG_UserInGroup, consignor_OSMG_UserInGroup, controllingCustomer_OSMG_UserInGroup, null);
			yield return (shipment5, true, true, "Shipment5: with OSMG groups set up and user is in all of these groups => shipment should be visible on Standard and Enhanced Security Levels");

			var shipment6 = GetShipment(Factory, consignee_NoOSMG_StaffAssignment, consignor_OSMG_UserInGroup, controllingCustomer_OSMG_UserInGroup, localCharges_OSMG_UserInGroup);
			yield return (shipment6, true, false, "Shipment6: with OSMG groups set up and user is in all of these groups except for one org where the user has a staff assignment instead => shipment should be visible with Standard Security Level but not with Enhanced Security Level");

			var shipment7 = GetShipment(Factory, consignee_NoOSMG, consignor_OSMG_UserInGroup, controllingCustomer_OSMG_UserInGroup, localCharges_OSMG_UserInGroup);
			var workflowItem = shipment7.WorkflowItems.AddNew();
			workflowItem.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			yield return (shipment7, true, false, "Shipment7: with OSMG groups set up and user is in all of these groups except for one org where the user has a task staff assignment instead => shipment should be visible with Standard Security Level but not with Enhanced Security Level");
		}

		internal static OrgHeader GetOrgForOSMGSecurityLevel(BusinessObjectFactory factory, bool hasOSMG, bool isUserInGroup = false)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			if (hasOSMG)
			{
				var osmg = factory.NewWithValidTestData<GlbGroup>();
				osmg.Organisation.Add(org);
				if (isUserInGroup)
				{
					osmg.Staff.Add(GlbStaff.CurrentUser);
				}
			}
			return org;
		}

		internal static ForwardingShipment GetShipment(BusinessObjectFactory factory, OrgHeader consignee, OrgHeader consignor, OrgHeader controllingCustomer, OrgHeader localCharges)
		{
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			if (localCharges != null)
			{
				var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_GS_NKRepSales = "U01";
				jobHeader.JH_OA_LocalChargesAddr = localCharges.Addresses[0].PK;
				jobHeader.JH_ParentID = shipment.PK;
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			return shipment;
		}

		#endregion
	}
}
