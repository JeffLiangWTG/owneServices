using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class PromptDeclarationForm : BasePromtForm
	{
		public PromptDeclarationForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public PromptDeclarationForm()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				if (DataSource is IBusiness business)
				{
					business.RunPreSaveValidation();
					TabPageNotificationsExposer.ExposeTabPageNotifications(this, business);
				}
			}
		}
	}
}
