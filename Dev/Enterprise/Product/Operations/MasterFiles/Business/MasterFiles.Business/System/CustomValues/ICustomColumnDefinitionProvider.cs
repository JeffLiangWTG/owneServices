using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public interface ICustomColumnDefinitionProvider
	{
		ICustomColumnDefinition GetCustomColumnDefinition();
	}
}
