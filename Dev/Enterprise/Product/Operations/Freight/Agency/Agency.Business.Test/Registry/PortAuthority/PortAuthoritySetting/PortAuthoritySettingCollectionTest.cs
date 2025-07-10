using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthoritySettingCollection))]
	internal class PortAuthoritySettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PortAuthoritySettingCollection>
	{
		public void TestFindPortSetting()
		{
			PortAuthoritySettingCollection collection = new PortAuthoritySettingCollection();
			PortAuthoritySetting setting1 = collection.AddNew();
			setting1.Port = "AUBNE";
			setting1.Status = PortAuthoritySettingStatus.Codes.Disabled;
			PortAuthoritySetting setting2 = collection.AddNew();
			setting2.Port = "AUMEL";
			setting2.Status = PortAuthoritySettingStatus.Codes.Production;
			AssertNull("dont find disabled ports.", collection.FindPortSetting("AUBNE"));
			AssertSame("find non-disabled ports.", setting2, collection.FindPortSetting("AUMEL"));
			AssertNull("dont find non-existant ports.", collection.FindPortSetting("AUFRE"));
		}

		public void TestCheckUniqueness()
		{
			var collection = new PortAuthoritySettingCollection();
			var setting1 = collection.AddNew();

			setting1.Port = "AUBNE";
			setting1.PrincipalPK = ZGuid.Empty;

			var setting2 = collection.AddNew();
			setting2.Port = "AUBNE";
			setting2.PrincipalPK = ZGuid.Empty;

			AssertHasError(setting1.PortInfo, "The same Port cannot be duplicated for the same Principal.");
			AssertHasError(setting1.PrincipalPKInfo, "The same Port cannot be duplicated for the same Principal.");

			AssertHasError(setting2.PortInfo, "The same Port cannot be duplicated for the same Principal.");
			AssertHasError(setting2.PrincipalPKInfo, "The same Port cannot be duplicated for the same Principal.");

			setting1.Port = "AUMEL";
			AssertNoError(setting1.PortInfo, "The same Port cannot be duplicated for the same Principal.");
			AssertNoError(setting1.PrincipalPKInfo, "The same Port cannot be duplicated for the same Principal.");

			AssertNoError(setting2.PortInfo, "The same Port cannot be duplicated for the same Principal.");
			AssertNoError(setting2.PrincipalPKInfo, "The same Port cannot be duplicated for the same Principal.");
		}

		public void TestFindByPortWithPrincipal()
		{
			var voyage = Factory.New<JobVoyage>();

			var principal = GlbBranch.CurrentBranch.OrgProxy;
			principal.OH_IsShippingProvider = true;
			principal.OH_IsShippingLine = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			principal.Factory.Save();

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_JX = voyage.Sailings[0].PK;
			billOfLading.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			var portCollection = new PortAuthorityPortCollection();
			var port1 = portCollection.AddNew();
			port1.Port = "AUBNE";
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);

			var settings = new PortAuthoritySettings();
			settings.Settings.RemoveAll();
			var setting1 = settings.Settings.AddNew();
			setting1.Port = "AUBNE";
			setting1.Status = PortAuthoritySettingStatus.Codes.Production;
			setting1.PrincipalPK = ZGuid.Empty;
			setting1.SenderID = "Sender ID";
			var setting2 = settings.Settings.AddNew();
			setting2.Port = "AUBNE";
			setting2.Status = PortAuthoritySettingStatus.Codes.Production;
			setting2.PrincipalPK = principal.PK;
			setting2.SenderID = "Sender ID2";

			AgencyRegistry.Instance.PortAuthoritySettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var portSettings = AgencyRegistry.Instance.PortAuthoritySettings.Value;
			var otherGuid = ZGuid.NewZGuid();

			AssertEquals(2, portSettings.Settings.Count);
			AssertEquals(ZGuid.Empty, portSettings.Settings.FindPortSettingWithPrincipal("AUBNE", otherGuid).PrincipalPK);
			AssertEquals(ZGuid.Empty, portSettings.Settings.FindPortSettingWithPrincipal("AUBNE", ZGuid.Empty).PrincipalPK);
			AssertEquals(principal.PK, portSettings.Settings.FindPortSettingWithPrincipal("AUBNE", principal.PK).PrincipalPK);
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override PortAuthoritySettingCollection GetCollectionToTest()
		{
			return new PortAuthoritySettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortAuthoritySetting();
		}
		#endregion
	}
}
