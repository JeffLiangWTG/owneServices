using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CustomFieldsWrapperControl : ZUserControl
	{
		public CustomFieldsWrapperControl()
		{
			InitializeComponent();
		}

		public string NothingSetupMessageLabelText
		{
			get => ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText;
			set => ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = value;
		}

		#region DataItemChanged

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (CurrentDataItem is ICustomFieldParent customFieldsParent)
			{
				customFieldsParent.OnResetCustomBusinessObject = null;
			}

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem is ICustomFieldParent customFieldsParent)
			{
				customFieldsParent.OnResetCustomBusinessObject = UpdateCustomFieldsControlBinding;
			}
			UpdateCustomFieldsControlBinding();
		}

		void UpdateCustomFieldsControlBinding()
		{
			ProcessTemplateCustomFieldsControl.SetDataBinding(CurrentDataItem, string.Empty);
		}

		#endregion
	}
}
