using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IPropertyValue
	{
		string PropertyName { get; }
		IZType Value { get; }
	}
}
