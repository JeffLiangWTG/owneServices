using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthorityPort))]
	internal sealed class PortAuthorityPortTest : RegistryBusinessObjectTemplateTestCase<PortAuthorityPort>
	{
		public void TestValidatePort()
		{
			Port1.Port = "";
			AssertHasError("Empty Port", Port1.PortInfo, "Please enter a Port.");
			Port1.Port = "XXX";
			AssertHasError("Invalid Port", Port1.PortInfo, "Enter a valid Port.");
			Port1.Port = "AUBNE";
			AssertNoErrors("Valid Port", Port1.PortInfo);
			Port2.Port = "AUBNE";
			AssertHasError("Duplicate Port", Port2.PortInfo, "A port may only appear once in this list.");

			Port1.Port = "NZAKL";
			AssertHasError(Port1.PortInfo, "Please enter a UNLOCO for an Australian port.");
		}

		public void TestValidateVersion()
		{
			Port1.Version = "";
			AssertHasError("Empty Version", Port1.VersionInfo, "Please enter a Version.");
			Port1.Version = PortAuthorityVersionList.Codes.V20;
			AssertNoErrors("Valid Version", Port1.VersionInfo);
			Port1.Version = "XXX";
			AssertHasError("Invalid Version", Port1.VersionInfo, "Enter a valid Version.");
		}

		public void TestValidateProductionEmail()
		{
			Port1.ProductionEmail = "";
			AssertHasError("Empty ProductionEmail", Port1.ProductionEmailInfo, "Please enter a Production Email.");
			Port1.ProductionEmail = "Bob@FreadNet.org";
			AssertNoErrors("Valid ProductionEmail", Port1.ProductionEmailInfo);
			Port1.ProductionEmail = "XXX";
			AssertHasError("Invalid ProductionEmail", Port1.ProductionEmailInfo, "Please enter a valid email address.");
		}

		public void TestValidateProductionID()
		{
			Port1.ProductionID = "";
			AssertHasError("Empty Recipient ProductionID", Port1.ProductionIDInfo, "Please enter a Production Recipient ID.");
			Port1.ProductionID = "Blat";
			AssertNoErrors("ValProductionID Recipient ProductionID", Port1.ProductionIDInfo);
		}

		public void TestValidateTestingEmail()
		{
			Port1.TestingEmail = "";
			AssertNoErrors("Empty TestingEmail", Port1.TestingEmailInfo);
			Port1.TestingEmail = "Bob@FreadNet.org";
			AssertNoErrors("Valid TestingEmail", Port1.TestingEmailInfo);
			Port1.TestingEmail = "XXX";
			AssertHasError("Invalid TestingEmail", Port1.TestingEmailInfo, "Please enter a valid email address.");
		}

		#region Implementation
		PortAuthorityPortCollection Collection
		{
			get
			{
				return collection ?? (collection = new PortAuthorityPortCollection());
			}
		}
		PortAuthorityPortCollection collection;
		PortAuthorityPort Port1
		{
			get
			{
				if (port1 == null)
				{
					port1 = Collection.AddNew();
				}
				return port1;
			}
		}

		PortAuthorityPort port1;
		PortAuthorityPort Port2
		{
			get
			{
				if (port2 == null)
				{
					port2 = Collection.AddNew();
				}
				return port2;
			}
		}
		PortAuthorityPort port2;

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

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortAuthorityPort GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortAuthorityPort GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PortAuthorityPort originalBusinessObject, PortAuthorityPort newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("Port", originalBusinessObject.Port, newBusinessObject.Port);
			AssertEquals("Version", originalBusinessObject.Version, newBusinessObject.Version);
			AssertEquals("ProdEmail", originalBusinessObject.ProductionEmail, newBusinessObject.ProductionEmail);
			AssertEquals("ProdID", originalBusinessObject.ProductionID, newBusinessObject.ProductionID);
			AssertEquals("TestEmail", originalBusinessObject.TestingEmail, newBusinessObject.TestingEmail);
			AssertEquals("TestID", originalBusinessObject.TestingID, newBusinessObject.TestingID);
		}

		PortAuthorityPort GetNewPopulatedBusinessObject()
		{
			PortAuthorityPort port = new PortAuthorityPort();
			port.Port = "AUFRE";
			port.Version = PortAuthorityVersionList.Codes.V11;
			port.ProductionEmail = "manifest@fremantleports.com.au";
			port.ProductionID = "Freemantle Ports";
			port.TestingEmail = "manitest@fremantleports.com.au";
			port.TestingID = "FP Test";
			return port;
		}

		#endregion
	}
}
