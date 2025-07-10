using System.Reflection;

namespace Enterprise.MasterFiles.Integration
{
	public interface IPropertyChecker
	{
		bool IsPropertyUpdatableViaXueAdditionalFields(PropertyInfo propertyInfo, object proposedValue, out string errorMessage);
	}
}
