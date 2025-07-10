using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Business.Testing
{
	class NZClassificationDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals("BusinessObjectType", typeof(Customs.Business.BaseCusClassification), new NZClassificationData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<Customs.Business.BaseClassificationCollection<CusClassification>>("CollectionType", new NZClassificationData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.DocManagerCodes.Classification, new NZClassificationData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Classification", new NZClassificationData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new NZClassificationData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.SingleTariffClassification, new NZClassificationData().ModuleID);
		}
	}
}
