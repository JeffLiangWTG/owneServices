using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(SailingValueObjectDataAdapter))]
	public class SailingValueObjectDataAdapterTest_Sea : SailingValueObjectDataAdapterTestCase
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		[ExpectNoExceptions]
		public void TestNoMaxLengthExceededExceptionThrowOnVoyageAndVessel()
		{
			ZString voyage = "1111111111111111111111111111111111111111111111111";
			ZString vessel = "asdfadsfkashdfkhaksdfkjdhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh";
			SailingValueObjectDataAdapter newAdapter = new SailingValueObjectDataAdapter
				("SEA", "AUSYD", "SGSIN", vessel, voyage, ZGuid.Empty);
		}
	}
}
