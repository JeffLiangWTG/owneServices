using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobUSDeclaration))]
	sealed class JobUSDeclarationTest : IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterTestCase<JobUSDeclaration>
	{
		public void TestIJobUSDeclarationIsCorrectlySetup()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.US.IJobUSDeclaration>();
			AssertType<JobUSDeclaration>(data);
			AssertType<JobUSDeclaration>(Factory.Load(data.TablePrefix, data.PK));
		}

		public void TestMakeNonPersistent()
		{
			var declaration = Factory.New<JobUSDeclaration>();
			declaration.HasChanges = true;
			Assert("Should be saved", declaration.IsSavedByFactory);
			declaration.MakeNonPersistent();
			Assert("Should no longer be saved", !declaration.IsSavedByFactory);
		}

		public void TestIsPersistent()
		{
			var declaration = Factory.New<JobUSDeclaration>();
			Assert("Default is persistent", declaration.IsPersistent);
			declaration.MakeNonPersistent();
			Assert("Now non-persistent", !declaration.IsPersistent);
		}

		protected override CargoWise.Schema.SchemaIntColumn ExpectedClusterKeyColumn => JobUSDeclarationSchema.USD_ClusterKey;

		protected override string ExpectedUniqueClusterIndexName => JobUSDeclarationSchema.Constants.Indexes.NR_UC__USD_ClusterKey;

		protected override string ExpectedUniqueIndexName => JobUSDeclarationSchema.Constants.Indexes.FK_UX__USD_JE;

		protected override EnterpriseBusinessObject GetParent(JobUSDeclaration bizObj) => bizObj.Declaration;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.USDeclaration;
		}
	}
}
