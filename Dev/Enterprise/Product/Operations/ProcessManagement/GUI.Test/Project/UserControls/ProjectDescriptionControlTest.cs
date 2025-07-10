using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectDescriptionControlTest : TestCaseWithFactory
	{
		public void TestDetailNoteRichTextBox_InnerTextBoxIsSameWidthAsOuterTextBox()
		{
			using (var control = new ProjectDescriptionControlForTest())
			using (var outerTextBox = control.DetailNoteRichTextBox_Exposed)
			using (var innerTextBox = outerTextBox.GetRichTextBoxForTest())
			{
				int outerTextBoxWidth = outerTextBox.Width;
				int innerTextBoxWidth = innerTextBox.Width;
				AssertEquals(outerTextBoxWidth, innerTextBoxWidth);
			}
		}

		class ProjectDescriptionControlForTest : ProjectDescriptionControl
		{
			public ZRichTextBox DetailNoteRichTextBox_Exposed
			{
				get { return DetailNoteRichTextBox; }
			}
		}
	}
}
