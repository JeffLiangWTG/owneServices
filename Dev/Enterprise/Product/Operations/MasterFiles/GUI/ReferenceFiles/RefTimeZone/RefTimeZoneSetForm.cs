using System;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefTimeZoneSetForm : ZTemplateForm
	{
		public RefTimeZoneSetForm(RefTimeZoneSet timeZoneSet)
			: base(timeZoneSet)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			PlugIns.Add(ControllerIDs.Audit);
		}

		void RefTimeZoneSetForm_Load(object sender, EventArgs e)
		{
			if (!DesignMode)
			{
				TimeZoneSetTextBox.GetExtension<LabelCaptionRenderer>().Font = new Font(TimeZoneSetTextBox.GetExtension<LabelCaptionRenderer>().Font, FontStyle.Bold);
			}
		}
	}
}
