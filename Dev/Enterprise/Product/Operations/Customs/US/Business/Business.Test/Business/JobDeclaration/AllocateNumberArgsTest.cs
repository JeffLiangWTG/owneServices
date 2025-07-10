using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class AllocateNumberArgsTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 8;
			args.EntryFilerCode = "XJ5";
			AssertEquals("MaxLength", 8, args.MaxLength);
			AssertEquals("EntryFilerCode", "XJ5", args.EntryFilerCode);
			AssertEquals("Branch", null, args.Branch);
		}
	}
}
