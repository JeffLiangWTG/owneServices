using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ZDropEditSizeBehaviourTest : TestCaseWithFactory
{
	public void TestUpdateBehaviour()
	{
		var businessObject = Factory.New<JobDeclaration>();
		using (var zDropEdit = new ZDropEdit())
		{
			zDropEdit.PreBoundMaxLength = 1;
			zDropEdit.ShouldResizeByMaxLength = true;
			zDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			zDropEdit.AllowDrop = true;

			CombineAssertions(() =>
			{
				var previousDropEditSize = zDropEdit.Size;
				AssertEquals("base zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);

				var behaviour = new ZDropEditSizeBehaviour(4);
				behaviour.UpdateBehaviour(zDropEdit, businessObject);
				AssertEquals("after PreBoundMaxLength", 4, zDropEdit.PreBoundMaxLength);
				AssertEquals("after ShouldResizeByMaxLength", false, zDropEdit.ShouldResizeByMaxLength);
				AssertEquals("after zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);
			});
		}
	}
}
