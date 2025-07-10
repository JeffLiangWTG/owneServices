using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.Testing
{
	sealed class ECIWriteOffFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertEquals("MessageTypeList of correct type", typeof(Common.NZ.NZJobMessageTypeList), lookups.MessageTypeList.GetType());
		}

		public void TestMessageSubTypeList()
		{
			AssertEquals("MessageSubTypeList of correct type", typeof(JobMessageSubTypeList), lookups.MessageSubTypeList().GetType());
		}

		public void TestEntryStatusList()
		{
			AssertEquals("EntryStatusList of correct type", typeof(LowValueConsignmentStatusList), lookups.EntryStatusList().GetType());
		}

		public void TestContainerModeList()
		{
			AssertEquals("ContainerModeList of correct type", typeof(ContainerModeList), lookups.ContainerModeList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new CUSCARFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		CUSCARFilterBusinessObject filterBizObj;
		ECIWriteOffFilterLookups lookups;
	}
}
