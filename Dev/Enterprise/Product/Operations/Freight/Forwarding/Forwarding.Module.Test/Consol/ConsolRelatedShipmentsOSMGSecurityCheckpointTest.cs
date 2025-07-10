using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class ConsolRelatedShipmentsOSMGSecurityCheckpointTest : TestCaseWithFactory
	{
		public void TestConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToTrue()
		{
			AssertConsolAllowAccessRegardlessOfShipmentsOSMGRights(true);
		}

		public void TestConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToFalse()
		{
			AssertConsolAllowAccessRegardlessOfShipmentsOSMGRights(false);
		}

		void AssertConsolAllowAccessRegardlessOfShipmentsOSMGRights(bool consolAllowAccessRegardlessOfShipmentsOSMGRights)
		{
			FreightDataRegistry.Instance.ConsolAllowAccessRegardlessOfShipmentsOSMGRights.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consolAllowAccessRegardlessOfShipmentsOSMGRights);

			var crmSecurityProvider = new JobShipmentCRMSecurityProvider();
			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			var testData = GetTestData(Factory, true);

			foreach (var consol in testData.AllConsols)
			{
				var checkpoint = new ConsolRelatedShipmentsOSMGSecurityCheckpoint(consol);
				Assert(!checkpoint.Visible);
				Assert(checkpoint.IsAllowed);
				AssertNull(checkpoint.ErrorMessageForNotAllowed);
			}

			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (consolAllowAccessRegardlessOfShipmentsOSMGRights)
			{
				foreach (var consol in testData.AllConsols)
				{
					var checkpoint = new ConsolRelatedShipmentsOSMGSecurityCheckpoint(consol);
					Assert(!checkpoint.Visible);
					Assert(checkpoint.IsAllowed);
					AssertNull(checkpoint.ErrorMessageForNotAllowed);
				}
			}
			else
			{
				foreach (var consol in testData.ConsolsWithAccessDenied)
				{
					var checkpoint = new ConsolRelatedShipmentsOSMGSecurityCheckpoint(consol);
					Assert(!checkpoint.Visible);
					Assert(!checkpoint.IsAllowed);
					AssertEquals("Should have shown an error", @"You do not have the appropriate security rights to run this function.

At least one of the shipments on this consolidation has its access restricted.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Search and View Records Assigned to Other Login Staff
Operate -> Forwarding -> Shipments -> Permit Unconditional access regardless of Task assignment
Operate -> Forwarding -> Shipments -> Permit Unconditional access regardless of Org. Security Groups", checkpoint.ErrorMessageForNotAllowed);
				}
				foreach (var consol in testData.ConsolsWithAccessGranted)
				{
					var checkpoint = new ConsolRelatedShipmentsOSMGSecurityCheckpoint(consol);
					Assert(!checkpoint.Visible);
					Assert(checkpoint.IsAllowed);
					AssertNull(checkpoint.ErrorMessageForNotAllowed);
				}
			}
		}

		internal static (ForwardingConsol[] AllConsols, ForwardingConsol[] ConsolsWithAccessDenied, ForwardingConsol[] ConsolsWithAccessGranted) GetTestData(BusinessObjectFactory factory, bool includeConsolTemplate = false)
		{
			var consignee_NoOSMG = JobShipmentCRMSecurityProviderTest.GetOrgForOSMGSecurityLevel(factory, hasOSMG: false);
			var consignee_OSMG_UserNotInGroup = JobShipmentCRMSecurityProviderTest.GetOrgForOSMGSecurityLevel(factory, hasOSMG: true, isUserInGroup: false);
			var consignee_OSMG_UserInGroup = JobShipmentCRMSecurityProviderTest.GetOrgForOSMGSecurityLevel(factory, hasOSMG: true, isUserInGroup: true);
			var consignee_NoOSMG_StaffAssignment = JobShipmentCRMSecurityProviderTest.GetOrgForOSMGSecurityLevel(factory, hasOSMG: false);
			var staffAssignment = consignee_NoOSMG_StaffAssignment.StaffAssignments.AddNew();
			staffAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			staffAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;

			var consol1 = factory.New<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.ConsigneePK = consignee_NoOSMG.PK;
			var shipment2 = consol1.Shipments.AddNew();
			shipment2.ConsigneePK = consignee_OSMG_UserNotInGroup.PK;

			var consol2 = factory.New<ForwardingConsol>();
			var shipment3 = consol2.Shipments.AddNew();
			shipment3.ConsigneePK = consignee_NoOSMG.PK;
			var shipment4 = consol2.Shipments.AddNew();
			shipment4.ConsigneePK = consignee_OSMG_UserInGroup.PK;

			var consol3 = factory.New<ForwardingConsol>();
			var shipment5 = consol3.Shipments.AddNew();
			shipment5.ConsigneePK = consignee_OSMG_UserInGroup.PK;
			var shipment6 = consol3.Shipments.AddNew();
			shipment6.ConsigneePK = consignee_OSMG_UserInGroup.PK;

			var consol4 = factory.New<ForwardingConsol>();
			var shipment7 = consol4.Shipments.AddNew();
			shipment7.ConsigneePK = consignee_OSMG_UserInGroup.PK;
			var shipment8 = consol4.Shipments.AddNew();
			shipment8.ConsigneePK = consignee_NoOSMG_StaffAssignment.PK;

			var consol5 = factory.New<ForwardingConsol>();

			factory.Save();

			if (includeConsolTemplate)
			{
				var templateRecordConsolFactory = new TemplateRecordBusinessObjectFactory { RefreshEnabled = false };
				var consol6 = templateRecordConsolFactory.New<ForwardingConsol>();

				var templateRecord = templateRecordConsolFactory.TemplateRecordFactory.New<StmTemplateRecord>();
				templateRecord.STR_ModuleID = "JobConsol";

				templateRecordConsolFactory.TemplateRecordProvider = consol6;
				templateRecordConsolFactory.TemplateRecordProvider.IsTemplateRecord = true;
				templateRecordConsolFactory.TemplateRecordProvider.TemplateRecord = templateRecord;

				consol6.JK_MasterBillNum = "ABC";
				templateRecordConsolFactory.Save();

				return (
					AllConsols: new[] { consol1, consol2, consol3, consol4, consol5, consol6 },
					ConsolsWithAccessDenied: new[] { consol1, consol2 },
					ConsolsWithAccessGranted: new[] { consol3, consol4, consol5, consol6 }
				);
			}
			else
			{
				return (
					AllConsols: new[] { consol1, consol2, consol3, consol4, consol5 },
					ConsolsWithAccessDenied: new[] { consol1, consol2 },
					ConsolsWithAccessGranted: new[] { consol3, consol4, consol5 }
				);
			}
		}
	}
}
