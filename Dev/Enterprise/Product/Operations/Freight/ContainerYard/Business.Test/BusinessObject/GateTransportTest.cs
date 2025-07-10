using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransport))]
	sealed class GateTransportTest : EnterpriseBusinessObjectTestCase
	{
		public new void TestDocumentSupportableNotNull()
		{
			var gateTransport = Factory.New<GateTransport>();
			AssertNull(gateTransport.DocumentSupporter);
		}

		public void TestDocManagerInfo()
		{
			var gateTransport = Factory.New<GateTransport>();
			var docManagerInfo = gateTransport.DocManagerInfo;
			AssertEquals(gateTransport, docManagerInfo.BusinessEntity);
			AssertEquals(Core.Constants.DocManagerCodes.GateTransport, docManagerInfo.DocManagerCode);
		}

		public void TestJobNumber_ShouldBeGTTJobNumber()
		{
			var gateTransport = Factory.New<GateTransport>();
			var job = gateTransport as IJobNumber;
			AssertNotNull(job);
			AssertEquals(gateTransport.GTT_JobNumber, job.JobNumber);
		}

		#region Overrides

		public void TestGateTransportCFSDetailCollection()
		{
			var gateTransportCFSDetail = Factory.New<GateTransportCFSDetail>();
			var gateTransport = Factory.New<GateTransport>();

			gateTransport.GateTransportCFSDetails.Add(gateTransportCFSDetail);

			AssertEquals(1, gateTransport.GateTransportCFSDetails.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		#endregion

		public void TestTransportCompany()
		{
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_FullName = "Transport Company";
			transportCompany.MainAddress.OA_Address1 = "1 Transport St";

			var gateTransport = Factory.New<GateTransport>();
			var transportCompanyDocAddress = gateTransport.DocAddresses.AddNew(DocAddressType.TransportCompanyDocumentaryAddress);
			transportCompanyDocAddress.E2_OA_Address = transportCompany.MainAddress.PK;

			AssertEquals("TransportCompany", "Transport Company", gateTransport.TransportCompany.OH_FullName);
			AssertEquals("TransportCompanyDocumentaryAddress", "1 Transport St", gateTransport.TransportCompanyDocumentaryAddress.Address1);
		}
	}
}
