using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass()]
	public class XmlHelperTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void XmlHelper_TestGetWithOverridesElement()
		{
			var assm = Assembly.GetExecutingAssembly();
			var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.uXML_3.xml")).CreateNavigator();
			var xnsm = new XmlNamespaceManager(xnav.NameTable);
			xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
			var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg/s0:PortOfLoading", xnsm);

			var result = new XmlHelper().GetWithOverrides(input);
            
			var actualValues = new List<string>();   
			while (result.MoveNext())
			{
				actualValues.Add(result.Current.Value);
			}
			CollectionAssert.AreEqual(new[] { "AUBRI", "AUSYD", "JPOSA" }, actualValues);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void XmlHelper_TestGetWithOverridesAttribute()
		{
			var assm = Assembly.GetExecutingAssembly();
			var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.uXML_3.xml")).CreateNavigator();
			var xnsm = new XmlNamespaceManager(xnav.NameTable);
			xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
			var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:TransportLegCollection/s0:TransportLeg/s0:PortOfDischarge/@Name", xnsm);

			var result = new XmlHelper().GetWithOverrides(input);

			var actualValues = new List<string>();
			while (result.MoveNext())
			{
				actualValues.Add(result.Current.Value);
			}
			CollectionAssert.AreEqual(new[] { "Sao Paulo", "Port of Osaka", "Khabarovsk" }, actualValues);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void XmlHelper_TestGetWithOverridesWithMissingNodeElement()
        {
            var assm = Assembly.GetExecutingAssembly();
            var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.Test12_OverrideMissingNode_input.xml")).CreateNavigator();
            var xnsm = new XmlNamespaceManager(xnav.NameTable);
            xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
            var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress", xnsm);

            var result = new XmlHelper().GetWithOverrides(input, "CompanyName");

            var actualValues = new List<string>();
            while (result.MoveNext())
            {
                actualValues.Add(result.Current.Value);
            }
            CollectionAssert.AreEqual(new[] { "Overriden ComponyName Not Mediterranean Shipping Company", "DSV AIR & SEA S.A.U. - R47F Overriden", "MSC Mediterranean Shipping Company", "DSV Air & Sea Ltd. - I701", "SAME AS CONSIGNEE" }, actualValues);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void XmlHelper_TestGetWithOverridesWithMissingNodeAttribute()
        {
            var assm = Assembly.GetExecutingAssembly();
            var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.Test12_OverrideMissingNode_input.xml")).CreateNavigator();
            var xnsm = new XmlNamespaceManager(xnav.NameTable);
            xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
            var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress", xnsm);

            var result = new XmlHelper().GetWithOverrides(input, "CompanyName/@Name");

            var actualValues = new List<string>();
            while (result.MoveNext())
            {
                actualValues.Add(result.Current.Value);
            }
            CollectionAssert.AreEqual(new[] { "Test Overriden", "Test Not Overriden" }, actualValues);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void XmlHelper_TestGetWithOverridesWithNotExistingAttribute()
        {
            var assm = Assembly.GetExecutingAssembly();
            var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.Test12_OverrideMissingNode_input.xml")).CreateNavigator();
            var xnsm = new XmlNamespaceManager(xnav.NameTable);
            xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
            var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress", xnsm);

            var result = new XmlHelper().GetWithOverrides(input, "Nothing/@Name");

            var actualValues = new List<string>();
            while (result.MoveNext())
            {
                actualValues.Add(result.Current.Value);
            }
            CollectionAssert.Equals(0, actualValues.Count);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void XmlHelper_TestGetWithOverridesWithNotExistingElement()
        {
            var assm = Assembly.GetExecutingAssembly();
            var xnav = new XPathDocument(assm.GetManifestResourceStream(assm.GetName().Name + ".TestFiles.Test12_OverrideMissingNode_input.xml")).CreateNavigator();
            var xnsm = new XmlNamespaceManager(xnav.NameTable);
            xnsm.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2012/11");
            var input = xnav.Select("/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress", xnsm);

            var result = new XmlHelper().GetWithOverrides(input, "Nothing");

            var actualValues = new List<string>();
            while (result.MoveNext())
            {
                actualValues.Add(result.Current.Value);
            }
            CollectionAssert.Equals(0, actualValues.Count);
        }
	}
}
