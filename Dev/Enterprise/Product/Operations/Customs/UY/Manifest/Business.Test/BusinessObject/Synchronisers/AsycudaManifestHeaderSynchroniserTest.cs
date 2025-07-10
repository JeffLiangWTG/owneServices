using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestAMA_DateAtCustomsOffice()
		{
			CreateLocoMapIfNotExists("3002", "UYMVD", USLocoMapSystemUsageList.Codes.Air, Core.Constants.CountryGuids.Uruguay);
			CreateLocoMapIfNotExists("2704", "CLSCL", USLocoMapSystemUsageList.Codes.Air, Core.Constants.CountryGuids.Chile);
			CreateLocoMapIfNotExists("5201", "CLCAL", USLocoMapSystemUsageList.Codes.Air, Core.Constants.CountryGuids.Chile);
			CreateLocoMapIfNotExists("1996", "UYPDP", USLocoMapSystemUsageList.Codes.Air, Core.Constants.CountryGuids.Uruguay);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "GBSOU";
			shipment.JS_RL_NKDestination = "LKCMB";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "UYMVD";
			leg1.JW_RL_NKDiscPortForBinding = "CLSCL";
			leg1.JW_ETDForBinding = ZDateTime.Today.AddDays(-12);
			leg1.JW_ETAForBinding = ZDateTime.Today.AddDays(-7);

			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "CLCAL";
			leg2.JW_RL_NKDiscPortForBinding = "UYPDP";
			leg1.JW_ETDForBinding = ZDateTime.Today.AddDays(-6);
			leg2.JW_ETAForBinding = ZDateTime.Today;

			Factory.Save();

			shipment.Consols.Add(consol);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Date at Customs Office", ZDateTime.Today, manifestHeader.AMA_DateAtCustomsOffice);

			consol.Transports.RemoveAndDeleteAll();
			Factory.Save();

			leg1 = consol.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "CLSCL";
			leg1.JW_RL_NKDiscPortForBinding = "UYMVD";
			leg1.JW_ETDForBinding = ZDateTime.Today.AddDays(-12);
			leg1.JW_ETAForBinding = ZDateTime.Today.AddDays(-10);

			Factory.Save();

			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Date at Customs Office", ZDateTime.Today.AddDays(-10), manifestHeader.AMA_DateAtCustomsOffice);
		}

		void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, ZGuid country)
		{
			ZQuery codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = country;
			}
		}
	}
}
