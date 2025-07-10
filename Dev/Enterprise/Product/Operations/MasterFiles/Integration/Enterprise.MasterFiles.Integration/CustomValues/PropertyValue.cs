using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public class PropertyValue : IPropertyValue
	{
		public PropertyValue(string propertyName, IZType value)
		{
			PropertyName = propertyName;
			Value = value;
		}

		public string PropertyName { get; private set; }
		public IZType Value { get; private set; }
	}
}
