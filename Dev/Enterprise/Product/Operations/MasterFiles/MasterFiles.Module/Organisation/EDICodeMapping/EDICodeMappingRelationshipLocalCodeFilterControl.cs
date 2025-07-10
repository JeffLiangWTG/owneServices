using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class EDICodeMappingRelationshipLocalCodeFilterControl : ZUserControl
	{
		public EDICodeMappingRelationshipLocalCodeFilterControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Filter != null)
			{
				SetVisibility(Filter.Relationship);
			}
		}

		EDICodeMappingRelationshipLocalCodeModuleFilter Filter => DataSource as EDICodeMappingRelationshipLocalCodeModuleFilter;

		void SetVisibility(string relationship)
		{
			LocalGuidFindBox.Visible = EDICodeMappingHelper.IsGuidFindBox(relationship);
			LocalCodeDropEdit.Visible = EDICodeMappingHelper.IsDropDownFindBox(relationship);
			LocalCodeFindBox.Visible = EDICodeMappingHelper.IsCodeFindBox(relationship);
		}

		void RelationshipDropEdit_TextChanged(object sender, EventArgs e)
		{
			SetVisibility(RelationshipDropEdit.Text);
		}
	}
}
