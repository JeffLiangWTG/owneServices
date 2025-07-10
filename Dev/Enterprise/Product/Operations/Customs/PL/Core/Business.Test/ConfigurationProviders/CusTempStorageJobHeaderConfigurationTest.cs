using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusTempStorageJobHeaderConfiguration))]
class CusTempStorageJobHeaderConfigurationTest : EU.Business.Testing.CusTempStorageJobHeaderConfigurationAbstractTest<CusTempStorageJobHeaderConfiguration>
{
	protected override bool IsUCC6_Expected => true;
}
