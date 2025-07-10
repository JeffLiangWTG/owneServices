using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Agency.Business.Testing
{
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	partial class SundryChargesTest
	{
		public void TestJobNumber()
		{
			Sundry.D4_JobNumber = "Blaticus";
			AssertEquals("Blaticus", ((IJobNumber)Sundry).JobNumber);
		}

		#region IJobInvoicingPlugIn Members

		public void TestJobInvoicingEmptyValues()
		{
			CombineAssertions(delegate
			{
				IJobInvoicingPlugIn job = Sundry;
				AssertEquals("OverriddenDepartmentPK", ZGuid.Empty, job.InvoicingSupporter.OverriddenDepartmentPK);
				AssertEquals("OperationsBranch", GlbBranch.CurrentBranch, job.InvoicingSupporter.OperationsBranch);
				AssertEquals("ATA", ZDateTime.Empty, job.InvoicingSupporter.ATA);
				AssertEquals("ATD", ZDateTime.Empty, job.InvoicingSupporter.ATD);
				AssertEquals("ActualChargeable", 0m, job.InvoicingSupporter.ActualChargeable);
				AssertEquals("ActualChargeableUnit", "", job.InvoicingSupporter.ActualChargeableUnit);
				AssertEquals("ActualVolume", 0m, job.InvoicingSupporter.ActualVolume);
				AssertEquals("ActualVolumeUnit", "", job.InvoicingSupporter.ActualVolumeUnit);
				AssertEquals("ActualWeight", 0m, job.InvoicingSupporter.ActualWeight);
				AssertEquals("ActualWeightUnit", "", job.InvoicingSupporter.ActualWeightUnit);
				AssertEquals("Broker", null, job.InvoicingSupporter.Broker);
				AssertEquals("Consignee", null, job.InvoicingSupporter.Consignee);
				AssertEquals("Consignor", null, job.InvoicingSupporter.Consignor);
				AssertEquals("ConsolExchangeRate", 0m, job.InvoicingSupporter.ConsolExchangeRate);
				AssertEquals("ConsolRateCurrency", null, job.InvoicingSupporter.ConsolRateCurrency);
				AssertEquals("ContainerCount", 0, job.InvoicingSupporter.ContainerCount);
				AssertEquals("ContainerMode", "", job.InvoicingSupporter.ContainerMode);
				AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", true, job.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
				AssertEquals("DefaultCreditor", null, job.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(null, null)));
				AssertEquals("Destination", null, job.InvoicingSupporter.Destination);
				AssertEquals("ETA", ZDateTime.Empty, job.InvoicingSupporter.ETA);
				AssertEquals("ETD", ZDateTime.Empty, job.InvoicingSupporter.ETD);
				AssertEquals("EditSecurityLock", false, job.InvoicingSupporter.EditSecurityLock);
				AssertEquals("EditSecurityMessage", "", job.InvoicingSupporter.EditSecurityMessage);
				AssertEquals("HouseBillNumber", "", job.InvoicingSupporter.HouseBillNumber);
				AssertEquals("PaymentTerm", null, job.InvoicingSupporter.PaymentTerm);
				AssertEquals("IsDirectShipment", false, job.InvoicingSupporter.IsDirectShipment);
				AssertEquals("IsDomestic", false, job.InvoicingSupporter.IsDomestic);
				AssertEquals("IsExport", false, job.InvoicingSupporter.IsExport);
				AssertEquals("IsImport", false, job.InvoicingSupporter.IsImport);
				AssertEquals("IsPlugInReadOnly", false, job.InvoicingSupporter.IsPlugInReadOnly);
				AssertEquals("MasterBillNumber", "", job.InvoicingSupporter.MasterBillNumber);
				AssertEquals("Origin", null, job.InvoicingSupporter.Origin);
				AssertEquals("ReceivingAgent", null, job.InvoicingSupporter.ReceivingAgent);
				AssertEquals("SendingAgent", null, job.InvoicingSupporter.SendingAgent);
				AssertEquals("ShipmentNumberOfColoadMaster", "", job.InvoicingSupporter.ShipmentNumberOfColoadMaster);
				AssertEquals("TranshipmentPort", null, job.InvoicingSupporter.GetTranshipmentPort(CostSell.Cost));
				AssertEquals("TranshipmentPort", null, job.InvoicingSupporter.GetTranshipmentPort(CostSell.Revenue));
				AssertEquals("TransportMode", Constants.TransportModes.Sea, job.InvoicingSupporter.TransportMode);
			});
		}

		public void TestJobInvoicingSecurity()
		{
			CombineAssertions(delegate
			{
				IJobInvoicingPlugIn job = Sundry;
				AssertEquals("AuditSecurity", Env.Security.AgencySundryChargesAuditBilling, job.InvoicingSupporter.AuditSecurity);
				AssertEquals("JobInvoicingSecurity", Env.Security.AgencySundryChargesJobInvoicing, job.InvoicingSupporter.JobInvoicingSecurity);
				AssertEquals("EditSecurityCheckpoint", Env.Security.None, job.InvoicingSupporter.EditSecurityCheckpoint);
			});
		}

		public void TestConsumerType()
		{
			IJobInvoicingPlugIn job = Sundry;
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.AgencySundryCharges, job.InvoicingSupporter.ConsumerType);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Sundry;
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent containerDetention = Factory.New<SundryCharges>();
			Assert(containerDetention.AllowInvoiceDeletion);
		}

		#endregion

		#region SundryChargesInvoicingSupporterTest

		[TestedType(typeof(SundryChargesInvoicingSupporter))]
		public class SundryChargesInvoicingSupporterTest : JobInvoicingSupporterTest
		{
			protected override IJobInvoicingPlugIn GetNewBusinessObject()
			{
				SundryCharges sundryCharges = Factory.NewWithValidTestData<SundryCharges>();
				return sundryCharges;
			}
		}

		#endregion

	}
}
