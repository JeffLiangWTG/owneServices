using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing;

[TestedType(typeof(EntryInstructionContainersUserControl))]
sealed class EntryInstructionContainersUserControlTest : TestCaseWithFactory
{
	public void TestColumnsInfos()
	{
		using var userControl = new EntryInstructionContainersUserControl();

		var controlGrid = (ZGrid)userControl.Controls.Find("ContainersGrid", searchAllChildren: true).Single();
		var containerNumberColumn = controlGrid.GetColumnStyle("ContainerNumber");
		var isForEntryColumn = controlGrid.GetColumnStyle("IsForEntry");

		CombineAssertions("Column Readonly check", () =>
		{
			Assert("ContainerNumber column is readonly", containerNumberColumn.IsReadOnly);
			Assert("IsForEntry column is not readonly", !isForEntryColumn.IsReadOnly);
		});
	}
}
