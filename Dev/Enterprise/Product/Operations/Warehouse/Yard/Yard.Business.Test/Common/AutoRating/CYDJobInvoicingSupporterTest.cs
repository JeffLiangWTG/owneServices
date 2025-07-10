using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public abstract class CYDJobInvoicingSupporterTest<T> : JobInvoicingSupporterTest
		where T : BusinessObject, IJobHeaderParent, IJobNumber, IJobInvoicingPlugIn, ICYDJobInvoicingSupporter
	{
		public void TestConsumerType() => AssertEquals(ExpectedConsumerType, GetNewJobInvoicingSupporter().ConsumerType);

		public void TestAuditSecurity() => AssertEquals(ExpectedAuditSecurityCheckpoint, GetNewJobInvoicingSupporter().AuditSecurity);

		public void TestJobInvoicingSecurity_Consignment() => AssertEquals(ExpectedJobInvoicingSecurityCheckpoint, GetNewJobInvoicingSupporter().JobInvoicingSecurity);

		public void TestIncludeInConsolCosting()
		{
			var invoicingSupporter = GetNewJobInvoicingSupporter();
			AssertEquals(ExpectedIncludeInConsolCosting, invoicingSupporter.IncludeInConsolCosting(true));
			AssertEquals(ExpectedIncludeInConsolCosting, invoicingSupporter.IncludeInConsolCosting(false));
		}

		public void TestDefaultLocalClient() => AssertEquals(GetNewJobInvoicingSupporter().OverriddenDefaultLocalClient, ExpectedDefaultLocalClient);

		protected abstract JobInvoicingConsumerType ExpectedConsumerType { get; }

		protected abstract SecurityCheckpoint ExpectedAuditSecurityCheckpoint { get; }

		protected abstract SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint { get; }

		protected override bool ExcludeFromTestBecauseNoBillingTab => true;

		protected virtual bool ExpectedIncludeInConsolCosting => false;

		protected virtual OrgHeader ExpectedDefaultLocalClient => null;

		IJobInvoicingSupporter GetNewJobInvoicingSupporter() => GetNewBusinessObject().InvoicingSupporter;

		protected virtual CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory)); }
		}

		CYDYardTestHelper helper;
	}

	[TestedType(typeof(CYDJobInvoicingSupporter<CYDReceiveAdvice>))]
	public class CYDJobReceiveAdviceInvoicingSupporterTest : CYDJobInvoicingSupporterTest<CYDReceiveAdvice>
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			defaultOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			receiveAdvice.DocAddresses.AddNew(defaultOrgAddress, DocAddressType.BookingPartyDocumentaryAddress);
			return receiveAdvice;
		}

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.CYDReceiveAdvice;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.CYDReceiveAdviceAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.CYDReceiveAdviceJobInvoicing;

		protected override OrgHeader ExpectedDefaultLocalClient => defaultOrgAddress.Header;
		OrgAddress defaultOrgAddress;
	}

	[TestedType(typeof(CYDJobInvoicingSupporter<CYDReleaseAdvice>))]
	public class CYDJobReleaseAdviceInvoicingSupporterTest : CYDJobInvoicingSupporterTest<CYDReleaseAdvice>
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var releaseAdvice = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			defaultOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			releaseAdvice.DocAddresses.AddNew(defaultOrgAddress, DocAddressType.BookingPartyDocumentaryAddress);
			return releaseAdvice;
		}

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.CYDReleaseAdvice;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.CYDReleaseAdviceAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.CYDReleaseAdviceJobInvoicing;

		protected override OrgHeader ExpectedDefaultLocalClient => defaultOrgAddress.Header;
		OrgAddress defaultOrgAddress;
	}

	[TestedType(typeof(CYDJobInvoicingSupporter<CYDTransportationUnit>))]
	public class CYDJobTransportationUnitInvoicingSupporterTest : CYDJobInvoicingSupporterTest<CYDTransportationUnit>
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			defaultOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportationUnit.DocAddresses.RemoveAll();
			transportationUnit.DocAddresses.AddNew(defaultOrgAddress, DocAddressType.TransportCompanyDocumentaryAddress);
			return transportationUnit;
		}

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.CYDTransportationUnit;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.CYDTransportationUnitAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.CYDTransportationUnitJobInvoicing;

		protected override OrgHeader ExpectedDefaultLocalClient => defaultOrgAddress.Header;
		OrgAddress defaultOrgAddress;
	}

	[TestedType(typeof(CYDJobInvoicingSupporter<MNRWorkOrderHeader>))]
	public class CYDJobMNRWorkOrderHeaderInvoicingSupporterTest : CYDJobInvoicingSupporterTest<MNRWorkOrderHeader>
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var client = Helper.CreateClient();
			defaultOrgAddress = client.MainAddress;
			var yard = Helper.CreateCYDWarehouse();
			var recieveAdvice = Helper.CreateReceiveAdvice(client, yard);
			var yardUnitState = Helper.AddReceiveAdviceLine(recieveAdvice, "CNT1234", "20GP");
			var workOrderHeader = Helper.CreateMNRWorkOrderHeader(yard, yardUnitState.PK);
			return workOrderHeader;
		}

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.MNRWorkOrderHeader;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.MNRWorkOrderAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.MNRWorkOrderJobInvoicing;

		protected override OrgHeader ExpectedDefaultLocalClient => defaultOrgAddress.Header;
		OrgAddress defaultOrgAddress;
	}
}
