using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ImportExportTest : TestCase
	{
		public void TestDefaultDirection()
		{
			AssertEquals(Directions.Unknown, new Directions());
		}
	}
}
