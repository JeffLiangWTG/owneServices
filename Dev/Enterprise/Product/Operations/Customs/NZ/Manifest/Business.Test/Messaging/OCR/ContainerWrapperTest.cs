using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class ContainerWrapperTest : TestCaseWithFactory
	{
		public void TestContainerWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "Test123";
			container.ACN_Seal1 = "TestSeal1";
			container.ACN_Seal2 = "TestSeal2";
			container.ACN_Seal3 = "TestSeal3";
			container.ACN_SealingPartyName = "TestSealingParty";
			container.ACN_StowageLocation = "LOC123";
			var wrapper = new ContainerWrapper(container);
			AssertEquals("Test123", wrapper.ContainerNumber);
			AssertEquals(ZString.Empty, wrapper.ContainerMode);
			AssertEquals(ContainerStatusList.Codes.C7, wrapper.Status);
			AssertEquals(ZString.Empty, wrapper.Size);
			AssertEquals("TestSeal1", wrapper.SealNumbers.ElementAt(0));
			AssertEquals("TestSeal2", wrapper.SealNumbers.ElementAt(1));
			AssertEquals("TestSeal3", wrapper.SealNumbers.ElementAt(2));
			AssertEquals("TestSealingParty", wrapper.SealingParty);
			AssertEquals(ZString.Empty, wrapper.AttachedEquipmentCode);
			AssertEquals("LOC123", wrapper.StowPosition);
			AssertEquals(false, wrapper.IsPallet);
			AssertEquals(container.PK, wrapper.PK);
		}
	}
}
