using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class NMFSEditForm : ZChildForm
	{
		readonly NMFSControlVisibilityHelper visibilityHelper;

		public NMFSEditForm(NMFSLine line)
			: base(line)
		{
			visibilityHelper = new NMFSControlVisibilityHelper(this.HarvestingDetailsGrid, this.DocumentDetailsGroupBox, this.HarvestingVesselsGroupBox)
			{
				CurrentLine = line
			};
			HideIrrelevantFields();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public new NMFSLine CurrentDataItem
		{
			get
			{
				return base.CurrentDataItem as NMFSLine;
			}
		}

		void HideIrrelevantFields()
		{
			IEnumerable<string> list = CurrentDataItem.GetFieldsToHide();
			if (CurrentDataItem != null)
			{
				LeftTopPanel.Controls
			   .Cast<Control>()
			   .Where(control => control.Tag != null &&
						list
						.Any(th => th.Equals(control.Tag as string, StringComparison.Ordinal)))
						.ForEach(c => c.Visible = false);
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				visibilityHelper.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
