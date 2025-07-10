using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(SailingValueObjectDataAdapter))]
	sealed class SailingValueObjectDataAdapterTest_Air : SailingValueObjectDataAdapterTestCase
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Air; }
		}
	}
}
