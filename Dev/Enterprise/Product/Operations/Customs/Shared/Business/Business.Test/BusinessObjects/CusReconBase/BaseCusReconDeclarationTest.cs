using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconBase.CusReconDeclaration))]
	class BaseCusReconDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusReconDeclarationTypeDecider>(CusReconDeclaration.TypeDecider);
		}
	}
}
