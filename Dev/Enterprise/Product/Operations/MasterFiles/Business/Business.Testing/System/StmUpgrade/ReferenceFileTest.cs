using CargoWise.Types;
using Enterprise.MasterFiles.Business.AU.CMR;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ReferenceFileTest : TestCase
	{
		public void TestFile()
		{
			ReferenceFile file = new ReferenceFile("foo", ZBlob.FromAscii("totally."));
			AssertEquals(".Name", "foo", file.Name);
			AssertEquals(".Data", ZBlob.FromAscii("totally."), file.Data);
		}
	}
}
