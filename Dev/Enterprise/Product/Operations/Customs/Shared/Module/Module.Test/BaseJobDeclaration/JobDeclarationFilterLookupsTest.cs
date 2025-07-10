using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : CommonFilterLookupsTest
	{
		public void TestDepartmentList()
		{
			AssertEquals("DepartmentList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.DepartmentList).IsLoaded);
			AssertEquals("DepartmentList of correct type", typeof(GlbDepartmentCollection), lookups.DepartmentList.GetType());
		}
		public void TestServiceTypeList()
		{
			AssertEquals("ServiceTypeList", service.Lookups.JobServiceType_List, lookups.ServiceTypeList);
		}

		public void TestEntryTypeList()
		{
			AssertNotNull("EntryTypeList should exist so it can be overridden", lookups.EntryTypeList);
		}

		public void TestContainerModeList()
		{
			AssertEquals("ContainerModeList", declaration.Lookups.CargoIdTypeList, lookups.ContainerModeList);
		}

		public void TestTransportTypeList()
		{
			AssertEquals("TransportTypeList of correct type", typeof(TransportTypeList), lookups.TransportTypeList.GetType());
		}

		public void TestServiceLevelList()
		{
			AssertEquals("ServiceLevelList of correct type", typeof(ActiveServiceLevelCollection), lookups.ServiceLevelList.GetType());
		}

		public void TestMessageTypeList()
		{
			AssertEquals("MessageTypeList", typeof(JobMessageTypeList), lookups.MessageTypeList.GetType());
		}

		public void TestMessageSubTypeList()
		{
			AssertNotNull("MessageSubTypeList should exist so it can be overridden", lookups.MessageSubTypeList());
		}

		public void TestApplicationCodeList()
		{
			AssertSame("ApplicationCodeList is cached", lookups.ApplicationCodeList(), filterBizObj.Factory.GetCachedValue<DeclarationApplicationCodeList>());
		}

		public void TestDropModeList()
		{
			AssertType("DropModeList", typeof(CombinedEquipmentNeededList), lookups.DropModeList);
			AssertSame("DropModeList is cached", lookups.DropModeList, filterBizObj.Factory.GetCachedValue<CombinedEquipmentNeededList>());
		}

		protected override FilterStripBusinessObject GetFilterStripBusinessObject()
		{
			return new JobDeclarationFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = new JobDeclarationFilterLookups(filterBizObj);
			declaration = Factory.GetNull<BaseJobDeclaration>();
			service = Factory.GetNull<JobService>();
		}

		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
		BaseJobDeclaration declaration;
		JobService service;
	}
}
