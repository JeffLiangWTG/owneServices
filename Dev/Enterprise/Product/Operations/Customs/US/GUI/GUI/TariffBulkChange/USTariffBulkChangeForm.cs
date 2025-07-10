using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USTariffBulkChangeForm : ZChildForm
	{
		public USTariffBulkChangeForm(USTariffBulkChange businessEntity)
			: base(businessEntity)
		{
		}

		public USTariffBulkChange TariffChanger
		{
			get { return base.BusinessEntity as USTariffBulkChange; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void ContinueBtn_Click(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.Show("Are you sure you want to continue?", "Continue", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (result == DialogResult.OK)
			{
				TariffChanger.RunPreSaveValidation();
				if (TariffChanger.HasErrors)
				{
					IEnumerable<INotification> collector = new ZNotificationCollector(TariffChanger, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
					Globals.Message.Show("Error(s) found.\r\n" + collector.ToUniqueMessageListString() + "\r\nPlease fix error(s) first and then update tariffs.", "Error(s)", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				using (ProgressForm form = new ProgressForm())
				{
					form.ShowProgressBar = true;
					form.TopMost = true;

					form.Cancelled += delegate
					{ TariffChanger.Cancel(); };
					form.Show();
					TariffChanger.ProgressChanged += new USTariffBulkChange.TariffBulkChangedEventHandler(delegate(int percentage)
					{ form.Status = TariffChanger.Status; form.PercentComplete = percentage; form.Refresh(); });
					TariffChanger.Update();
				}

				string message = string.Format(@"{0} Product(s) changed.

{1} Lookup(s) changed.", TariffChanger.ProductsChanged.ToString().Trim(), TariffChanger.ClassificationsChanged.ToString().Trim());

				result = Globals.Message.Show(message, "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Close();
			}
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Prevent base class method from asking the user if they are sure they want to close
		}
	}
}
