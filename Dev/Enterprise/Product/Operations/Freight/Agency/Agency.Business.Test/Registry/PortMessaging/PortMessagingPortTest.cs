using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortMessagingPort))]
	public class PortMessagingPortTest : RegistryBusinessObjectTemplateTestCase<PortMessagingPort>
	{
		public void TestValidatePort()
		{
			var ports = new PortMessagingPortCollection();
			var port1 = ports.AddNew();
			port1.Enabled = true;
			port1.ValidatePort();
			AssertHasError(port1.PortInfo, "Please enter a Port.");
			port1.Port = "NZAKL";
			AssertNoErrors(port1.PortInfo);
		}

		public void TestValidatePrincipalPK()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			var ports = new PortMessagingPortCollection();
			var port1 = ports.AddNew();
			AssertNoErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal.", port1.PrincipalPKInfo);

			port1.PrincipalPK = orgProxy.PK;
			AssertHasErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal.", port1.PrincipalPKInfo);

			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.OH_IsShippingProvider = true;
			orgProxy.Factory.Save();
			port1.ValidatePrincipalPK();
			AssertNoErrors("Please select valid Principal organization as configured on Organization > Carrier > Configuration > Sea > Principal.", port1.PrincipalPKInfo);
		}

		public void TestCheckUniquenessPortAndPrincipalPK()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			orgProxy.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			orgProxy.OH_IsShippingProvider = true;
			orgProxy.Factory.Save();

			var ports = new PortMessagingPortCollection();
			var port1 = ports.AddNew();

			port1.Port = "NZAKL";
			AssertNoErrors(port1.PortInfo);
			AssertNoErrors(port1.PrincipalPKInfo);

			var port2 = ports.AddNew();
			port2.Enabled = true;
			port2.Port = "NZAKL";
			AssertHasError(port2.PortInfo, "The same Port cannot be duplicated for the same Principal.");
			AssertHasError(port2.PrincipalPKInfo, "The same Port cannot be duplicated for the same Principal.");

			port2.PrincipalPK = orgProxy.PK;
			AssertNoErrors(port1.PortInfo);
			AssertNoErrors(port1.PrincipalPKInfo);
		}

		public void TestValidateSenderID()
		{
			var port = new PortMessagingPort();
			port.Enabled = true;
			port.SenderID = string.Empty;
			port.ValidateSenderID();
			AssertHasError(port.SenderIDInfo, "Sender ID is required.");
			port.SenderID = "ABLI";
			AssertNoErrors(port.SenderIDInfo);
		}

		public void TestDefaultValue()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UPS"))
			{
				var port = new PortMessagingPort();

				AssertEquals(false, port.Enabled);
				AssertEquals("UPS", port.SenderID);
			}
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
				return true;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortMessagingPort GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortMessagingPort GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PortMessagingPort originalBusinessObject, PortMessagingPort newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("Port", originalBusinessObject.Port, newBusinessObject.Port);
			AssertEquals("Version", originalBusinessObject.SenderID, newBusinessObject.SenderID);
			AssertEquals("ProdEmail", originalBusinessObject.Enabled, newBusinessObject.Enabled);
		}

		PortMessagingPort GetNewPopulatedBusinessObject()
		{
			var collection = new PortMessagingPortCollection();
			var port = collection.AddNew();
			port.Port = "NZAKL";
			port.SenderID = "HCLK";
			port.Enabled = true;
			return port;
		}
		#endregion
	}
}
