using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	public partial class USDrawbackCustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public USDrawbackCustomsBrokerageUserControl()
		{
			InitializeComponent();
			MessagesTabPage.Text = "Messages";
			InvoiceLinesTabPage.Text = "Lines";
			InvoicesTabPage.TabRelevant = false;
			InvoiceGroupingTabPage.TabRelevant = false;
			ReOrderTabPages();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		#region Change User Control on Sub Message

		void ReOrderTabPages()
		{
			List<TabPage> tabPages = new List<TabPage>();
			foreach (TabPage tabPage in MainTabControl.Controls)
			{
				tabPages.Add(tabPage);
			}
			foreach (TabPage tabPage in tabPages)
			{
				MainTabControl.Controls.Remove(tabPage);
				MainTabControl.Controls.Add(tabPage);
			}
		}

		#endregion

		#region Create New User Controls for each tab

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			return new DrawbackInvoiceLineUserControl();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new DrawbackJobDeclarationUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new ImportMessagesUserControl();
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new DrawbackMiscOptionsUserControl();
		}

		#endregion

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			ChangeMiscTabVisibility();
		}

		protected override void HookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);

			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}
		}

		protected override void UnHookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}

			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeMiscTabVisibility();
		}

		void ChangeMiscTabVisibility()
		{
			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			MiscOptionsTabPage.TabRelevant = isACEDrawback;
			StatusTabPage.TabRelevant = isACEDrawback;
		}
	}
}
