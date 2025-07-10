using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class InvoiceModuleButtonGrid : ZModuleButtonGrid
	{
		public InvoiceModuleButtonGrid()
		{
			InnerGrid.AfterBind += new EventHandler(InnerGrid_AfterBind);
		}

		protected override bool AllowDoubleClick => false;

		void InnerGrid_AfterBind(object sender, EventArgs e)
		{
			EnableAttachInvoiceSupportIfAppropriate();
		}

		readonly Hashtable parentTabControls = new Hashtable();

		void DiscoverAndHookParentTabControls(Control topControl, ZTabPage lastTabPage)
		{
			ZTabControl tabControl = topControl as ZTabControl;
			ZTabPage tabPage = topControl as ZTabPage;
			if (tabControl != null)
			{
				parentTabControls.Add(tabControl, tabControl.TabPages.IndexOf(lastTabPage));
				tabControl.SelectedIndexChanged += new EventHandler(TabControl_SelectedIndexChanged);
			}

			if (topControl.Parent != null)
			{
				DiscoverAndHookParentTabControls(topControl.Parent, tabPage ?? lastTabPage);
			}
		}

		protected void DiscoverAndHookParentTabControls()
		{
			if (parentTabControls.Count > 0)
			{
				throw new NotSupportedException("Cannot hook parent controls twice");
			}

			DiscoverAndHookParentTabControls(this, null);
		}

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateButtonsVisible();
		}

		protected void UpdateButtonsVisible()
		{
			if (addedAttachGUI)
			{
				foreach (ZTabControl control in parentTabControls.Keys)
				{
					if (control.SelectedIndex != (int)parentTabControls[control])
					{
						SetButtonVisible(false);
						return;
					}
				}
				SetButtonVisible(true);
			}
		}

		protected void EnableAttachInvoiceSupportIfAppropriate()
		{
			if (!addedAttachGUI && Collection != null)
			{
				var datasource = this.DataSource as BaseJobDeclaration;
				if (datasource != null && datasource.EnableAttachCommercialInvoice)
				{
					AddAttachMenu();
					AddAttachButtons();
				}
				AddOnelinerMenuItem();
				DiscoverAndHookParentTabControls();
				addedAttachGUI = true;
				UpdateButtonsVisible();
			}
		}

		void AddOnelinerMenuItem()
		{
			InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("dffe8689-907e-4500-bb54-e8d4dd4dca62", "Auto-create &one-liner"), AutoCreatOneLinerMenu_Click));
		}
		bool addedAttachGUI;

		void AddAttachMenu()
		{
			InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("InvoiceModuleButtonGrid.AttachInvoiceButton", "&Attach Commercial Invoices"), AttachButton_Click));
			InnerGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("InvoiceModuleButtonGrid.DetachInvoiceButton", "&Detach Commercial Invoice"), DetachButton_Click));
		}

		internal ZButton attachInvoiceButton;
		internal ZButton detachInvoiceButton;

		void AddAttachButtons()
		{
			attachInvoiceButton = CreateNewBottomOfScreenButton(Res.GetString("InvoiceModuleButtonGrid.AttachInvoiceButton", "&Attach Commercial Invoices"), 0, AttachButton_Click);
			detachInvoiceButton = CreateNewBottomOfScreenButton(Res.GetString("InvoiceModuleButtonGrid.DetachInvoiceButton", "&Detach Commercial Invoice"), 148, DetachButton_Click);
		}

		void SetButtonVisible(bool isVisible)
		{
			if (attachInvoiceButton != null)
			{
				attachInvoiceButton.Visible = isVisible;
			}

			if (detachInvoiceButton != null)
			{
				detachInvoiceButton.Visible = isVisible;
			}
		}

		ZButton CreateNewBottomOfScreenButton(string text, int leftOffset, EventHandler clickEvent)
		{
			ZButton button = new ZButton();
			button.Anchor = AnchorStyles.Bottom;
			button.Text = text;
			ControlDpiScalingHelper.SetLeft(ref button, 327 + leftOffset, true);
			ControlDpiScalingHelper.SetTop(ref button, Form.ClientRectangle.Bottom - button.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(26), false);
			ControlDpiScalingHelper.SetWidth(ref button, 150, true);
			Form.Controls.Add(button);
			button.BringToFront();
			button.Visible = false;
			button.Click += clickEvent;
			return button;
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ZArchitecture.Modules.ModuleIdentifier moduleID)
		{
			return new InvoiceAttacher(destinationCollection, findBoxList);
		}

		void AutoCreatOneLinerMenu_Click(object sender, EventArgs e)
		{
			AutoCreateOneLiner();
		}

		void AutoCreateOneLiner()
		{
			SelectFirstRowIfOnlyRowInGrid();
			BusinessObject[] selected = InnerGrid.SelectedElements;
			if (selected.Length == 0)
			{
				ShowNotSelectedMessage();
			}
			else
			{
				foreach (BaseJobComInvoiceHeader invoiceHeader in selected)
				{
					invoiceHeader.AutoCreateOneInvoiceLineFromHeaderDetailsIfNoLinesAlreadyExist();
				}
			}
		}

		protected override ZGridWithoutColumnStylesSerialisation CreateNewGrid()
		{
			return new BaseInvoiceArrayBoundGrid();
		}

		public BaseInvoiceArrayBoundGrid InvoiceInnerGrid // public newing this causes designer to eat everything
		{
			get { return (BaseInvoiceArrayBoundGrid)InnerGrid; }
		}

		protected internal void AttachButton_ClickInternal(object sender, EventArgs e) => AttachButton_Click(sender, e);

		protected internal void DetachButton_ClickInternal(object sender, EventArgs e) => DetachButton_Click(sender, e);

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (Collection != null)
			{
				if (Collection.ReadOnly)
				{
					Globals.Message.ShowInformation(Res.GetString("232a4291-7162-4e44-84ad-14845483f1c6", "Sorry, Commercial Invoices can be viewed but not detached."), Res.GetString("eb0323d6-a5cf-4e51-bf30-5314e68dbe56", "Cannot detach..."));
				}
				else
				{
					base.DetachButton_Click(sender, e);
				}
			}
		}
	}
}
