using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceJobCluster))]
	public class AccDraftInvoiceJobClusterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOperationalJobs()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));

			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_RX_NKTransactionCurrency = "AUD";
			draftInvoice.AIH_OH_Creditor = creditor1.PK;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;

			var jobCluster1 = Factory.NewWithValidTestData<AccDraftInvoiceJobCluster>();
			jobCluster1.AIC_AIH_Header = draftInvoice.PK;
			jobCluster1.AIC_Amount = 0m;
			jobCluster1.AIC_RX_NKCurrency = "AUD";
			jobCluster1.AIC_GC_Company = GlbCompany.CurrentCompany.PK;

			var jobClusterJob1 = Factory.NewWithValidTestData<AccDraftInvoiceJob>();
			jobClusterJob1.AIJ_AIC_Cluster = jobCluster1.PK;
			jobClusterJob1.AIJ_ParentID = shipment1.PK;
			jobClusterJob1.AIJ_ParentTableCode = "JS";
			jobClusterJob1.AIJ_GC_Company = GlbCompany.CurrentCompany.PK;

			var jobClusterJob2 = Factory.NewWithValidTestData<AccDraftInvoiceJob>();
			jobClusterJob2.AIJ_AIC_Cluster = jobCluster1.PK;
			jobClusterJob2.AIJ_ParentID = shipment2.PK;
			jobClusterJob2.AIJ_ParentTableCode = "JS";
			jobClusterJob2.AIJ_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertEquals(2, jobCluster1.OperationalJobs.Count);
			jobCluster1.OperationalJobs.Select(x => x.AIJ_ParentID).ContainsSameElementsInAnyOrder(new List<ZGuid> { shipment1.PK, shipment2.PK });
		}
	}
}
