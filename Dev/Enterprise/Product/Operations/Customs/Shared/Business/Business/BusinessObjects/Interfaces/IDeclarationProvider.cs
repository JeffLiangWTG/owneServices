using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IDeclarationProvider
	{
		BaseJobDeclaration Declaration { get; }
		BusinessObjectFactory Factory { get; }
	}
}
