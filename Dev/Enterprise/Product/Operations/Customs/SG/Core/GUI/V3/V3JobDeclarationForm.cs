using System;
using System.Windows.Forms;
using Enterprise.Customs.SG.V3.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V3.GUI
{
	public partial class V3JobDeclarationForm : ZChildForm
	{
		public V3JobDeclarationForm()
		{
		}

		public V3JobDeclarationForm(V3Brokerage brokerage)
			: base(brokerage)
		{
			ZUserControl control = new V3CustomsBrokerageUserControl();
			control.Dock = DockStyle.Fill;
			MainPanel.Controls.Add(control);
		}

		public override string FormCaption => V4.GUI.Res.GetString("2A5DD658-06C8-40C4-B328-32D52DB4E710", "TradeNet V3 Declaration");

		public override string FormVerb => V4.GUI.Res.GetString("B3F67CC4-551C-4F6F-8105-AC16C3EFD108", "View");

		void Close_Button_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}
	}
}
