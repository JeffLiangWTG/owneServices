using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.Business.Testing
{
	public class SGV4ClassificationDataTest : TestCaseWithFactory
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(Customs.Business.BaseCusClassification), new SGV4ClassificationData().BusinessObjectType);
		}

		public void TestCollectionType()
		{
			AssertType<ClassificationCollection>("CollectionType", new SGV4ClassificationData().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals("ReferenceType", Core.Constants.DocManagerCodes.Classification, new SGV4ClassificationData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Classification", new SGV4ClassificationData().HumanReadableName.GetUnresolvedValue());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			Assert("IsAllowedForUnallocatedeDocs", new SGV4ClassificationData().IsAllowedForUnallocatedeDocs);
		}

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.Customs.SG.SG4Classification, new SGV4ClassificationData().ModuleID);
		}
	}
}
