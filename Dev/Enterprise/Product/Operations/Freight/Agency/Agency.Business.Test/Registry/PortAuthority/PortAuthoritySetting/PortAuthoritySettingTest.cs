using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthoritySettingTestTest : TestCaseWithFactory
	{
		public void TestDefaultStatus()
		{
			PortAuthoritySetting setting = Collection.AddNew();
			AssertEquals("Default status should be disabled", PortAuthoritySettingStatus.Codes.Disabled, setting.Status);
		}

		public void TestSenderIDReadonlyIfDisabled()
		{
			Setting1.Status = PortAuthoritySettingStatus.Codes.Production;
			AssertEquals(false, Setting1.SenderIDInfo.ReadOnly);
			Setting1.Status = PortAuthoritySettingStatus.Codes.Disabled;
			AssertEquals(true, Setting1.SenderIDInfo.ReadOnly);
			Setting1.Status = PortAuthoritySettingStatus.Codes.Testing;
			AssertEquals(false, setting1.SenderIDInfo.ReadOnly);
		}

		public void TestValidatePort()
		{
			var portCollection = new PortAuthorityPortCollection();
			var port1 = portCollection.AddNew();
			port1.Port = "AUFRE";
			port1.Version = PortAuthorityVersionList.Codes.V11;
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";

			var port2 = portCollection.AddNew();
			port2.Port = "AUMEL";
			port2.Version = PortAuthorityVersionList.Codes.V20;
			port2.ProductionEmail = "prod2@freadnet.org";
			port2.ProductionID = "prod2";
			port2.TestingEmail = "test2@freadnet.org";
			port2.TestingID = "test2";

			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);

			var settingCollection = new PortAuthoritySettingCollection();
			var setting = settingCollection.AddNew();

			setting.ValidatePort();
			AssertHasError(setting.PortInfo, "Please enter a Port.");

			setting.Port = "AUBNE";
			setting.ValidatePort();
			AssertHasError(setting.PortInfo, "Please select a port from the drop-down.");

			setting.Port = "AUMEL";
			setting.ValidatePort();
			AssertNoError(setting.PortInfo, "Please select a port from the drop-down.");
			AssertNoError(setting.PortInfo, "Please enter a Port.");
		}

		public void TestValidateStatus()
		{
			Setting1.Status = "";
			AssertHasError("Empty Status", Setting1.StatusInfo, "Please enter a Status.");
			Setting1.Status = PortAuthoritySettingStatus.Codes.Production;
			AssertNoErrors("Valid Status", Setting1.StatusInfo);
			Setting1.Status = "XXX";
			AssertHasError("Invalid Status", Setting1.StatusInfo, "Enter a valid Status.");
		}

		public void TestValidateSenderID()
		{
			Setting1.Status = PortAuthoritySettingStatus.Codes.Disabled;
			Setting1.SenderID = "";
			AssertNoErrors("SenderID not required for disabled ports", Setting1.SenderIDInfo);
			Setting1.Status = PortAuthoritySettingStatus.Codes.Production;
			Setting1.SenderID = "";
			AssertHasError("SenderID required for production", Setting1.SenderIDInfo, "Please enter a Sender ID.");
			Setting1.Status = PortAuthoritySettingStatus.Codes.Testing;
			Setting1.SenderID = "";
			AssertHasError("SenderID required for testing", Setting1.SenderIDInfo, "Please enter a Sender ID.");
			Setting1.SenderID = "Blat";
			AssertNoErrors("value provided", Setting1.SenderIDInfo);
		}

		public void TestValidatePrincipalPK()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			Setting1.PrincipalPK = ZGuid.Empty;
			AssertNoErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal).", Setting1.PrincipalPKInfo);

			Setting1.PrincipalPK = orgProxy.PK;
			AssertHasErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal).", Setting1.PrincipalPKInfo);

			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.OH_IsShippingProvider = true;
			orgProxy.Factory.Save();
			Setting1.ValidatePrincipalPK();
			AssertNoErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal).", Setting1.PrincipalPKInfo);
		}

		public void TestProxyValues()
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
			port2.TestingEmail = "test2@freadnet.org";
			port2.TestingID = "test2";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
			PortAuthoritySettingCollection settingCollection = new PortAuthoritySettingCollection();
			PortAuthoritySetting setting = settingCollection.AddNew();
			setting.Port = "";
			setting.Status = PortAuthoritySettingStatus.Codes.Production;
			AssertEquals("", setting.Version);
			AssertEquals("", setting.Email);
			AssertEquals("", setting.RecipientID);
			setting.Port = "AUFRE";
			AssertEquals(PortAuthorityVersionList.Codes.V11, setting.Version);
			AssertEquals("prod1@freadnet.org", setting.Email);
			AssertEquals("prod1", setting.RecipientID);
			setting.Status = PortAuthoritySettingStatus.Codes.Testing;
			AssertEquals(PortAuthorityVersionList.Codes.V11, setting.Version);
			AssertEquals("test1@freadnet.org", setting.Email);
			AssertEquals("test1", setting.RecipientID);
			setting.Port = "AUMEL";
			AssertEquals(PortAuthorityVersionList.Codes.V20, setting.Version);
			AssertEquals("test2@freadnet.org", setting.Email);
			AssertEquals("test2", setting.RecipientID);
			setting.Status = "";
			AssertEquals(PortAuthorityVersionList.Codes.V20, setting.Version);
			AssertEquals("", setting.Email);
			AssertEquals("", setting.RecipientID);
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

		PortAuthoritySetting Setting1
		{
			get
			{
				if (setting1 == null)
				{
					setting1 = Collection.AddNew();
				}
				return setting1;
			}
		}

		PortAuthoritySetting setting1;

		#endregion
	}
}
