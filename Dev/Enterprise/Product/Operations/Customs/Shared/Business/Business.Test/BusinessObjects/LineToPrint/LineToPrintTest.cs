using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LineToPrintTest : TestCaseWithFactory
	{
		public void TestDefaultShouldBePrinted()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var line = new DummyLineToPrint(bizObj);
			Assert(line.ShouldBePrinted);
			bizObj.Z0_Date = ZDateTime.Now;
			line = new DummyLineToPrint(bizObj);
			Assert(!line.ShouldBePrinted);
		}

		class DummyLineToPrint : LineToPrint
		{
			public DummyLineToPrint(DummyBusinessObject bizObj)
				: base(bizObj)
			{
			}

			public override ZString Organisation
			{
				get { return "XXX"; }
			}

			public override ZString Identifier
			{
				get { return "YYY"; }
			}

			public override ZDateTime LastPrintDate
			{
				get { return ((DummyBusinessObject)bizObj).Z0_Date; }
			}
		}
	}
}
