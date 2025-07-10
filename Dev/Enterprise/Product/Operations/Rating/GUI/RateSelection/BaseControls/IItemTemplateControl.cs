using System;
using System.Windows.Forms;

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	public interface IItemTemplateControl
	{
		void DataBind(object data);
		DockStyle Dock { get; set; }
		bool IsSelected { get; set; }
		void ClearSelection();
		EventHandler SelectionChanged { get; set; }
	}
}
