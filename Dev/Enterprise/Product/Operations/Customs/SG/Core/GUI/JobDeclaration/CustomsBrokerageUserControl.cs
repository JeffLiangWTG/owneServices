using System;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class CustomsBrokerageUserControl : Customs.GUI.BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
		}

		protected override void OnJobDeclarationSet()
		{
			base.OnJobDeclarationSet();
			AddSGDetailsTab();
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (InvoiceGroupingTabPage != null && !InvoiceGroupingTabPage.IsDisposed)
			{
				InvoiceGroupingTabPage.Dispose();
			}
		}

		protected override void JobDeclaration_MergedSuccessfully()
		{
		}

		#region Change User Control on Application Version

		protected override void HookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			if (declaration != null)
			{
				var sgDeclaration = (JobDeclaration)declaration;
				sgDeclaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}
		}

		protected override void UnHookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				var sgDeclaration = (JobDeclaration)declaration;
				sgDeclaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			RemoveControl(sGDeclarationTabPage);
			RefreshSGDetailsTab();
		}

		#endregion

		#region SGDetails Tab

		void AddSGDetailsTab()
		{
			if (sGDeclarationTabPage == null)
			{
				sGDeclarationTabPage = new Customs.GUI.BaseDeclarationTabPage();
				sGDeclarationTabPage.CheckForNotifications = true;
				sGDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23);
				sGDeclarationTabPage.Name = "SGDetailsTabPage";
				sGDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629);
				sGDeclarationTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("10E55357-32E2-4C48-9208-EE4295BE5B6F", "Details");
				MainTabControl.TabPages.Insert(sGDeclarationTabPage, 1);
				sGDeclarationTabPage.LazyCreateControls += new EventHandler(SGDeclarationTabPage_LazyCreateControls);
			}
		}
		Customs.GUI.BaseDeclarationTabPage sGDeclarationTabPage;

		void RefreshSGDetailsTab()
		{
			if (JobDeclaration.JE_ApplicationCode == SGConstants.TradeNetVersion.FourPointOne)
			{
				LoadSGTN41DetailsUserControl();
			}
			else
			{
				LoadSGDetailsUserControl();
			}
		}

		void SGDeclarationTabPage_LazyCreateControls(object sender, EventArgs e)
		{
			RefreshSGDetailsTab();
		}

		void LoadSGTN41DetailsUserControl()
		{
			if (sGDeclarationTabPage != null && sGDeclarationTabPage.Controls.Count == 0)
			{
				fSGTN41DeclarationUserControl = new SGTN41DeclarationUserControl();
				fSGTN41DeclarationUserControl.JobDeclaration = JobDeclaration;
				fSGTN41DeclarationUserControl.Dock = DockStyle.Fill;
				sGDeclarationTabPage.Controls.Add(fSGTN41DeclarationUserControl);
				fSGTN41DeclarationUserControl.SetDataBinding(JobDeclaration, "");
			}
		}
		SGTN41DeclarationUserControl fSGTN41DeclarationUserControl;

		void LoadSGDetailsUserControl()
		{
			if (sGDeclarationTabPage != null && sGDeclarationTabPage.Controls.Count == 0)
			{
				fSGDeclarationUserControl = new SGDeclarationUserControl();
				fSGDeclarationUserControl.JobDeclaration = JobDeclaration;
				fSGDeclarationUserControl.Dock = DockStyle.Fill;
				sGDeclarationTabPage.Controls.Add(fSGDeclarationUserControl);
				fSGDeclarationUserControl.SetDataBinding(JobDeclaration, "");
			}
		}
		SGDeclarationUserControl fSGDeclarationUserControl;

		#endregion

		#region Overridden Controls

		protected override Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new SGJobDeclarationUserControl();
		}

		protected override Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new MiscOptionsUserControl();
		}

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new SGSupplierHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			return new SGInvoiceLineUserControl();
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new ImportMessageUserControl();
		}

		protected override Customs.GUI.BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl()
		{
			return new GroupInvoiceUserControl();
		}

		protected override Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new ContainerUserControl();
		}

		#endregion
	}
}
