using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(BillOfLadingInvoicingSupporter))]
	internal class BillOfLadingInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestInvoicingSecurityCheckPoints()
		{
			IJobInvoicingPlugIn invoicing = Factory.New<BillOfLading>();
			IJobInvoicingSupporter supporter = invoicing.InvoicingSupporter;
			AssertEquals("AuditSecurity", Env.Security.AgencyBillOfLadingAuditBilling, supporter.AuditSecurity);
			AssertEquals("JobInvoicing", Env.Security.AgencyBillOfLadingJobInvoicing, supporter.JobInvoicingSecurity);
			AssertEquals("EditSecurityCheckpoint", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays), supporter.EditSecurityCheckpoint);
		}

		public void TestInvoicingEditSecurityObjLock()
		{
			const string HomePort = "AUBNE";
			const string AlternateHomePort = "AUSYD";
			const string OverseasPort = "SGSIN";
			BillOfLading shipment = Factory.New<BillOfLading>();
			IJobInvoicingPlugIn testJob = shipment;
			ZDateTime today = ZDateTime.Today;
			SecurityCheckpoint checkPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays);
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_E_DEP = today.AddDays(-25);
			origin1.JA_A_DEP = today.AddDays(-24);
			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = AlternateHomePort;
			destination1.JB_E_ARV = today.AddDays(-20);
			destination1.JB_A_ARV = today.AddDays(-19);
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			origin2.JA_E_DEP = today.AddDays(-18);
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = OverseasPort;
			destination2.JB_E_ARV = today.AddDays(-14);
			destination2.JB_A_ARV = today.AddDays(-12);
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge).PK;
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5m);
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			checkPoint.IsAllowed = false;
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30m);
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			checkPoint.IsAllowed = true;
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			voyage.Sailings.RemoveAndDeleteAll();
			origin2.JA_A_DEP = today.AddDays(-15);
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge(origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge).PK;
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5m);
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
			checkPoint.IsAllowed = false;
			AssertEquals(ZBool.True, testJob.InvoicingSupporter.EditSecurityLock);
			AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 30m);
			AssertEquals(ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestInvoicingEditSecurityMessage()
		{
			IJobInvoicingPlugIn testJob = Factory.New<AgencyShipment>();
			if (testJob.InvoicingSupporter.EditSecurityLock)
			{
				ZString message = "You do not have security rights to make adjustments/posting of costs & charges " + "to this job because this billing job exceeds the number " + "of days (" + AgencyRegistry.Instance.AllowInvoiceAmendmentsDays.Value + ") the changes are allowed (as set in the Registry: " + ((IRegistryItemInternals)AgencyRegistry.Instance.AllowInvoiceAmendmentsDays).Location + "). " + (char)13 + (char)10 + "Contact the accounts department or a user who has security rights to override this restriction." + (char)13 + (char)10 + (char)13 + (char)10 + "Please enter user name & password to save/post this change.";
				AssertEquals("EditSecurityMessage NOT Empty", message, testJob.InvoicingSupporter.EditSecurityMessage);
			}
			else
			{
				AssertEquals("EditSecurityMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			}
		}

		#region Test
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			BillOfLading billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			return billOfLading;
		}
		#endregion
	}
}
