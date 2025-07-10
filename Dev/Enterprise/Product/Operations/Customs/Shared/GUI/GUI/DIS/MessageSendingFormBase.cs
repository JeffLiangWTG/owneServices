using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public abstract partial class MessageSendingFormBase<T1, T2> : ZChildForm where T1 : NonPersistentBusinessObject, IMessageSendingActionBase
		where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		protected MessageSendingFormBase(MessageSendingActionCollectionBase<T1, T2> coll) : base(coll)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return (NoResString)"Send DIS Messages"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public new MessageSendingActionCollectionBase<T1, T2> BusinessEntity
		{
			get { return (MessageSendingActionCollectionBase<T1, T2>)base.BusinessEntity; }
		}

		public bool ProceedWithSend
		{
			get;
			private set;
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			ProceedWithSend = false;

			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.Cast<T1>().Any(x => x.HasAnyDocumentsToSend))
			{
				Globals.Message.ShowError(Res.GetString("236F9F31-7875-41D9-B788-6847014EAC36",
					"Please indicate which of the Documents in the grid you want to submit to Customs."));
			}
			else if (BusinessEntity.Cast<T1>().Any(x => x.HasErrors))
			{
				Globals.Message.ShowError(Res.GetString("D66719D3-FB23-41F0-820B-9A8696F228D1", "There are errors. You are not able to send the message(s)."));
			}
			else if (!BusinessEntity.HasMessageErrors() ||
					 Globals.Message.Show(Res.GetString("13E083B9-CED3-4CE0-86D6-CCA641BA2422", "There are notifications. Are you sure you wish to continue?"),
						 Res.GetString("40826881-FF60-4672-9D59-B53BEB4D0190", "Continue to send"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) ==
					 System.Windows.Forms.DialogResult.OK)
			{
				ProceedWithSend = true;
				Close();
			}
		}

		void CancelButton1_Click(object sender, EventArgs e)
		{
			ProceedWithSend = false;
			Close();
		}
	}
}
