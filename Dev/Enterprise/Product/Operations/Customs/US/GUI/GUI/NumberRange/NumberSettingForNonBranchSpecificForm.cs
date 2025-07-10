using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class NumberSettingForNonBranchSpecificForm : ZChildForm
	{
		public NumberSettingForNonBranchSpecificForm()
		{
		}

		public NumberSettingForNonBranchSpecificForm(NumberSetting numberSetting)
			: base(numberSetting)
		{
			Argument.NotNull(numberSetting, "numberSetting");
		}

		public new NumberSetting BusinessEntity
		{
			get { return (NumberSetting)base.BusinessEntity; }
		}

		#region Overrides

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return "InBond Number Settings"; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		public static string ChangingNumberCanCauseCustomsRejectDuplication(ZDecimal nextNumber)
		{
			return string.Format(CultureInfo.InvariantCulture, "Changing the Number can potentially cause Customs to reject the message if the Number is duplicated.\n\nAre you sure you want to set the next Number to '{0}'?", nextNumber);
		}

		void SetNextNumberButton_Click(object sender, EventArgs e)
		{
			try
			{
				BusinessEntity.RunPreSaveValidation();
				ZStringBuilder builder = new ZStringBuilder(BusinessEntity.NextNumberInfo.Notifications.GetNotifications(NotificationType.Error).GetUniqueMessageList());
				ZString errorMessage = builder.ToStringWithNewLineBetweenAppends();

				if (errorMessage.IsEmpty)
				{
					string message = ChangingNumberCanCauseCustomsRejectDuplication(BusinessEntity.NextNumber);

					if (Globals.Message.ShowConfirmation(message, "Set Next Number", "yes", MessageBoxIcon.Warning) == DialogResult.OK)
					{
						BusinessEntity.PostNextNumber();
					}
				}
				else
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleSaveException(ex);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
