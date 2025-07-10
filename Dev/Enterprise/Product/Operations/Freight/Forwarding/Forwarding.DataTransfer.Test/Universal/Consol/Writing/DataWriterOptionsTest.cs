using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class DataWriterOptionsTest : TestCase
	{
		public void TestDefaults()
		{
			var options = new DataWriterOptions();
			Assert(options.IncludeContainers);
			Assert(options.IncludeSubShipments);
		}
	}
}
