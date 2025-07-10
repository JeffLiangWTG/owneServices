using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSContainer))]
	public class SPTSContainerTest : Customs.Business.Testing.CusInBondContainerTest<SPTSContainer>
	{
		public void TestBC_ContainerNumMaxLength()
		{
			var sptsContainer = Factory.New<SPTSContainer>();
			AssertEquals("Max Length", 20, sptsContainer.BC_ContainerNumInfo.MaxLength);
		}

		public void TestRemoveEmptyContainer()
		{
			var header = Factory.New<SPTSHeader>();
			var container = header.HeaderContainers.AddNew();
			container.BC_ContainerNum = "CNT00003";
			Factory.Save();

			var newFactory = NewFactory();
			var reLoadHeader = newFactory.Load<SPTSHeader>(header.PK);
			AssertEquals(1, reLoadHeader.HeaderContainers.Count);

			container = reLoadHeader.HeaderContainers[0];
			container.BC_ContainerNum = ZString.Empty;
			newFactory.Save();

			newFactory = NewFactory();
			reLoadHeader = newFactory.Load<SPTSHeader>(header.PK);
			AssertEquals(0, reLoadHeader.HeaderContainers.Count);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<SPTSHeader>();
			var container = factory.New<SPTSContainer>();
			header.HeaderContainers.Add(container);
			container.BC_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
			container.BC_ContainerNum = "Test";
			return container;
		}
	}
}
