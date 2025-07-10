using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	sealed class BaseJobDeclarationMasterClusterKeyTest : ClusterKeyMasterMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity() => Factory.New<BaseJobDeclaration>();
	}
}
