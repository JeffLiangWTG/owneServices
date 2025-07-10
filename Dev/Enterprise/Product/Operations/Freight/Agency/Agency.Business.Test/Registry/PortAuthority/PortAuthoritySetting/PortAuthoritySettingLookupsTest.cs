using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthoritySettingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			PortAuthorityPortCollection portCollection = new PortAuthorityPortCollection();
			PortAuthorityPort port1 = portCollection.AddNew();
			port1.Port = "AUFRE";
			port1.Version = PortAuthorityVersionList.Codes.V11;
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";
			PortAuthorityPort port2 = portCollection.AddNew();
			port2.Port = "AUMEL";
			port2.Version = PortAuthorityVersionList.Codes.V20;
			port2.ProductionEmail = "prod2@freadnet.org";
			port2.ProductionID = "prod2";
			port2.TestingEmail = "";
			port2.TestingID = "";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
			PortAuthoritySettingLookups lookups = new PortAuthoritySettingLookups(Setting, Factory);
			Setting.Port = "AUFRE";
			AssertEquals("DIS, TST, LIV", lookups.Status_List.CodesAsString);
			Setting.Port = "AUMEL";
			AssertEquals("DIS, LIV", lookups.Status_List.CodesAsString);
		}

		public void TestPortList()
		{
			PortAuthorityPortCollection portCollection = new PortAuthorityPortCollection();
			PortAuthorityPort port1 = portCollection.AddNew();
			port1.Port = "AUFRE";
			port1.Version = PortAuthorityVersionList.Codes.V11;
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";
			PortAuthorityPort port2 = portCollection.AddNew();
			port2.Port = "AUMEL";
			port2.Version = PortAuthorityVersionList.Codes.V20;
			port2.ProductionEmail = "prod2@freadnet.org";
			port2.ProductionID = "prod2";
			port2.TestingEmail = "";
			port2.TestingID = "";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
			PortAuthoritySettingLookups lookups = new PortAuthoritySettingLookups(Setting, Factory);
			AssertEquals(2, lookups.Port_List.Count);
			AssertEquals("AUFRE", lookups.Port_List[0].Code);
			AssertEquals("AUMEL", lookups.Port_List[1].Code);
		}

		#region Implementation
		PortAuthoritySettingCollection Collection
		{
			get
			{
				return collection ?? (collection = new PortAuthoritySettingCollection());
			}
		}

		PortAuthoritySettingCollection collection;
		PortAuthoritySetting Setting
		{
			get
			{
				return setting ?? (setting = Collection.AddNew());
			}
		}

		PortAuthoritySetting setting;
		#endregion
	}
}
