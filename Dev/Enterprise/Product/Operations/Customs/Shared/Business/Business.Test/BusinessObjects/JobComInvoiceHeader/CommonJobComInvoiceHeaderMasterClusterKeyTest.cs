using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommonJobComInvoiceHeader))]
	sealed class CommonJobComInvoiceHeaderMasterClusterKeyTest : ClusterKeyMasterMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity() => Factory.New<BaseJobComInvoiceHeader>();
	}
}
