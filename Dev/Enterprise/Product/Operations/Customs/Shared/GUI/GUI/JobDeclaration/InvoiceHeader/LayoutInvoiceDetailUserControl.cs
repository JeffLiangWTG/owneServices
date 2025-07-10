using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class LayoutInvoiceDetailUserControl : ZUserControl
	{
		public LayoutInvoiceDetailUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var currentDeclaration = CurrentDataItem as BaseJobDeclaration;
			base.SetDataBinding(dataSource, dataMember);
			var declaration = GetDeclaration(dataSource);
			var layoutProvider = GUI.DeclarationFormLayoutProvider.GetLayoutProvider(declaration);
			if (currentLayoutProvider != layoutProvider || declaration != currentDeclaration)
			{
				currentLayoutProvider = layoutProvider;
				var layout = currentLayoutProvider?.GetCommercialInvoiceDetailsLayout(declaration);
				dynamicDetailsPanel.UpdateLayout(layout);
			}
		}
		IDeclarationFormLayoutProvider currentLayoutProvider;

		BaseJobDeclaration GetDeclaration(object dataSource)
		{
			BaseJobDeclaration result = null;
			if (dataSource is BaseJobDeclaration declaration)
			{
				result = declaration;
			}
			return result;
		}
	}
}

