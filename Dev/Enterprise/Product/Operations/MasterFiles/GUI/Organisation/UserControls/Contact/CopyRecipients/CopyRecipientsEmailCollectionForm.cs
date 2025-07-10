using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CopyRecipientsEmailCollectionForm : ZChildForm
	{
		public CopyRecipientsEmailCollectionForm(IBusinessObjectCollection copyRecipients, string emailAddressPropertyName) : base(copyRecipients)
		{
			CopyRecipients = copyRecipients;
			EmailAddressPropertyName = emailAddressPropertyName;
			if (emailAddressPropertyName != null)
			{
				zDropEditColumnStyleInfo1.ColumnName = emailAddressPropertyName;

				if (copyRecipients != null)
				{
					BindingSource.SetBindingMember(Grid_CopyRecipients, ".");
					DataSourceAssemblyName = copyRecipients.GetType().Assembly.GetName().Name;
					DataSourceType = copyRecipients.GetType();
					DataSourceTypeName = copyRecipients.GetType().FullName;
					Grid_CopyRecipients.SetDataBinding(copyRecipients, string.Empty);
				}
			}
		}

		#region Public Methods

		public static void ShowDialog(IBusinessObjectCollection copyRecipients, string emailAddressPropertyName)
		{
			ZFormModaliser.ShowDialogAndDispose(new CopyRecipientsEmailCollectionForm(copyRecipients, emailAddressPropertyName));
		}

		#endregion

		#region Properties

		public IBusinessObjectCollection CopyRecipients { get; private set; }

		public string EmailAddressPropertyName { get; private set; }

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Implementations

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Button_Close.Focus();
			if (CopyRecipients.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("CopyRecipientsEmailCollectionForm|E423B7DB-D265-4116-B20E-3BBF67BE8D88", "Please fix the errors before closing."));
				e.Cancel = true;
			}
			base.OnClosing(e);
		}

		void Button_Ok_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
