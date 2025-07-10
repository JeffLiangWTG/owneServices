using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContUnpackTime()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var cont = header.Containers.AddNew();
			cont.ContUnpackTime = ZDateTime.Today;
			Factory.Save();
			cont = new BusinessObjectFactory().Load<AsycudaContainer>(cont.PK);
			AssertEquals(ZDateTime.Today, cont.ContUnpackTime);
		}

		public void TestGateInOutDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			var container = header.Containers.AddNew();
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			container.Factory.Save();
			container = new BusinessObjectFactory().Load<AsycudaContainer>(container.PK);
			AssertEquals(ZDateTime.BrettsBirthday, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.Empty;
			container.Factory.Save();
			container = new BusinessObjectFactory().Load<AsycudaContainer>(container.PK);
			AssertEquals(ZDateTime.Empty, container.GateInOutDate);
		}

		public void TestGateInOutDate_Cleanup()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			header.AMA_TransportMode = ZString.Empty;
			AssertEquals(ZDateTime.Empty, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			header.AMA_TransportMode = Constants.TransportModes.Air;
			AssertEquals(ZDateTime.Empty, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			header.AMA_TransportMode = Constants.TransportModes.Mail;
			AssertEquals(ZDateTime.Empty, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			header.AMA_TransportMode = Constants.TransportModes.Road;
			AssertEquals(ZDateTime.Empty, container.GateInOutDate);
			container.GateInOutDate = ZDateTime.BrettsBirthday;
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZDateTime.BrettsBirthday, container.GateInOutDate);
		}

		public void TestHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var cont = header.Containers.AddNew();
			AssertEquals(header, cont.Header);
		}

		public void TestDelete()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header1.Bills.AddNew();
			var cont1 = header1.Containers.AddNew();
			var pack1 = bill1.Packs.AddNew();
			cont1.Delete();
			Factory.Save();
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "2222";
			var bill2 = header2.Bills.AddNew();
			var cont2 = header2.Containers.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.ContainerPK = cont2.PK;
			cont2.Delete();
			Factory.Save();
			AssertEquals(0, Factory.Load<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, pack2.PK)).Length);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Containers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
