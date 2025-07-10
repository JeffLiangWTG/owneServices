using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class ContainersForm : ZTemplateForm, IButtonPostTextOverride
	{
		public ContainersForm(CommonContainer businessEntity)
			: base(businessEntity)
		{
			MainTabPage.RunWhenBindingOrFirstShown(delegate
			{
				ContainersControl.SetupPlugIn();
				ContainersControl.CurrentContainer = businessEntity;
			});
			WorkflowTabPage.Initialize(businessEntity);
		}

		CommonContainer ContainerBO
		{
			get { return (CommonContainer)DataSource; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("23896F67-2CF1-401C-9574-73AEB53596FA", "Container") + (ContainerBO != null ? " " + ContainerBO.JC_ContainerNum : string.Empty); }
		}

		#region IButtonPostTextOverride Members

		string IButtonPostTextOverride.PostButtonText
		{
			get { return IsPostOnly ? Res.GetString("Freight|ContainersForm|ButtonSave", "S&ave") : string.Empty; }
		}

		#endregion
	}
}
