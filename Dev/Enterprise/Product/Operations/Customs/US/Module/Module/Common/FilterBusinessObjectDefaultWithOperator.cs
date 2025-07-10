using CargoWise.Types;

namespace Enterprise.Customs.US.Module
{
	class FilterBusinessObjectDefaultWithOperator
	{
		public FilterBusinessObjectDefaultWithOperator(ZString filterName, ZString propertyName, IZType value, ZString comparisonOperator)
		{
			FilterName = filterName;
			PropertyName = propertyName;
			Value = value;
			ComparisonOperator = comparisonOperator;
		}

		internal ZString FilterName { get; }

		internal ZString PropertyName { get; }

		internal IZType Value { get; }

		internal ZString ComparisonOperator { get; }
	}
}
