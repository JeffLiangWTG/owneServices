using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EDICodeMappingUserControl : ZUserControl
	{
		public EDICodeMappingUserControl()
		{
			InitializeComponent();
			SetVisibility();
		}

		void SetVisibility()
		{
			LocalCodeFindBox.Visible = EDICodeMappingHelper.IsCodeFindBox(RelationshipDropEdit.Text);
			LocalGuidFindBox.Visible = EDICodeMappingHelper.IsGuidFindBox(RelationshipDropEdit.Text);
			LocalCodeDropEdit.Visible = EDICodeMappingHelper.IsDropDownFindBox(RelationshipDropEdit.Text);
		}

		void RelationshipTextBox_TextChanged(object sender, EventArgs e)
		{
			SetVisibility();
		}
	}
}
