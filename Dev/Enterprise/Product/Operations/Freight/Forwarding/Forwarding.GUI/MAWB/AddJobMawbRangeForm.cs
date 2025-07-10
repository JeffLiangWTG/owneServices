using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	[SuppressBindingMemberBashingTest] // for CarrierMAWBRadioButton
	public partial class AddJobMawbRangeForm : ZForm
	{
		public AddJobMawbRangeForm(RangeJobMawb businessEntity) : base(businessEntity)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
#if DEBUG
			TypeDescriptor.AddAttributes(HomePortTextLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(Airline2LetterCodeLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			RangeJobMawb rangeJobMawb = (RangeJobMawb)BusinessEntity;
			rangeJobMawb.Save();
			base.Save(factories);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			ZString duplicatesMessage = ((RangeJobMawb)BusinessEntity).DuplicatesInSaveRange();
			if (!duplicatesMessage.IsEmpty)
			{
				result = ContinueWithSave.No;
				Globals.Message.ShowError(duplicatesMessage, Res.GetString("9dca0912-f086-45be-a139-e0317f02bec1", "Error"));
			}

			return result;
		}

		protected void NumericOnly_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (!Char.IsControl(e.KeyChar) && !Char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}
	}
}
