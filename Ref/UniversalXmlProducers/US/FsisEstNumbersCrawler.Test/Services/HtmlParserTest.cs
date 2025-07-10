using System.Net;
using FsisEstNumbersCrawler.Services;
using HtmlAgilityPack;
using NUnit.Framework;

namespace FsisEstNumbersCrawler.Test.Services
{
	[TestFixture]
	public class HtmlParserTest
	{
		static readonly HtmlParser Parser = new HtmlParser();

		[Test]
		public void FindNodeShouldReturnNullIfNotFound()
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var url = Utilities.CurrentFolder() + "\\Resources\\WiseTechGlobal.html";
				var node = Parser.FindNode(url, htmlNode => htmlNode.Name == "span", webClient);
				Assert.Null(node);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}

		[Test]
		public void FindNodeShouldWork()
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var url = Utilities.CurrentFolder() + "\\Resources\\WiseTechGlobal.html";
				var node = Parser.FindNode(url, htmlNode => htmlNode.Name == "h1", webClient);
				Assert.True(node.InnerText == "About WiseTech Global");
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}

		[Test]
		public void GetAttributeValueShouldReturnNullIfNotFound()
		{
			var html = "<p>Hello World!</p>";
			var node = HtmlNode.CreateNode(html);
			var id = Parser.GetAttributeValue(node, "id");
			Assert.Null(id);
		}

		[Test]
		public void GetAttributeValueShouldWork()
		{
			var html = "<p id=\"slogan\">Hello World!</p>";
			var node = HtmlNode.CreateNode(html);
			var id = Parser.GetAttributeValue(node, "id");
			Assert.AreEqual("slogan", id);
		}

		[Test]
		public void IsExcelNode()
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var url = Utilities.CurrentFolder() + "\\Resources\\Meat, Poultry and Egg Product Inspection Directory _ Food Safety and Inspection Service.html";
				var node = Parser.FindNode(url, Program.IsDataFileNode, webClient);
				Assert.NotNull(node);

				url = Utilities.CurrentFolder() + "\\Resources\\Meat, Poultry and Egg Product Inspection Directory _ Food Safety and Inspection Service.html";
				node = Parser.FindNode(url, Program.IsDataFileNode, webClient);
				Assert.NotNull(node);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}

		[Test]
		public void IsDateNode()
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var url = Utilities.CurrentFolder() + "\\Resources\\Meat, Poultry and Egg Product Inspection Directory _ Food Safety and Inspection Service.html";
				var node = Parser.FindNode(url, Program.IsDateNode, webClient);
				Assert.NotNull(node);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}

		[Test]
		public void FindNodeSetsCookie()
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				var url = Utilities.CurrentFolder() + "\\Resources\\Meat, Poultry and Egg Product Inspection Directory _ Food Safety and Inspection Service.html";

				var node = Parser.FindNode(url, Program.IsDateNode, webClient);
				Assert.AreNotEqual("", webClient.Headers.Get("cookies"));
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}
	}
}
