using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class DataGroupingRelatedFilterControl : ZUserControl
	{
		public DataGroupingRelatedFilterControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var dataSource = DataSource as DataGroupingRelatedFilter;
			if (dataSource != null)
			{
				Property2FindBox.CaptionResourceString = dataSource.Property2ResourceString;
				Property2DropEdit.CaptionResourceString = dataSource.Property2ResourceString;
				switch (dataSource.Property2FieldType)
				{
					case FieldType.TextCodeFindBox:
						Property2FindBox.Enabled = true;
						Property2FindBox.Visible = true;
						Property2DropEdit.Enabled = false;
						Property2DropEdit.Visible = false;
						break;
					case FieldType.TextDropEdit:
						Property2FindBox.Enabled = false;
						Property2FindBox.Visible = false;
						Property2DropEdit.Enabled = true;
						Property2DropEdit.PreBoundMaxLength = dataSource.Property2MaxLength;
						Property2DropEdit.Visible = true;
						break;
					default:
						Property2FindBox.Enabled = false;
						Property2FindBox.Visible = false;
						Property2DropEdit.Enabled = false;
						Property2DropEdit.Visible = false;
						break;
				}
			}
		}
	}
}
