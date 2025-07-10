using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeclarationDocManagerInfo))]
	class USDeclarationDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.NewWithValidTestData<JobDeclaration>();
		}

		public void TestIDocManagerSupportRelatedObjects()
		{
			var declaration = Factory.New<JobDeclaration>();
			var inBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inBondHeader.BH_ParentID = declaration.PK;
			inBondHeader.BH_ParentTableCode = declaration.TablePrefix;
			var docManagerInfo = declaration.DocManagerInfo;
			AssertType<USDeclarationDocManagerInfo>(docManagerInfo);
			AssertCollectionContains("Should have InBondHeader", inBondHeader, docManagerInfo.RelatedObjects);
		}
	}
}
