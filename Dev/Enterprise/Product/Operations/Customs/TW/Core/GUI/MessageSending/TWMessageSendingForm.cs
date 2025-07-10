using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.GUI
{
	public abstract partial class TWMessageSendingForm : MessageSendingObjectForm
	{
		protected TWMessageSendingForm()
		{
		}

		protected TWMessageSendingForm(BaseMessageSendingObjectParent declarationWrapper)
		: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		protected virtual void InitializeNewColumns()
		{
		}

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			MessageSendingNotificationCollection result;
			var businessEntity = BusinessEntity;
			var header = TopBusinessObject;
			if (header == null)
			{
				result = base.RunPreSendValidation();
			}
			else
			{
				var decIsTopLevel = header.IsTopLevel;
				try
				{
					header.IsTopLevel = false;
					if (ShouldRunPreSendValidation)
					{
						businessEntity.RegisterEditableChildObject(header);
					}
					result = base.RunPreSendValidation();
				}
				finally
				{
					header.IsTopLevel = decIsTopLevel;
					if (ShouldRunPreSendValidation)
					{
						businessEntity.UnRegisterEditableChildObject(header);
					}
				}
			}
			return result;
		}

		public virtual BusinessObject TopBusinessObject => BusinessEntity;

		protected virtual ZBool ShouldRunPreSendValidation => ZBool.True;

		public static class Constants
		{
			public static string TestingEnvironment => Res.GetString("9CB31447-DB4A-4DF9-9C0F-000CA4EC1609", "The selected messages will be sent to a testing environment");
		}

		protected override bool CheckIsOKToSend()
		{
			bool result = true;
			if (TWCustomsDataRegistry.IsTestMode && ShouldRunPreSendValidation)
			{
				if (Globals.Message.Show(Constants.TestingEnvironment, Res.GetString("3892569A-59D6-466E-8FEF-9F931E5ADAE1", "Continue to Send"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
				{
					result = false;
				}
			}
			if (result)
			{
				result = base.CheckIsOKToSend();
			}
			return result;
		}
	}
}
