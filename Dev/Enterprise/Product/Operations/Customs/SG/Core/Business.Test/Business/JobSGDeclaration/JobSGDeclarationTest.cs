using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobSGDeclaration))]
	class JobSGDeclarationUniqueIndexTest : IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporterTestCase<JobSGDeclaration>
	{
		protected override string ExpectedUniqueIndexName => JobSGDeclarationSchema.Constants.Indexes.FK_UX__SGE_JE;

		protected override SchemaIntColumn ExpectedClusterKeyColumn => JobSGDeclarationSchema.SGE_ClusterKey;

		protected override string ExpectedUniqueClusterIndexName => JobSGDeclarationSchema.Constants.Indexes.NR_UC__SGE_ClusterKey;

		protected override EnterpriseBusinessObject GetParent(JobSGDeclaration bizObj) => bizObj.Parent;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.NewWithValidTestData<JobDeclaration>().AddInfoChild;
	}

	[TestedType(typeof(JobSGDeclaration))]
	public class JobSGDeclarationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity() => Factory.New<JobDeclaration>().AddInfoChild;

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<JobDeclaration>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;
	}
}
