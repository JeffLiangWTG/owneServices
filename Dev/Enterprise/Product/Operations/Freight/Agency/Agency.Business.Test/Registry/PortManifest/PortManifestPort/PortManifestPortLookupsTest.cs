using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	class PortManifestPortLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValidatePortLookups_System()
		{
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var portCollection = new PortManifestPortCollection();
				var port = portCollection.AddNew();
				port.Port = "NZAKL";
				port.PrincipalPK = ZGuid.NewZGuid();
				port.SenderID = "SenderID_1";
				port.Enabled = true;
				port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
				{
					var lookups = new PortManifestPortLookups(port, Factory);

					var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
					AssertEquals("NZAKL,NZLYT,NZNPE,NZORR,NZPOE,NZTRG,NZWLG", result);
				}
			}
		}

		public void TestValidatePortLookupsWithSpanishPortsEnabled_System()
		{
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var spanishPortCollection = new PortManifestPortCollection();
				var port = spanishPortCollection.AddNew();
				port.Port = "ESGAN";
				port.PrincipalPK = ZGuid.NewZGuid();
				port.SenderID = "SenderID_1";
				port.Enabled = true;
				port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

				using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, spanishPortCollection))
				{
					var lookups2 = new PortManifestPortLookups(port, Factory);

					var result = string.Join(",", lookups2.Port_List.Select(x => x.Code).OrderBy(s => s));
					AssertEquals("ESBCN,ESGAN,ESPDS,ESVLC,NZAKL,NZLYT,NZNPE,NZORR,NZPOE,NZTRG,NZWLG", result);
				}
			}
		}
	}
}
