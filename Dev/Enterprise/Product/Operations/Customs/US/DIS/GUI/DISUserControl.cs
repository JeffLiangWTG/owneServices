using System;
using System.Windows.Forms;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.DIS.GUI
{
	public partial class DISUserControl : ZUserControl
	{
		public DISUserControl()
		{
			InitializeComponent();

			DocumentsGrid.AfterBind += DocumentsGrid_AfterBind;
			OptionalDataTabControl.SelectedIndexChanged += OptionalDataTabControl_SelectedIndexChanged;

			MainSplitter.AllowOverlap(BottomDetailsPanel);
			OptionalDataUnavailableLabel.AllowOverlap(OptionalDataTabPagePanel);
			splitter1.AllowOverlap(MessagesGridPanel);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetVisibilityForExportDeclaration();
		}

		void SetVisibilityForExportDeclaration()
		{
			string[] columns = new string[] { DISDocument.Schema.ShipmentNo, DISDocument.Schema.ITN, DISDocument.Schema.XTN };
			var disHost = CurrentDataItem as DISHostWrapper;
			if (disHost != null)
			{
				ShipmentNoDropEdit.Visible = disHost.IsExport;
				DocumentsGrid.SetAvailability(disHost.IsExport, columns);
			}
			else
			{
				ShipmentNoDropEdit.Visible = false;
				DocumentsGrid.SetAvailability(false, columns);
			}
		}
		void OptionalDataTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			ChangeOptionalDataVisiblity();
		}

		void DocumentsGrid_AfterBind(object sender, EventArgs e)
		{
			DocumentsGrid.ListManager.PositionChanged += ListManager_PositionChanged;
			ListenToDocumentLabelValueChanged();
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			ListenToDocumentLabelValueChanged();

			previousDISDocument = (DISDocument)DocumentsGrid.ListManager.GetCurrent();

			ChangeOptionalDataVisiblity();
		}

		void ListenToDocumentLabelValueChanged()
		{
			if (previousDISDocument != null)
			{
				previousDISDocument.DocumentLabelInfo.ValueChanged -= DocumentLabelInfo_ValueChanged;
			}

			var currentDISDocument = (DISDocument)DocumentsGrid.ListManager.GetCurrent();

			if (currentDISDocument != null)
			{
				currentDISDocument.DocumentLabelInfo.ValueChanged += DocumentLabelInfo_ValueChanged;
			}
		}

		DISDocument previousDISDocument;
		void ChangeOptionalDataVisiblity()
		{
			var currentDISDocument = (DISDocument)DocumentsGrid.ListManager.GetCurrent();

			if (currentDISDocument != null)
			{
				BondDataGroupBox.Visible = currentDISDocument.BondDataVisible;
				ToxicSubstanceGroupBox.Visible = currentDISDocument.ToxicSubstanceVisible;
				PermitGroupBox.Visible = currentDISDocument.PermitVisible;
				CertificateGroupBox.Visible = currentDISDocument.CertificateVisible;
				PackingListGroupBox.Visible = currentDISDocument.PackingListVisible;
				InvoiceGroupBox.Visible = currentDISDocument.InvoiceVisible;
				CommodityGroupBox.Visible = currentDISDocument.CommodityDataVisible;
				OptionalDataUnavailableLabel.Visible = currentDISDocument.NoOptionalDataVisible;

				BringVisibleControlsToFront();
			}
			else
			{
				OptionalDataUnavailableLabel.Text = DISDocument.EnterAFormTypeMessage;
				OptionalDataUnavailableLabel.Visible = true;
				OptionalDataUnavailableLabel.BringToFront();
			}
		}

		void DocumentLabelInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeOptionalDataVisiblity();
		}

		void BringVisibleControlsToFront()
		{
			if (OptionalDataUnavailableLabel.Visible)
			{
				OptionalDataUnavailableLabel.BringToFront();
			}
			else
			{
				OptionalDataTopPanel.Visible = true;
				InvoiceCommodityPanel.Visible = true;

				bool hasVisibleControlsInTopPanel = false;

				foreach (Control control in OptionalDataTopPanel.Controls)
				{
					if (control.Visible)
					{
						control.BringToFront();

						if (!hasVisibleControlsInTopPanel)
						{
							hasVisibleControlsInTopPanel = true;
						}
					}
				}

				bool hasVisibleControlsInBottomPanel = false;
				foreach (Control control in InvoiceCommodityPanel.Controls)
				{
					if (control.Visible)
					{
						control.BringToFront();

						if (!hasVisibleControlsInBottomPanel)
						{
							hasVisibleControlsInBottomPanel = true;
						}
					}
				}

				if (!hasVisibleControlsInTopPanel && hasVisibleControlsInBottomPanel)
				{
					OptionalDataTopPanel.Visible = false;
				}

				if (!hasVisibleControlsInBottomPanel && hasVisibleControlsInTopPanel)
				{
					InvoiceCommodityPanel.Visible = false;
				}
			}
		}
	}
}
