using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeDetailSelectionForm : ZChildForm
	{
		public TradeDetailSelectionForm(EntityTradeDetailWrapperCollection tradeDetailCollection)
			: base(tradeDetailCollection)
		{
			Argument.NotNull(tradeDetailCollection, "tradeDetailCollection");

			InitializeComponent();

			tradeDetailsGridIdWhenShowingTradeLaneColumns = tradeDetailsGrid.GridId;
			tradeDetailsGridLayoutKeyWhenShowingTradeLaneColumns = tradeDetailsGrid.LayoutKey;
		}

		public new EntityTradeDetailWrapperCollection BusinessEntity
		{
			get { return (EntityTradeDetailWrapperCollection)base.BusinessEntity; }
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Columns

		public ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}

		[DefaultValue(true)]
		public bool ShowTradeLaneColumns
		{
			get { return showTradeLaneColumns; }
			set
			{
				if (showTradeLaneColumns != value)
				{
					showTradeLaneColumns = value;

					tradeDetailsGrid.SetAvailability(false,
						new[]
						{
							"Parent+" + OrgSalesSchema.Constants.OW_OriginID,
							"Parent+" + OrgSalesSchema.Constants.OW_DestinationID,
							"Parent+" + OrgSalesSchema.Constants.OW_OH_Buyer,
							"Parent+" + OrgSalesSchema.Constants.OW_OH_Supplier,
						});

					if (value)
					{
						this.tradeDetailsGrid.GridId = tradeDetailsGridIdWhenShowingTradeLaneColumns;
						this.tradeDetailsGrid.LayoutKey = tradeDetailsGridLayoutKeyWhenShowingTradeLaneColumns;
					}
					else
					{
						this.tradeDetailsGrid.GridId = tradeDetailsGridIdWhenShowingTradeLaneColumns + ".HideTradeLaneColumns";
						this.tradeDetailsGrid.LayoutKey = tradeDetailsGridLayoutKeyWhenShowingTradeLaneColumns + "|HideTradeLaneColumns";
					}
				}
			}
		}
		bool showTradeLaneColumns = true;

		readonly string tradeDetailsGridIdWhenShowingTradeLaneColumns;
		readonly string tradeDetailsGridLayoutKeyWhenShowingTradeLaneColumns;

		#endregion

		#region Select Button

		public ZButton SelectButton
		{
			get { return selectButton; }
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			SelectAndClose();
		}

		void TradeDetailsGrid_DoubleClick(object sender, EventArgs e)
		{
			SelectAndClose();
		}

		void SelectAndClose()
		{
			SelectedTradeDetail = tradeDetailsGrid.ListManager.GetCurrent() as OrgTradeDetail;
			if (SelectedTradeDetail != null)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("bbad3b08-8034-4ba4-ab0f-2597780ea813", "Please select a row."));
			}
		}

		#endregion

		#region Continue Without Selection

		[DefaultValue(true)]
		public bool AllowContinueWithoutSelection
		{
			get { return continueButton.Visible; }
			set { continueButton.Visible = value; }
		}

		void ContinueButton_Click(object sender, EventArgs e)
		{
			SelectedTradeDetail = null;
			DialogResult = DialogResult.OK;
			Close();
		}

		#endregion

		#region Cancel

		public string CancelButtonText
		{
			get { return cancelButton.Text; }
			set { cancelButton.Text = value; }
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		public OrgTradeDetail SelectedTradeDetail;
	}
}
