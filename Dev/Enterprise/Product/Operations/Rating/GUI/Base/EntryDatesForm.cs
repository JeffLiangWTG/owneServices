using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class EntryDatesForm : ZChildForm
	{
		public EntryDatesForm(EntryStartEndDates startEnd)
			: base(startEnd)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("e06e20cc-1277-4e5a-bddb-9fd47f672d23", "Start & End"); }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}

