using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	interface IReadOnlyFactoryForSecurityValidationProvider
	{
		BusinessObjectFactory ReadOnlyFactory { get; }
	}
}
