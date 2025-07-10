using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class LinksManagerTest : TestCaseWithFactory
	{
		public void TestAddLinkFromBizO()
		{
			var links = new LinksManager();
			links.AddLink(LinksManager.LinkType.Container, null, null);
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, null));
			links.AddLink(LinksManager.LinkType.Container, 1, null);
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, 1));
			var bizO1 = Factory.New<DummyEnterpriseBusinessObject>();
			links.AddLink(LinksManager.LinkType.Container, 1, bizO1);
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 1));
			AssertEquals(bizO1.PK, links[LinksManager.LinkType.Container, 1]);
			var bizO2 = Factory.New<DummyEnterpriseBusinessObject>();
			links.AddLink(LinksManager.LinkType.Container, 2, bizO2);
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 2));
			AssertEquals(bizO2.PK, links[LinksManager.LinkType.Container, 2]);
		}

		public void TestAddLinkFromZGuid()
		{
			var links = new LinksManager();
			links.AddLink(LinksManager.LinkType.Container, null, ZGuid.Empty);
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, null));
			links.AddLink(LinksManager.LinkType.Container, 1, ZGuid.Empty);
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, 1));
			var guid1 = ZGuid.NewZGuid();
			links.AddLink(LinksManager.LinkType.Container, 1, guid1);
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 1));
			AssertEquals(guid1, links[LinksManager.LinkType.Container, 1]);
			var guid2 = ZGuid.NewZGuid();
			links.AddLink(LinksManager.LinkType.Container, 3, guid2);
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 3));
			AssertEquals(guid2, links[LinksManager.LinkType.Container, 3]);
		}

		public void TestIndexer_ThrowsException()
		{
			var links = new LinksManager();
			ZGuid key;
			AssertExceptionThrown(typeof(KeyNotFoundException), () => key = links[LinksManager.LinkType.Container, null]);
			AssertExceptionThrown(typeof(KeyNotFoundException), () => key = links[LinksManager.LinkType.Container, 1]);
		}

		public void TestContainsLink()
		{
			var links = new LinksManager();
			var guid1 = ZGuid.NewZGuid();
			links.AddLink(LinksManager.LinkType.Container, 1, guid1);
			var guid2 = ZGuid.NewZGuid();
			links.AddLink(LinksManager.LinkType.Container, 3, guid2);
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 1));
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, 2));
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 3));
		}
	}
}
