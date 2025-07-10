using System;

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	public class ItemTemplateControlBase : ViewModelBasedControl, IItemTemplateControl
	{
		public ItemTemplateControlBase()
		{
		}

		public virtual bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				if (value != isSelected)
				{
					isSelected = value;
					SelectionChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}
		bool isSelected;

		public EventHandler SelectionChanged { get; set; }

		public virtual void DataBind(object data)
		{
			if (data != null)
			{
				SetDataBinding(data, BindingSource.DataMember);
			}
		}

		public virtual void ClearSelection()
		{
			isSelected = false;
		}
	}
}
