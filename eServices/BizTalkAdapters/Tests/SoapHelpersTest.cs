using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.BizTalk.Streaming;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass]
	public class SoapHelpersTest
	{
		public const string SoapEnvelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body></s:Body></s:Envelope>";
		public const string SoapEnvelopeWithHeader = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><ActivityId CorrelationId=""0311cc8d-7fce-47ef-bef1-0b7fd84c4a6d"" xmlns=""http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics"">d1c0853f-a668-4882-91c0-eca1c99b6a11</ActivityId></s:Header><s:Body></s:Body></s:Envelope>";
		public const string SoapFaultWithEnvelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><s:Fault><s:Code><s:Value>s:Sender</s:Value><s:Subcode><s:Value>ActionNotSupported</s:Value></s:Subcode></s:Code><s:Reason><s:Text xml:lang=""en-AU"">The message with Action 'blabla' cannot be processed at the receiver.</s:Text></s:Reason></s:Fault></s:Body></s:Envelope>";
		public const string SoapFaultWithoutEnvelope = @"<s:Fault xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Code><s:Value>s:Sender</s:Value><s:Subcode><s:Value>ActionNotSupported</s:Value></s:Subcode></s:Code><s:Reason><s:Text xml:lang=""en-AU"">The message with Action 'blabla' cannot be processed at the receiver.</s:Text></s:Reason></s:Fault>";

		[TestMethod]
		public void WrapBodyInEnvelope()
		{
			var bodyXML = new XDocument();
			bodyXML.Add(CreateTestBodyXML());

			using (var bodyStream = new VirtualStream())
			{
				bodyXML.Save(bodyStream);
				bodyStream.Position = 0;

				using (var envelopedStream = new VirtualStream())
				{
					SoapHelpers.WrapBodyInEnvelope(bodyStream, envelopedStream);
					envelopedStream.Position = 0;
					XDocument resultXML = XDocument.Load(envelopedStream);
					XDocument emptyEnvelope = XDocument.Parse(SoapEnvelope);
					Assert.IsTrue(ElementsAreEqual(emptyEnvelope.Root, resultXML.Root, false));
					var resultSoapBodyWrapperElement = (XElement)resultXML.Root.FirstNode;
					Assert.IsTrue(ElementsAreEqual((XElement)emptyEnvelope.Root.FirstNode, resultSoapBodyWrapperElement, false));
					Assert.IsTrue(ElementsAreEqual(bodyXML.Root, (XElement)resultSoapBodyWrapperElement.FirstNode, true));
				}
			}
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UnwrapBodyFromEnvelope()
		{
			XDocument wrappedXML = XDocument.Parse(SoapEnvelopeWithHeader);
			var soapBodyElement = (XElement)wrappedXML.Root.FirstNode.NextNode;
			var rootElement = CreateTestBodyXML();
			soapBodyElement.Add(rootElement);

			using (var wrappedStream = new VirtualStream())
			{
				wrappedXML.Save(wrappedStream);
				wrappedStream.Position = 0;

				using (var bodyStream = SoapHelpers.UnwrapBodyFromEnvelope(wrappedStream, "/*"))
				{
					XDocument resultXML = XDocument.Load(bodyStream);
					Assert.IsTrue(ElementsAreEqual(resultXML.Root, rootElement, true));
				}
			}
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UnwrapBodyPathFromEnvelope()
		{
			XDocument wrappedXML = XDocument.Parse(SoapEnvelopeWithHeader);
			var soapBodyElement = (XElement)wrappedXML.Root.FirstNode.NextNode;
			var rootElement = CreateTestBodyXML();
			soapBodyElement.Add(rootElement);

			using (var wrappedStream = new VirtualStream())
			{
				wrappedXML.Save(wrappedStream);
				wrappedStream.Position = 0;

				using (var bodyStream = SoapHelpers.UnwrapBodyFromEnvelope(wrappedStream, "/Root/Child1"))
				{
					XDocument resultXML = XDocument.Load(bodyStream);
					Assert.IsTrue(ElementsAreEqual(resultXML.Root, (XElement)rootElement.FirstNode, true));
				}
			}
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetHeaderFromEnvelope()
		{
			XDocument wrappedXML = XDocument.Parse(SoapEnvelopeWithHeader);
			var soapBodyElement = (XElement)wrappedXML.Root.FirstNode.NextNode;
			var rootElement = CreateTestBodyXML();
			soapBodyElement.Add(rootElement);

			using (var wrappedStream = new VirtualStream())
			{
				wrappedXML.Save(wrappedStream);
				wrappedStream.Position = 0;

				var actualHeaderElement = SoapHelpers.GetHeaderFromEnvelope(wrappedStream);
				var expectedHeaderElement = new XElement("Headers", ((XElement)wrappedXML.Root.FirstNode).FirstNode);
				Assert.IsTrue(ElementsAreEqual(actualHeaderElement, expectedHeaderElement, true));
			}
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UnwrapFaultFromEnvelope()
		{
			XDocument soapFaultXML = XDocument.Parse(SoapFaultWithEnvelope);
			var soapFaultElement = (XElement)((XElement)soapFaultXML.Root.FirstNode).FirstNode;

			using (var soapFaultStream = new VirtualStream())
			{
				soapFaultXML.Save(soapFaultStream);
				soapFaultStream.Position = 0;

				using (var bodyStream = SoapHelpers.UnwrapBodyFromEnvelope(soapFaultStream, "/*"))
				{
					XDocument resultXML = XDocument.Load(bodyStream);
					XDocument soapFaultwithoutEnvelopeXML = XDocument.Parse(SoapFaultWithoutEnvelope);
					Assert.IsTrue(ElementsAreEqual(resultXML.Root, soapFaultwithoutEnvelopeXML.Root, true));
				}
			}
		}

		[TestMethod]
		public void ContainsEnvelopeGivenSoapFault()
		{
			XDocument soapFaultXML = XDocument.Parse(SoapFaultWithEnvelope);

			using (var soapFaultStream = new VirtualStream())
			{
				soapFaultXML.Save(soapFaultStream);
				soapFaultStream.Position = 0;

				Assert.IsTrue(SoapHelpers.ContainsEnvelope(soapFaultStream));
			}
		}

		[TestMethod]
		public void ContainsEnvelopeGivenEmptyStream()
		{
			using (var soapFaultStream = new VirtualStream())
			{
				Assert.IsFalse(SoapHelpers.ContainsEnvelope(soapFaultStream));
			}
		}

		[TestMethod]
		public void ContainsEnvelopeGivenXMLWithNoEnvelope()
		{
			var bodyXML = new XDocument();
			bodyXML.Add(CreateTestBodyXML());

			using (var bodyStream = new VirtualStream())
			{
				bodyXML.Save(bodyStream);
				bodyStream.Position = 0;

				Assert.IsFalse(SoapHelpers.ContainsEnvelope(bodyStream));
			}
		}

		XElement CreateTestBodyXML()
		{
			var leaf1 = new XElement("Leaf1", new XAttribute("Leaf1Attribute", "Leaf1AttributeValue"));
			var child1 = new XElement("Child1", leaf1, new XAttribute("Child1Attribute", "Child1AttributeValue"));
			var leaf2 = new XElement("Leaf2", new XAttribute("Leaf2Attribute", "Leaf2AttributeValue"));
			var child2 = new XElement("Child2", leaf2, new XAttribute("Child2Attribute", "Child2AttributeValue"));
			return new XElement("Root", child1, child2, new XAttribute("RootAttribute", "RootAttributeValue"));
		}

		public static bool ElementsAreEqual(XElement element1, XElement element2, bool compareChildren)
		{
			if (element1.Name != element2.Name)
			{
				return false;
			}

			if ((element1.HasAttributes != element2.HasAttributes) || (!AttributeCollectionsAreEqual(element1.Attributes(), element2.Attributes())))
			{
				return false;
			}

			if (!compareChildren)
			{
				return true;
			}

			return ElementCollectionsAreEqual(element1.Elements(), element2.Elements());
		}

		static bool AttributesAreEqual(XAttribute attribute1, XAttribute attribute2)
		{
			return (attribute1.Name == attribute2.Name) && (attribute1.Value == attribute2.Value);
		}

		static bool AttributeCollectionsAreEqual(IEnumerable<XAttribute> attributes1, IEnumerable<XAttribute> attributes2)
		{
			int count = attributes1.Count();
			if (count != attributes2.Count())
			{
				return false;
			}

			int matches = 0;
			foreach (var attribute1 in attributes1)
			{
				foreach (var attribute2 in attributes2)
				{
					if (AttributesAreEqual(attribute1, attribute2))
					{
						++matches;
					}
				}
			}

			return (count == matches);
		}

		static bool ElementCollectionsAreEqual(IEnumerable<XElement> elements1, IEnumerable<XElement> elements2)
		{
			int count = elements1.Count();
			if (count != elements2.Count())
			{
				return false;
			}

			int matches = 0;
			foreach (var element1 in elements1)
			{
				foreach (var element2 in elements2)
				{
					if (ElementsAreEqual(element1, element2, true))
					{
						++matches;
					}
				}
			}

			return (count == matches);
		}
	}
}
