using System.IO;
using System.Text;
using System.Xml.XPath;
using NUnit.Framework;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class MessageFactResolverTests
	{
		[Test]
		public void TestGetMessageType_NoNamespace_Root()
		{
			var message = "<GlobalElectronicInvoicing><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("GlobalElectronicInvoicing", result);
		}

		[Test]
		public void TestGetMessageType_NamespaceWithoutAlias_NamespaceHashRoot()
		{
			var message = "<GlobalElectronicInvoicing xmlns=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing", result);
		}

		[Test]
		public void TestGetMessageType_NamespaceWithPrefix_NamespaceHashRoot()
		{
			var message = "<ns1:GlobalElectronicInvoicing xmlns:ns1=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></ns1:GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing", result);
		}

		[Test]
		public void TestGetMessageType_NoNamespaceWithOtherPrefix_Root()
		{
			var message = "<GlobalElectronicInvoicing xmlns:ns1=\"http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing\"><Payload>Hello</Payload></GlobalElectronicInvoicing>";

			var result = ActOnGetMessageType(message);

			StringAssert.StartsWith("GlobalElectronicInvoicing", result);
		}

		public string ActOnGetMessageType(string message)
		{
			var msgAsByteArray = Encoding.UTF8.GetBytes(message);
			var messageFactResolver = new MessageFactResolver(msgAsByteArray);
			XPathDocument xpDoc;
			
			using (var ms = new MemoryStream(msgAsByteArray))
			{
				xpDoc = new XPathDocument(ms);
			}

			return messageFactResolver.GetMessageType(xpDoc);
		}
	}
}