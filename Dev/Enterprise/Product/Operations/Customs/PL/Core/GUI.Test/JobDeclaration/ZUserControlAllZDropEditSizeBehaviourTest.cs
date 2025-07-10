using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ZUserControlAllZDropEditSizeBehaviourTest : TestCaseWithFactory
{
	public void TestResizeZDropEdit()
	{
		var businessObject = Factory.New<JobDeclaration>();
		using (var userControl = new ZUserControl())
		{
			userControl.Size = new System.Drawing.Size(231, 20);
			var zdropEdit = new ZDropEdit();
			zdropEdit.PreBoundMaxLength = 1;
			zdropEdit.ShouldResizeByMaxLength = true;
			zdropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			zdropEdit.AllowDrop = true;
			userControl.Controls.Add(zdropEdit);

			CombineAssertions(() =>
			{
				var previousDropEditSize = zdropEdit.Size;
				AssertEquals("base userControl Size", new System.Drawing.Size(231, 20), userControl.Size);
				AssertEquals("base PreBoundMaxLength", 1, zdropEdit.PreBoundMaxLength);
				AssertEquals("base ShouldResizeByMaxLength", true, zdropEdit.ShouldResizeByMaxLength);
				AssertEquals("base Location", new System.Drawing.Point(0, 0), zdropEdit.Location);

				var behaviour = new ZUserControlAllZDropEditSizeBehaviour();
				behaviour.UpdateBehaviour(userControl, businessObject);
				AssertEquals("after userControl Size", new System.Drawing.Size(255, 20), userControl.Size);
				AssertEquals("after PreBoundMaxLength", 5, zdropEdit.PreBoundMaxLength);
				AssertEquals("after ShouldResizeByMaxLength", false, zdropEdit.ShouldResizeByMaxLength);
				AssertEquals("after Location", new System.Drawing.Point(0, 0), zdropEdit.Location);

				AssertNotEquals("Size difference", new System.Drawing.Size(0, 0), zdropEdit.Size - previousDropEditSize);
			});
		}
	}

	public void TestResizeZDropEditWithLocationUpdate()
	{
		var businessObject = Factory.New<JobDeclaration>();
		using (var userControl = new ZUserControl())
		{
			userControl.Size = new System.Drawing.Size(231, 20);
			var zDropEdit = AddZDropEditToUserControl(userControl);
			var zTextBox = AddZTextBoxToUserControl(userControl);

			CombineAssertions(() =>
			{
				var previousDropEditSize = zDropEdit.Size;
				AssertEquals("base userControl Size", new System.Drawing.Size(231, 20), userControl.Size);
				AssertEquals("base zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);

				var behaviour = new ZUserControlAllZDropEditSizeBehaviour();
				behaviour.UpdateBehaviour(userControl, businessObject);
				AssertEquals("after userControl Size", new System.Drawing.Size(255, 20), userControl.Size);
				AssertEquals("after PreBoundMaxLength", 5, zDropEdit.PreBoundMaxLength);
				AssertEquals("after ShouldResizeByMaxLength", false, zDropEdit.ShouldResizeByMaxLength);
				AssertEquals("after zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);
				AssertEquals("after zTextBox Size", new System.Drawing.Size(50, 20), zTextBox.Size);
				AssertEquals("after zTextBox Location", new System.Drawing.Point(24, 0), zTextBox.Location);
			});
		}
	}

	public void TestMultipleZDropEdit()
	{
		var businessObject = Factory.New<JobDeclaration>();
		using (var userControl = new ZUserControl())
		{
			userControl.Size = new System.Drawing.Size(231, 20);
			var zDropEdit = AddZDropEditToUserControl(userControl);
			var zDropEdit2 = AddZDropEditToUserControl(userControl);

			CombineAssertions(() =>
			{
				var previousDropEditSize = zDropEdit.Size;
				AssertEquals("base userControl Size", new System.Drawing.Size(231, 20), userControl.Size);
				AssertEquals("base zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);
				AssertEquals("base zDropEdit2 Location", new System.Drawing.Point(0, 0), zDropEdit2.Location);

				var behaviour = new ZUserControlAllZDropEditSizeBehaviour();
				behaviour.UpdateBehaviour(userControl, businessObject);
				AssertEquals("after userControl Size", new System.Drawing.Size(279, 20), userControl.Size);
				AssertEquals("after zDropEdit PreBoundMaxLength", 5, zDropEdit.PreBoundMaxLength);
				AssertEquals("after zDropEdit ShouldResizeByMaxLength", false, zDropEdit.ShouldResizeByMaxLength);
				AssertEquals("after zDropEdit Location", new System.Drawing.Point(0, 0), zDropEdit.Location);
				AssertEquals("after zDropEdit2 PreBoundMaxLength", 5, zDropEdit2.PreBoundMaxLength);
				AssertEquals("after zDropEdit2 ShouldResizeByMaxLength", false, zDropEdit2.ShouldResizeByMaxLength);
				AssertEquals("after zDropEdit2 Location", new System.Drawing.Point(24, 0), zDropEdit2.Location);
			});
		}
	}

	static ZDropEdit AddZDropEditToUserControl(ZUserControl userControl)
	{
		var zdropEdit = new ZDropEdit();
		zdropEdit.PreBoundMaxLength = 1;
		zdropEdit.ShouldResizeByMaxLength = true;
		zdropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
		zdropEdit.AllowDrop = true;
		userControl.Controls.Add(zdropEdit);

		return zdropEdit;
	}

	static ZTextBox AddZTextBoxToUserControl(ZUserControl userControl)
	{
		var zTextBox = new ZTextBox();
		zTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
		zTextBox.AllowDrop = true;
		userControl.Controls.Add(zTextBox);

		return zTextBox;
	}
}
