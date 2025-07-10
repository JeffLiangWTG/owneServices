using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class SendingActionForm : ZChildForm
	{
		public SendingActionForm(IShipmentActionsProvider provider, ZString messageDescription)
			: base(provider)
		{
			this.messageDescription = messageDescription;
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return messageDescription; }
		}

		public override string FormVerb
		{
			get { return "Send"; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			using (var validation = new Validation((IShipmentActionsProvider)BusinessEntity))
			{
				if (validation.CheckAtLeastOneShipmentSelected()
					&& validation.CheckNoValidationErrors())
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		readonly ZString messageDescription;

		#region Validation

		internal
		sealed class Validation : NonPersistentBusinessObject, IDisposable
		{
			internal Validation(IShipmentActionsProvider provider)
			{
				this.provider = provider;
				RegisterEditableChildObject(provider.ShipmentsActions);
			}

			internal bool CheckAtLeastOneShipmentSelected()
			{
				var result = provider.ShipmentsActions.Cast<ShipmentAction>().Any(a => !a.B0_ActionCode.IsEmpty);
				if (!result)
				{
					Globals.Message.ShowError(Res.GetString("0449059f-c9b7-4f22-9af2-4200cd356703", "No shipments have been selected for sending."), CannotSendMessage);
				}

				return result;
			}

			internal bool CheckNoValidationErrors()
			{
				var notifications = MessageSendingValidation.New(this, null).CheckBusinessObjectLevelValidation();
				var result = !notifications.ContainsError();
				if (!result)
				{
					Globals.Message.ShowError(notifications.NotificationsAsString(), CannotSendMessage);
				}

				return result;
			}

			public void Dispose()
			{
				UnRegisterEditableChildObject(provider.ShipmentsActions);
			}

			static string CannotSendMessage
			{
				get { return Res.GetString("f1694286-37cb-4bec-a1a0-9531733f7dd0", "Cannot Send Message"); }
			}

			readonly IShipmentActionsProvider provider;
		}

		#endregion
	}
}
