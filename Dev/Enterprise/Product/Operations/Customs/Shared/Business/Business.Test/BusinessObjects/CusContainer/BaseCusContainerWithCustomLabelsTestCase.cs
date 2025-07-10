using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseCusContainer))]
	public abstract class BaseCusContainerWithCustomLabelsTestCase : BusinessObjectWithCustomLabelsTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			return declaration.CusContainers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			return declaration.CusContainers.AddNew();
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			return new BaseCusContainer.CustomLabelsProvider(((BaseCusContainer)bO).Declaration);
		}
	}
}
