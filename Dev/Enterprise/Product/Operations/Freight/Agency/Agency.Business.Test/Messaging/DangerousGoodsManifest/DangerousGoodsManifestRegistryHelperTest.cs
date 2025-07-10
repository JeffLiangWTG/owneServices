using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class DangerousGoodsManifestRegistryHelperTest : TestCaseWithFactory
	{
		public void TestRetrievePortConfiguration_Port_and_Principal()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", "AU");
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			AddNewPortDangeoursGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", principal.PK, true);

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD", principal.PK);
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
				AssertEquals("Principal", principal.PK, portConfig.PrincipalPK);
			}

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD", principal.PK);
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
				AssertEquals("Principal", principal.PK, portConfig.PrincipalPK);
			}

			dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			AddNewPortDangeoursGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", ZGuid.Empty, true);

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD", ZGuid.Empty);
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
				AssertEquals("Principal", Guid.Empty, portConfig.PrincipalPK);
			}

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD", ZGuid.Empty);
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
				AssertEquals("Principal", Guid.Empty, portConfig.PrincipalPK);
			}
		}

		public void TestRetrievePortConfiguration_Port()
		{
			var dangerousGoodsManifestPorts = new DangerousGoodsManifestPortCollection();
			AddNewPortDangeoursGoodsManifestPort(dangerousGoodsManifestPorts, "AUSYD", ZGuid.Empty, true);

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD");
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
			}

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dangerousGoodsManifestPorts))
			{
				var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration("AUSYD");
				AssertNotNull(portConfig);
				AssertEquals("Port", "AUSYD", portConfig.Port);
				AssertEquals("Enabled", true, portConfig.Enabled);
			}
		}

		#region Implementation

		DangerousGoodsManifestPortCollection AddNewPortDangeoursGoodsManifestPort(DangerousGoodsManifestPortCollection dangerousGoodsManifestPorts, string port, ZGuid principalPK, bool enabled, string senderID = "Sender")
		{
			var dangerousGoodsManifestPort = dangerousGoodsManifestPorts.AddNew();
			dangerousGoodsManifestPort.Port = port;
			dangerousGoodsManifestPort.PrincipalPK = principalPK;
			dangerousGoodsManifestPort.SenderID = $"{senderID}_{dangerousGoodsManifestPorts.Count}";
			dangerousGoodsManifestPort.Enabled = enabled;

			return dangerousGoodsManifestPorts;
		}

		#endregion
	}
}
