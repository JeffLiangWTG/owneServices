using System;
using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class LooseBookedMoveSplitForm : ZChildForm
	{
		public LooseBookedMoveSplitForm(LooseBookedMoveSplitMaster master)
			: base(master)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		LooseBookedMoveSplitMaster Master
		{
			get { return (LooseBookedMoveSplitMaster)BusinessEntity; }
		}

		void AddSplitButton_Click(object sender, EventArgs e)
		{
			Master.AddRemaining();
		}

		void RemoveSplitButton_Click(object sender, EventArgs e)
		{
			if (SplitsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("33195a4b-f3f0-41d3-b2ce-a22ce98a2b77", "No rows selected."), Res.GetString("29479f2a-999a-4476-b949-8f268d4e2423", "Remove Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				foreach (LooseBookedMoveSplit split in SplitsGrid.SelectedElements)
				{
					Master.Splits.RemoveAndDelete(split);
				}
			}
		}

		void DivideEquallyButton_Click(object sender, EventArgs e)
		{
			Master.DivideEqually();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			Master.RunPreSaveValidation();
			if (Master.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Master.Done();
				if (Master.MoveToSplit.EW_BookedHeight != 0 || Master.MoveToSplit.EW_BookedLength != 0 || Master.MoveToSplit.EW_BookedWidth != 0)
				{
					Globals.Message.Show(Res.GetString("70336ff8-9db3-4e9b-adc6-09bc58f8ac50", "The original Loose Booked Move has dimensions that are unable to be split. Please split manually."), Res.GetString("9d454ded-8495-4430-bcc7-9ab270d9b6e9", "Dimensions"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				Close();
			}
		}

		void TheCancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
