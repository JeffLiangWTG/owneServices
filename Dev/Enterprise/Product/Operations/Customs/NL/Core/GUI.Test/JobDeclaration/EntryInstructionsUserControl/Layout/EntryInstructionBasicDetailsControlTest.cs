using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryInstructionBasicDetailsControlTest : TestCase
{
	public void TestTransNatureDropEdit()
	{
		AssertType<ZDropEdit>(control.TransNatureDropEdit);
	}

	public void TestIsHighValueOvrdCheckBox()
	{
		AssertType<ZCheckBox>(control.IsHighValueOvrdCheckBox);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryInstructionBasicDetailsControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	EntryInstructionBasicDetailsControl control;
}
