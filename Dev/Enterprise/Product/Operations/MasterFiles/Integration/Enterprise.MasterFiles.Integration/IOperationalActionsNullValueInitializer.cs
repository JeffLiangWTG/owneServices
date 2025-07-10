using System.Reflection;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOperationalActionsNullValueInitializer
	{
		void CreateValueIfNull(PropertyInfo info);
	}
}
