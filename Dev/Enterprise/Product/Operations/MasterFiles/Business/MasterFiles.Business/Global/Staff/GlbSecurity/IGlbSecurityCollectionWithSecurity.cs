using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	interface IGlbSecurityCollectionWithSecurity : IReadOnlyFactoryForSecurityValidationProvider
	{
		SecurityCore Security { get; }
		ISecurityMap SecurityMap { get; }
	}
}
