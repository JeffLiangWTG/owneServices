using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ManufacturerQueryForm : ZChildForm
	{
		public ManufacturerQueryForm(USMIDQuery messageData)
			: base(messageData)
		{
		}

		public new USMIDQuery BusinessEntity
		{
			get { return (USMIDQuery)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Manufacturer Name And Address Query"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public static bool CanSend(USMIDQuery entity)
		{
			if (entity.HasErrors)
			{
				Globals.Message.Show(
					"Please fix the following error to continue." + System.Environment.NewLine + entity.GetErrors().ToUniqueMessageListString(),
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			else if (entity.HasMessageErrors || entity.HasWarnings)
			{
				var warningsAndMessageErrors =
					entity.GetMessageErrors().Concat(
					entity.GetWarnings());
				return GetConfirmationWithThisWarning(warningsAndMessageErrors.ToUniqueMessageListString()) == DialogResult.Yes;
			}
			else
			{
				return true;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public bool IsOKToSendMessage;
		void SendButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			IsOKToSendMessage = CanSend(BusinessEntity);
			if (IsOKToSendMessage)
			{
				Close();
			}
		}

		static DialogResult GetConfirmationWithThisWarning(string warningMessage)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + warningMessage + "\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}

		void GiveUpButton_Click(object sender, EventArgs e)
		{
			IsOKToSendMessage = false;
			Close();
		}
	}
}
