using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class LayoutEntryInstructionDetailBasicUserControl : ZUserControl
	{
		public LayoutEntryInstructionDetailBasicUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetDetailsLayout();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			UnHookValueChangeEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookValueChangeEvents();
		}

		void HookValueChangeEvents()
		{
			if (DataSource is BaseJobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageType_ValueChanged;
			}
		}

		void UnHookValueChangeEvents()
		{
			if (DataSource is BaseJobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageType_ValueChanged;
			}
		}

		void JE_MessageType_ValueChanged(object sender, EventArgs e) => SetDetailsLayout();

		void SetDetailsLayout()
		{
			if (CurrentDataItem is BaseJobDeclaration declaration)
			{
				var layoutProvider = DeclarationFormLayoutProvider.GetLayoutProvider(declaration);
				var layout = layoutProvider?.GetInstructionDetailsLayoutProvider(declaration);
				dynamicDetailsPanel.UpdateLayout(layout);
				UpdateControlsAfterLayoutUpdated(declaration);
			}
		}

		protected virtual void UpdateControlsAfterLayoutUpdated(BaseJobDeclaration declaration)
		{
		}
	}
}
