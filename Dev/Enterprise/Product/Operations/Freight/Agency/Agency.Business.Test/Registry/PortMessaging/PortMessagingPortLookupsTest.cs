using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class PortMessagingPortLookupsTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			SetUpOrgs();
			Factory.Save();

			var lookups = new PortMessagingPortLookups(new PortMessagingPort(), Factory);
			var collection = lookups.Principals;

			collection.Load();
			AssertEquals("Principal should be in the collection", true, collection.Contains(Principal));
			AssertEquals("ShippingLine should be in the collection", false, collection.Contains(ShippingLine));
			AssertEquals("NonShippingLine should not be in the collection", false, collection.Contains(NotShippingLine));
		}

		public void TestValidatePortLookups_System()
		{
			var portCollection = new PortMessagingPortCollection();
			var port = portCollection.AddNew();
			port.Port = "NZAKL";
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var lookups = new PortMessagingPortLookups(port, Factory);

				var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
				AssertEquals("NZAKL,NZLYT,NZNPE,NZPOE,NZTRG,NZWLG", result);
			}
		}

		public void TestValidatePortLookupsWithSpanishPortsEnabled_System()
		{
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var portCollection = new PortMessagingPortCollection();
				var port = portCollection.AddNew();
				port.Port = "ESGAN";
				port.SenderID = "SenderID_1";
				port.Enabled = true;
				port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

				using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
				{
					var lookups = new PortMessagingPortLookups(port, Factory);

					var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
					AssertEquals("ESBCN,ESGAN,ESPDS,ESVLC,NZAKL,NZLYT,NZNPE,NZPOE,NZTRG,NZWLG", result);
				}
			}
		}

		public void TestValidatePortLookups_ImportReleaseOrderPorts_System()
		{
			var portCollection = new PortMessagingPortCollection();
			var port = portCollection.AddNew();
			port.Port = "NZAKL";
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var lookups = new PortMessagingPortLookups(port, Factory);

				var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
				AssertEquals("NZAKL,NZLYT,NZNPE,NZPOE,NZTRG,NZWLG", result);
			}
		}

		public void TestValidatePortLookups_Australia()
		{
			var portCollection = new PortMessagingPortCollection();
			var port = portCollection.AddNew();
			port.Port = "NZAKL";
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty);

			using (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.SetTemporaryValue(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty, portCollection))
			{
				var lookups = new PortMessagingPortLookups(port, Factory);

				var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
				AssertEquals("", result);
			}
		}

		public void TestValidatePortLookups_ImportReleaseOrderPorts_Australia()
		{
			var portCollection = new PortMessagingPortCollection();
			var port = portCollection.AddNew();
			port.Port = "NZAKL";
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty);

			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty, portCollection))
			{
				var lookups = new PortMessagingPortLookups(port, Factory);

				var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
				AssertEquals("", result);
			}
		}

		#region Implementation

		GlbCompany CompanyAU => companyAU ?? (companyAU = Factory.NewWithValidTestData<GlbCompany>());

		GlbCompany companyAU;
		OrgHeader NotShippingLine;
		OrgHeader ShippingLine;
		OrgHeader Principal;

		public void SetUpOrgs()
		{
			NotShippingLine = Factory.NewWithValidTestData<OrgHeader>();

			ShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			ShippingLine.OH_IsShippingProvider = true;

			Principal = Factory.NewWithValidTestData<OrgHeader>();
			Principal.OH_IsShippingProvider = true;
			Principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
		}

		#endregion
	}
}
