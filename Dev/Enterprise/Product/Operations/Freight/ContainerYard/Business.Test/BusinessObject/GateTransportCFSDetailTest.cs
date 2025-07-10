using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetail))]
	sealed class GateTransportCFSDetailTest : EnterpriseBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var gateTransport = Factory.New<GateTransport>();
			gateTransport.GTT_FacilityType = "AAA";
			var gateTransportCFSDetail = Factory.New<GateTransportCFSDetail>();
			gateTransportCFSDetail.GTF_JobNumber = "JOB123";
			gateTransport.GateTransportCFSDetails.Add(gateTransportCFSDetail);

			return gateTransportCFSDetail;
		}

		#endregion
	}
}
