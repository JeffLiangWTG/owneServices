using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(PortCallManager))]
	public class PortCallManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetupRequest()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TOM";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "JON";
			vessel.RV_LloydsNumber = "IMO1234";
			vessel.RV_RadioCallSign = "CALL";

			Factory.Save();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYAGE";
			voyage.JV_OH_Line = carrier.PK;

			var estimatedTime = new ZDateTime(2017, 8, 12);

			var manager = new PortCallManager(Factory);
			manager.SetupRequest("ZADUR", estimatedTime, voyage, PortCallRequestType.Load);

			CombineAssertions(() =>
			{
				AssertEquals("RequestType", PortCallRequestType.Load, manager.Request.RequestType);
				AssertEquals("Port", "ZADUR", manager.Request.Port);
				AssertEquals("EstimatedTime", estimatedTime, manager.Request.EstimatedTime);
				AssertEquals("CarrierPK", carrier.PK, manager.Request.CarrierPK);
				AssertEquals("Voyage", "VOYAGE", manager.Request.Voyage);
				AssertEquals("IMO", "IMO1234", manager.Request.IMO);
				AssertEquals("CallSign", "CALL", manager.Request.CallSign);
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortCallManager(Factory);
		}

		#endregion
	}
}
