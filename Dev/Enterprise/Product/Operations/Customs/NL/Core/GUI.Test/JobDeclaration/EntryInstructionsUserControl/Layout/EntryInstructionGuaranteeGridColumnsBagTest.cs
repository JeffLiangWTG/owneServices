using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing
{
	sealed class EntryInstructionGuaranteeGridColumnsBagTest : TestCaseWithFactory
	{
		public void TestBondTypeDropEditColumnCaption()
		{
			var columnInfo = ColumnsBag.BondTypeDropEditColumn.CreateGridColumnInfo();

			AssertEquals("CaptionResourceString", Res.GetData("8A2822F9-DD39-4FF6-8FA0-649C9CA1E6C3", "[UCC 8/2] Type"), columnInfo.CaptionResourceString);
		}

		public void TestBondNumberDropEditColumnCaption()
		{
			var columnInfo = ColumnsBag.BondNumberMultiControlColumn.CreateGridColumnInfo() as ZMultiControlColumnStyleInfo;

			AssertEquals("CaptionResourceString", Res.GetData("8A2822F9-DD39-4FF6-8FA0-649C9CA1E6C2", "[UCC 8/3] Reference"), columnInfo.CaptionResourceString);
		}

		EntryInstructionGuaranteeGridColumnsBag ColumnsBag => EntryInstructionGuaranteeGridColumnsBag.Instance;
	}
}
